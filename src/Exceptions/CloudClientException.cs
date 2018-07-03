//-----------------------------------------------------------------------------------------------
// <copyright file="CloudClientException.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Exceptions
{
    using System;

    /// <summary>
    /// Defines the cloud  client error codes.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// The error code is not defined.
        /// </summary>
        None,

        /// <summary>
        /// The path does not exists.
        /// </summary>
        PathNotExists,

        /// <summary>
        /// The maximum uploading size limit.
        /// </summary>
        UploadingSizeLimit,

        /// <summary>
        /// The maximum downloading size limit.
        /// </summary>
        DownloadingSizeLimit,

        /// <summary>
        /// The cloud structure items have a different parent folders.
        /// </summary>
        DifferentParentPaths,

        /// <summary>
        /// The file history does not found.
        /// </summary>
        HistoryNotExists,

        /// <summary>
        /// The operation is not supported.
        /// </summary>
        NotSupportedOperation,

        /// <summary>
        /// The public link does not exists.
        /// </summary>
        PublicLinkNotExists
    }

    /// <summary>
    /// Defines the cloud client exception.
    /// </summary>
    public class CloudClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClientException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public CloudClientException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClientException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="errorCode">Error code or any other additional info.</param>
        public CloudClientException(string message, ErrorCode errorCode) :
            base(string.Format("{0} Error code: {1}", message, errorCode))
        {
            this.HResult = (int)errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClientException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="source">The object was throws this exception.</param>
        /// <param name="errorCode">Error code or any other additional info.</param>
        public CloudClientException(string message, string source, ErrorCode errorCode) :
            base(string.Format("{0} Error code: {1}", message, errorCode))
        {
            this.Source = source;
            this.HResult = (int)errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudClientException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        public CloudClientException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
