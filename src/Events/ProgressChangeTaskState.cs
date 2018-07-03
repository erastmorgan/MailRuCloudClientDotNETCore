//-----------------------------------------------------------------------------------------------
// <copyright file="ProgressChangeTaskState.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------
namespace MailRuCloudClient.Events
{
    using MailRuCloudClient.Data;

    /// <summary>
    /// Task state is used for progress changed event.
    /// </summary>
    public class ProgressChangeTaskState
    {
        /// <summary>
        /// Gets the total operation bytes.
        /// </summary>
        /// <value>Total bytes.</value>
        public Size TotalBytes { get; internal set; }

        /// <summary>
        /// Gets the bytes in progress for currently operation.
        /// </summary>
        /// <value>Bytes in progress.</value>
        public Size BytesInProgress { get; internal set; }
    }
}
