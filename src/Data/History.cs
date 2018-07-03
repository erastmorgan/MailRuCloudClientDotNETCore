//-----------------------------------------------------------------------------------------------
// <copyright file="History.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the file modification history.
    /// </summary>
    public class History
    {
        /// <summary>
        /// Gets the unique ID of the current history.
        /// </summary>
        [JsonProperty("uid")]
        public long Id { get; internal set; }

        /// <summary>
        /// Gets last modified time of file in UTC format.
        /// </summary>
        [JsonIgnore]
        public DateTime LastModifiedTimeUTC
        {
            get => DateTimeOffset.FromUnixTimeSeconds(this.LastModifiedTimeUnix).DateTime.ToUniversalTime();
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the full path of the file in the cloud.
        /// </summary>
        [JsonProperty("path")]
        public string FullPath { get; internal set; }

        /// <summary>
        /// Gets the file size.
        /// </summary>
        [JsonIgnore]
        public Size Size { get => new Size(this.SizeBytes); }

        /// <summary>
        /// Gets a value indicating whether the file is current version of history.
        /// </summary>
        [JsonIgnore]
        public bool IsCurrentVersion { get; internal set; }

        /// <summary>
        /// Gets the revision.
        /// </summary>
        [JsonProperty("rev")]
        public long Revision { get; internal set; }

        /// <summary>
        /// Gets the file hash for the current file modification.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; internal set; }

        /// <summary>
        /// Gets or sets the last modified time of file in UNIX format.
        /// </summary>
        [JsonProperty("time")]
        internal long LastModifiedTimeUnix { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        [JsonProperty("size")]
        internal long SizeBytes { get; set; }
    }
}
