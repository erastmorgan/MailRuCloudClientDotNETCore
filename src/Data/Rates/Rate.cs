//-----------------------------------------------------------------------------------------------
// <copyright file="Rate.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.Rates
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the rate info.
    /// </summary>
    public class Rate
    {
        /// <summary>
        /// The rate name.
        /// </summary>
        private string name;

        /// <summary>
        /// Gets the rate name.
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => this.name ?? this.Id;
            internal set => this.name = value;
        }

        /// <summary>
        /// Gets a value indicating whether is active the current rate.
        /// </summary>
        [JsonProperty("active")]
        public bool IsActive { get; internal set; }

        /// <summary>
        /// Gets the unique ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether is available to turn on.
        /// </summary>
        [JsonProperty("available")]
        public bool IsAvailable { get; internal set; }

        /// <summary>
        /// Gets the additional size, which will be applied for cloud disk.
        /// </summary>
        [JsonIgnore]
        public Size Size { get => new Size(this.SizeBytes); }

        /// <summary>
        /// Gets the cost info of the current rate.
        /// </summary>
        [JsonProperty("cost")]
        public CostItem[] Cost { get; internal set; }

        /// <summary>
        /// Gets or sets the additional size in bytes, which will be applied for cloud disk.
        /// </summary>
        [JsonProperty("size")]
        internal long SizeBytes { get; set; }
    }
}
