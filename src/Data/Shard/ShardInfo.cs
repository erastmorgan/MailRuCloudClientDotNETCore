//-----------------------------------------------------------------------------------------------
// <copyright file="ShardInfo.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.Shard
{
    /// <summary>
    /// Defines the shard info.
    /// </summary>
    internal class ShardInfo
    {
        /// <summary>
        /// Gets or sets the number of shard of the current type.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the shard URL.
        /// </summary>
        public string Url { get; set; }
    }
}
