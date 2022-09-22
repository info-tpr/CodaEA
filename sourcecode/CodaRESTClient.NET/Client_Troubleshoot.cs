// This file contains functions related to Troubleshooting solution entries

using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Web;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Posts a Troubleshooting solution to an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="NetworkVersion"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject AddTroubleshooting(string ErrorId, string Network, string NetworkVersion, string Body, List<JObject> Links)
        {
            if (Links is null)
            {
                return AddTroubleshooting(ErrorId, Network, NetworkVersion, Body);
            }
            else
            {
                return AddTroubleshooting(ErrorId, Network, NetworkVersion, Body, Links.ToArray());
            }
        }

        /// <summary>
        /// Posts a Troubleshooting solution to an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="NetworkVersion"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject AddTroubleshooting(string ErrorId, string Network, string NetworkVersion, string Body, JObject[] Links)
        {
            if (Links is null)
            {
                return AddTroubleshooting(ErrorId, Network, NetworkVersion, Body);
            }
            else
            {
                return AddTroubleshooting(ErrorId, Network, NetworkVersion, Body, JArray.FromObject(Links));
            }
        }

        /// <summary>
        /// Posts a Troubleshooting solution to an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="NetworkVersion"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public JObject AddTroubleshooting(string ErrorId, string Network, string NetworkVersion, string Body)
        {
            JArray links = null;
            return AddTroubleshooting(ErrorId, Network, NetworkVersion, Body, links);
        }

        /// <summary>
        /// Posts a Troubleshooting solution to an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="NetworkVersion"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject AddTroubleshooting(string ErrorId, string Network, string NetworkVersion, string Body, JArray Links)
        {
            var request = NewRequest("/api/troubleshoot", Method.Post);
            var data = new JObject()
            {
                ["errorId"] = ErrorId,
                ["network"] = Network,
                ["version"] = NetworkVersion,
                ["steps"] = Body,
            };
            if (Links != null)
            {
                data.Add("links", Links);
            }
            request.AddBody(data.ToString(), "text/json");
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves all Troubleshooting solutions for a given Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Full">Whether or not to retrieve comments as well</param>
        /// <returns></returns>
        public JArray GetTroubleshooting(string ErrorId, string Network, bool Full = false)
        {
            var errid = HttpUtility.UrlEncode(ErrorId);
            var net = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/troubleshoot/{errid}/{net}", Method.Get);
            request.AddHeader("full", Full);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Retrieves a specific Troubleshooting solution
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <param name="Full">Whether or not to also return related Comments</param>
        /// <returns></returns>
        public JObject GetTroubleshooting(long TroubleshootId, bool Full = false)
        {
            var request = NewRequest($"/api/troubleshoot/{TroubleshootId}", Method.Get);
            request.AddHeader("full", Full);
            return GetResponse(request);
        }

        /// <summary>
        /// Updates a Troubleshooting solution
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <param name="Body"></param>
        /// <param name="ModificationComments"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateTroubleshooting(long TroubleshootId, string Body, string ModificationComments, List<JObject> Links)
        {
            if (Links == null)
            {
                return UpdateTroubleshooting(TroubleshootId, Body, ModificationComments);
            }
            else
            {
                return UpdateTroubleshooting(TroubleshootId, Body, ModificationComments, Links.ToArray());
            }
        }

        /// <summary>
        /// Updates a Troubleshooting solution
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <param name="Body"></param>
        /// <param name="ModificationComments"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateTroubleshooting(long TroubleshootId, string Body, string ModificationComments, JObject[] Links)
        {
            if (Links is null)
            {
                return UpdateTroubleshooting(TroubleshootId, Body, ModificationComments);
            }
            else
            {
                return UpdateTroubleshooting(TroubleshootId, Body, ModificationComments, JArray.FromObject(Links));
            }
        }

        /// <summary>
        /// Updates a Troubleshooting solution
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <param name="Body"></param>
        /// <param name="ModificationComments"></param>
        /// <returns></returns>
        public JObject UpdateTroubleshooting(long TroubleshootId, string Body, string ModificationComments)
        {
            JArray links = null;
            return UpdateTroubleshooting(TroubleshootId, Body, ModificationComments, links);
        }

        /// <summary>
        /// Updates a Troubleshooting solution
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <param name="Body"></param>
        /// <param name="ModificationComments"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateTroubleshooting(long TroubleshootId, string Body, string ModificationComments, JArray Links)
        {
            var request = NewRequest($"/api/troubleshoot/{TroubleshootId}", Method.Patch);
            var data = new JObject()
            {
                ["steps"] = Body,
                ["modificationComments"] = ModificationComments,
            };
            if (Links != null)
            {
                data.Add("links", Links);
            }
            request.AddBody(data.ToString(), "text/json");
            return GetResponse(request);
        }

    }
}
