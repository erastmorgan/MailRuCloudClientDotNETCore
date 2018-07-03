//-----------------------------------------------------------------------------------------------
// <copyright file="NotAuthorizedException.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Exceptions
{
    using System;

    /// <summary>
    /// Defines exception for account.
    /// </summary>
    public class NotAuthorizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorizedException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public NotAuthorizedException(string message) : 
            base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorizedException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="parameter">Error code or any other additional info.</param>
        public NotAuthorizedException(string message, string parameter) : 
            base(string.Format("{0} Error code: {1}", message, parameter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorizedException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        public NotAuthorizedException(string message, Exception innerException) : 
            base(message, innerException)
        {
        }
    }
}
