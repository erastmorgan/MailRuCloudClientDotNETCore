//-----------------------------------------------------------------------------------------------
// <copyright file="Urls.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Static
{
    /// <summary>
    /// Default application settings.
    /// </summary>
    internal class Urls
    {
        /// <summary>
        /// The base address of cloud.
        /// </summary>
        public const string BaseMailRuCloud = "https://cloud.mail.ru";

        /// <summary>
        /// The base authorization address of MAILRU.
        /// </summary>
        public const string BaseMailRuAuth = "https://auth.mail.ru";

        /// <summary>
        /// The authorization URL.
        /// </summary>
        public const string Auth = "/cgi-bin/auth";

        /// <summary>
        /// The address to ensure SDC cookies.
        /// </summary>
        public const string EnsureSdc = "/sdc?from=https://cloud.mail.ru/home";

        /// <summary>
        /// The authorization token getting URL.
        /// </summary>
        public const string AuthToken = "/api/v2/tokens/csrf";

        /// <summary>
        /// The disk space info.
        /// </summary>
        public const string DiskSpace = "/api/v2/user/space?api=2&email={0}&token={1}";

        /// <summary>
        /// The cloud items list.
        /// </summary>
        public const string ItemsList = "/api/v2/folder?token={0}&home={1}";

        /// <summary>
        /// The start of public link.
        /// </summary>
        public const string PublicLink = "https://cloud.mail.ru/public/";

        /// <summary>
        /// The shards info.
        /// </summary>
        public const string Dispatcher = "/api/v2/dispatcher?token={0}";

        /// <summary>
        /// The upload file link.
        /// </summary>
        public const string UploadFile = "{0}?cloud_domain=2&x-email={1}";

        /// <summary>
        /// Create the file or folder record on cloud structure.
        /// </summary>
        public const string CreateFileOrFolder = "/api/v2/{0}/add";

        /// <summary>
        /// Prepare the ZIP archive to download.
        /// </summary>
        public const string CreateZipArchive = "/api/v2/zip";

        /// <summary>
        /// The any file request start with this URL.
        /// </summary>
        public const string FileRequest = "/api/v2/file/";

        /// <summary>
        /// Rename the file or folder.
        /// </summary>
        public const string Rename = "/api/v2/file/rename";

        /// <summary>
        /// Remove the file or folder.
        /// </summary>
        public const string Remove = "/api/v2/file/remove";

        /// <summary>
        /// Get the file history.
        /// </summary>
        public const string History = "/api/v2/file/history?home={0}&api=2&email={1}&x-email={1}&token={2}";

        /// <summary>
        /// Get the rates.
        /// </summary>
        public const string Rates = "/api/v2/billing/rates?api=2email={0}&x-email={0}&token={1}";

        /// <summary>
        /// One time downloading token.
        /// </summary>
        public const string DownloadToken = "/api/v2/tokens/download";
    }
}
