/*=====================
 * Functions having to do with Community Moderation
 */

using CodaRESTClient;
using Newtonsoft.Json.Linq;
using System;

namespace codaclient.classes
{
    public partial class Program
    {
        /// <summary>
        /// Menu for moderation of posts
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="Post"></param>
        /// <param name="CodaClient"></param>
        /// <param name="MyAccount"></param>
        public static void ModerateItem(string Breadcrumbs, JObject Configuration, JObject Post, Client CodaClient, JObject MyAccount)
        {
            var report = GetItemReport(CodaClient, Post);
            if (report.ContainsKey("code"))
            {
                ShowErrorMessage(report);
                return;
            }
            var menu = BuildModeratorMenu(report, MyAccount);
            string input;
            do
            {
                ShowItemReport(report, CodaClient, MyAccount);
                ShowBreadcrumbs(Breadcrumbs);
                input = ShowMenu(Configuration, menu);
                switch(input.ToUpper())
                {
                    case "V":
                        ShowReferencedItem(Breadcrumbs + "ReferencedItem/", Configuration, CodaClient, $"{Post["referencedItemType"]}", Convert.ToInt64(Post["referencedItemId"]), MyAccount);
                        break;
                    case "C":
                        var postid = GetPostIdentifier(Post);
                        if (postid.ContainsKey("code"))
                        {
                            ShowErrorMessage(postid);
                        }
                        else
                        {
                            var comments = "Enter your comments";
                            comments = EditBody(Breadcrumbs + "EditComments/", Configuration, "EDIT CONFIRMATION COMMENTS", comments);
                            var result = CodaClient.Vote(Enum.Parse<CodaObjectTypeEnum>($"{postid["itemType"]}"), Convert.ToInt64(postid["itemId"]), VoteTypeEnum.ConfirmReport, comments);
                            if (Convert.ToBoolean(result["result"]))
                            {
                                Pause("Thanks!  Your input has been recorded.  Press ENTER to continue.");
                            }
                            else
                            {
                                Pause("Sorry, something went wrong.  Please try again or contact support.  Press ENTER to continue.");
                            }
                        }
                        break;
                    case "D":
                        postid = GetPostIdentifier(Post);
                        if (postid.ContainsKey("code"))
                        {
                            ShowErrorMessage(postid);
                        }
                        else
                        {
                            var comments = "Enter your comments";
                            comments = EditBody(Breadcrumbs + "EditComments/", Configuration, "EDIT DENIAL COMMENTS", comments);
                            var result = CodaClient.Vote(Enum.Parse<CodaObjectTypeEnum>($"{postid["itemType"]}"), Convert.ToInt64(postid["itemId"]), VoteTypeEnum.DenyReport, comments);
                            if (Convert.ToBoolean(result["result"]))
                            {
                                Pause("Thanks!  Your input has been recorded.  Press ENTER to continue.");
                            }
                            else
                            {
                                Pause("Sorry, something went wrong.  Please try again or contact support.  Press ENTER to continue.");
                            }
                        }
                        break;
                }
            } while (input.ToUpper() != "Q");
        }

        private static JObject GetPostIdentifier(JObject Post)
        {
            var retVal = new JObject();
            if (Post.ContainsKey("troubleshootId"))
            {
                retVal["itemType"] = $"{CodaObjectTypeEnum.TroubleshootSolution}";
                retVal["itemId"] = Post["troubleshootId"];
            }
            else if (Post.ContainsKey("discussionId"))
            {
                retVal["itemType"] = $"{CodaObjectTypeEnum.ErrorLogDiscussion}";
                retVal["itemId"] = Post["discussionId"];
            }
            else if (Post.ContainsKey("commentId"))
            {
                retVal["itemType"] = $"{CodaObjectTypeEnum.TroubleshootComment}";
                retVal["itemId"] = Post["commentId"];
            }
            else
            {
                return new JObject()
                {
                    ["code"] = "error",
                    ["message"] = "Cannot moderate this item, it is the wrong type.  Press ENTER to continue",
                };
            }
            return retVal;
        }


