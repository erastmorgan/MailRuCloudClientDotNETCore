//-----------------------------------------------------------------------------------------------
// <copyright file="CloudStructureEntry.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.CloudStructure
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the DTO object of item for cloud structure.
    /// </summary>
    internal class CloudStructureEntry
    {
        /// <summary>
        /// Gets or sets the number of elements for every unit type.
        /// </summary>
        public Count Count { get; set; }

        /// <summary>
        /// Gets or sets the unique value of current structure unit.
        /// </summary>
        public string Tree { get; set; }

        /// <summary>
        /// Gets or sets the sctructure name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the general revision.
        /// </summary>
        public string Grev { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the sorting parameters.
        /// </summary>
        public Sort Sort { get; set; }

        /// <summary>
        /// Gets or sets the kind of structure.
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        public int Rev { get; set; }

        /// <summary>
        /// Gets or sets the type of structure.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the full path of the current structure in cloud.
        /// </summary>
        public string Home { get; set; }

        /// <summary>
        /// Gets or sets the public link to anonymously downloading.
        /// </summary>
        public string Weblink { get; set; }

        /// <summary>
        /// Gets or sets the last modified time.
        /// </summary>
        public long Mtime { get; set; }

        /// <summary>
        /// Gets or sets the structure requested time.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the virus scan comment.
        /// </summary>
        [JsonProperty("virus_scan")]
        public string VirusScan { get; set; }

        /// <summary>
        /// Gets or sets the hash, if the current unit is file.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the list of the nested items of the current structure.
        /// </summary>
        public CloudStructureEntry[] List { get; set; }
    }
}
