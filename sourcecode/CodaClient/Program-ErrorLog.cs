/*----------------------------------------------------------------
 * Functions related to managing Error Log entries
 */

using CodaRESTClient;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace codaclient.classes
{
    public partial class Program
    {
        private enum ErrorQueryModeEnum
        {
            Direct,
            Unanalyzed,
            Unsolved
        }

        /// <summary>
        /// Provides noninteractive update of error code; user must have EE badge for that network
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="Args"></param>
        /// <param name="CodaClient"></param>
        /// <param name="MyAccount"></param>
        private static void ErrorUpdate(JObject Configuration, string[] Args, CodaRESTClient.Client CodaClient)
        {
            string errId = String.Empty;
            string meaning = String.Empty;
            int severity = -1;
            foreach (var arg in Args)
            {
                if (arg.StartsWith("--meaning"))
                {
                    meaning = arg[10..];
                }
                else if (arg.StartsWith("--severity"))
                {
                    severity = Convert.ToInt32(arg[11..]);
                    if (severity < 1)
                    {
                        severity = 0;
                    }
                    else if (severity > 3)
                    {
                        severity = 3;
                    }
                }
                else if (arg.StartsWith("--error"))
                {
                    errId = arg[10..];
                }
            }
            if (meaning == String.Empty && severity == -1)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "EUP-0001", "Severity and Meaning not specified, nothing to do", ErrorLogSeverityEnum.Warning);
                Console.WriteLine("Usage: ");
                Console.WriteLine("   codaclient --errorupdate --error={Error ID} --meaning=\"Accepted meaning\" --severity={1|2|3}");
                return;
            }
            if (errId == String.Empty)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "EUP-0002", "Error ID not specified, nothing to do", ErrorLogSeverityEnum.Warning);
                Console.WriteLine("Error ID not specified.  Usage: ");
                Console.WriteLine("   codaclient --errorupdate --error={Error ID} --meaning=\"Accepted meaning\" --severity={1|2|3}");
                return;
            }
            var result = CodaClient.UpdateError(errId, $"{Configuration["network"]}", severity, meaning);
            if (result.ContainsKey("code"))
            {
                LogError(Configuration, result);
            }
        }


        private static void ErrorQuery(string Breadcrumbs, JObject Configuration, string[] Args, CodaRESTClient.Client CodaClient, JObject MyAccount)
        {
            var mode = ErrorQueryModeEnum.Direct;
            var args = Args.ToList<string>();
            if (args.Contains("--unanalyzed") || args.Contains("--uz"))
            {
                mode = ErrorQueryModeEnum.Unanalyzed;
            }
            else if (args.Contains("--unanswered") || args.Contains("--ua"))
            {
                mode = ErrorQueryModeEnum.Unsolved;
            }
            if (mode == ErrorQueryModeEnum.Direct)
            {
                var errorCode = String.Empty;
                foreach (var arg in Args)
                {
                    if (arg.StartsWith("--code"))
                    {
                        errorCode = arg[7..];
                        break;
                    }
                }
                if (String.IsNullOrEmpty(errorCode))
                {
                    Console.WriteLine("Invalid query.  Use --code={error-code} to specify an individual error, or --unanalyzed or --unanswered to find related Errors.");
                    Pause(null);
                    return;
                }
                var error = CodaClient.GetError(errorCode, $"{Configuration["network"]}", true);
                if (error.ContainsKey("code"))
                {
                    ShowErrorMessage(error);
                    return;
                }
                DisplayError(Breadcrumbs + "Error/", Configuration, error, CodaClient, MyAccount);
            }
            else
            {
                JArray? errors = mode switch
                {
                    ErrorQueryModeEnum.Unanalyzed => CodaClient.GetUnanalyzedErrors($"{Configuration["network"]}", true),
                    ErrorQueryModeEnum.Unsolved => CodaClient.GetUnresolvedErrors($"{Configuration["network"]}", true),
                    _ => null
                };
                if (errors is null)
                {
                    Pause("Something went wrong, please try again, press ENTER to continue");
                    return;
                }
                else if (errors.Count == 0)
                {
                    Pause("No results returned, press ENTER to continue");
                }
                else
                {
                    DisplayErrors(Breadcrumbs, Configuration, errors, CodaClient, MyAccount, mode);
                }
            }
        }

        private static void DisplayErrors(string Breadcrumbs, JObject Configuration, JArray Errors, Client CodaClient, JObject MyAccount, ErrorQueryModeEnum Mode)
        {
            var menu = "V)iew [#] {enter # to view} Q)uit";
            string input;
            do
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                if (Mode == ErrorQueryModeEnum.Unanalyzed)
                {
                    CConsole.WriteLine($"{"======= UNANALYZED ERRORS FOR":cyan} ({Configuration["network"]:blue}) {"=========[Current User:":cyan} {MyAccount["accountName"]:blue}{"}]=":cyan}");
                }
                else
                {
                    CConsole.WriteLine($"{"======= UNSOLVEDED ERRORS FOR":cyan} ({Configuration["network"]:blue}) {"=========[Current User:":cyan} {MyAccount["accountName"]:blue}{"}]=":cyan}");
                }
                ShowErrors(Errors);
                input = ShowMenu(Configuration, menu);
                if (input.ToUpper().StartsWith("V"))
                {
                    var parts = input.Split(' ');
                    try
                    {
                        int errNum = Convert.ToInt32(parts[1]);
                        DisplayError(Breadcrumbs + "ViewError/", Configuration, (JObject)Errors[errNum - 1], CodaClient, MyAccount);
                    }
                    catch
                    {
                        Pause("Invalid Error index.  Press ENTER to continue.");
                    }
                }
            } while (input.ToUpper() != "Q");
        }

        private static void ShowErrors(JArray errors)
        {
            int i = 0;
            foreach (JObject error in errors)
            {
                CConsole.WriteLine($"{" " + (++i) + " ":black:white}. {error["errorId"]} - {error["description"]}");
            }
        }

        private static void DisplayError(string Breadcrumbs, JObject Configuration, JObject Error, CodaRESTClient.Client CodaClient, JObject MyAccount)
        {
            var menu = "D)iscussion T) Troubleshooting steps S)ubscribe/Unsubscribe O)ptions";
            if (CodaClient.AccountHasBadge(MyAccount, "EE", $"{Configuration["network"]}"))
            {
                menu += " E)dit";
            }
            menu += " Q)uit";
            string input;
            do
            {
                ShowError(Breadcrumbs, Error, MyAccount);
                Console.WriteLine();
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "D":
                        Discussion(Breadcrumbs + "Discussion/", Configuration, Error, CodaClient, MyAccount);
                        break;
                    case "T":
                        Troubleshoot(Breadcrumbs + "Troubleshoot/", Configuration, Error, CodaClient, MyAccount);
                        break;
                    case "E":
                        EditError(Breadcrumbs + "EditError/", Configuration, Error, CodaClient, MyAccount);
                        break;
                    case "S":
                        ToggleSubscribe(Configuration, Error, CodaClient, MyAccount);
                        break;
                    case "O":
                        EditOptions(Breadcrumbs + "Options/", Configuration, CodaClient, MyAccount);
                        break;
                }
            } while (input.ToUpper() != "Q");
        }

        private static void ToggleSubscribe(JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount)
        {
            var isSubscribed = Client.IsSubscribed(MyAccount, $"{Error["network"]}", $"{Error["errorId"]}");
            string response = isSubscribed switch
            {
                true => YesNo(Configuration, "Do you wish to UNsubscribe from this Error Message?  You will no longer be notified of activity on the item."),
                false => YesNo(Configuration, "Do you wish to Subscribe to notifications of updates to this item?")
            };
            JObject result;
            if (response.ToUpper() == "Y")
            {
                if (isSubscribed)
                {
                    result = CodaClient.Unsubscribe($"{Error["network"]}", $"{Error["errorId"]}");
                }
                else
                {
                    result = CodaClient.Subscribe($"{Error["network"]}", $"{Error["errorId"]}");
                }
                if (result.ContainsKey("code"))
                {
                    ShowErrorMessage(result);
                }
                else
                {
                    Pause("Your subscription is changed.  Please wait at least 60 seoonds an re-query your account to get the latest updates.  Press ENTER to continue.");
                }
            }
        }

        private static void EditError(string Breadcrumbs, JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount)
        {
            var body = $"{Error["acceptedMeaning"]}";
            string input;
            int severity = Convert.ToInt32(Error["severity"]);
            do
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                CConsole.WriteLine($"{"======= ERROR ID":cyan} {Error["errorId"]:blue} {"(":cyan}{Error["network"]:blue}{") =========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
                CConsole.Write($" {"Severity:":cyan} ");
                if (severity == 0)
                {
                    CConsole.WriteLine($"{severity:darkyellow} (Unanalyzed)");
                }
                else if (severity == 1)
                {
                    CConsole.WriteLine($"{severity:red} (Critical)");
                }
                else if (severity == 2)
                {
                    CConsole.WriteLine($"{severity:yellow} (Important)");
                }
                else
                {
                    CConsole.WriteLine($"{severity:white} (Nominal)");
                }
                CConsole.WriteLine($"{body}");
                CConsole.WriteLine($"{"--------------------------------":cyan}");
                input = ShowMenu(Configuration, "M)eaning Edit V) Severity Assignment S)ave Q)uit/Cancel");
                switch (input.ToUpper())
                {
                    case "M":
                        body = EditBody(Breadcrumbs + "EditBody/", Configuration, $"======= ERROR ID {Error["errorId"]} ({Error["network"]}) =========[Current User: {MyAccount["accountName"]}]=", body);
                        break;
                    case "V":
                        input = ShowMenu(Configuration, "1) Critical {very bad} 2) Important 3) Nominal {can be ignored}");
                        severity = Convert.ToInt32(input);
                        break;
                    case "S":
                        var result = CodaClient.UpdateError($"{Error["errorId"]}", $"{Error["network"]}", severity, body);
                        if (result.ContainsKey("code"))
                        {
                            ShowErrorMessage(result);
                        }
                        else
                        {
                            Pause("Your changes have been saved.  Please wait at least 60 seconds and requery to see updates. Press ENTER to continue.");
                        }
                        break;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "S");
        }

        private static void Troubleshoot(string Breadcrumbs, JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount)
        {
            int tnum = 1;
            string menu, input;
            var tshoot = (JArray)Error["troubleshooting"]!;
            do
            {
                menu = "P)ost Entry C)omments on this Post";
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                CConsole.WriteLine($"{"======= ERROR ID":cyan} {Error["errorId"]:blue} {"(":cyan}{Error["network"]:blue}{") =========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
                if (tshoot is null || tshoot.Count == 0)
                {
                    CConsole.WriteLine($"  {"NO TROUBLESHOOTING ENTRIES YET":red}");
                }
                else
                {
                    var ts = (JObject)tshoot[tnum - 1];
                    ShowTroubleshoot(Breadcrumbs, ts, tnum, tshoot.Count, MyAccount, CodaClient);
                    Console.WriteLine();
                    if (tnum > 1)
                    {
                        menu += " <)Previous";
                    }
                    if (tnum < tshoot.Count)
                    {
                        menu += " >)Next";
                    }
                    if ($"{ts["createdById"]}" == $"{MyAccount["accountId"]}")
                    {
                        menu += " E)dit D)elete";
                    }
                    else if (CodaClient.AccountHasBadge(MyAccount, "MD", $"{Configuration["network"]}"))
                    {
                        menu += " E)dit";
                    }
                    bool isModerator = CodaClient.AccountHasBadge(MyAccount, "MD", $"{Configuration["network"]}");
                    bool isNotAuthor = $"{MyAccount["accountId"]}" != $"{ts["createdById"]}";
                    if (isModerator && isNotAuthor)
                    {
                        menu += " M)oderate Item";
                    }
                }
                menu += " Q)uit";
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "<":
                        tnum--;
                        break;
                    case ">":
                        tnum++;
                        break;
                    case "P":
                        PostTroubleshoot(Breadcrumbs + "PostTshoot/", Configuration, Error, CodaClient, MyAccount);
                        break;
                    case "C":
                        CommentsOnTroubleshoot(Breadcrumbs + "PostComment/", Configuration, (JObject)tshoot![tnum - 1], CodaClient, MyAccount);
                        break;
                    case "E":
                        EditTroubleshoot(Breadcrumbs + "EditTshoot/", Configuration, (JObject)tshoot![tnum - 1], CodaClient);
                        break;
                    case "D":
                        Pause("Not implemented yet, press ENTER");
                        break;
                    case "M":
                        ModerateItem(Breadcrumbs + "Moderate/", Configuration, (JObject)tshoot![tnum - 1], CodaClient, MyAccount);
                        break;
                }
            } while (input.ToUpper() != "Q");
        }

        private static void EditTroubleshoot(string Breadcrumbs, JObject Configuration, JObject Troubleshoot, Client CodaClient)
        {
            var body = $"{Troubleshoot["steps"]}";
            var comments = String.Empty;
            JArray? links = null;
            try { links = (JArray?)Troubleshoot["links"]; } catch { }
            var menu = "B)ody Edit L)inks Edit C)omments [required] R)edisply S)ave Q)uit/cancel";
            string input;
            do
            {
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "B":
                        body = EditBody(Breadcrumbs + "EditBody/", Configuration, $"======== Troubleshoot ID {Troubleshoot["troubleshootId"]} Body ========", body);
                        break;
                    case "C":
                        comments = EditBody(Breadcrumbs + "EditComments/", Configuration, $"======== Comments for this Modification ========", comments);
                        break;
                    case "R":
                        ShowTshootEdits(Breadcrumbs + "ShowEdits/", Troubleshoot, body, comments, links!);
                        break;
                    case "L":
                        links = LinksEdit(Breadcrumbs + "EditLinks/", Configuration, links!);
                        break;
                    case "S":
                        var result = CodaClient.UpdateTroubleshooting(Convert.ToInt64(Troubleshoot["troubleshootId"]), body, comments, links!);
                        if (result.ContainsKey("code"))
                        {
                            ShowErrorMessage(result);
                        }
                        else
                        {
                            Pause("Your updates were saved.  Please wait 60 seconds and re-query to see the new changes.  Press ENTER to continue.");
                        }
                        break;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "Q");

            static void ShowTshootEdits(string Breadcrumbs, JObject Troubleshoot, string body, string comments, JArray links)
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                CConsole.WriteLine($"{"======== Troubleshoot ID":cyan} {Troubleshoot["troubleshootId"]:blue} {"Edits ========":cyan}");
                Console.WriteLine(body);
                CConsole.WriteLine($"{"-----------------------------------------------------------":cyan}");
                Console.WriteLine(comments);
                ShowLinks(links);
                CConsole.WriteLine($"{"-----------------------------------------------------------":cyan}");
            }
        }

        private static void CommentsOnTroubleshoot(string Breadcrumbs, JObject Configuration, JObject Troubleshoot, Client CodaClient, JObject MyAccount)
        {
            var comments = (JArray?)Troubleshoot["discussion"];
            int cmtNum = 1;
            string input;
            do
            {
                var menu = "P)ost Comment ";
                JObject? cmt = null;
                if (comments == null || comments.Count == 0)
                {
                    CConsole.WriteLine($"      {"NO COMMENTS YET":red}");
                }
                else
                {
                    cmt = (JObject)comments[cmtNum - 1];
                    if (cmtNum > 1)
                    {
                        menu += " <) Previous";
                    }
                    if (cmtNum < comments.Count)
                    {
                        menu += " >) Next";
                    }
                    if ($"{cmt["createdById"]}" == $"{MyAccount["accountId"]}")
                    {
                        menu += " E)dit D)elete";
                    }
                    else if (CodaClient.AccountHasBadge(MyAccount, "MD", $"{Configuration["network"]}"))
                    {
                        menu += " D)elete";
                    }
                    menu += " V)ote/Report";
                    bool isModerator = CodaClient.AccountHasBadge(MyAccount, "MD", $"{Configuration["network"]}");
                    bool isNotAuthor = $"{MyAccount["accountId"]}" != $"{Troubleshoot["createdById"]}";
                    if (isModerator && isNotAuthor)
                    {
                        menu += " M)oderate Item";
                    }
                    ShowComment(Breadcrumbs, cmt, Troubleshoot, cmtNum, comments.Count, CodaClient, MyAccount);
                }
                menu += " Q)uit";
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "<":
                        cmtNum--;
                        break;
                    case ">":
                        cmtNum++;
                        break;
                    case "P":
                        PostComment(Breadcrumbs + "PostComment/", Configuration, Troubleshoot, CodaClient, MyAccount);
                        break;
                    case "E":
                        EditComment(Breadcrumbs + "EditComment/", Configuration, cmt!, CodaClient);
                        break;
                    case "D":
                        DeleteComment(Configuration, cmt!, CodaClient);
                        break;
                    case "V":
                        VoteItem(Breadcrumbs + "Vote/", Configuration, CodaObjectTypeEnum.TroubleshootComment, Convert.ToInt64(cmt!["commentId"]), CodaClient, MyAccount);
                        break;
                    case "M":
                        ModerateItem(Breadcrumbs + "Moderate/", Configuration, cmt!, CodaClient, MyAccount);
                        break;
                }
            } while (input.ToUpper() != "Q");
        }

        private static void DeleteComment(JObject Configuration, JObject Comment, Client CodaClient)
        {
            string input = YesNo(Configuration, "Are you sure you want to delete the comment? ");
            if (input.ToUpper() == "Y")
            {
                var result = CodaClient.DeleteComment(Convert.ToInt64(Comment["commentId"]));
                if (result.ContainsKey("code"))
                {
                    ShowErrorMessage(result);
                }
                else
                {
                    Pause("The comment was deleted.  You will need to wait a minute and re-query to see the updates.  Press ENTER to continue.");
                }
            }
        }

        private static void EditComment(string Breadcrumbs, JObject Configuration, JObject Comment, Client CodaClient)
        {
            var body = $"{Comment["body"]}";
            var menu = "B)ody Edit L)inks Edit R)edisplay S)ave Q)uit";
            JArray? links = null;
            try { links = (JArray?)Comment["links"]; } catch { }
            string input;
            do
            {
                ShowBreadcrumbs(Breadcrumbs);
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "B":
                        body = EditBody(Breadcrumbs + "EditBody/", Configuration, "===== Troubleshooting Comment =====", body);
                        break;
                    case "L":
                        if (links is null)
                        {
                            links = new JArray();
                        }
                        links = LinksEdit(Breadcrumbs + "EditLinks/", Configuration, links);
                        break;
                    case "R":
                        Console.Clear();
                        ShowBreadcrumbs(Breadcrumbs);
                        CConsole.WriteLine($"{"===== Troubleshooting Comment =====":cyan}");
                        CConsole.WriteLine($"{body}");
                        ShowLinks(links);
                        CConsole.WriteLine($"{"--------------------------":cyan}");
                        break;
                    case "S":
                        var result = CodaClient.UpdateComment(Convert.ToInt64(Comment["commentId"]), body, links);
                        if (result.ContainsKey("code"))
                        {
                            ShowErrorMessage(result);
                        }
                        else
                        {
                            Pause("Update successful.  You will need to wait a minute and re-query to see the updates.  Press ENTER to continue.");
                        }
                        break;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "Q");
        }

        private static void ShowComment(string Breadcrumbs, JObject Comment, JObject Troubleshoot, int Cnum, int Ccount, Client CodaClient, JObject MyAccount)
        {
            var writer = CodaClient.GetAccountInfo(Convert.ToInt64(Comment["createdById"]));
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            CConsole.WriteLine($"{"======= TROUBLESHOOT ID ":cyan}{Troubleshoot["troubleshootId"]:blue}{" - ":cyan}{Troubleshoot["errorId"]:blue} {"(":cyan}{Troubleshoot["network"]:blue}{") =========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
            CConsole.WriteLine($"                                        Comment {Cnum} / {Ccount}");
            CConsole.WriteLine($" {"(Comment ID:":cyan} {Comment["commentId"]})");
            CConsole.WriteLine($" {"Written By:":cyan}   {writer["accountName"]} ({writer["reputation"]}) (ID:{writer["accountId"]})  {Comment["dateCreated"]}");
            if (Comment.ContainsKey("modifiedById"))
            {
                if ($"{Comment["modifiedById"]}" != "-1")
                {
                    writer = CodaClient.GetAccountInfo(Convert.ToInt64(Comment["modifiedById"]));
                    if (!writer.ContainsKey("code"))
                    {
                        CConsole.WriteLine($" {"Modified By:":cyan}  {writer["accountName"]} ({writer["reputation"]}) (ID:{writer["accountId"]})  {Comment["modifiedDate"]}");
                    }
                }
            }
            CConsole.WriteLine();
            CConsole.WriteLine($"{Comment["body"]}");
            CConsole.WriteLine();
            if (Comment.ContainsKey("links"))
            {
                JArray? links = null;
                try { links = (JArray?)Comment["links"]; } catch { }
                ShowLinks(links);
                CConsole.WriteLine();
            }
        }

        private static void ShowTroubleshoot(string Breadcrumbs, JObject Troubleshoot, int Tnum, int Tcount, JObject MyAccount, Client CodaClient)
        {
            var writer = CodaClient.GetAccountInfo(Convert.ToInt64(Troubleshoot["createdById"]));
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            CConsole.WriteLine($"{"======= TROUBLESHOOT ID":cyan} {Troubleshoot["troubleshootId"]:blue} {"-":cyan} {Troubleshoot["errorId"]:blue} {"(":cyan}{Troubleshoot["network"]:blue}{") =========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
            var info = $"Troubleshoot {Tnum} / {Tcount}";
            CConsole.WriteLine($"                                     {info:cyan}");
            CConsole.WriteLine($" {"Written By:":cyan}   {writer["accountName"]} {"(":cyan}{writer["reputation"]}{") (ID:":cyan}{writer["accountId"]}{")":cyan}  {Troubleshoot["dateCreated"]}");
            if (Troubleshoot.ContainsKey("modifiedById"))
            {
                if ($"{Troubleshoot["modifiedById"]}" != "-1")
                {
                    writer = CodaClient.GetAccountInfo(Convert.ToInt64(Troubleshoot["modifiedById"]));
                    if (!writer.ContainsKey("code"))
                    {
                        CConsole.WriteLine($" {"Modified By:":cyan}  {writer["accountName"]} ({writer["reputation"]}) (ID:{writer["accountId"]})  {Troubleshoot["modifiedDate"]}");
                    }
                }
            }
            CConsole.WriteLine($" {"Upvotes:":cyan}      {Troubleshoot["upvotes"]}  {"/":cyan} Downvotes: {Troubleshoot["downvotes"]}   {"Version:":cyan} {Troubleshoot["version"]}");
            CConsole.WriteLine($"{Troubleshoot["steps"]}");
            CConsole.WriteLine($"{"---------------------------":cyan}");
            JArray? discussion = null;
            try { discussion = (JArray)Troubleshoot["discussion"]!; } catch { }
            if (discussion is null)
            {
                CConsole.WriteLine($" {"Comments:":cyan} 0");
            }
            else
            {
                CConsole.WriteLine($" {"Comments:":cyan} {discussion.Count}");
            }
            JArray? links = null;
            try { links = (JArray?)Troubleshoot["links"]; } catch { }
            if (links is not null)
            {
                ShowLinks(links);
            }
            CConsole.WriteLine();
        }

        private static void Discussion(string Breadcrumbs, JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount)
        {
            int discNum = 1;
            string menu, input;
            var discussion = (JArray)Error["discussion"]!;
            do
            {
                JObject? disc = null;
                menu = "P)ost Entry";
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                CConsole.WriteLine($"{"======= ERROR ID":cyan} {Error["errorId"]:blue} {"(":cyan}{Error["network"]:blue}{") =========[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
                if (discussion is null || discussion.Count == 0)
                {
                    CConsole.WriteLine($"  {"NO DISCUSSION ENTRIES YET":yellow}");
                }
                else
                {
                    menu += " V)ote / Report";
                    disc = (JObject)discussion[discNum - 1];
                    bool isModerator = CodaClient.AccountHasBadge(MyAccount, "MD", $"{Configuration["network"]}");
                    bool isNotAuthor = $"{MyAccount["accountId"]}" != $"{disc["createdById"]}";
                    if (isModerator && isNotAuthor)
                    {
                        menu += " M)oderate Item";
                    }
                    ShowDiscussion(Breadcrumbs, disc, discNum, discussion.Count, CodaClient);
                    Console.WriteLine();
                    if (discNum > 1)
                    {
                        menu += "<)Previous";
                    }
                    if (discNum < discussion.Count)
                    {
                        menu += " >)Next";
                    }
                    if ($"{disc["createdById"]}" == $"{MyAccount["accountId"]}")
                    {
                        menu += " E)dit D)elete";
                    }
                    else if (CodaClient.AccountHasBadge(MyAccount, "MD", $"{Configuration["network"]}"))
                    {
                        menu += " D)elete M)oderate";
                    }
                }
                menu += " Q)uit";
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "<":
                        discNum--;
                        break;
                    case ">":
                        discNum++;
                        break;
                    case "P":
                        PostDiscussion(Breadcrumbs + "PostDiscussion/", Configuration, Error, CodaClient, MyAccount);
                        break;
                    case "E":
                        EditDiscussion(Breadcrumbs + "EditDicsussion/", Configuration, disc!, CodaClient, MyAccount);
                        break;
                    case "D":
                        DeleteDiscussion(Configuration, Error, discNum, CodaClient);
                        break;
                    case "V":
                        VoteItem(Breadcrumbs + "Vote/", Configuration, CodaObjectTypeEnum.ErrorLogDiscussion, Convert.ToInt64(disc!["discussionId"]), CodaClient, MyAccount);
                        break;
                    case "M":
                        ModerateItem(Breadcrumbs + "Moderate/", Configuration, disc!, CodaClient, MyAccount);
                        break;
                }
            } while (input.ToUpper() != "Q");
        }

        private static void EditDiscussion(string Breadcrumbs, JObject Configuration, JObject Discussion, Client CodaClient, JObject MyAccount)
        {
            var menu = "B)ody Edit L)inks Edit R)edisplay Edits S)ave Q)uit/cancel";
            var body = $"{Discussion["body"]}";
            JArray? links = null;
            try { links = (JArray?)Discussion["links"]; } catch { }
            string input;
            do
            {
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "B":
                        body = EditBody(Breadcrumbs + "EditBody/", Configuration, $"========= Discussion ID {Discussion["discussionId"]} Body =========", body);
                        break;
                    case "L":
                        if (links is null)
                        {
                            links = new JArray();
                        }
                        links = LinksEdit(Breadcrumbs + "EditLinks/", Configuration, links);
                        break;
                    case "R":
                        ShowDiscussionEdit(Breadcrumbs, Discussion, body, links, MyAccount);
                        break;
                    case "S":
                        var result = CodaClient.UpdateDiscussion(Convert.ToInt64(Discussion["discussionId"]), body, links);
                        if (result.ContainsKey("code"))
                        {
                            ShowErrorMessage(result);
                        }
                        else
                        {
                            Pause("Your updates have been saved.  Please wait 60 seconds and re-query to see the updates.  Press ENTER to continue.");
                        }
                        break;
                }
            } while (input.ToUpper() != "Q" && input.ToUpper() != "S");

            static void ShowDiscussionEdit(string Breadcrumbs, JObject Discussion, string Body, JArray? Links, JObject MyAccount)
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                CConsole.WriteLine($"{"========= Discussion ID":cyan} {Discussion["discussionId"]:blue} {"Body =======[Current User: ":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
                Console.WriteLine(Body);
                CConsole.WriteLine($"{"-----------------------------------------":cyan}");
                ShowLinks(Links);
            }
        }

        private static void DeleteDiscussion(JObject Configuration, JObject Error, int DiscNum, Client CodaClient)
        {
            Console.WriteLine();
            var result = YesNo(Configuration, "Are you sure you want to delete this post?");
            if (result.ToUpper() == "Y")
            {
                result = YesNo(Configuration, "This cannot be undone.  Are you ABSOLUTELY certain?");
                if (result.ToUpper() == "Y")
                {
                    var discArray = (JArray?)Error["discussion"];
                    if (discArray is not null)
                    {
                        var disc = (JObject)discArray[DiscNum - 1];
                        var response = CodaClient.DeleteDiscussion(Convert.ToInt64(disc["discussionId"]));
                        if (response.ContainsKey("code"))
                        {
                            ShowErrorMessage(response);
                        }
                        else
                        {
                            Pause("Your entry has been deleted.  Please wait a couple of minutes for the server cache to reset, and re-query to see the updated results.  Press ENTER to continue.");
                        }
                    }
                }
            }
        }

        private static void PostDiscussion(string Breadcrumbs, JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount)
        {
            PostItem(Breadcrumbs, Configuration, Error, CodaClient, MyAccount, CodaObjectTypeEnum.ErrorLogDiscussion);
        }

        private static void PostTroubleshoot(string Breadcrumbs, JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount)
        {
            PostItem(Breadcrumbs, Configuration, Error, CodaClient, MyAccount, CodaObjectTypeEnum.TroubleshootSolution);
        }

        private static void PostComment(string Breadcrumbs, JObject Configuration, JObject Troubleshoot, Client CodaClient, JObject MyAccount)
        {
            PostItem(Breadcrumbs, Configuration, Troubleshoot, CodaClient, MyAccount, CodaObjectTypeEnum.TroubleshootComment);
        }

        private static void PostItem(string Breadcrumbs, JObject Configuration, JObject Error, Client CodaClient, JObject MyAccount, CodaObjectTypeEnum ObjectType)
        {
            var body = String.Empty;
            var menu = "B)ody Edit L)inks Edit R)edisplay S)ave Q)uit";
            var links = new JArray();
            string input;
            do
            {
                ShowBreadcrumbs(Breadcrumbs);
                input = ShowMenu(Configuration, menu);
                switch (input.ToUpper())
                {
                    case "B":
                        body = EditBody(Breadcrumbs + "EditBody/", Configuration, $"======= ERROR ID {Error["errorId"]} ({Error["network"]}) =========[Current User: {MyAccount["accountName"]}]=", body);
                        break;
                    case "L":
                        links = LinksEdit(Breadcrumbs + "EditLinks/", Configuration, links);
                        break;
                    case "R":
                        Console.Clear();
                        Console.WriteLine(body);
                        ShowLinks(links);
                        break;
                    case "S":
                        if (String.IsNullOrEmpty(body))
                        {
                            Console.WriteLine("Can't save!  You haven't entered any comments yet.");
                            input = String.Empty;
                        }
                        else
                        {
                            var result = new JObject()
                            {
                                ["code"] = "BadType",
                                ["message"] = $"{ObjectType} not supported"
                            };
                            if (ObjectType == CodaObjectTypeEnum.ErrorLogDiscussion)
                            {
                                result = CodaClient.AddDiscussion($"{Error["errorId"]}", $"{Error["network"]}", body, links);
                            }
                            else if (ObjectType == CodaObjectTypeEnum.TroubleshootSolution)
                            {
                                result = CodaClient.AddTroubleshooting($"{Error["errorId"]}", $"{Error["network"]}", $"{Configuration["currentVersion"]}", body, links);
                            }
                            else if (ObjectType == CodaObjectTypeEnum.TroubleshootComment)
                            {
                                result = CodaClient.AddComment(Convert.ToInt64(Error["troubleshootId"]), body, links);
                            }
                            if (result.ContainsKey("code"))
                            {
                                ShowErrorMessage(result);
                                input = String.Empty;
                            }
                            else
                            {
                                Pause("Saved.  Press ENTER to continue.");
                            }
                        }
                        break;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "Q");
        }

        private static JArray LinksEdit(string Breadcrumbs, JObject Configuration, JArray Links)
        {
            string input;
            string menu = "A)dd link D)elete [#] link S)ave Q)uit/cancel";
            var newLinks = Links;
            do
            {
                ShowBreadcrumbs(Breadcrumbs);
                ShowLinks(newLinks);
                CConsole.WriteLine($"{"---------------------------":cyan}");
                input = ShowMenu(Configuration, menu);
                if (input.ToUpper() == "A")
                {
                    newLinks.Add(InputLink());
                }
                else if (input.ToUpper().StartsWith("D"))
                {
                    int idx;
                    try
                    {
                        idx = Convert.ToInt32(input.Split(' ')[1]);
                        newLinks.RemoveAt(idx - 1);
                    }
                    catch { Console.WriteLine("Invalid number"); }
                }
                else if (input.ToUpper() == "Q")
                {
                    newLinks = Links;
                }
            } while (input.ToUpper() != "S" && input.ToUpper() != "Q");
            return newLinks;
        }

        private static JObject InputLink()
        {
            Console.Write("Enter Link Name: ");
            var name = Console.ReadLine();
            Console.Write("Enter URL: ");
            var link = Console.ReadLine();
            return new JObject()
            {
                { "display", name },
                { "link", link }
            };
        }

        private static void ShowLinks(JArray? Links)
        {
            if (Links is null)
            {
                return;
            }
            CConsole.WriteLine($"{"---------------------------":cyan}");
            int i = 0;
            foreach (JObject link in Links)
            {
                Console.WriteLine($"{++i}. {link["display"]} ({link["link"]})");
            }
        }

        private static string EditBody(string Breadcrumbs, JObject Configuration, string TitleBar, string Body)
        {
            var nano = $"{Configuration["uiOptions"]!["textEditor"]}";
            string newBody = Body;
            //nano = string.Empty; // TODO: Get shell editor working
            if (!String.IsNullOrEmpty(nano))
            {
                newBody = ShellEdit(nano, Body);
            }
            else
            {
                string input;
                do
                {
                    Console.Clear();
                    ShowBreadcrumbs(Breadcrumbs);
                    Console.WriteLine(TitleBar);
                    Console.WriteLine(newBody);
                    Console.WriteLine();
                    Console.WriteLine("===============================================================================================");
                    var menu = "A)dd more text C)lear and re-enter text S)ave Q)uit";
                    input = ShowMenu(Configuration, menu);
                    switch (input.ToUpper())
                    {
                        case "A":
                            var more = GetBodyInput();
                            newBody += "\r\n" + more;
                            break;
                        case "C":
                            newBody = GetBodyInput();
                            break;
                        case "Q":
                            newBody = Body;
                            break;
                    }
                } while (input.ToUpper() != "S" && input.ToUpper() != "Q");
            }
            return newBody;
        }

        /// <summary>
        /// Edit body text in Nano editor
        /// </summary>
        /// <param name="Body"></param>
        /// <returns></returns>
        private static string ShellEdit(string Editor, string Body)
        {
            try
            {
                var bodyfile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"{Client.PathSeparator}body_temp.txt";
                var cmd = $"{bodyfile}";
                Console.WriteLine(cmd);
                if (File.Exists(bodyfile))
                {
                    File.Delete(bodyfile);
                }
                var sw = new StreamWriter(bodyfile);
                sw.Write(Body);
                sw.Close();
                ClientUtilities.BashInteractive(Editor, cmd, !Client.TargetOS.Contains("Windows"));
                var sr = new StreamReader(bodyfile);
                var body = sr.ReadToEnd();
                sr.Close();
                return body;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.HResult}: {ex.Message}");
                return Body;
            }
        }

        private static string GetBodyInput()
        {
            var body = String.Empty;
            Console.Clear();
            Console.WriteLine("Enter your comments, as many lines as you want.  Type END on its own line to end input.");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            string line;
            do
            {
                line = $"{Console.ReadLine()}";
                if (body.Length > 0)
                {
                    body += "\r\n";
                }
                if (line != "END")
                {
                    body += line;
                }
            } while (line != "END");
            return body;
        }

        private static void ShowDiscussion(string Breadcrumbs, JObject Discussion, int Index, int Count, CodaRESTClient.Client CodaClient)
        {
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            var writer = CodaClient.GetAccountInfo(Convert.ToInt64(Discussion["createdById"]));
            var info = $"----- DISCUSSION {Index} / {Count} -----";
            CConsole.WriteLine($"{info:cyan}");
            CConsole.WriteLine($" {"(Discussion ID:":cyan} {Discussion["discussionId"]})");
            CConsole.WriteLine($" {"Written By:":cyan}   {writer["accountName"]} {"(":cyan}{writer["reputation"]}{") (ID:":cyan}{writer["accountId"]}{")":cyan}  {Discussion["dateCreated"]}");
            if (Discussion.ContainsKey("modifiedById"))
            {
                if ($"{Discussion["modifiedById"]}" != "-1")
                {
                    writer = CodaClient.GetAccountInfo(Convert.ToInt64(Discussion["modifiedById"]));
                    if (!writer.ContainsKey("code"))
                    {
                        CConsole.WriteLine($" Modified By:  {writer["accountName"]} ({writer["reputation"]}) (ID:{writer["accountId"]})  {Discussion["modifiedDate"]}");
                    }
                }
            }
            CConsole.WriteLine();
            CConsole.WriteLine($"{Discussion["body"]}");
            CConsole.WriteLine();
            if (Discussion.ContainsKey("links"))
            {
                JArray? links = null;
                try { links = (JArray?)Discussion["links"]; } catch { }
                ShowLinks(links);
            }
        }

        private static void ShowError(string Breadcrumbs, JObject Error, JObject MyAccount)
        {
            Console.Clear();
            ShowBreadcrumbs(Breadcrumbs);
            CConsole.WriteLine($"{"======= ERROR ID":cyan} {Error["errorId"]:blue} {"(":cyan}{Error["network"]:blue}{") =========[Current User:":cyan} {MyAccount["accountName"]}{"]=":cyan}");
            var anal = Convert.ToBoolean(Error["analyzed"]) switch
            {
                true => "ANALYZED",
                false => "!analyzed"
            };
            var solved = Convert.ToBoolean(Error["solved"]) switch
            {
                true => "SOLVED",
                false => "!solved"
            };
            var sub = Client.IsSubscribed(MyAccount, $"{Error["network"]}", $"{Error["errorId"]}") switch
            {
                true => "SUBSCRIBED",
                false => "!subscribed"
            };
            CConsole.WriteLine($"{"------------":cyan} {anal:yellow} {"-----":cyan} {solved:yellow} {"-----":cyan} {sub:yellow} {"--------":cyan}");
            CConsole.WriteLine($" {"Description:":cyan}         {Error["description"]}");
            CConsole.WriteLine($" {"Version Reported on:":cyan} {Error["version"]}");
            CConsole.WriteLine($" {"Accepted Meaning:":cyan}    {Error["acceptedMeaning"]}");
            CConsole.WriteLine($" {"Severity:":cyan}            {Error["acceptedSeverity"]}");
            CConsole.Write($" {"Discussion:":cyan}          ");
            var disc = (JArray?)Error["discussion"];
            if (disc is not null)
            {
                Console.WriteLine(disc.Count);
            }
            else
            {
                Console.WriteLine("0");
            }
            CConsole.Write($" {"Troubleshooting:":cyan}     ");
            disc = (JArray?)Error["troubleshooting"];
            if (disc is not null)
            {
                Console.WriteLine(disc.Count);
            }
            else
            {
                Console.WriteLine("0");
            }
        }
    }
}
