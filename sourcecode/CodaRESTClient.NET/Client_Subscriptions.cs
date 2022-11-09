// This file contains functions related to Subscriptions

using Newtonsoft.Json.Linq;
using RestSharp;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Removes a specific subscription and all pending notifications
        /// </summary>
        /// <param name="SubscriptionId"></param>
        /// <returns></returns>
        public JObject Unsubscribe(long SubscriptionId)
        {
            var request = NewRequest($"/api/subscriptions/{SubscriptionId}", Method.Delete);
            return GetResponse(request);
        }
    }
}
