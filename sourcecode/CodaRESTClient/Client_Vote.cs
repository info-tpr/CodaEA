// This file contains functions pertaining to managing Networks

using Newtonsoft.Json.Linq;
using RestSharp;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Post a vote on a conversation item
        /// </summary>
        /// <param name="ObjectType"></param>
        /// <param name="ObjectId"></param>
        /// <param name="Vote"></param>
        /// <returns></returns>
        public bool Vote(CodaObjectTypeEnum ObjectType, long ObjectId, VoteTypeEnum Vote, string? Comments = null)
        {
            string objType = String.Empty;
            switch (ObjectType)
            {
                case CodaObjectTypeEnum.TroubleshootSolution:
                    objType = "Troubleshoot";
                    break;
                case CodaObjectTypeEnum.ErrorLogDiscussion:
                    objType = "Discussion";
                    break;
                case CodaObjectTypeEnum.TroubleshootComment:
                    objType = "Comment";
                    break;
            }
            string dir = Vote switch
            {
                VoteTypeEnum.VoteDown => "down",
                VoteTypeEnum.Report => "report",
                VoteTypeEnum.ConfirmReport => "confirmreport",
                VoteTypeEnum.DenyReport => "denyreport",
                _ => "up",
            };
            var request = NewRequest($"/api/vote/{dir}/{objType}/{ObjectId}", Method.Post);
            switch (Vote)
            {
                case VoteTypeEnum.Report:
                case VoteTypeEnum.ConfirmReport:
                case VoteTypeEnum.DenyReport:
                    request.AddBody(new JObject() { ["comments"] = Comments });
                    break;
            }
            var response = CodaClient.ExecuteAsync(request).Result;
            return (response.StatusCode == System.Net.HttpStatusCode.OK);
        }

    }
}
