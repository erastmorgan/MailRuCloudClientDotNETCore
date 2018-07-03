//-----------------------------------------------------------------------------------------------
// <copyright file="CostItem.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.Rates
{
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the cost item.
    /// </summary>
    public class CostItem
    {
        /// <summary>
        /// Gets the cost of rate.
        /// </summary>
        [JsonProperty("cost")]
        public int Cost { get; internal set; }

        /// <summary>
        /// Gets the currency of rate.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; internal set; }

        /// <summary>
        /// Gets the unique ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the special cost of rate.
        /// </summary>
        [JsonProperty("special_cost")]
        public int SpecialCost { get; internal set; }

        /// <summary>
        /// Gets the duration of rate.
        /// </summary>
        [JsonIgnore]
        public Duration Duration { get => this.ConvertDuration(this.DurationStr); }

        /// <summary>
        /// Gets the special duration of rate.
        /// </summary>
        [JsonIgnore]
        public Duration SpecialDuration { get => this.ConvertDuration(this.SpecialDurationStr); }

        /// <summary>
        /// Gets or sets the duration of rate as string.
        /// </summary>
        [JsonProperty("duration")]
        internal string DurationStr { get; set; }

        /// <summary>
        /// Gets or sets the special diration of rate as string.
        /// </summary>
        [JsonProperty("special_duration")]
        internal string SpecialDurationStr { get; set; }

        /// <summary>
        /// Convert the string duration to object.
        /// </summary>
        /// <param name="duration">The source duration.</param>
        /// <returns>Converted duration.</returns>
        private Duration ConvertDuration(string duration)
        {
            var result = new Duration();
            var match = Regex.Match(duration, "(\\d+)M");
            if (match.Success)
            {
                result.MonthsCount = int.Parse(match.Groups[1].Value);
            }

            match = Regex.Match(duration, "(\\d+)D");
            if (match.Success)
            {
                result.DaysCount = int.Parse(match.Groups[1].Value);
            }

            return result;
        }
    }
}
