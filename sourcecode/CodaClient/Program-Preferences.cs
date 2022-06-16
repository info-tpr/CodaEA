using CodaRESTClient;
using Newtonsoft.Json.Linq;
using System;

namespace codaclient.classes
{
    public partial class Program
    {
        /// <summary>
        /// Allows editing of CodaEA Account preferences for the current account
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="CodaClient"></param>
        /// <param name="MyAccount"></param>
        private static void EditOptions(string Breadcrumbs, JObject Configuration, Client CodaClient, JObject MyAccount)
        {
            var prefs = (JObject)MyAccount["options"]!;
            var menu = "N)otification Management S)ave Q)uit/Cancel";
            string input;
            do
            {
                ShowOptions(Breadcrumbs, Configuration, prefs, MyAccount);
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "N":
                        prefs["notifications"] = EditNotifications(Breadcrumbs + "Notifications/", Configuration, (JObject)prefs["notifications"]!);
                        break;
                    case "S":
                        var result = CodaClient.UpdateAccountOptions(Convert.ToInt64(MyAccount["accountId"]), prefs);
                        if (result.ContainsKey("code"))
                        {
                            ShowErrorMessage(result);
                        }
                        else
                        {
                            Pause("Your options have been updated.  Wait at least 60 seonds and requery account to see latest.  Press ENTER to continue.");
                        }
                        break;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "Q");
        }

        private static JObject EditNotifications(string Breadcrumbs, JObject Configuration, JObject Notifications)
        {
            var menu = "C)hange Schedule E)rror Edits R)eports D)isucssions T)roubleshoots M)Comments U)p-vote N)down-vote  S)ave Q)uit/cancel";
            var nots = Notifications;
            string input;
            do
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                ShowNotificationOptions(nots);
                Console.WriteLine("----");
                Console.WriteLine("-- Refer to online documentation for description of options: https://prod.codaea.io/index.html#/operations/account_update_options");
                Console.WriteLine("----");
                input = ShowMenu(Configuration, menu);
                switch(input.ToUpper())
                {
                    case "C":
                        input = ShowMenu(Configuration, "D)aily W)eekly I)mmediate?");
                        nots["Schedule"] = input.ToUpper() switch
                        {
                            "D" => "Daily",
                            "W" => "Weekly",
                            _ => "Immediate"
                        };
                        break;
                    case "E":
                        ToggleOption(nots, "ErrorLogEdit");
                        break;
                    case "R":
                        ToggleOption(nots, "ErrorLogReport");
                        break;
                    case "D":
                        ToggleOption(nots, "Discussion");
                        break;
                    case "T":
                        ToggleOption(nots, "Troubleshoot");
                        break;
                    case "M":
                        ToggleOption(nots, "Comment");
                        break;
                    case "U":
                        ToggleOption(nots, "VoteUp");
                        break;
                    case "N":
                        ToggleOption(nots, "VoteDown");
                        break;
                    case "Q":
                        nots = Notifications;
                        break;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "Q");
            return nots;

            static void ToggleOption(JObject Nots, string Option)
            {
                Nots[Option] = Convert.ToBoolean(Nots[Option]) switch
                {
                    true => "false",
                    false => "true"
                };
            }
        }

        private static void ShowOptions(string Breadcrumbs, JObject Configuration, JObject Preferences, JObject MyAccount)
        {
            var nots = (JObject)Preferences["notifications"]!;
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            CConsole.WriteLine($"{"======= ACCOUNT OPTIONS =========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
            ShowNotificationOptions(nots);
            CConsole.WriteLine($"{"---------------- OTHER OPTIONS ----------------":cyan}");
            // sub = (JObject)Preferences["something-else"];
            // ShowSomethingElse(sub);
            CConsole.WriteLine($"                 (nothing yet)");
        }

        private static void ShowNotificationOptions(JObject NotifictionOptions)
        {
            CConsole.WriteLine($"{"------------- NOTIFICATION OPTIONS -------------":cyan}");
            CConsole.WriteLine($" {"Notification Schedule:":cyan} {NotifictionOptions["Schedule"]}");
            CConsole.WriteLine($"      {"--------------------------":cyan}");
            CConsole.WriteLine($"       {"Error Log Edits:":cyan} {NotifictionOptions["ErrorLogEdit"]}");
            CConsole.WriteLine($"      {"Error Log Report:":cyan} {NotifictionOptions["ErrorLogReport"]}");
            CConsole.WriteLine($"     {"Discussion Posted:":cyan} {NotifictionOptions["Discussion"]}");
            CConsole.WriteLine($"   {"Troubleshoot Posted:":cyan} {NotifictionOptions["Troubleshoot"]}");
            CConsole.WriteLine($"{"T-shoot Comment Posted:":cyan} {NotifictionOptions["Comment"]}");
            CConsole.WriteLine($"               {"Vote Up:":cyan} {NotifictionOptions["VoteUp"]}");
            CConsole.WriteLine($"             {"Vote Down:":cyan} {NotifictionOptions["VoteDown"]}");
        }
    }
}
