using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Queues up a bulk email to go out to various Networks
        /// </summary>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="ToNetworks"></param>
        /// <returns></returns>
        public JObject SendBulkMail(string Subject, string Body, string[] ToNetworks)
        {
            var request = NewRequest("/api/admin/email/network", Method.Post);
            request.AddHeader("subject", Subject);
            request.AddHeader("networks", string.Join(",", ToNetworks));
            request.AddBody(Body);
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves an alternate Account ID by type
        /// </summary>
        /// <param name="AccountId">CodaEA Account ID</param>
        /// <param name="AlternateIdName">Alternate ID name (e.g. Stripe, Discord...)</param>
        /// <returns></returns>
        public JObject GetAccountAlternateKey(long AccountId, string AlternateIdName)
        {
            var request = NewRequest($"/api/account/{AccountId}/alternateId", Method.Get);
            request.AddHeader("systemname", AlternateIdName); 
            return GetResponse(request);
        }

        /// <summary>
        /// Set an alternate Account ID by type
        /// </summary>
        /// <param name="AccountId">CodaEA Account ID</param>
        /// <param name="AlternateIdName">Alternate ID name</param>
        /// <param name="AlternateId">Alternate ID value</param>
        /// <param name="TimeToLiveMinutes">How long before the ID expires; if not specified, default is 20 years</param>
        /// <returns></returns>
        public JObject SetAccountAlternateKey(long AccountId, string AlternateIdName, string AlternateId, Int32 TimeToLiveMinutes = -1)
        {
            var request = NewRequest($"/api/account/{AccountId}/alternateId", Method.Put);
            request.AddHeader("systemname", AlternateIdName);
            request.AddHeader("alternateid", AlternateId);
            if (TimeToLiveMinutes > 0)
            {
                request.AddHeader("altidttl", TimeToLiveMinutes.ToString());
            }
            return GetResponse(request);
        }
    }
}
