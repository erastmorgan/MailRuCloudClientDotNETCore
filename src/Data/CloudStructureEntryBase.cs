//-----------------------------------------------------------------------------------------------
// <copyright file="CloudStructureEntryBase.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    using static MailRuCloudClient.CloudClient;

    /// <summary>
    /// Defined the base class of cloud structure entry. It can be file or folder.
    /// </summary>
    public abstract class CloudStructureEntryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStructureEntryBase"/> class.
        /// </summary>
        /// <param name="account">The MAILRU account.</param>
        internal CloudStructureEntryBase(Account account)
        {
            this.Account = account;
            this.Client = new CloudClient(this.Account);
        }

        /// <summary>
        /// Changing progress event, works only for upload and download operations.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChangedEvent
        {
            add => this.Client.ProgressChangedEvent += value;
            remove => this.Client.ProgressChangedEvent -= value;
        }

        /// <summary>
        /// Gets the item name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the item size.
        /// </summary>
        public Size Size { get; internal set; }

        /// <summary>
        /// Gets full item path in cloud.
        /// </summary>
        public string FullPath { get; internal set; }

        /// <summary>
        /// Gets public item link to share without authentication.
        /// </summary>
        public string PublicLink { get; internal set; }

        /// <summary>
        /// Gets the cloud account of MAILRU.
        /// </summary>
        protected Account Account { get; }

        /// <summary>
        /// Gets the cloud account of MAILRU.
        /// </summary>
        protected CloudClient Client { get; }
    }
}
