//-----------------------------------------------------------------------------------------------
// <copyright file="ShardsList.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------
namespace MailRuCloudClient.Data.Shard
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the different type shards list.
    /// </summary>
    internal class ShardsList
    {
        /// <summary>
        /// Gets or sets the video type shards.
        /// </summary>
        public ShardInfo[] Video { get; set; }

        /// <summary>
        /// Gets or sets the view direct shards.
        /// </summary>
        [JsonProperty("view_direct")]
        public ShardInfo[] ViewDirect { get; set; }

        /// <summary>
        /// Gets or sets the weblink view shards.
        /// </summary>
        [JsonProperty("weblink_view")]
        public ShardInfo[] WeblinkView { get; set; }

        /// <summary>
        /// Gets or sets the weblink video shards.
        /// </summary>
        [JsonProperty("weblink_video")]
        public ShardInfo[] WeblinkVideo { get; set; }

        /// <summary>
        /// Gets or sets the weblink get shards.
        /// </summary>
        [JsonProperty("weblink_get")]
        public ShardInfo[] WeblinkGet { get; set; }

        /// <summary>
        /// Gets or sets the stock shards.
        /// </summary>
        public ShardInfo[] Stock { get; set; }

        /// <summary>
        /// Gets or sets the weblink thumbnails shards.
        /// </summary>
        [JsonProperty("weblink_thumbnails")]
        public ShardInfo[] WeblinkThumbnails { get; set; }

        /// <summary>
        /// Gets or sets the web shards.
        /// </summary>
        public ShardInfo[] Web { get; set; }

        /// <summary>
        /// Gets or sets the authorization shards.
        /// </summary>
        public ShardInfo[] Auth { get; set; }

        /// <summary>
        /// Gets or sets the view shards.
        /// </summary>
        public ShardInfo[] View { get; set; }

        /// <summary>
        /// Gets or sets the get type shards.
        /// </summary>
        public ShardInfo[] Get { get; set; }

        /// <summary>
        /// Gets or sets the upload shards.
        /// </summary>
        public ShardInfo[] Upload { get; set; }

        /// <summary>
        /// Gets or sets the thumbnails shards.
        /// </summary>
        public ShardInfo[] Thumbnails { get; set; }
    }
}
