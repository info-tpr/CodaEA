// This file contains functions related to moderating posts

using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves a list of User Reports for a given Account (each User Report is attached to an Item Violation Report)
        /// </summary>
        /// <param name="ForAccountId"></param>
        /// <returns></returns>
        public JArray GetUserReports(long ForAccountId)
        {
            var request = NewRequest($"/api/moderate/reports/by/{ForAccountId}", Method.Get);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Retrieves a list of Item Reports against a specific Account
        /// </summary>
        /// <param name="AgainstAccountId"></param>
        /// <returns></returns>
        public JArray GetItemReports(long AgainstAccountId)
        {
            var request = NewRequest($"/api/moderate/reports/against/{AgainstAccountId}", Method.Get);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Retrieves a specific existing Item Violation Report
        /// </summary>
        /// <param name="ItemReportId"></param>
        /// <param name="Full"></param>
        /// <returns></returns>
        public JObject GetItemReport(long ItemReportId, bool Full = false)
        {
            var request = NewRequest($"/api/moderate/report/{ItemReportId}", Method.Get, Full);
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves an Item Violation Report for a given item
        /// </summary>
        /// <param name="ItemType"></param>
        /// <param name="ItemId"></param>
        /// <returns></returns>
        public JObject GetItemReport(CodaObjectTypeEnum ItemType, long ItemId, bool Full = false)
        {
            string typeName = ItemType switch
            {
                CodaObjectTypeEnum.ErrorLogDiscussion => "Discussion",
                CodaObjectTypeEnum.TroubleshootComment => "Comment",
                CodaObjectTypeEnum.TroubleshootSolution => "Troubleshoot",
                _ => "Unsupported",
            };
            var request = NewRequest($"/api/moderate/report/{typeName}/{ItemId}", Method.Get, Full);
            return GetResponse(request);
        }

        public JArray GetOpenReportsForNetwork(string Network)
        {
            var request = NewRequest($"/api/moderate/reports/{HttpUtility.UrlEncode(Network)}", Method.Get);
            return GetResponseArray(request);
        }
    }
}
