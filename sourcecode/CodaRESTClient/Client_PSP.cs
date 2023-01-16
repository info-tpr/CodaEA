/***************************************************
 * This file deals with functions specifically related to Paid Subscription Plans
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves a specific Paid Subscription Plan
        /// </summary>
        /// <param name="SubscriptionId"></param>
        /// <returns></returns>
        public JObject GetPSP(long SubscriptionId)
        {
            var request = NewRequest($"/api/psp/planid/{SubscriptionId}", RestSharp.Method.Get);
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves an array of PSPs that grant access to the given Network
        /// </summary>
        /// <param name="Network">Network code, or use "all" to retrieve plans for all networks</param>
        /// <returns></returns>
        public JArray GetPSPsForNetwork(string Network)
        {
            var request = NewRequest($"/api/psp/plans/{Network}", RestSharp.Method.Get);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Assigns a Paid Subscription Plan to an Account (assuming payment has been processed)
        /// </summary>
        /// <param name="AccountId"></param>
        /// <param name="SubscriptionId"></param>
        /// <returns></returns>
        public JObject SubscribeAccountToPSP(long AccountId, long SubscriptionId)
        {
            var request = NewRequest($"/api/account/{AccountId}/subscribepsp/{SubscriptionId}", Method.Put);
            return GetResponse(request);
        }
    }
}
