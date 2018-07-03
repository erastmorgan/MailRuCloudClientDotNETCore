//-----------------------------------------------------------------------------------------------
// <copyright file="Tree.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the cloud structure
    /// </summary>
    public class Tree
    {
        /// <summary>
        /// Gets the list of folders.
        /// </summary>
        public List<Folder> Folders { get; internal set; }

        /// <summary>
        /// Gets the list of files
        /// </summary>
        public List<File> Files { get; internal set; }
    }
}
