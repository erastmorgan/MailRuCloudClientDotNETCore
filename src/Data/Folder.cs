//-----------------------------------------------------------------------------------------------
// <copyright file="Folder.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using MailRuCloudClient.Data.CloudStructure;
    using MailRuCloudClient.Static;

    /// <summary>
    /// Folder type of item on server. Folder object can contains only 1 level of sub items.
    /// </summary>
    public class Folder : CloudStructureEntryBase
    {
        /// <summary>
        /// The previous value of used cloud disk space.
        /// </summary>
        private long prevDiskUsed = 0;

        /// <summary>
        /// The last iems getting time.
        /// </summary>
        private DateTime lastItemsGettingTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder"/> class.
        /// </summary>
        /// <param name="account">The MAILRU account.</param>
        internal Folder(Account account)
            : base(account)
        {
        }

        /// <summary>
        /// Changing folder content event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The folder info.</param>
        public delegate void ContentChangedEventHandler(object sender, Folder e);

        /// <summary>
        /// Changing folder content event, works only for upload and download operations.
        /// </summary>
        public event ContentChangedEventHandler FolderContentChangedEvent;

        /// <summary>
        /// Gets number of folders in this folder on cloud.
        /// </summary>
        public int FoldersCount { get; internal set; }

        /// <summary>
        /// Gets number of files in this folder on cloud.
        /// </summary>
        public int FilesCount { get; internal set; }

        /// <summary>
        /// Gets the list of files in the current folder.
        /// </summary>
        public IEnumerable<File> Files
        {
            get
            {
                this.UpdateFolderInfo().Wait();
                if (this.Items == null)
                {
                    return new List<File>();
                }

                return this.Items.Where(x => x.Type == "file").Select(x => new File(this.Account)
                {
                    FullPath = x.Home,
                    Hash = x.Hash,
                    LastModifiedTimeUTC = DateTimeOffset.FromUnixTimeSeconds(x.Mtime).DateTime.ToUniversalTime(),
                    Name = x.Name,
                    PublicLink = string.IsNullOrEmpty(x.Weblink) ? null : Urls.PublicLink + x.Weblink,
                    Size = new Size(x.Size)
                });
            }
        }

        /// <summary>
        /// Gets the list of subfolders in the current folder.
        /// </summary>
        public IEnumerable<Folder> Folders
        {
            get
            {
                this.UpdateFolderInfo().Wait();
                if (this.Items == null)
                {
                    return new List<Folder>();
                }

                return this.Items.Where(x => x.Type == "folder").Select(x => new Folder(this.Account)
                {
                    FullPath = x.Home,
                    Name = x.Name,
                    PublicLink = string.IsNullOrEmpty(x.Weblink) ? null : Urls.PublicLink + x.Weblink,
                    Size = new Size(x.Size),
                    FilesCount = x.Count.Files,
                    FoldersCount = x.Count.Folders,
                    Items = x.List
                });
            }
        }

        /// <summary>
        /// Gets or sets the entries list of cloud structure.
        /// </summary>
        internal CloudStructureEntry[] Items { get; set; }

        /// <summary>
        /// Publish the current folder.
        /// </summary>
        /// <returns>The published folder info.</returns>
        public async Task<Folder> Publish()
        {
            return await this.Client.Publish<Folder>(this.FullPath);
        }

        /// <summary>
        /// Unpublish the current folder.
        /// </summary>
        /// <returns>The unpublished folder info.</returns>
        public async Task<Folder> Unpublish()
        {
            if (this.PublicLink == null)
            {
                return this;
            }

            return await this.Client.Unpublish<Folder>(this.PublicLink);
        }

        /// <summary>
        /// Remove the current folder from cloud.
        /// </summary>
        /// <returns>The simple task.</returns>
        public async Task Remove()
        {
            await this.Client.Remove(this.FullPath);
            await this.UpdateFolderInfo(true);
        }

        /// <summary>
        /// Rename the current folder.
        /// </summary>
        /// <param name="newName">The new name of folder.</param>
        /// <returns>The renamed folder.</returns>
        public async Task<Folder> Rename(string newName)
        {
            var result = await this.Client.Rename<Folder>(this.FullPath, newName);
            await this.UpdateFolderInfo(true);
            return result;
        }

        /// <summary>
        /// Copy the folder to another space.
        /// </summary>
        /// <param name="destFolderPath">The destination path of current folder.</param>
        /// <returns>The new foler info in the new location.</returns>
        public async Task<Folder> Copy(string destFolderPath)
        {
            return await this.Client.Copy<Folder>(this.FullPath, destFolderPath);
        }

        /// <summary>
        /// Move the folder to another space.
        /// </summary>
        /// <param name="destFolderPath">The destination path of current folder.</param>
        /// <returns>The new foler info in the new location.</returns>
        public async Task<Folder> Move(string destFolderPath)
        {
            var result = await this.Client.Move<Folder>(this.FullPath, destFolderPath);
            await this.UpdateFolderInfo(true);
            return result;
        }

        /// <summary>
        /// Download the files and folders from the current folder to ZIP archive.
        /// Do enter only file or folder names from current folder.
        /// </summary>
        /// <param name="fileAndFolderNames">The file and folder names from current directory.</param>
        /// <param name="destZipArchiveName">The output ZIP archive name. If not set, will be generated the GUID.</param>
        /// <param name="destFolderPath">The destination folder on the local machine</param>
        /// <returns>The downloaded ZIP archive info.</returns>
        public async Task<FileInfo> DownloadItemsAsZIPArchive(List<string> fileAndFolderNames, string destZipArchiveName, string destFolderPath)
        {
            for (var i = 0; i < fileAndFolderNames.Count; i++)
            {
                fileAndFolderNames[i] = this.FullPath + "/" + fileAndFolderNames[i];
            }

            return await this.Client.DownloadItemsAsZIPArchive(fileAndFolderNames, destZipArchiveName, destFolderPath);
        }

        /// <summary>
        /// Download the current folder from cloud as ZIP archive. Downloading limit 4GB.
        /// </summary>
        /// <param name="destStream">The destination stream.</param>
        /// <returns>The simple task.</returns>
        public async Task DownloadFolderAsZIP(Stream destStream)
        {
            await this.Client.DownloadItemsAsZIPArchive(new List<string> { this.FullPath }, destStream);
        }

        /// <summary>
        /// Download the current folder from cloud as ZIP archive. Downloading limit 4GB.
        /// </summary>
        /// <returns>The network stream and his length.
        /// The length inaccurate for ZIP archive, because it's not possible to compute.
        /// Usually, the real length is larger, than specified.</returns>
        public async Task<(Stream NetworkStream, long Length)> DownloadFolderAsZIP()
        {
            return await this.Client.DownloadItemsAsZIPArchive(new List<string> { this.FullPath });
        }

        /// <summary>
        /// Download the current folder from cloud as ZIP archive. Downloading limit 4GB.
        /// </summary>
        /// <param name="destZipArchiveName">The destination ZIP archive name. If not set, will bw use the original folder name.</param>
        /// <param name="destFolderPath">The destination file path on the local machine.</param>
        /// <returns>The downloaded ZIP archive on the local machine.</returns>
        public async Task<FileInfo> DownloadFolderAsZIP(string destZipArchiveName, string destFolderPath)
        {
            if (string.IsNullOrEmpty(destZipArchiveName))
            {
                destZipArchiveName = this.FullPath.Split(new[] { '/' }).Last();
            }

            return await this.Client.DownloadItemsAsZIPArchive(new List<string> { this.FullPath }, destZipArchiveName, destFolderPath);
        }

        /// <summary>
        /// Create the new folder in the current folder.
        /// </summary>
        /// <param name="folderName">The new folder name.</param>
        /// <returns>The created folder info.</returns>
        public async Task<Folder> CreateFolder(string folderName)
        {
            if (folderName.Contains("/"))
            {
                throw new ArgumentException(
                    "The nested subdirectories is not allowed. Use CloudClient.CreateFolder insted.", nameof(folderName));
            }

            var result = await this.Client.CreateFolder(this.FullPath + "/" + folderName);
            await this.UpdateFolderInfo(true);
            return result;
        }

        /// <summary>
        /// Upload the file in the cloud.
        /// </summary>
        /// <param name="sourceFilePath">The source file path on the local machine.</param>
        /// <returns>The created file info.</returns>
        public async Task<File> UploadFile(string sourceFilePath)
        {
            var result = await this.Client.UploadFile(null, sourceFilePath, this.FullPath);
            await this.UpdateFolderInfo(true);
            return result;
        }

        /// <summary>
        /// Upload the file in the cloud.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The content as stream.</param>
        /// <returns>The created file info.</returns>
        public async Task<File> UploadFile(string fileName, Stream content)
        {
            var result = await this.Client.UploadFile(fileName, content, this.FullPath);
            await this.UpdateFolderInfo();
            return result;
        }

        /// <summary>
        /// Update the folder info if required.
        /// </summary>
        /// <param name="forceUpdate">When true, the folder info will be updated anyway.</param>
        /// <returns>The simple task.</returns>
        internal async Task UpdateFolderInfo(bool forceUpdate = false)
        {
            if (this.lastItemsGettingTime == null)
            {
                this.lastItemsGettingTime = DateTime.Now;
            }

            var diffTime = (DateTime.Now - this.lastItemsGettingTime).TotalSeconds;
            DiskUsage currentDiskSpace = null;
            if (this.Items == null 
                || (diffTime > 1.0 && (currentDiskSpace = await this.Account.GetDiskUsage()).Used.DefaultValue != this.prevDiskUsed)
                || forceUpdate)
            {
                var folder = await this.Client.GetFolder(this.FullPath);
                this.Items = folder != null ? folder.Items : null;
                this.Size = folder != null ? folder.Size : new Size(0);
                this.PublicLink = folder != null ? folder.PublicLink : null;
                this.FilesCount = folder != null ? folder.FilesCount : 0;
                this.FoldersCount = folder != null ? folder.FoldersCount : 0;
                this.lastItemsGettingTime = DateTime.Now;
                this.OnChangedFolderContent(this);
            }

            if (currentDiskSpace != null)
            {
                this.prevDiskUsed = currentDiskSpace.Used.DefaultValue;
            }
        }

        /// <summary>
        /// Function to set data for <see cref="FolderContentChangedEvent"/>.
        /// </summary>
        /// <param name="e">The changed folder info.</param>
        private void OnChangedFolderContent(Folder e)
        {
            if (this.FolderContentChangedEvent == null)
            {
                return;
            }

            this.FolderContentChangedEvent(this, e);
        }
    }
}
