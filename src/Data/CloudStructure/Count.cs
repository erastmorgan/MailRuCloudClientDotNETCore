//-----------------------------------------------------------------------------------------------
// <copyright file="Count.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.CloudStructure
{
    /// <summary>
    /// Defines the number of different entry types.
    /// </summary>
    internal class Count
    {
        /// <summary>
        /// Gets or sets the number of folders.
        /// </summary>
        public int Folders { get; set; }

        /// <summary>
        /// Gets or sets the number of files.
        /// </summary>
        public int Files { get; set; }
    }
}
