//-----------------------------------------------------------------------------------------------
// <copyright file="DiskUsage.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    /// <summary>
    /// Disk usage on current account.
    /// </summary>
    public class DiskUsage
    {
        /// <summary>
        /// Gets total disk size.
        /// </summary>
        public Size Total { get; internal set; }

        /// <summary>
        /// Gets used disk size.
        /// </summary>
        public Size Used { get; internal set; }

        /// <summary>
        /// Gets free disk size.
        /// </summary>
        public Size Free
        {
            get
            {
                return new Size(this.Total.DefaultValue - this.Used.DefaultValue);
            }
        }
    }
}
