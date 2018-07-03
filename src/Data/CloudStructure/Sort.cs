//-----------------------------------------------------------------------------------------------
// <copyright file="Sort.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.CloudStructure
{
    /// <summary>
    /// Defines the items sorting parameter.
    /// </summary>
    internal class Sort
    {
        /// <summary>
        /// Gets or sets the order type.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Gets or sets the field by which did the sorting.
        /// </summary>
        public string Type { get; set; }
    }
}
