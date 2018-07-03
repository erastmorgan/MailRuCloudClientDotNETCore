//-----------------------------------------------------------------------------------------------
// <copyright file="CloudClient.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using MailRuCloudClient.Data;
    using MailRuCloudClient.Data.CloudStructure;
    using MailRuCloudClient.Data.Shard;
    using MailRuCloudClient.Events;
    using MailRuCloudClient.Exceptions;
    using MailRuCloudClient.HttpContent;
    using MailRuCloudClient.Static;
    using MailRuCloudClient.Static.Helpers;

    /// <summary>
    /// General connector with MAILRU API.
    /// </summary>
    public class CloudClient
    {
        /// <summary>
        /// Async tasks cancelation token.
        /// </summary>
        private CancellationTokenSource cancelToken = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClient" /> class. Do not forget to set Account properties before using any API functions.
        /// </summary>
        public CloudClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClient" /> class.
        /// </summary>
        /// <param name="account">Cloud account.</param>
        public CloudClient(Account account)
        {
            Debug.Assert(account != null, "Is null.");
            account.CheckAuthorization(true).Wait();
            this.Account = account;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClient" /> class. Do not forget to authorize account.
        /// </summary>
        /// <param name="email">Login name as the email.</param>
        /// <param name="password">Password, associated with this email.</param>
        public CloudClient(string email, string password)
        {
            this.Account = new Account(email, password);
        }

        /// <summary>
        /// Changing progress event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);

        /// <summary>
        /// Changing progress event, works only for upload and download operations.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChangedEvent;

        /// <summary>
        /// Gets or sets the related MAILRU account.
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Provides the one-time anonymous direct link to download the file. Important: the file should has the public link.
        /// </summary>
        /// <param name="publicLink">The public file link.</param>
        /// <returns>One-time direct link.</returns>
        public async Task<string> GetFileOneTimeDirectLink(string publicLink)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(publicLink) && publicLink.StartsWith(Urls.PublicLink), "Is not correct public link.");

            await this.Account.CheckAuthorization(false);
            var values = this.GetDefaultFormDataFields();
            values.RemoveAll(x => x.Key == "conflict");
            var responseMsg = await this.Account.HttpClient.PostAsync(Urls.DownloadToken, values.ToFormUrlEncodedContent());
            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var key = responseStr.Deserialize<AuthToken>().Token;
            var shardUrl = (await this.GetShardsInfo()).WeblinkGet[0].Url;
            return string.Format("{0}/{1}?key={2}", shardUrl, publicLink.Replace(Urls.PublicLink, string.Empty), key);
        }

        /// <summary>
        /// Publish the file or folder.
        /// </summary>
        /// <typeparam name="T">The type of entry. File or folder.</typeparam>
        /// <param name="sourceFullPath">The full file or folder path.</param>
        /// <returns>The published file or folder info.</returns>
        public async Task<T> Publish<T>(string sourceFullPath) where T : CloudStructureEntryBase
        {
            return await this.PublishUnpublishInternal<T>(sourceFullPath, true);
        }

        /// <summary>
        /// Unpublish the file or folder.
        /// </summary>
        /// <typeparam name="T">The type of entry. File or folder.</typeparam>
        /// <param name="publicLink">The public file or folder path.</param>
        /// <returns>The unpublished file or folder info.</returns>
        public async Task<T> Unpublish<T>(string publicLink) where T : CloudStructureEntryBase
        {
            return await this.PublishUnpublishInternal<T>(publicLink, false);
        }

        /// <summary>
        /// Restore the file from history.
        /// </summary>
        /// <param name="sourceFullPath">The source file full path.</param>
        /// <param name="historyRevision">The unique history revision number from which will be the restoring.</param>
        /// <param name="rewriteExisting">When true, an existing parent file will be overriden,
        /// otherwise the file from history will be created as new.</param>
        /// <param name="newFileName">The new file name. It will be applied only if previous parameter is false.</param>
        /// <returns>The restored file info.</returns>
        public async Task<Data.File> RestoreFileFromHistory(
            string sourceFullPath, long historyRevision, bool rewriteExisting, string newFileName = null)
        {
            Debug.Assert(historyRevision > 0, "Less than 1.");
            if (this.Account.Has2GBUploadSizeLimit)
            {
                throw new CloudClientException(
                    "The current operation is not supported for your account. Please, upgrade your tariff plan.",
                    ErrorCode.NotSupportedOperation);
            }

            var histories = await this.GetFileHistory(sourceFullPath);
            var history = histories.FirstOrDefault(x => x.Revision == historyRevision);
            if (history == null)
            {
                throw new CloudClientException(
                    "History not exists by specified revision number.", nameof(historyRevision), ErrorCode.HistoryNotExists);
            }

            var originalFileName = sourceFullPath.Split(new[] { '/' }).Last();
            var extension = Path.GetExtension(originalFileName);
            if (newFileName == null)
            {
                newFileName = originalFileName;
            }
            else if (!newFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                newFileName += extension;
            }

            var newFullPath = rewriteExisting ? sourceFullPath : this.GetParentCloudPath(sourceFullPath) + newFileName;
            var created = await this.CreateFileOrFolder(true, newFullPath, history.Hash, history.SizeBytes, rewriteExisting);
            return new Data.File(this.Account)
            {
                FullPath = created.NewPath,
                Hash = history.Hash,
                LastModifiedTimeUTC = history.LastModifiedTimeUTC,
                Name = created.NewName,
                Size = history.Size
            };
        }

        /// <summary>
        /// Get the file history.
        /// </summary>
        /// <param name="sourceFullPath">The full file path in the cloud.</param>
        /// <returns>The file modification history.</returns>
        public async Task<History[]> GetFileHistory(string sourceFullPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFullPath), "Is null or empty.");

            await this.Account.CheckAuthorization(false);
            sourceFullPath = this.GetPathStartEndSlash(sourceFullPath, setAtEnd: false);
            var values = this.GetDefaultFormDataFields(sourceFullPath);
            values.RemoveAll(x => x.Key == "conflict");
            var responseMsg = await this.Account.HttpClient.PostAsync(
                Urls.History, values.ToFormUrlEncodedContent(), sourceFullPath, this.Account.Email, this.Account.AuthToken);
            if (responseMsg.StatusCode == HttpStatusCode.NotFound)
            {
                throw new CloudClientException(
                    "The file by specified path does not exists.", nameof(sourceFullPath), ErrorCode.PathNotExists);
            }

            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var historyList = responseStr.Deserialize<History[]>();
            historyList[0].IsCurrentVersion = true;
            return historyList;
        }

        /// <summary>
        /// Remove the file or folder.
        /// </summary>
        /// <param name="sourceFullPath">The full path of file or folder in the cloud.</param>
        /// <returns>The simple task.</returns>
        public async Task Remove(string sourceFullPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFullPath), "Is null or empty.");

            await this.Account.CheckAuthorization(false);
            sourceFullPath = this.GetPathStartEndSlash(sourceFullPath, setAtEnd: false);
            var values = this.GetDefaultFormDataFields(sourceFullPath);
            await this.Account.HttpClient.PostAsync(Urls.Remove, values.ToFormUrlEncodedContent());
        }

        /// <summary>
        /// Rename the cloud structure entry.
        /// </summary>
        /// <typeparam name="T">The entry type. Folder or file.</typeparam>
        /// <param name="sourceFullPath">The source folder or file full path.</param>
        /// <param name="name">The new name of file or folder.</param>
        /// <returns>The renamed file or folder.</returns>
        public async Task<T> Rename<T>(string sourceFullPath, string name) where T : CloudStructureEntryBase
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFullPath), "Is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(name), "Is null or empty.");

            await this.Account.CheckAuthorization(false);
            sourceFullPath = this.GetPathStartEndSlash(sourceFullPath, setAtEnd: false);
            var item = await this.CheckUnknownItemExisting<T>(sourceFullPath);
            if (typeof(T) == typeof(Data.File))
            {
                var extension = Path.GetExtension(item.Name);
                if (!name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    name += extension;
                }
            }

            var values = this.GetDefaultFormDataFields(sourceFullPath);
            values.Add(new KeyValuePair<string, object>("name", name));
            var responseMsg = await this.Account.HttpClient.PostAsync(Urls.Rename, values.ToFormUrlEncodedContent());
            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var newPath = responseStr.Deserialize<string>();
            var newName = newPath.Split(new[] { '/' }).Last();
            item.PublicLink = null;
            item.FullPath = newPath;
            item.Name = newName;
            return item;
        }

        /// <summary>
        /// Copy the cloud structure entry.
        /// </summary>
        /// <typeparam name="T">The entry type. Folder or file.</typeparam>
        /// <param name="sourceFullPath">Source folder or file full path.</param>
        /// <param name="destFolderPath">The destination path of entry.</param>
        /// <returns>The entry info in the new location of cloud.</returns>
        public async Task<T> Copy<T>(string sourceFullPath, string destFolderPath) where T : CloudStructureEntryBase
        {
            return await this.MoveOrCopyInternal<T>(sourceFullPath, destFolderPath, false);
        }

        /// <summary>
        /// Move the cloud structure entry.
        /// </summary>
        /// <typeparam name="T">The entry type. Folder or file.</typeparam>
        /// <param name="sourceFullPath">Source folder or file full path.</param>
        /// <param name="destFolderPath">The destination path of entry.</param>
        /// <returns>The entry info in the new location of cloud.</returns>
        public async Task<T> Move<T>(string sourceFullPath, string destFolderPath) where T : CloudStructureEntryBase
        {
            return await this.MoveOrCopyInternal<T>(sourceFullPath, destFolderPath, true);
        }

        /// <summary>
        /// Create all directories and subdirectories in the specified path unless they already exists.
        /// </summary>
        /// <param name="fullFolderPath">The full path of the new folder.</param>
        /// <returns>The created folder info.</returns>
        public async Task<Folder> CreateFolder(string fullFolderPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(fullFolderPath), "Is null or empty.");

            await this.Account.CheckAuthorization(false);
            fullFolderPath = this.GetPathStartEndSlash(fullFolderPath);
            var createdFolder = await this.CreateFileOrFolder(false, fullFolderPath);
            return new Folder(this.Account)
            {
                Name = createdFolder.NewName,
                FullPath = createdFolder.NewPath
            };
        }

        /// <summary>
        /// Download the files and folders to ZIP archive by selected paths. 
        /// All files and folders should be in the same folder.
        /// </summary>
        /// <param name="filesAndFoldersPaths">The full paths of the files and folders.</param>
        /// <param name="destZipArchiveName">The output ZIP archive name. If not set, will be generated the GUID.</param>
        /// <param name="destFolderPath">The destination folder on the local machine</param>
        /// <returns>The downloaded ZIP archive info.</returns>
        public async Task<FileInfo> DownloadItemsAsZIPArchive(
            List<string> filesAndFoldersPaths, string destZipArchiveName, string destFolderPath)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(destFolderPath) && Directory.Exists(destFolderPath),
                "Is null or empty or folder does not exists.");

            await this.Account.CheckAuthorization(false);
            if (destFolderPath.EndsWith(@"\"))
            {
                destFolderPath += @"\";
            }

            destZipArchiveName = string.IsNullOrEmpty(destZipArchiveName) ? Guid.NewGuid().ToString() : destZipArchiveName;
            if (!destZipArchiveName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                destZipArchiveName += ".zip";
            }

            var fullDestPath = destFolderPath + destZipArchiveName;
            using (var stream = new FileStream(fullDestPath, FileMode.Create))
            {
                await this.DownloadItemsAsZIPArchive(filesAndFoldersPaths, stream);
            }

            return new FileInfo(fullDestPath);
        }

        /// <summary>
        /// Download the files and folders to ZIP archive by selected paths. 
        /// All files and folders should be in the same folder.
        /// </summary>
        /// <param name="filesAndFoldersPaths">The full paths of the files and folders.</param>
        /// <param name="destStream">The destination ZIP archive stream.</param>
        /// <returns>The simple task.</returns>
        public async Task DownloadItemsAsZIPArchive(List<string> filesAndFoldersPaths, Stream destStream)
        {
            Debug.Assert(destStream != null, "Destination stream is null.");

            var file = await this.DownloadItemsAsZIPArchive(filesAndFoldersPaths);
            await file.NetworkStream.CopyToAsync(file.Length, destStream, this.ProgressChangedEvent, this, this.cancelToken);
        }

        /// <summary>
        /// Download the files and folders to ZIP archive by selected paths. 
        /// All files and folders should be in the same folder.
        /// </summary>
        /// <param name="filesAndFoldersPaths">The full paths of the files and folders.</param>
        /// <returns>The network stream and his length.
        /// The length inaccurate for ZIP archive, because it's not possible to compute.
        /// Usually, the real length is larger, than specified.</returns>
        public async Task<(Stream NetworkStream, long Length)> DownloadItemsAsZIPArchive(List<string> filesAndFoldersPaths)
        {
            await this.Account.CheckAuthorization(false);
            var link = await this.GetDirectLinkZIPArchive(filesAndFoldersPaths, null);

            for (var i = 0; i < filesAndFoldersPaths.Count; i++)
            {
                filesAndFoldersPaths[i] = filesAndFoldersPaths[i].TrimEnd('/');
                filesAndFoldersPaths[i] = filesAndFoldersPaths[i].Replace("\"", string.Empty);
                filesAndFoldersPaths[i] = this.GetPathStartEndSlash(filesAndFoldersPaths[i], setAtEnd: false);
            }

            var parentPath = filesAndFoldersPaths[0];
            var parentFolderInfo = await this.GetFolder(parentPath);
            var contentLength = 0L;
            var files = parentFolderInfo.Files.ToList();
            var folders = parentFolderInfo.Folders.ToList();
            foreach (var path in filesAndFoldersPaths)
            {
                files.ForEach(x => TryAddContentLength(x.FullPath, path, x.Size));
                folders.ForEach(x => TryAddContentLength(x.FullPath, path, x.Size));
            }

            void TryAddContentLength(string fullPath1, string fullPath2, Size size)
            {
                if (fullPath1.Equals(fullPath2, StringComparison.OrdinalIgnoreCase))
                {
                    contentLength += size.DefaultValue;
                }
            }

            var networkStream = await this.Account.HttpClient.GetStreamAsync(link);
            return (networkStream, contentLength);
        }

        /// <summary>
        /// Provides the anonymous direct link to download of ZIP archive for selected files and folders.
        /// </summary>
        /// <param name="filesAndFoldersPaths">The files and folders paths list in cloud.</param>
        /// <param name="destZipArchiveName">The output ZIP archive name. If not set, will be generated the GUID.</param>
        /// <returns>The direct link to download as ZIP archive.</returns>
        public async Task<string> GetDirectLinkZIPArchive(List<string> filesAndFoldersPaths, string destZipArchiveName)
        {
            Debug.Assert(filesAndFoldersPaths != null && filesAndFoldersPaths.Count > 0, "Is null or empty.");
            Debug.Assert(
                filesAndFoldersPaths.All(x => !string.IsNullOrEmpty(x) && x != "/"),
                "One of paths is null or empty or indicates to home directory.");

            await this.Account.CheckAuthorization(false);
            destZipArchiveName = string.IsNullOrEmpty(destZipArchiveName) ? Guid.NewGuid().ToString() : destZipArchiveName;
            if (!destZipArchiveName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                destZipArchiveName += ".zip";
            }

            var allHasCommonPath = true;
            var commonPath = string.Empty;
            for (var i = 0; i < filesAndFoldersPaths.Count; i++)
            {
                var parentPath = this.GetParentCloudPath(filesAndFoldersPaths[i]);
                if (string.IsNullOrEmpty(commonPath))
                {
                    commonPath = parentPath;
                }

                allHasCommonPath &= commonPath == parentPath;
                filesAndFoldersPaths[i] = string.Format("\"{0}\"", this.GetPathStartEndSlash(filesAndFoldersPaths[i], setAtEnd: false));
            }

            if (!allHasCommonPath)
            {
                throw new CloudClientException(
                    "Some of files or folders have the different common paths. All of items should have the common parent folder.",
                    nameof(filesAndFoldersPaths),
                    ErrorCode.DifferentParentPaths);
            }

            var pathsStr = string.Format("[{0}]", string.Join(",", filesAndFoldersPaths));
            var values = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("home_list", pathsStr),
                new KeyValuePair<string, object>("name", destZipArchiveName),
                new KeyValuePair<string, object>("api", 2),
                new KeyValuePair<string, object>("token", this.Account.AuthToken),
                new KeyValuePair<string, object>("email", this.Account.Email)
            };

            var responseMsg = await this.Account.HttpClient.PostAsync(Urls.CreateZipArchive, values.ToFormUrlEncodedContent());
            if ((int)responseMsg.StatusCode == 422)
            {
                throw new CloudClientException("The maximum downloading size limit is 4GB.", ErrorCode.DownloadingSizeLimit);
            }

            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            return responseStr.Deserialize<string>();
        }

        /// <summary>
        /// Download the file from cloud.
        /// </summary>
        /// <param name="destFileName">The destination file name. If not set, will bw use the original file name.</param>
        /// <param name="sourceFilePath">The full source file path in the cloud.</param>
        /// <param name="destFolderPath">The destination file path on the local machine.</param>
        /// <returns>The downloaded file on the local machine.</returns>
        public async Task<FileInfo> DownloadFile(string destFileName, string sourceFilePath, string destFolderPath)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(destFolderPath) && Directory.Exists(destFolderPath),
                "Is null or empty or folder does not exists.");

            var originalFileName = sourceFilePath.Split(new[] { '/' }).Last();
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrEmpty(destFileName))
            {
                destFileName = originalFileName;
            }
            else if (!destFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                destFileName += extension;
            }

            if (destFolderPath.EndsWith(@"\"))
            {
                destFolderPath += @"\";
            }

            var fullDestPath = destFolderPath + destFileName;
            using (var stream = new FileStream(fullDestPath, FileMode.Create))
            {
                await this.DownloadFile(sourceFilePath, stream);
            }

            return new FileInfo(fullDestPath);
        }

        /// <summary>
        /// Download the file from cloud.
        /// </summary>
        /// <param name="sourceFilePath">The full source file path in the cloud.</param>
        /// <param name="destStream">The destination stream.</param>
        /// <returns>The simple task.</returns>
        public async Task DownloadFile(string sourceFilePath, Stream destStream)
        {
            Debug.Assert(destStream != null, "Destination stream is null.");
            var file = await this.DownloadFile(sourceFilePath);
            await file.NetworkStream.CopyToAsync(file.Length, destStream, this.ProgressChangedEvent, this, this.cancelToken);
        }

        /// <summary>
        /// Download the file from cloud.
        /// </summary>
        /// <param name="sourceFilePath">The full source file path in the cloud.</param>
        /// <returns>The network stream and his length.</returns>
        public async Task<(Stream NetworkStream, long Length)> DownloadFile(string sourceFilePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFilePath), "Is null ot empty.");

            sourceFilePath = sourceFilePath.TrimStart(new[] { '/' });
            await this.Account.CheckAuthorization(false);
            var shardUrl = (await this.GetShardsInfo()).Get[0].Url;
            var responseMsg = await this.Account.HttpClient.GetAsync(shardUrl + sourceFilePath, HttpCompletionOption.ResponseHeadersRead);
            if ((int)responseMsg.StatusCode == 422)
            {
                throw new CloudClientException("The maximum downloading size limit is 4GB.", nameof(sourceFilePath), ErrorCode.DownloadingSizeLimit);
            }

            if (responseMsg.StatusCode == HttpStatusCode.NotFound)
            {
                throw new CloudClientException("The file does not exists in cloud.", nameof(sourceFilePath), ErrorCode.PathNotExists);
            }

            var contentLength = responseMsg.Content.Headers.ContentLength.Value;
            var networkStream = await responseMsg.Content.ReadAsStreamAsync();
            return (networkStream, contentLength);
        }

        /// <summary>
        /// Upload the file in the cloud. Uploading limit 4GB.
        /// </summary>
        /// <param name="destFileName">The destination file name. If not set, will be use original file name.</param>
        /// <param name="sourceFilePath">The source file path on the local machine.</param>
        /// <param name="destFolderPath">The destination file folder path in the cloud.</param>
        /// <returns>The created file info.</returns>
        public async Task<Data.File> UploadFile(string destFileName, string sourceFilePath, string destFolderPath)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(sourceFilePath) && System.IO.File.Exists(sourceFilePath),
                "Is null or empty, or does not exists.");
            var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            var originalFileName = Path.GetFileName(sourceFilePath);
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrEmpty(destFileName))
            {
                destFileName = originalFileName;
            }
            else if (!destFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                destFileName += extension;
            }

            return await this.UploadFile(destFileName, stream, destFolderPath);
        }

        /// <summary>
        /// Upload the file in the cloud. Uploading limit 4GB.
        /// </summary>
        /// <param name="destFileName">The destination file name.</param>
        /// <param name="content">The content as stream.</param>
        /// <param name="destFolderPath">The destination file folder path in the cloud.</param>
        /// <returns>The created file info.</returns>
        public async Task<Data.File> UploadFile(string destFileName, Stream content, string destFolderPath)
        {
            await this.Account.CheckAuthorization(false);
            destFolderPath = this.GetPathStartEndSlash(destFolderPath);

            Debug.Assert(!string.IsNullOrEmpty(destFileName), "Is null ot empty.");
            Debug.Assert(content != null && content.Length > 0, "Is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(destFolderPath), "Is null or empty..");
            if (await this.GetFolder(destFolderPath) == null)
            {
                throw new CloudClientException("Path does not exists.", nameof(destFolderPath), ErrorCode.PathNotExists);
            }

            var sizeLimit = (this.Account.Has2GBUploadSizeLimit ? 2048L : 32768L) * 1024L * 1024L;
            if (content.Length > sizeLimit)
            {
                throw new CloudClientException(
                    string.Format("Max uploading size limit is {0}GB.", sizeLimit),
                    nameof(content),
                    ErrorCode.UploadingSizeLimit);
            }

            var shardUrl = (await this.GetShardsInfo()).Upload[0].Url;
            var fileSize = content.Length;
            using (var streamContent = new ProgressableStreamContent(content, this.ProgressChangedEvent, this.cancelToken))
            {
                using (var response = await this.Account.HttpClient.PutAsync(
                    string.Format(Urls.UploadFile, shardUrl, this.Account.Email),
                    streamContent,
                    this.cancelToken.Token))
                {
                    var hash = await response.Content.ReadAsStringAsync();
                    var createdFile = await this.CreateFileOrFolder(true, destFolderPath + destFileName, hash, fileSize);
                    return new Data.File(this.Account)
                    {
                        FullPath = createdFile.NewPath,
                        Hash = hash,
                        LastModifiedTimeUTC = DateTime.Now.ToUniversalTime(),
                        Name = createdFile.NewName,
                        Size = new Size(fileSize)
                    };
                }
            }
        }

        /// <summary>
        /// Get the root folder info that include the list of files and folders. Folder object can contains only 1 level of sub items.
        /// </summary>
        /// <returns>Folder object.</returns>
        public async Task<Folder> GetFolder()
        {
            return await this.GetFolder(null);
        }

        /// <summary>
        /// Get the folder info that include the list of files and folders. Folder object can contains only 1 level of sub items.
        /// </summary>
        /// <param name="fullPath">Path from whence should be retrieved the items.</param>
        /// <returns>An existing folder info or null if does not exists.</returns>
        public async Task<Folder> GetFolder(string fullPath)
        {
            await this.Account.CheckAuthorization(false);
            fullPath = this.GetPathStartEndSlash(fullPath);
            var responseMsg = await this.Account.HttpClient.GetAsync(Urls.ItemsList, this.Account.AuthToken, fullPath);
            if (!responseMsg.IsSuccessStatusCode)
            {
                return null;
            }

            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var deserialized = responseStr.Deserialize<CloudStructureEntry>();
            return new Folder(this.Account)
            {
                FilesCount = deserialized.Count.Files,
                FoldersCount = deserialized.Count.Folders,
                FullPath = deserialized.Home,
                Items = deserialized.List,
                Name = deserialized.Name,
                PublicLink = string.IsNullOrEmpty(deserialized.Weblink) ? null : Urls.PublicLink + deserialized.Weblink,
                Size = new Size(deserialized.Size)
            };
        }

        /// <summary>
        /// Abort the run asynchronous tasks. Will be affected on download and upload operations.
        /// </summary>
        /// <param name="throwOnFirstException">True, if exception should immedeately propagate; otherwase, false.</param>
        public void AbortAllAsyncTasks(bool throwOnFirstException)
        {
            this.cancelToken.Cancel(throwOnFirstException);
        }

        /// <summary>
        /// Create the new folder or file in the cloud.
        /// </summary>
        /// <param name="addFile">When true, will be created the file.</param>
        /// <param name="path">The full path of the new entry.</param>
        /// <param name="hash">The file hash.</param>
        /// <param name="size">The file size.</param>
        /// <param name="rewriteExisting">The conflict resolving method.</param>
        /// <returns>The new file or folder name and full path.</returns>
        private async Task<(string NewName, string NewPath)> CreateFileOrFolder(
            bool addFile, string path, string hash = null, long size = 0, bool rewriteExisting = false)
        {
            await this.Account.CheckAuthorization(false);
            var values = this.GetDefaultFormDataFields(path, rewriteExisting);
            if (addFile && !string.IsNullOrEmpty(hash) && size != 0)
            {
                values.Add(new KeyValuePair<string, object>("hash", hash));
                values.Add(new KeyValuePair<string, object>("size", size));
            }

            var responseMsg = await this.Account.HttpClient.PostAsync(
                Urls.CreateFileOrFolder, values.ToFormUrlEncodedContent(), addFile ? "file" : "folder");
            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var newPath = responseStr.Deserialize<string>();
            var newName = newPath.Split(new[] { '/' }).Last();

            return (newName, newPath);
        }

        /// <summary>
        /// Get shards info. Can be use for anonymous user.
        /// </summary>
        /// <returns>Shards info.</returns>
        private async Task<ShardsList> GetShardsInfo()
        {
            await this.Account.CheckAuthorization(false);
            var shardsList = await this.Account.HttpClient.GetStringAsync(Urls.Dispatcher, this.Account.AuthToken);
            return shardsList.Deserialize<ShardsList>();
        }

        /// <summary>
        /// Move or copy the cloud structure entry.
        /// </summary>
        /// <typeparam name="T">The entry type. Folder or file.</typeparam>
        /// <param name="sourceFullPath">Source folder or file full path.</param>
        /// <param name="destFolderPath">The destination path of entry.</param>
        /// <param name="move">When true, will be applied the moving operation.</param>
        /// <returns>The entry info in the new location of cloud.</returns>
        private async Task<T> MoveOrCopyInternal<T>(string sourceFullPath, string destFolderPath, bool move)
            where T : CloudStructureEntryBase
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFullPath), "Is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(destFolderPath), "Is null or empty.");

            await this.Account.CheckAuthorization(false);
            sourceFullPath = this.GetPathStartEndSlash(sourceFullPath, setAtEnd: false);
            destFolderPath = this.GetPathStartEndSlash(destFolderPath, setAtEnd: false);

            var item = await this.CheckUnknownItemExisting<T>(sourceFullPath);
            if (await this.GetFolder(destFolderPath) == null)
            {
                throw new CloudClientException(
                    "Destination folder does not exists in the cloud.", nameof(destFolderPath), ErrorCode.PathNotExists);
            }

            var values = this.GetDefaultFormDataFields(sourceFullPath);
            values.Add(new KeyValuePair<string, object>("folder", destFolderPath));

            var responseMsg = await this.Account.HttpClient.PostAsync(
                Urls.FileRequest + (move ? "move" : "copy"), values.ToFormUrlEncodedContent());
            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var newPath = responseStr.Deserialize<string>();
            var newName = newPath.Split(new[] { '/' }).Last();
            item.PublicLink = null;
            item.FullPath = newPath;
            item.Name = newName;
            return item;
        }

        /// <summary>
        /// Check the unknown cloud structure entry existing.
        /// </summary>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <param name="sourceFullPath">The full path of entry.</param>
        /// <returns>An existing entry..</returns>
        private async Task<T> CheckUnknownItemExisting<T>(string sourceFullPath) where T : CloudStructureEntryBase
        {
            var parentPath = this.GetParentCloudPath(sourceFullPath);
            var itemName = sourceFullPath.TrimEnd('/').Split(new[] { '/' }).Last();
            var parentFolderItems = await this.GetFolder(parentPath);
            var hasFolder = typeof(T) == typeof(Folder);
            T item = null;
            if ((hasFolder && (item = (T)(object)parentFolderItems.Folders.FirstOrDefault(x => x.Name == itemName)) == null)
                || (!hasFolder && (item = (T)(object)parentFolderItems.Files.FirstOrDefault(x => x.Name == itemName)) == null))
            {
                throw new CloudClientException(
                    "Source item does not exists in the cloud.", nameof(sourceFullPath), ErrorCode.PathNotExists);
            }

            return item;
        }

        /// <summary>
        /// Publish or unpublish the file or folder.
        /// </summary>
        /// <typeparam name="T">The type of entry. File or folder.</typeparam>
        /// <param name="link">The full entry path or public link.</param>
        /// <param name="publish">When true, the publish operation will be applied.</param>
        /// <returns>The published ot unpublished entry info.</returns>
        private async Task<T> PublishUnpublishInternal<T>(string link, bool publish) where T : CloudStructureEntryBase
        {
            Debug.Assert(!string.IsNullOrEmpty(link), "Is null or empty.");

            await this.Account.CheckAuthorization(false);
            link = publish ? this.GetPathStartEndSlash(link, setAtEnd: false) : link.Replace(Urls.PublicLink, string.Empty);
            T item = null;
            var values = this.GetDefaultFormDataFields(link);
            values.RemoveAll(x => x.Key == "conflict");
            if (!publish)
            {
                values.RemoveAll(x => x.Key == "home");
                values.Add(new KeyValuePair<string, object>("weblink", link));
            }
            else
            {
                item = await this.CheckUnknownItemExisting<T>(link);
            }

            var operationStr = publish ? "publish" : "unpublish";
            var responseMsg = await this.Account.HttpClient.PostAsync(Urls.FileRequest + operationStr, values.ToFormUrlEncodedContent());
            if (responseMsg.StatusCode == HttpStatusCode.NotFound || responseMsg.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new CloudClientException(
                    (publish ? "The entry by entered path" : "The entered public link") + " does not exists",
                    nameof(link),
                    publish ? ErrorCode.PathNotExists : ErrorCode.PublicLinkNotExists);
            }

            var responseStr = await responseMsg.Content.ReadAsStringAsync();
            var result = responseStr.Deserialize<string>();
            if (!publish)
            {
                return await this.CheckUnknownItemExisting<T>(result);
            }

            item.PublicLink = Urls.PublicLink + result;
            return item;
        }

        /// <summary>
        /// Get the parent cloud path.
        /// </summary>
        /// <param name="path">The original path with subfolder or file at the end.</param>
        /// <returns>The parent path.</returns>
        private string GetParentCloudPath(string path)
        {
            path = path.TrimEnd('/');
            return path.Substring(0, path.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Get and set the slash at the start and at the end of path. By default will be inserted at the both sides.
        /// </summary>
        /// <param name="path">Path to add slashes.</param>
        /// <param name="setAtStart">Add slash at the start of path.</param>
        /// <param name="setAtEnd">Add slash at the end of path.</param>
        /// <returns>New path or old with has not changes.</returns>
        private string GetPathStartEndSlash(string path, bool setAtStart = true, bool setAtEnd = true)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (setAtStart)
            {
                path = "/" + path;
            }

            if (setAtEnd)
            {
                path = path + "/";
            }

            path = Regex.Replace(path, @"\/+|\\", "/");
            return path;
        }

        /// <summary>
        /// Get the default form data fields.
        /// </summary>
        /// <param name="sourceFullPath">The home path file or folder..</param>
        /// <param name="rewriteExisting">The conflict resolving, if the same entry already exists.</param>
        /// <returns>The key/value pairs.</returns>
        private List<KeyValuePair<string, object>> GetDefaultFormDataFields(
            string sourceFullPath = null, bool rewriteExisting = false)
        {
            var result = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("conflict", !rewriteExisting ? "rename" : "rewrite"),
                new KeyValuePair<string, object>("api", 2),
                new KeyValuePair<string, object>("token", this.Account.AuthToken),
                new KeyValuePair<string, object>("email", this.Account.Email),
                new KeyValuePair<string, object>("x-email", this.Account.Email)
            };

            if (!string.IsNullOrEmpty(sourceFullPath))
            {
                result.Add(new KeyValuePair<string, object>("home", sourceFullPath));
            }

            return result;
        }
    }
}
