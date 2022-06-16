// This file contains functions related to moderating posts

using Newtonsoft.Json.Linq;
using RestSharp;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves an Item Violation Report for a given item
        /// </summary>
        /// <param name="ItemType"></param>
        /// <param name="ItemId"></param>
        /// <returns></returns>
        public JObject GetItemReport(CodaObjectTypeEnum ItemType, long ItemId)
        {
            string typeName = ItemType switch
            {
                CodaObjectTypeEnum.ErrorLogDiscussion => "Discussion",
                CodaObjectTypeEnum.TroubleshootComment => "Comment",
                CodaObjectTypeEnum.TroubleshootSolution => "Troubleshoot",
                _ => "Unsupported",
            };
            var request = NewRequest($"/api/moderate/report/{typeName}/{ItemId}", Method.Get);
            return GetResponse(request);
        }
    }
}
