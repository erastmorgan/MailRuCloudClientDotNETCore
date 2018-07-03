//-----------------------------------------------------------------------------------------------
// <copyright file="Duration.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data.Rates
{
    /// <summary>
    /// Defines the duration of fare.
    /// </summary>
    public class Duration
    {
        /// <summary>
        /// Gets the number of months.
        /// </summary>
        public int MonthsCount { get; internal set; }

        /// <summary>
        /// Gets the number of days.
        /// </summary>
        public int DaysCount { get; internal set; }
    }
}
