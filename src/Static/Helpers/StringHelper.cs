//-----------------------------------------------------------------------------------------------
// <copyright file="StringHelper.cs" company="Erast Korolev">
//     Created in 2018, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Static.Helpers
{
    using MailRuCloudClient.Data;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the extension to work with the strings.
    /// </summary>
    internal static class StringHelper
    {
        /// <summary>
        /// Deserialize the JSON to <see cref="DefaultResponse"/> object.
        /// </summary>
        /// <typeparam name="T">The body node object type.</typeparam>
        /// <param name="json">The JSON which will be deserialized.</param>
        /// <returns>Deserialized object.</returns>
        public static T Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<DefaultResponse<T>>(json).Body;
        }
    }
}
