using CodaRESTClient;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace codaclient.classes
{
    partial class Program
    {
        private static void AccountQuery(string Breadcrumbs, JObject Configuration, string[] Args, CodaRESTClient.Client CodaClient, JObject myAccount)
        {
            var full = (Args.ToList<string>().Contains("--full"));
            var queryAccount = myAccount;
            if (Args.Length > 2)
            {
                try
                {
                    long queryId = Convert.ToInt64(Args[2]);
                    queryAccount = CodaClient.GetAccountInfo(queryId, full);
                    if (queryAccount.ContainsKey("code"))
                    {
                        ShowErrorMessage(queryAccount);
                        return;
                    }
                }
                catch 
                {
                    try
                    {
                        long queryId = Convert.ToInt64(Args[3]);
                        queryAccount = CodaClient.GetAccountInfo(queryId, full);
                        if (queryAccount.ContainsKey("code"))
                        {
                            ShowErrorMessage(queryAccount);
                            return;
                        }
                    }
                    catch { }
                }
            }
            DisplayAccount(Breadcrumbs, Configuration, CodaClient, myAccount, queryAccount);
        }

        private static void DisplayAccount(string Breadcrumbs, JObject Configuration, Client CodaClient, JObject MyAccount, JObject QueryAccount)
        {
            string menu = "V)iew Reputation History O)ptions";
            if (CodaClient.AccountHasBadge(MyAccount, "SA", "*") || CodaClient.AccountHasBadge(MyAccount, "OA", "*") || CodaClient.AccountHasBadge(MyAccount, "DA", $"{Configuration["network"]}"))
            {
                menu += " C)reate Account";
                // Super Admin & Dev/Org Admins who own this account...
                bool isUserAnAdmin = (CodaClient.AccountHasBadge(MyAccount, "SA", "*")) ||
                                    (($"{QueryAccount["owningAccountId"]}" == $"{MyAccount["owningAccountId"]}") &&
                                        (CodaClient.AccountHasBadge(MyAccount, "OA", "*") || CodaClient.AccountHasBadge(MyAccount, "DA", $"{Configuration["network"]}")));
                if (isUserAnAdmin)
                {
                    menu += " U)pdate D)eactivate G)en New Key A)dmin toggle";
                }
            }
            else if (MyAccount["accountId"] == QueryAccount["accountId"])
            {
                menu += " U)pdate G)en New Key";
            }
            if (CodaClient.AccountHasBadge(QueryAccount, "OR", "*") || CodaClient.AccountHasBadge(QueryAccount, "DV", $"{Configuration["network"]}"))
            {
                menu += " L)ist Owned Accounts";
            }
            menu += " P)lugin Management Q)uit";
            string userinput;
            do
            {
                ShowAccount(Breadcrumbs, Configuration, MyAccount, QueryAccount, CodaClient);
                userinput = ShowMenu(Configuration, menu);
                switch (userinput.ToUpper())
                {
                    case "A":
                        ToggleAdminAccount(Configuration, MyAccount, QueryAccount, CodaClient);
                        break;
                    case "U":
                        UpdateAccount(Breadcrumbs + "Update/", Configuration, CodaClient, QueryAccount);
                        break;
                    case "P":
                        ManagePlugins(Breadcrumbs + "Manage Plugins/", Configuration, MyAccount);
                        break;
                    case "V":
                        ShowReputationHistory(Breadcrumbs + "RepHistory/", QueryAccount);
                        break;
                    case "C":
                        UpdateAccount(Breadcrumbs + "Create/", Configuration, CodaClient, QueryAccount, true);
                        break;
                    case "D":
                        DeactivateAccount(Breadcrumbs + "Deactivate/", Configuration, CodaClient, MyAccount, QueryAccount);
                        break;
                    case "G":
                        GenerateAPIKey(Configuration, CodaClient, QueryAccount);
                        break;
                    case "L":
                        ListOwnedAccounts(Breadcrumbs + "ListOwned/", Configuration, CodaClient, MyAccount, QueryAccount);
                        break;
                    case "O":
                        EditOptions(Breadcrumbs + "EditOptions/", Configuration, CodaClient, MyAccount);
                        break;
                }
            } while (userinput.ToUpper() != "Q");
        }

        /// <summary>
        /// Determines what kind of admin badge to toggle
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="MyAccount"></param>
        /// <param name="QueryAccount"></param>
        /// <param name="CodaClient"></param>
        private static void ToggleAdminAccount(JObject Configuration, JObject MyAccount, JObject QueryAccount, Client CodaClient)
        {
            bool result = false;
            //1. No toggling on your own account
            if ($"{MyAccount["accountId"]}" == $"{QueryAccount["accountId"]}")
            {
                Pause("You cannot toggle Admin on your own account.  Please contact info@theparallelrevolution.com for help.  Press ENTER to continue.");
                return;
            }
            if (CodaClient.AccountHasBadge(MyAccount, "OA", "*"))
            {
                result = ToggleAdminAccount(Configuration, QueryAccount, "OA", "", CodaClient);
            }
            else if (CodaClient.AccountHasBadge(MyAccount, "DA", $"{Configuration["network"]}"))
            {
                result = ToggleAdminAccount(Configuration, QueryAccount, "DA", $"{Configuration["network"]}", CodaClient);
            }
            else
            {
                var response = ShowMenu(Configuration, "O)rganization Admin D)eveloper Admin S)ystem Admin Q)uit/cancel");
                switch (response.ToUpper())
                {
                    case "O":
                        result = ToggleAdminAccount(Configuration, QueryAccount, "OA", "", CodaClient);
                        break;
                    case "D":
                        var usernetwork = Pause("Enter the network to assign for:");
                        result = ToggleAdminAccount(Configuration, QueryAccount, "DA", usernetwork, CodaClient);
                        break;
                    case "S":
                        result = ToggleAdminAccount(Configuration, QueryAccount, "SA", "", CodaClient);
                        break;
                }
            }
            if (result)
            {
                Pause("Admin rights have been toggled on this account.  Wait at least 60 seconds and re-query to see updates.  Press ENTER to continue.");
            }
            else
            {
                Pause("An error occurred - please try again.  Press ENTER to continue.");
            }
        }

        /// <summary>
        /// Toggles admin badge on the account
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="UserAccount"></param>
        /// <param name="BadgeCode"></param>
        /// <param name="Network"></param>
        private static bool ToggleAdminAccount(JObject Configuration, JObject UserAccount, string BadgeCode, string Network, Client CodaClient)
        {
            if (CodaClient.AccountHasBadge(UserAccount, BadgeCode, Network))
            {
                var result = CodaClient.UnassignBadge(Convert.ToInt64(UserAccount["accountId"]), BadgeCode, Network);
                return (!result.ContainsKey("code"));
            }
            else
            {
                var result = CodaClient.AssignBadge(Convert.ToInt64(UserAccount["accountId"]), BadgeCode, Network);
                return (!result.ContainsKey("code"));
            }
        }

        private static void ListOwnedAccounts(string Breadcrumbs, JObject Configuration, Client CodaClient, JObject MyAccount, JObject QueryAccount)
        {
            var accounts = CodaClient.GetOwnedAccountList(Convert.ToInt64(QueryAccount["owningAccountId"]));
            if (accounts == null)
            {
                Pause("No owned accounts, press ENTER to continue");
            }
            else
            {
                string input;
                do
                {
                    DisplayAccountList(Breadcrumbs, accounts);
                    Console.WriteLine();
                    input = ShowMenu(Configuration, "V)iew [#] {include item number after a space}, Q)uit");
                    if (input.ToUpper().StartsWith("V"))
                    {
                        var parts = input.Split(' ');
                        try
                        {
                            var idx = Convert.ToInt32(parts[1]);
                            DisplayAccount(Breadcrumbs + $"View {idx}/", Configuration, CodaClient, MyAccount, (JObject)accounts[idx]);
                        }
                        catch
                        {
                            Pause("Invalid number, press ENTER to continue");
                        }                    }
                } while (input.ToUpper() != "Q");
            }
        }

        private static void DisplayAccountList(string Breadcrumbs, JArray accounts)
        {
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            CConsole.WriteLine($" {"======= ACCOUNTS OWNED BY ORGANIZATION ========":cyan}");
            int i = 0;
            foreach (JObject account in accounts)
            {
                CConsole.WriteLine($"{" ":black:white}{i++:black:white}{".":black:white} {account["accountName"]} ({account["accountId"]})");
            }
        }

        private static void GenerateAPIKey(JObject Configuration, Client CodaClient, JObject QueryAccount)
        {
            var response = YesNo(Configuration, "Generating a new key will invalidate any previous keys.  Are you sure you want to do this?");
            if (response.ToUpper() == "Y")
            {
                var result = CodaClient.GenerateNewAPIKey(Convert.ToInt64(QueryAccount["accountId"]));
                if (result.ContainsKey("code"))
                {
                    ShowErrorMessage(result);
                    Pause(null);
                }
                else
                {
                    Pause($"{result["message"]}...");
                }
            }
        }

        private static void DeactivateAccount(string Breadcrumbs, JObject Configuration, CodaRESTClient.Client CodaClient, JObject MyAccount, JObject QueryAccount)
        {
            if (Convert.ToBoolean(QueryAccount["isActive"]))
            {
                ShowBreadcrumbs(Breadcrumbs);
                var response = YesNo(Configuration, "WARNING! Deactivating this account means it can no longer perform API calls.  " +
                    "Reactivation can only be accomplished by contacting The Parallel Revolution.  Are you sure you want to do this?");
                if (response.ToUpper() == "Y")
                {
                    if ($"{QueryAccount["accountId"]}" == $"{MyAccount["accountId"]}")
                    {
                        response = YesNo(Configuration, "**** WARNING!! *****  You are deactivating your own account.  You will no longer be able to log into CodaEA without contacting info@theparallelrevolution.com.  Are you sure?");
                        if (response != "Y")
                        {
                            return;
                        }
                    }
                    var result = CodaClient.DeactivateAccount(Convert.ToInt64(QueryAccount["accountId"]));
                    if (result.ContainsKey("code"))
                    {
                        ShowErrorMessage(result);
                    }
                    else
                    {
                        Pause("Success.  Press ENTER to continue.");
                    }
                }
            }
            else
            {
                Pause("Account is already inactive.  Please contact info@theparallelrevolution to get it activated again.  Press ENTER to continue.");
            }
        }

        private static void ShowReputationHistory(string Breadcrumbs, JObject QueryAccount)
        {
            ShowBreadcrumbs(Breadcrumbs);
            if (QueryAccount.ContainsKey("reputationHistory"))
            {
                var hist = (JArray)QueryAccount["reputationHistory"]!;
                foreach (JObject item in hist)
                {
                    CConsole.WriteLine($"{item["creationDate"]:cyan} -- {item["reason"]} ({item["score"]:yellow})");
                }
            }
            else
            {
                CConsole.WriteLine($"{"History not retrieved.  Try using --full option.":red:gray}");
            }
            Pause(null);
        }

        /// <summary>
        /// Menu for updating account
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="CodaClient"></param>
        /// <param name="QueryAccount"></param>
        /// <param name="Create">True if creating an account, False if updating existing account</param>
        private static void UpdateAccount(string Breadcrumbs, JObject Configuration, Client CodaClient, JObject QueryAccount, bool Create = false)
        {
            string? name = null;
            string? wallet = null;
            string? email = null;
            var menu = "N)ame W)allet E)mail, S)ave, Q)uit/cancel, which to change?";
            string userinput;
            do
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                if (Create)
                {
                    Console.WriteLine("=== CREATE NEW ACCOUNT ===");
                }
                else
                {
                    Console.WriteLine("--- UPDATE ACCOUNT ---");
                }
                if (name != null)
                {
                    Console.WriteLine($" New Name:   {name}");
                }
                if (wallet != null)
                {
                    Console.WriteLine($" New Wallet: {wallet}");
                }
                if (email != null)
                {
                    Console.WriteLine($" New Email:  {email}");
                }
                userinput = ShowMenu(Configuration, menu);
                switch(userinput.ToUpper())
                {
                    case "N":
                        Console.Write("Enter name: ");
                        name = Console.ReadLine();
                        break;
                    case "W":
                        Console.Write("Enter wallet address: ");
                        wallet = Console.ReadLine();
                        break;
                    case "E":
                        Console.Write("Enter email: ");
                        email = Console.ReadLine();
                        break;
                    case "S":
                        if (Create && (name == null || email == null || wallet == null))
                        {
                            Pause("You must enter all 3 values to create an account.  Press ENTER to continue.");
                        }
                        else
                        {
                            var acctid = Convert.ToInt64(QueryAccount["accountId"]);
                            JObject result;
                            if (Create)
                            {
                                result = CodaClient.AddNewAccount(name!, email!, wallet!);
                            }
                            else
                            {
                                result = CodaClient.UpdateAccountInfo(acctid, name, email, wallet);
                            }
                            if (result.ContainsKey("accountId"))
                            {
                                if (Create)
                                {
                                    Pause($"New account created with ID {result["accountId"]}.  Press ENTER to continue.");
                                }
                                else
                                {
                                    Pause("Account updated.  Press ENTER to continue.");
                                }
                            }
                            else
                            {
                                ShowErrorMessage(result);
                            }
                            return;
                        }
                        break;
                }
            } while (userinput.ToUpper() != "Q");
        }

        private static void ShowAccount(string Breadcrumbs, JObject Configuration, JObject MyAccount, JObject QueryAccount, Client CodaClient)
        {
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            CConsole.WriteLine($"{"======= ACCOUNT ID ":blue}{QueryAccount["accountId"]:cyan}{" =========[Current User: ":blue}{MyAccount["accountName"]:cyan:black}{"]===":blue}");
            if (!(Convert.ToBoolean(QueryAccount["isActive"])))
            {
                CConsole.Write($"{" >>>> INACTIVE <<<<":yellow}");
            }
            if (CodaClient.AccountHasBadge(QueryAccount, "SA", "*"))
            {
                CConsole.Write($"{" >>>> SYSTEM ADMIN <<<< ":yellow}");
            }
            if (CodaClient.AccountHasBadge(QueryAccount, "OA", "*"))
            {
                CConsole.Write($"{" >>>> ORG ADMIN <<<< ":yellow}");
            }
            if (CodaClient.AccountHasBadge(QueryAccount, "SA", $"{Configuration["network"]}"))
            {
                CConsole.Write($"{" >>>> DEV ADMIN <<<< ":yellow}");
            }
            Console.WriteLine();
            CConsole.WriteLine($"{" Name:        ":cyan} {QueryAccount["accountName"]}");
            CConsole.WriteLine($"{" Reputation:  ":cyan} {QueryAccount["reputation"]}");
            CConsole.WriteLine($"{" Active Since:":cyan} {QueryAccount["activeSince"]}");
            var badges = String.Empty;
            foreach (JObject badge in (JArray)QueryAccount["badges"]!)
            {
                if (badges.Length > 0)
                {
                    badges += ", ";
                }
                badges += $"{badge["badge"]}";
            }
            CConsole.WriteLine($" {"Badges:":cyan}       {badges}");
            CConsole.WriteLine($" {"Owned By:":cyan}     {QueryAccount["owningAccountId"]}");
            try { CConsole.WriteLine($" {"Email:":cyan}        {QueryAccount["email"]}"); } catch { }
            try { CConsole.WriteLine($" {"Expires:":cyan}      {QueryAccount["expireDate"]}"); } catch { }
            try { CConsole.WriteLine($" {"Wallet:":cyan}       {QueryAccount["walletAddress"]}"); } catch { }
            Console.WriteLine();
            CConsole.WriteLine($"{"============================================================":blue}");
        }
    }
}
