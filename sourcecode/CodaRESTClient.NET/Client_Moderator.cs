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
            string typeName;
            switch (ItemType)
            {
                case CodaObjectTypeEnum.ErrorLogDiscussion:
                    typeName = "Discussion";
                    break;
                case CodaObjectTypeEnum.TroubleshootComment:
                    typeName = "Comment";
                    break;
                case CodaObjectTypeEnum.TroubleshootSolution:
                    typeName = "Troubleshoot";
                    break;
                default:
                    typeName = "Unsupported";
                    break;
            };
            var request = NewRequest($"/api/moderate/report/{typeName}/{ItemId}", Method.Get);
            return GetResponse(request);
        }
    }
}
