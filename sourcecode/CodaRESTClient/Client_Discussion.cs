// This file contains functions related to Error Log Discussions

using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web;

namespace CodaRESTClient
{
    public partial class Client
    {
        /// <summary>
        /// Creates a Discussion entry for an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject AddDiscussion(string ErrorId, string Network, string Body, List<JObject>? Links)
        {
            if (Links == null)
            {
                return AddDiscussion(ErrorId, Network, Body);
            }
            else
            {
                return AddDiscussion(ErrorId, Network, Body, Links.ToArray());
            }
        }

        /// <summary>
        /// Creates a Discussion entry for an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject AddDiscussion(string ErrorId, string Network, string Body, JObject[]? Links)
        {
            if (Links == null)
            {
                return AddDiscussion(ErrorId, Network, Body);
            }
            else
            {
                return AddDiscussion(ErrorId, Network, Body, JArray.FromObject(Links));
            }
        }

        /// <summary>
        /// Creates a Discussion entry for an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public JObject AddDiscussion(string ErrorId, string Network, string Body)
        {
            JArray? links = null;
            return AddDiscussion(ErrorId, Network, Body, links);
        }

        /// <summary>
        /// Creates a Discussion entry for an Error Log
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject AddDiscussion(string ErrorId, string Network, string Body, JArray? Links)
        {
            var request = NewRequest("/api/discussion", Method.Post);
            var body = new JObject
            {
                ["errorId"] = ErrorId,
                ["network"] = Network,
                ["body"] = Body
            };
            if (Links != null)
            {
                body["links"] = Links;
            }
            request.AddBody(body.ToString(), "text/json");
            return GetResponse(request);
        }

        /// <summary>
        /// Updates a discussion entry for corrections
        /// </summary>
        /// <param name="DiscussionId"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateDiscussion(long DiscussionId, string? Body, List<JObject>? Links)
        {
            if (Links == null)
            {
                if (Body == null)
                {
                    var ret = new JObject
                    {
                        { "message", "Nothing to do - no fields are updated" }
                    };
                    return ret;
                }
                else
                {
                    return UpdateDiscussion(DiscussionId, Body);
                }
            }
            else
            {
                return UpdateDiscussion(DiscussionId, Body, Links.ToArray());
            }
        }

        /// <summary>
        /// Updates a discussion entry for corrections
        /// </summary>
        /// <param name="DiscussionId"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateDiscussion(long DiscussionId, string? Body, JObject[]? Links)
        {
            if (Links == null)
            {
                if (Body == null)
                {
                    var ret = new JObject
                    {
                        { "message", "Nothing to do - no fields are updated" }
                    };
                    return ret;
                }
                else
                {
                    return UpdateDiscussion(DiscussionId, Body);
                }
            }
            else
            {
                return UpdateDiscussion(DiscussionId, Body, JArray.FromObject(Links));
            }
        }

        /// <summary>
        /// Updates a discussion entry for corrections
        /// </summary>
        /// <param name="DiscussionId"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public JObject UpdateDiscussion(long DiscussionId, string Body)
        {
            JArray? links = null;
            return UpdateDiscussion(DiscussionId, Body, links);
        }

        /// <summary>
        /// Updates a discussion entry for corrections
        /// </summary>
        /// <param name="DiscussionId"></param>
        /// <param name="Body"></param>
        /// <param name="Links">(Optional) list of JObjects containing "display" and "link" fields</param>
        /// <returns></returns>
        public JObject UpdateDiscussion(long DiscussionId, string? Body, JArray? Links)
        {
            var request = NewRequest($"/api/discussion/{DiscussionId}", Method.Patch);
            var body = new JObject();
            if (Body != null)
            {
                body.Add("body", Body);
            }
            if (Links != null)
            {
                body.Add("links", Links);
            }
            if (body.Count > 0)
            {
                request.AddBody(body.ToString(), "text/json");
                return GetResponse(request);
            }
            else
            {
                body.Add("message", "Nothing to do - no fields are updated");
                return body;
            }
        }

        /// <summary>
        /// Deletes a Discussion entry
        /// </summary>
        /// <param name="DiscussionId"></param>
        /// <returns></returns>
        public JObject DeleteDiscussion(long DiscussionId)
        {
            var request = NewRequest($"/api/discussion/{DiscussionId}", Method.Delete);
            return GetResponse(request);
        }

        /// <summary>
        /// Gets a list of all Discussions on a given Error Log entry.
        /// </summary>
        /// <param name="ErrorId"></param>
        /// <param name="Network"></param>
        /// <returns></returns>
        public JArray GetAllDiscussions(string ErrorId, string Network)
        {
            ErrorId = HttpUtility.UrlEncode(ErrorId);
            Network = HttpUtility.UrlEncode(Network);
            var request = NewRequest($"/api/discussion/{ErrorId}/{Network}", Method.Get);
            return GetResponseArray(request);
        }

    }
}
