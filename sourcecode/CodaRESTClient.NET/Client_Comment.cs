// This file contains functions related to Troubleshooting Comments

using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Posts a new Comment to a Troubleshooting entry
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <param name="Body"></param>
        /// <param name="Links"></param>
        /// <returns></returns>
        public JObject AddComment(long TroubleshootId, string Body, JArray Links)
        {
            var request = NewRequest("/api/comment", Method.Post);
            request.AddBody(new JObject()
            {
                ["troubleshootId"] = TroubleshootId,
                ["body"] = Body,
                ["links"] = Links
            }.ToString(Newtonsoft.Json.Formatting.None), "text/json") ;
            return GetResponse(request);
        }

        /// <summary>
        /// Updates a Troubleshooting Comment to make additions/corrections
        /// </summary>
        /// <param name="CommentId"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateComment(long CommentId, string Body, List<JObject> Links)
        {
            if (Links == null)
            {
                return UpdateComment(CommentId, Body);
            }
            else
            {
                return UpdateComment(CommentId, Body, Links.ToArray());
            }
        }

        /// <summary>
        /// Updates a Troubleshooting Comment to make additions/corrections
        /// </summary>
        /// <param name="CommentId"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateComment(long CommentId, string Body, JObject[] Links)
        {
            if (Links == null)
            {
                return UpdateComment(CommentId, Body);
            }
            else
            {
                return UpdateComment(CommentId, Body, JArray.FromObject(Links));
            }
        }

        /// <summary>
        /// Updates a Troubleshooting Comment to make additions/corrections
        /// </summary>
        /// <param name="CommentId"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public JObject UpdateComment(long CommentId, string Body)
        {
            JArray links = null;
            return UpdateComment(CommentId, Body, links);
        }

        /// <summary>
        /// Updates a Troubleshooting Comment to make additions/corrections
        /// </summary>
        /// <param name="CommentId"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateComment(long CommentId, string Body, JArray Links)
        {
            var request = NewRequest($"/api/comment/{CommentId}", Method.Patch);
            var body = new JObject
            {
                { "body", Body }
            };
            if (Links != null)
            {
                body.Add("links", Links);
            }
            request.AddBody(body);
            return GetResponse(request);
        }

        /// <summary>
        /// Deletes a Troubleshooting Comment
        /// </summary>
        /// <param name="CommentId"></param>
        /// <returns></returns>
        public JObject DeleteComment(long CommentId)
        {
            var request = NewRequest($"/api/comment/{CommentId}", Method.Delete);
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves a single Troubleshooting Comment
        /// </summary>
        /// <param name="CommentId"></param>
        /// <returns></returns>
        public JObject GetComment(long CommentId)
        {
            var request = NewRequest($"/api/comment/{CommentId}", Method.Get);
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves all comments for a Troubleshooting solution
        /// </summary>
        /// <param name="TroubleshootId"></param>
        /// <returns></returns>
        public JArray GetAllComments(long TroubleshootId)
        {
            var request = NewRequest($"/api/comment/getall/{TroubleshootId}", Method.Get);
            return GetResponseArray(request);
        }
    }
}
