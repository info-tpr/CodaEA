/*------------------------------------------------------------------------
 * Code related to voting on items
 */

using CodaRESTClient;
using Newtonsoft.Json.Linq;
using System;

namespace codaclient.classes
{
    public partial class Program
    {
        private static void VoteItem(string Breadcrumbs, JObject Configuration, CodaObjectTypeEnum ItemType, long ItemId, Client CodaClient, JObject MyAccount)
        {
            var msg = "Your vote has been recorded, thank you for your participation!  Please note, subsequent votes on the same " +
                "item from the same account overwrites the previous vote.";
            var response = ShowMenu(Configuration, "U)p-vote D)own-vote R)eport violation Q)uit/cancel");
            if (response.ToUpper() == "Q")
            {
                return;
            }
            var vote = response.ToUpper() switch
            {
                "D" => VoteTypeEnum.VoteDown,
                "R" => VoteTypeEnum.Report,
                _ => VoteTypeEnum.VoteUp
            };
            string? comments = null;
            if (vote.ToString() == "R")
            {
                comments = EditBody(Breadcrumbs + "EditComments/", Configuration, $"======= Enter Comments to Report {ItemType} {ItemId} =========[Current User: {MyAccount["accountName"]}]=", $"{comments}");
            }
            var result = CodaClient.Vote(ItemType, ItemId, vote, comments);
            if (Convert.ToBoolean(result["result"]))
            {
                Pause(msg);
            }
            else
            {
                Pause("An error occurred.  Please try again or report the issue to The Parallel Revolution via GitHub CodaEA project.  Press ENTER to continue.");
            }
        }

    }
}
