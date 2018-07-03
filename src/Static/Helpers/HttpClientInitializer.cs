//-----------------------------------------------------------------------------------------------
// <copyright file="HttpClientInitializer.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Static.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the HTTP client initializer.
    /// </summary>
    internal static class HttpClientInitializer
    {
        /// <summary>
        /// Create the HTTP client as new, if in the account entity does not exists or the host is modified, else will be returned an existing.
        /// </summary>
        /// <param name="account">An existing account.</param>
        /// <param name="host">The base address.</param>
        /// <returns>The created HTTP client with the new host address.</returns>
        public static HttpClient Create(Account account, string host)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            //// Check if was modified the account amadeus URL and set if modified.
            if (account.HttpClient == null || account.HttpClient.BaseAddress.OriginalString != host)
            {
                var handler = new HttpClientHandler();
                handler.CookieContainer = account.Cookies;
                var httpClient = new HttpClient(handler);
                httpClient.BaseAddress = new Uri(host);
                httpClient.DefaultRequestHeaders.Add("User-Agent", HttpHeaders.UserAgent);
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
                account.HttpClient = httpClient;
                return httpClient;
            }

            return account.HttpClient;
        }

        /// <summary>
        /// Send the GET request and receive result as string.
        /// </summary>
        /// <param name="httpClient">The HTTP client or sender.</param>
        /// <param name="requestUri">The request URL.</param>
        /// <param name="uriFormat">The format of request URL.</param>
        /// <returns>The response as string.</returns>
        public static async Task<string> GetStringAsync(this HttpClient httpClient, string requestUri, params object[] uriFormat)
        {
            return await httpClient.GetStringAsync(string.Format(requestUri, uriFormat));
        }

        /// <summary>
        /// Send the GET request and receive result as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="httpClient">The HTTP client or sender.</param>
        /// <param name="requestUri">The request URL.</param>
        /// <param name="uriFormat">The format of request URL.</param>
        /// <returns>The response as <see cref="HttpResponseMessage"/>.</returns>
        public static async Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string requestUri, params object[] uriFormat)
        {
            return await httpClient.GetAsync(string.Format(requestUri, uriFormat));
        }

        /// <summary>
        /// Send the POST request and receive result as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="httpClient">The HTTP client or sender.</param>
        /// <param name="requestUri">The request URL.</param>
        /// <param name="content">The HTTP content.</param>
        /// <param name="uriFormat">The format of request URL.</param>
        /// <returns>The response as <see cref="HttpResponseMessage"/>.</returns>
        public static async Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, string requestUri, HttpContent content, params object[] uriFormat)
        {
            return await httpClient.PostAsync(string.Format(requestUri, uriFormat), content);
        }

        /// <summary>
        /// Convert the value/pair list to form URL encoded content.
        /// </summary>
        /// <param name="source">The valie/pait list.</param>
        /// <returns>The converted content for HTTP client.</returns>
        public static FormUrlEncodedContent ToFormUrlEncodedContent(this List<KeyValuePair<string, object>> source)
        {
            return new FormUrlEncodedContent(source.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())));
        }
    }
}
