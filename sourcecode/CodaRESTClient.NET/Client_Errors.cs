/*===============================================================================
// This file contains functions related to Error Log entries
 * IMPORTANT NOTES:
 * 1.  ErrorId and Network values are sent to API server in route path, and must not have route delimiter character '/' in them
 */

using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves the data related to an Error Log, and reports it as having occurred.  If you are just browsing or investigating errors,
        /// use GetError() instead.
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="NetworkVersion"></param>
        /// <param name="NumberOccurrences"></param>
        /// <param name="DescriptionMessage"></param>
        /// <param name="Full"></param>
        /// <returns></returns>
        public JObject ReportError(string ErrorId, string Network, string NetworkVersion, int NumberOccurrences, string DescriptionMessage, bool Full = false)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/{ErrorId}/{Network}", Method.Put);
            request.AddHeader("reporting", true);
            request.AddHeader("numinstances", NumberOccurrences);
            request.AddHeader("full", Full);
            var data = new JObject()
            {
                { "description", DescriptionMessage },
                { "version", NetworkVersion }
            };
            request.AddBody(data.ToString(), "text/json");
            return GetResponse(request);
        }

        /// <summary>
        /// Use this to retrieve Error Log and its related info (if Full=true), without reporting the occurrence of the error.  If you wish to report
        /// that an error occurred, please use ReportError() instead.
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Full"></param>
        /// <returns></returns>
        public JObject GetError(string ErrorId, string Network, bool Full = false)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/{ErrorId}/{Network}", Method.Put);
            request.AddHeader("full", Full);
            request.AddHeader("reporting", false);
            return GetResponse(request);
        }

        /// <summary>
        /// Updates an Error Log, you need EE or DV badge for that Network
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Severity"></param>
        /// <param name="Meaning"></param>
        /// <returns></returns>
        public JObject UpdateError(string ErrorId, string Network, int Severity, string Meaning)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/{ErrorId}/{Network}", Method.Patch);
            var data = new JObject
            {
                { "acceptedSeverity", Severity },
                { "acceptedMeaning", Meaning }
            };
            request.AddBody(data.ToString(), "text/json");
            return GetResponse(request);
        }

        /// <summary>
        /// Gets all Error Logs that do not have Troubleshooting solutions
        /// </summary>
        /// <param name="Network"></param>
        /// <returns></returns>
        public JArray GetUnresolvedErrors(string Network, bool Full = false)
        {
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/getunresolved/{Network}", Method.Get, Full);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Gets all Error Logs that do not have assigned Severity or Meaning
        /// </summary>
        /// <param name="Network"></param>
        /// <returns></returns>
        public JArray GetUnanalyzedErrors(string Network, bool Full = false)
        {
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/getunanalyzed/{Network}", Method.Get, Full);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Determines if an account is subscribed to an error code
        /// </summary>
        /// <param name="Account"></param>
        /// <param name="Network"></param>
        /// <param name="ErrorId"></param>
        /// <returns></returns>
        public static bool IsSubscribed(JObject Account, string Network, string ErrorId)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            if (!Account.ContainsKey("subscriptions"))
            {
                return false;
            }
            var subs = (JArray)Account["subscriptions"];
            if (subs is null)
            {
                return false;
            }
            foreach (JObject sub in subs)
            {
                if ($"{sub["itemType"]}" == "ErrorLog")
                {
                    if (($"{sub["itemId2"]}" == ErrorId) && ($"{sub["itemId3"]}" == Network))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public JObject Subscribe(string Network, string ErrorId)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/{Network}/{ErrorId}/subscribe", Method.Get);
            return GetResponse(request);
        }

        public JObject Unsubscribe(string Network, string ErrorId)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/errors/{Network}/{ErrorId}/unsubscribe", Method.Get);
            return GetResponse(request);
        }
    }
}
