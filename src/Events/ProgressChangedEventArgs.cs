//-----------------------------------------------------------------------------------------------
// <copyright file="ProgressChangedEventArgs.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Events
{
    using MailRuCloudClient.Data;

    /// <summary>
    /// Defines the progress changing event argument.
    /// </summary>
    public class ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressChangedEventArgs" /> class.
        /// </summary>
        /// <param name="progressPercentage">The progress percentage.</param>
        /// <param name="totalBytes">The total bytes.</param>
        /// <param name="bytesInProgress">The bytes in progress.</param>
        public ProgressChangedEventArgs(int progressPercentage, long totalBytes, long bytesInProgress)
        {
            this.ProgressPercentage = progressPercentage;
            this.State = new ProgressChangeTaskState
            {
                TotalBytes = new Size(totalBytes),
                BytesInProgress = new Size(bytesInProgress)
            };
        }

        /// <summary>
        /// Gets the progress percentage.
        /// </summary>
        public int ProgressPercentage { get; }

        /// <summary>
        /// Gets the progress state.
        /// </summary>
        public ProgressChangeTaskState State { get; }
    }
}
