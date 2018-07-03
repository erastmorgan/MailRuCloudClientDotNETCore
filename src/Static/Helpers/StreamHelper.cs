//-----------------------------------------------------------------------------------------------
// <copyright file="StreamHelper.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Static.Helpers
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MailRuCloudClient.Events;
    using static MailRuCloudClient.CloudClient;

    /// <summary>
    /// Defines the streams helper.
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// Porgressable write one stream to another.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="inputStreamLength">The input stream length.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="progressEventHandler">The progress checking event.</param>
        /// <param name="sender">The owner of this call.</param>
        /// <param name="cancelToken">The cancelation token.</param>
        /// <returns>The simple task.</returns>
        public static async Task CopyToAsync(
            this Stream inputStream,
            long inputStreamLength,
            Stream outputStream,
            ProgressChangedEventHandler progressEventHandler,
            object sender,
            CancellationTokenSource cancelToken)
        {
            using (var networkStream = inputStream)
            {
                var sourceStream = new BinaryReader(networkStream);
                OnChangedProgressPercent(new ProgressChangedEventArgs(0, inputStreamLength, 0L));
                var bufferLength = 8192;
                var totalWritten = 0L;
                if (inputStreamLength < 1 || inputStreamLength < bufferLength)
                {
                    sourceStream.BaseStream.CopyTo(outputStream);
                }
                else
                {
                    double percentComplete = 0;
                    var buffer = new byte[bufferLength];
                    while (true)
                    {
                        cancelToken.Token.ThrowIfCancellationRequested();

                        var readBytes = sourceStream.Read(buffer, 0, bufferLength);
                        if (readBytes == 0)
                        {
                            break;
                        }

                        await outputStream.WriteAsync(buffer, 0, readBytes).ConfigureAwait(false);

                        totalWritten += readBytes;
                        if (inputStreamLength >= totalWritten)
                        {
                            double tempPercentComplete = 100.0 * (double)totalWritten / (double)inputStreamLength;
                            if (tempPercentComplete - percentComplete >= 1)
                            {
                                percentComplete = tempPercentComplete;
                                OnChangedProgressPercent(new ProgressChangedEventArgs((int)percentComplete, inputStreamLength, totalWritten));
                            }
                        }
                    }

                    OnChangedProgressPercent(new ProgressChangedEventArgs(100, totalWritten, totalWritten));
                }
            }

            void OnChangedProgressPercent(ProgressChangedEventArgs e)
            {
                if (progressEventHandler == null)
                {
                    return;
                }

                progressEventHandler(sender, e);
            }
        }
    }
}
