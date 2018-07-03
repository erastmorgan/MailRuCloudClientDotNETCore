//-----------------------------------------------------------------------------------------------
// <copyright file="Rates.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.Rates
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the rates.
    /// </summary>
    public class Rates
    {
        /// <summary>
        /// Gets the rates array.
        /// </summary>
        [JsonProperty("rates")]
        public Rate[] Items { get; internal set; }
    }
}
