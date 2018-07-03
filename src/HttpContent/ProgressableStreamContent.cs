//-----------------------------------------------------------------------------------------------
// <copyright file="ProgressableStreamContent.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.HttpContent
{
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using MailRuCloudClient.Static.Helpers;
    using static MailRuCloudClient.CloudClient;

    /// <summary>
    /// Defines the progressable stream content for HTTP client requests.
    /// </summary>
    internal sealed class ProgressableStreamContent : StreamContent
    {
        /// <summary>
        /// The input stream to write.
        /// </summary>
        private Stream content;

        /// <summary>
        /// Async tasks cancelation token.
        /// </summary>
        private CancellationTokenSource cancelationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressableStreamContent"/> class.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="progressEventHandler">The progress event handler.</param>
        /// <param name="cancelToken">The cancelation token.</param>
        public ProgressableStreamContent(Stream stream, ProgressChangedEventHandler progressEventHandler, CancellationTokenSource cancelToken)
            : base(stream)
        {
            this.ProgressChangedEvent = progressEventHandler;
            this.cancelationToken = cancelToken;
            this.content = stream;
        }

        /// <summary>
        /// Changing progress event, works only for upload and download operations.
        /// </summary>
        private event ProgressChangedEventHandler ProgressChangedEvent;

        /// <summary>
        /// Write the input stream to network.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="context">The transport context.</param>
        /// <returns>The simple task.</returns>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Debug.Assert(stream != null, "The output stream is missing.");

            await this.content.CopyToAsync(this.content.Length, stream, this.ProgressChangedEvent, this, this.cancelationToken);
        }

        /// <summary>
        /// Dispose the resources.
        /// </summary>
        /// <param name="disposing">When true, the disposing will be applied.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.content.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
