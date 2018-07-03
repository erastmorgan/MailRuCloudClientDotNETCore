//-----------------------------------------------------------------------------------------------
// <copyright file="File.cs" company="Erast Korolev">
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

    /// <summary>
    /// File type of item on server.
    /// </summary>
    public class File : CloudStructureEntryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="File"/> class.
        /// </summary>
        /// <param name="account">The MAILRU account.</param>
        internal File(Account account)
            : base(account)
        {
        }

        /// <summary>
        /// Gets file hash. SHA1 + SALT.
        /// </summary>
        public string Hash { get; internal set; }

        /// <summary>
        /// Gets last modified time of file in UTC format.
        /// </summary>
        public DateTime LastModifiedTimeUTC { get; internal set; }

        /// <summary>
        /// Provides the one-time anonymous direct link to download the file. Important: the file should has the public link.
        /// </summary>
        /// <returns>One-time direct link.</returns>
        public async Task<string> GetFileOneTimeDirectLink()
        {
            return await this.Client.GetFileOneTimeDirectLink(this.PublicLink);
        }

        /// <summary>
        /// Publish the current file.
        /// </summary>
        /// <returns>The published file info.</returns>
        public async Task<File> Publish()
        {
            return await this.Client.Publish<File>(this.FullPath);
        }

        /// <summary>
        /// Unpublish the current file.
        /// </summary>
        /// <returns>The unpublished file info.</returns>
        public async Task<File> Unpublish()
        {
            if (this.PublicLink == null)
            {
                return this;
            }

            return await this.Client.Unpublish<File>(this.PublicLink);
        }

        /// <summary>
        /// Restore the file from history.
        /// </summary>
        /// <param name="historyRevision">The unique history revision number from which will be the restoring.</param>
        /// <param name="rewriteExisting">When true, an existing parent file will be overriden,
        /// otherwise the file from history will be created as new.</param>
        /// <param name="newFileName">The new file name. It will be applied only, if previous parameter is false.</param>
        /// <returns>The restored file info.</returns>
        public async Task<File> RestoreFileFromHistory(long historyRevision, bool rewriteExisting, string newFileName = null)
        {
            return await this.Client.RestoreFileFromHistory(this.FullPath, historyRevision, rewriteExisting, newFileName);
        }

        /// <summary>
        /// Get the current file history.
        /// </summary>
        /// <returns>The file modification history.</returns>
        public async Task<History[]> GetFileHistory()
        {
            return await this.Client.GetFileHistory(this.FullPath);
        }

        /// <summary>
        /// Remove the current file from cloud.
        /// </summary>
        /// <returns>The simple task.</returns>
        public async Task Remove()
        {
            await this.Client.Remove(this.FullPath);
        }

        /// <summary>
        /// Rename the current file.
        /// </summary>
        /// <param name="newName">The new name of file.</param>
        /// <returns>The renamed file.</returns>
        public async Task<File> Rename(string newName)
        {
            return await this.Client.Rename<File>(this.FullPath, newName);
        }

        /// <summary>
        /// Copy the file to another space.
        /// </summary>
        /// <param name="destFolderPath">The destination path of current folder.</param>
        /// <returns>The new file info in the new location.</returns>
        public async Task<File> Copy(string destFolderPath)
        {
            return await this.Client.Copy<File>(this.FullPath, destFolderPath);
        }

        /// <summary>
        /// Move the file to another space.
        /// </summary>
        /// <param name="destFolderPath">The destination path of current folder.</param>
        /// <returns>The file foler info in the new location.</returns>
        public async Task<File> Move(string destFolderPath)
        {
            return await this.Client.Move<File>(this.FullPath, destFolderPath);
        }

        /// <summary>
        /// Download the current file from cloud as ZIP archive. Downloading limit 4GB.
        /// </summary>
        /// <param name="destStream">The destination stream.</param>
        /// <returns>The simple task.</returns>
        public async Task DownloadFileAsZIP(Stream destStream)
        {
            await this.Client.DownloadItemsAsZIPArchive(new List<string> { this.FullPath }, destStream);
        }

        /// <summary>
        /// Download the current file from cloud as ZIP archive. Downloading limit 4GB.
        /// </summary>
        /// <param name="destZipArchiveName">The destination ZIP archive name. If not set, will bw use the original file name.</param>
        /// <param name="destFolderPath">The destination file path on the local machine.</param>
        /// <returns>The downloaded ZIP archive on the local machine.</returns>
        public async Task<FileInfo> DownloadFileAsZIP(string destZipArchiveName, string destFolderPath)
        {
            var originalFileName = this.FullPath.Split(new[] { '/' }).Last();
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrEmpty(destZipArchiveName))
            {
                destZipArchiveName = originalFileName;
            }
            else if (!destZipArchiveName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                destZipArchiveName += extension;
            }

            return await this.Client.DownloadItemsAsZIPArchive(new List<string> { this.FullPath }, destZipArchiveName, destFolderPath);
        }

        /// <summary>
        /// Download the current file from cloud as ZIP archive. Downloading limit 4GB.
        /// </summary>
        /// <returns>The network stream and his length.
        /// The length inaccurate for ZIP archive, because it's not possible to compute.
        /// Usually, the real length is larger, than specified.</returns>
        public async Task<(Stream NetworkStream, long Length)> DownloadFileAsZIP()
        {
            return await this.Client.DownloadItemsAsZIPArchive(new List<string> { this.FullPath });
        }

        /// <summary>
        /// Download the current file from cloud.
        /// </summary>
        /// <param name="destFileName">The destination file name. If not set, will bw use the original file name.</param>
        /// <param name="destFolderPath">The destination file path on the local machine.</param>
        /// <returns>The downloaded file on the local machine.</returns>
        public async Task<FileInfo> DownloadFile(string destFileName, string destFolderPath)
        {
            return await this.Client.DownloadFile(destFileName, this.FullPath, destFolderPath);
        }

        /// <summary>
        /// Download the current file from cloud.
        /// </summary>
        /// <param name="destStream">The destination stream.</param>
        /// <returns>The simple task.</returns>
        public async Task DownloadFile(Stream destStream)
        {
            await this.Client.DownloadFile(this.FullPath, destStream);
        }

        /// <summary>
        /// Download the current file from cloud. Downloading limit 4GB.
        /// </summary>
        /// <returns>The network stream and his length.</returns>
        public async Task<(Stream NetworkStream, long Length)> DownloadFile()
        {
            return await this.Client.DownloadFile(this.FullPath);
        }
    }
}