        /// <summary>
        /// Retrieves an Item Report for a given Post
        /// </summary>
        /// <param name="CodaClient"></param>
        /// <param name="Post"></param>
        /// <returns></returns>
        private static JObject GetItemReport(Client CodaClient, JObject Post)
        {
            CodaObjectTypeEnum itemType;
            long itemId;
            if (Post.ContainsKey("troubleshootId"))
            {
                itemType = CodaObjectTypeEnum.TroubleshootSolution;
                itemId = Convert.ToInt64(Post["troubleshootId"]);
            }
            else if (Post.ContainsKey("discussionId"))
            {
                itemType = CodaObjectTypeEnum.ErrorLogDiscussion;
                itemId = Convert.ToInt64(Post["discussionId"]);
            }
            else if (Post.ContainsKey("commentId"))
            {
                itemType = CodaObjectTypeEnum.TroubleshootComment;
                itemId = Convert.ToInt64(Post["commentId"]);
            }
            else
            {
                return new JObject()
                {
                    ["code"] = "error",
                    ["message"] = "Cannot moderate this item, it is the wrong type.  Press ENTER to continue",
                };
            }
            return CodaClient.GetItemReport(itemType, itemId);
        }


        /// <summary>
        /// Displays the related item
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="CodaClient"></param>
        /// <param name="ItemType"></param>
        /// <param name="ItemId"></param>
        /// <param name="MyAccount"></param>
        private static void ShowReferencedItem(string Breadcrumbs, JObject Configuration, Client CodaClient, string ItemType, long ItemId, JObject MyAccount)
        {
            switch (ItemType)
            {
                case "Comment":
                    var item = CodaClient.GetComment(ItemId);
                    var tshoot = CodaClient.GetTroubleshooting(Convert.ToInt64(item["troubleshootId"]));
                    ShowComment(Breadcrumbs, item, tshoot, 1, 1, CodaClient, MyAccount);
                    break;
                case "Troubleshoot":
                    tshoot = CodaClient.GetTroubleshooting(ItemId);
                    ShowTroubleshoot(Breadcrumbs, tshoot, 1, 1, MyAccount, CodaClient);
                    break;
                case "Discussion":
                    item = CodaClient.GetDiscussion(ItemId);
                    ShowDiscussion(Breadcrumbs, item, 1, 1, CodaClient);
                    break;
            }
        }

        private static void ShowItemReport(JObject Report, Client CodaClient, JObject MyAccount)
        {
            Console.Clear();
            var closerName = "-- NOT CLOSED --";
            try
            {
                var closerId = Convert.ToInt64(Report["reportClosedById"]);
                var closer = CodaClient.GetAccountInfo(closerId);
                closerName = $"{closer["accountName"]} (ID: {closer["accountId"]})";
            }
            catch { }
            CConsole.WriteLine($"{"======= VIOLATION REPORT FOR (":cyan}{Report["reportedItemType"]:blue} {"ID:":cyan} {Report["reportedItemId"]:blue}) {"=========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
            CConsole.WriteLine($" {"Report Date:":cyan} {Report["reportDate"]}     {"STATUS:":cyan} {Report["reportStatus"]}");
            CConsole.WriteLine($" {"CLOSURE:":cyan}    {Report["reportClosedDate"]} - {closerName}");
            CConsole.WriteLine($"{Report["reportComments"]}");
            CConsole.WriteLine($"{"------------------------------------------------------------------------":cyan}");
            var userReports = (JArray)Report["userReports"]!;
            foreach (JObject userReport in userReports)
            {
                ShowUserReport(CodaClient, userReport);
            }
        }

        private static void ShowUserReport(Client CodaClient, JObject UserReport)
        {
            var user = CodaClient.GetAccountInfo(Convert.ToInt64(UserReport["reportedById"]));
            var username = user.ContainsKey("code") switch
            {
                true => "-- User not found --",
                false => $"{user["accountName"]}",
            };
            CConsole.WriteLine($"    {UserReport["reportDate"]} - {username:yellow}");
            CConsole.WriteLine($"    {UserReport["reportComments"]}");
            CConsole.WriteLine($"    {"--------------------------------------------------------------------":cyan}");
        }

        private static string BuildModeratorMenu(JObject Report, JObject MyAccount)
        {
            var retVal = "V)iew Item ";
            // This enforces the lifecycle of the report
            if ($"{Report["reportStatus"]}" == "Reported")
            {
                retVal = "C)onfirm D)eny ";
            }
            else if ($"{Report["reportStatus"]}" == "Appealed")
            {
                if ($"{Report["reportClosedById"]}" != $"{MyAccount["accountId"]}")
                {
                    retVal = "C)onfirm [final], D)eny ";
                }
            }
            else if ($"{Report["reportStatus"]}" == "Denied")
            {
                retVal = "Report has been Denied, you cannot moderate further. ";
            }
            else if (($"{Report["reportStatus"]}" == "Confirmed") || ($"{Report["reportStatus"]}" == "FinalConfirm"))
            {
                retVal = "Report has been Confirmed by a Moderator, nothing to do. ";
            }
            retVal += "Q)uit";
            return retVal;
        }
    }
}
