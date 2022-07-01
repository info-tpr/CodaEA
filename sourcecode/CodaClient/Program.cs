using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace codaclient.classes
{
    partial class Program
    {
        public enum ErrorLogSeverityEnum
        {
            Debug,
            Warning,
            Error
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowUsage();
                return;
            }
            if (args.ToList<string>().Contains("--?") || args.ToList<string>().Contains("--help") || args.ToList<string>().Contains("--h"))
            {
                ShowUsage();
                return;
            }
            JObject? configuration = null;
            try
            {
                LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "START-0003", $"Running on {CodaRESTClient.Client.TargetOS}.  Loading config...", ErrorLogSeverityEnum.Debug);
                // Load specified config file
                configuration = LoadConfig(args[0]);
                if (configuration == null)
                {
                    Console.WriteLine("Invalid Configuration!");
                    Environment.Exit(1);
                    return;
                }
                LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "START-0004", $"Instantiating client library: {configuration["apiserver"]}", ErrorLogSeverityEnum.Debug);
                var client = new CodaRESTClient.Client($"{configuration["apiserver"]}", $"{configuration["apikey"]}");

                LoadPlugins(configuration);

#if DEBUG
                foreach (var arg in args)
                {
                    Console.Write(arg + " ");
                }
                Console.WriteLine();
                Console.Write("Running in DEBUG mode, press ENTER to continue:");
                Console.ReadLine();
#endif

                LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "START-0003", "Getting your account...", ErrorLogSeverityEnum.Debug);

                var myAccount = GetMyAccount(configuration, client);

                if (myAccount is null)
                {
                    Environment.Exit(1);
                }
                switch (args[1])
                {
                    case "accountquery":
                    case "aq":
                        AccountQuery("AccountQuery/", configuration, args, client, myAccount);
                        break;
                    case "errorquery":
                    case "eq":
                        ErrorQuery("ErrorQuery/", configuration, args, client, myAccount);
                        break;
                    case "errorupdate":
                    case "eu":
                        ErrorUpdate(configuration, args, client);
                        break;
                    case "analyze":
                    case "az":
                        AnalyzeLogs(configuration, args, client);
                        break;
                    default:
                        ShowUsage();
                        break;
                }
                foreach (var arg in args)
                {
                    if (arg.StartsWith("--prometheusmerge") || arg.StartsWith("pm"))
                    {
                        MergePrometheusFiles(configuration, arg);
                    }
                }
                SaveConfig(args[0], configuration);
            }
            catch (Exception ex)
            {
                LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "START-0001", $"Error {ex.HResult}: {ex.Message} / {ex.InnerException}", ErrorLogSeverityEnum.Error);
                Environment.Exit(1);
            }
        }

        private static void ShowBreadcrumbs(string Breadcrumbs)
        {
            CConsole.WriteLine($"{" " + Breadcrumbs + " ":yellow:darkblue}");
        }

        /// <summary>
        /// Merges multiple Prometheus files into one
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="Arg"></param>
        private static void MergePrometheusFiles(JObject Configuration, string Arg)
        {
            try
            {
                var tokens = Arg.Split('=');
                if (File.Exists(tokens[1]))
                {
                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "MERG-0001", "Merging Prometheus stats outputs...", ErrorLogSeverityEnum.Debug);
                    var sr = new StreamReader(tokens[1]);
                    var files = sr.ReadToEnd();
                    sr.Close();
                    var filelist = JArray.Parse(files);
                    var collectedLines = new List<string>();
                    int i = 0;
                    foreach (string? file in filelist)
                    {
                        if (file is not null)
                        {
                            i++;
                            if (i < filelist.Count)
                            {
                                // Read all but the last file
                                var lines = ReadInputFile(file, 0);
                                collectedLines.AddRange(lines);
                            }
                            else
                            {
                                // Output to the last file
                                var sw = new StreamWriter(file);
                                foreach (string line in collectedLines)
                                {
                                    sw.WriteLine(line);
                                }
                                sw.Close();
                            }
                        }
                    }
                }
                else
                {
                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "MERG-0001", "Specified Merge spec file does not exist, skipping merge...", ErrorLogSeverityEnum.Warning);
                }
            }
            catch (Exception ex)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "MERG-0002", $"Merge failed, Error {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
            }
        }

        private static JObject? GetMyAccount(JObject Configuration, CodaRESTClient.Client CodaClient)
        {
            var result = CodaClient.GetMyAccountInfo(true);
            if (result.ContainsKey("code"))
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "START-0002", $"CRITICAL ERROR: Unable to retrieve your account.  {result["message"]}", ErrorLogSeverityEnum.Error);
                ShowErrorMessage(result);
                return null;
            }
            return result;

        }

        private static JObject? LoadConfig(string ConfigFile)
        {
            try
            {
                var cr = new StreamReader(ConfigFile);
                string cfg = cr.ReadToEnd();
                cr.Close();
                return JObject.Parse(cfg);
            }
            catch (Exception ex)
            {
                LogMessage(null, MethodBase.GetCurrentMethod()!.Name, "CFG-0001", $"Unable to access file '{ConfigFile}' : {ex.Message}", ErrorLogSeverityEnum.Error);
                return null;
            }
        }

        private static void SaveConfig(string ConfigFile, JObject Configuration)
        {
            var cw = new StreamWriter(ConfigFile);
            cw.Write(Configuration.ToString());
            cw.Close();
        }

        /// <summary>
        /// Outputs CodaClient log statements
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="FunctionName"></param>
        /// <param name="MessageCode"></param>
        /// <param name="Message"></param>
        /// <param name="Severity"></param>
        /// <param name="MaskConsole">If true, console stdout is squelched, and only output to log file</param>
        private static void LogMessage(JObject? Configuration, string FunctionName, string MessageCode, string Message, ErrorLogSeverityEnum Severity, bool MaskConsole = false)
        {
            if (Configuration is null)
            {
                var msg = $"{DateTime.UtcNow}\t{FunctionName}\t{MessageCode}\t{Severity}\t{Message}";
                if (!MaskConsole)
                {
                    if (Severity == ErrorLogSeverityEnum.Error)
                    {
                        CConsole.WriteLine($"{msg:red}");
                    }
                    else if (Severity == ErrorLogSeverityEnum.Warning)
                    {
                        CConsole.WriteLine($"{msg:yellow}");
                    }
                    else
                    {
                        CConsole.WriteLine($"{msg:green}");
                    }
                }
                return;
            }
            if ($"{Configuration["logging"]!["logLevel"]}" == "Off")
            {
                return;
            }
            ErrorLogSeverityEnum logSev;
            try
            {
                logSev = Enum.Parse<ErrorLogSeverityEnum>($"{Configuration["logging"]!["logLevel"]}");
            }
            catch
            {
                logSev = ErrorLogSeverityEnum.Debug;
            }
            if (logSev <= Severity)
            {
                var msg = $"{DateTime.UtcNow}\t{FunctionName}\t{MessageCode}\t{Severity}\t{Message}";
                if (!MaskConsole)
                {
                    if (Severity == ErrorLogSeverityEnum.Error)
                    {
                        CConsole.WriteLine($"{msg:red}");
                    }
                    else if (Severity == ErrorLogSeverityEnum.Warning)
                    {
                        CConsole.WriteLine($"{msg:yellow}");
                    }
                    else
                    {
                        CConsole.WriteLine($"{msg:green}");
                    }
                }
                var logFP = $"{Configuration["logging"]!["logPath"]}";
                if (!(logFP == String.Empty))
                {
                    try
                    {
                        var sw = new StreamWriter(logFP, true);
                        sw.WriteLine(msg);
                        sw.Close();
                    }
                    catch { }
                }
            }
        }


        private static void ShowUsage()
        {
            CConsole.WriteLine($"CodaEA Client for Linux, MacOS, Windows -- {"(c) 2022 The Parallel Revolution":green}");
            CConsole.WriteLine();
            CConsole.WriteLine($"{"USAGE:":white}");
            CConsole.WriteLine($"    {"codaclient.linux ":gray}{{path-to-config-file}} {{command}} {{command-options}}");
            CConsole.WriteLine($"  {"Where:":gray}");
            CConsole.WriteLine($"    {"{path-to-config-file}":white}  {"- Specifies path to JSON config file":gray}");
            CConsole.WriteLine($"    {"{command}:":white} {"(See":gray} {"https://github.com/info-tpr/CodaEA":blue} {"for full command documentation)":gray}");
            CConsole.WriteLine($"        {"accountquery | aq":white}  {"- Query your account":gray}");
            CConsole.WriteLine($"        {"analyze | az":white}       {"- Analyze application logs according to the configuration options.  This can query the":gray}");
            CConsole.WriteLine($"                             {"system Journal, or a text log file with delimiters or fixed-width columns.  Entries":gray}");
            CConsole.WriteLine($"                             {"are submitted to CodaEA, and the analysis results (meaning & severity) are stored in":gray}");
            CConsole.WriteLine($"                             {"a report file, which can be viewed with a web browser or text editor.  Metrics output":gray}");
            CConsole.WriteLine($"                             {"in Prometheus format, and email notifications can be configured to help monitor and":gray}");
            CConsole.WriteLine($"                             {"respond to critical issues.":gray}");
            CConsole.WriteLine($"        {"errorquery | eq":white}    {"- Query the CodaEA server for a specific error code, and examine its community feedback.":gray}");
            CConsole.WriteLine($"        {"errorupdate | eu":white}   {"- Update the error with specified analysis":gray}");
        }

        /// <summary>
        /// Displays the menu to the user, and gets their choice; user can only pick one of the choices presented
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="Menu"></param>
        /// <returns></returns>
            private static string ShowMenu(JObject Configuration, string Menu)
        {
            string retVal;
            var menuType = $"{Configuration["uiOptions"]!["menuType"]}".ToLower();
            string shortMenu = ShortenMenu(Menu);
            string longMenu = WordwrapMenu(Menu);
            do
            {
                if (menuType == "full")
                {
                    ShowColorMenu(longMenu + " >");
                    //Console.Write(longMenu + " >");
                }
                else
                {
                    Console.Write(shortMenu + " >");
                }
                retVal = $"{Console.ReadLine()}";
                if (retVal == "?" || retVal == String.Empty)
                {
                    if (menuType != "full")
                    {
                        //Console.WriteLine(longMenu);
                        ShowColorMenu(longMenu);
                        Console.WriteLine();
                    }
                    retVal = "/";
                }
            } while (!shortMenu.Contains(retVal[..1].ToUpper()));
            return retVal;
        }

        private static void ShowColorMenu(string Menu)
        {
            var positions = MenuItemPositions(Menu);
            CConsole.Write($"{Menu[..(positions[0] + 1)]:black:white}");
            for (int i = 1; i < positions.Length; i++)
            {
                CConsole.Write($"{Menu.Substring(positions[i - 1] + 1, positions[i] - positions[i - 1] - 1)}{Menu.Substring(positions[i], 1):black:white}");
                if (i == positions.Length - 1)
                {
                    Console.Write(Menu[(positions[i] + 1)..]);
                }
            }
        }

        /// <summary>
        /// Wraps menu to fit nicely in the screen
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        private static string WordwrapMenu(string Menu)
        {
            int pos = Console.WindowWidth;
            if (Menu.Length < pos)
            {
                return Menu;
            }
            var itemPositions = MenuItemPositions(Menu);
            // Find the break position between menu options
            int i = itemPositions.Length - 1;
            while (itemPositions[i] > pos)
            {
                i--;
            }
            return Menu[..(itemPositions[i] - 2)] + "\r\n" + Menu[(itemPositions[i] - 1)..];
        }

        /// <summary>
        /// Returns an array of menu item positions
        /// </summary>
        /// <param name="Menu"></param>
        /// <returns></returns>
        private static int[] MenuItemPositions(string Menu)
        {
            var retVal = new List<int>();
            for (int pos = 0; pos < Menu.Length; pos++)
            {
                if (Menu[pos] == ')')
                {
                    retVal.Add(pos - 1);
                }
            }
            return retVal.ToArray();
        }

        /// <summary>
        /// Displays prompt and gets a yes/no answer from the user
        /// </summary>
        /// <param name="Prompt"></param>
        /// <returns></returns>
        private static string YesNo(JObject Configuration, string Prompt)
        {
            string retVal;
            do
            {
                Console.Write($"{Prompt} ");
                retVal = ShowMenu(Configuration, "Y)es, N)o");
            } while (retVal.ToUpper() != "Y" && retVal.ToUpper() != "N");
            return retVal;
        }

        /// <summary>
        /// Used to display an error occurrence
        /// </summary>
        /// <param name="Result"></param>
        private static void ShowErrorMessage(JObject Result, bool WaitForUser = true)
        {
            CConsole.WriteLine($"{"--- AN ERROR OCCURRED ---":red}");
            CConsole.WriteLine($"Error {Result["code"]}: {Result["message"]}");
            if (WaitForUser)
            {
                CConsole.WriteLine();
                CConsole.Write($"Press {" ENTER ":black:white} to continue...");
                _ = Console.ReadLine();
            }
        }

        private static void LogError(JObject Configuration, JObject Result)
        {
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "API-0001", $"Error calling API: {Result["message"]}", ErrorLogSeverityEnum.Error);
        }

        private static string Pause(string? Prompt)
        {
            if (String.IsNullOrEmpty(Prompt))
            {
                Prompt = "Press ENTER to continue...";
            }
            Console.Write(Prompt);
            return $"{Console.ReadLine()}";
        }

        /// <summary>
        /// Returns short menu
        /// </summary>
        /// <param name="Menu"></param>
        /// <returns></returns>
        private static string ShortenMenu(string Menu)
        {
            string retVal = String.Empty;
            var s = Menu.Split(')');
            for (int i = 0; i < (s.Length - 1); i++)
            {
                var t = s[i];
                if (i == 0)
                {
                    retVal += t[..1];
                }
                else
                {
#pragma warning disable IDE0056 // Use index operator
                    retVal += t[t.Length - 1];
#pragma warning restore IDE0056 // Use index operator
                }
            }
            return retVal + "?";
        }
    }
}
