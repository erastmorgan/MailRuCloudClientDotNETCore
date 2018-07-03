//-----------------------------------------------------------------------------------------------
// <copyright file="DefaultResponse.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    /// <summary>
    /// Defines the default response from cloud API.
    /// </summary>
    /// <typeparam name="BodyType">The type of content.</typeparam>
    internal class DefaultResponse<BodyType>
    {
        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the response content.
        /// </summary>
        public BodyType Body { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int Status { get; set; }
    }
}
