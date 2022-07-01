using CodaClient.Plugin;
using CodaRESTClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
//using System.Xml;
//using System.Xml.Serialization;

namespace codaclient.classes
{
    partial class Program
    {
        /// <summary>
        /// Analyzes the logs as configured
        /// </summary>
        /// <param name="configuration">Client configuration</param>
        /// <param name="args"></param>
        private static void AnalyzeLogs(JObject configuration, string[] args, CodaRESTClient.Client CodaClient)
        {
            if (!configuration.ContainsKey("analyze"))
            {
                LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "ANAL-0001", $"Missing 'analyze' section in config", ErrorLogSeverityEnum.Error);
                return;
            }
            JArray logConfigs = (JArray)configuration["analyze"]!;
            JArray logsToAnalyze;
            if (args.Length == 2)
            {
                // If no other args specified, then use the whole analyze array
                try
                {
                    logsToAnalyze = logConfigs;
                }
                catch
                {
                    LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "ANAL-0002", "`analyze` section has no configs to process", ErrorLogSeverityEnum.Error);
                    return;
                }
            }
            else
            {
                LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0001", "Analysis started", ErrorLogSeverityEnum.Debug);
                logsToAnalyze = new JArray();
                // Gather each specified log to analyze
                string[] logArgs = args[2].Split('=');
                if (logArgs[0] == "--analyze")
                {
                    string[] logs = logArgs[1].Split(',');
                    foreach (string log in logs)
                    {
                        foreach (JObject logObj in logConfigs)
                        {
                            if (logObj["name"]!.ToString() == log)
                            {
                                logsToAnalyze.Add(logObj);
                            }
                        }
                    }
                }
                else
                {
                    LogMessage(configuration, MethodBase.GetCurrentMethod()!.Name, "ANAL-0003", $"Unrecognized option '{args[2]}'", ErrorLogSeverityEnum.Error);
                    return;
                }
            }

            AnalyzeLogs(configuration, logsToAnalyze, CodaClient);
        }

        /// <summary>
        /// Analyzes the array of logs
        /// </summary>
        /// <param name="Configuration">Client configuration</param>
        /// <param name="LogsToAnalyze">Array of logs to analyze</param>
        private static void AnalyzeLogs(JObject Configuration, JArray LogsToAnalyze, CodaRESTClient.Client CodaClient)
        {
            // ErrorAnalysis is a collection of error messages grouped by category
            // Categories are:  Unanalyzed, Critical (Sev 1), Important (Sev 2), Nominal (Sev 3)
            // Load Cached Error Logs
            Dictionary<String, ErrorLogItem> errorLogs = GetCachedItems();
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0002", $"{errorLogs.Count} entries in message cache", ErrorLogSeverityEnum.Debug);
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0003", $"Analyzing {LogsToAnalyze.Count} log sources...", ErrorLogSeverityEnum.Debug);
            var errorAnalysis = new SerializableDictionary<string, ErrorAnalysis>();
            var prometheusStats = InitializePrometheusStats();
            foreach (JObject logSource in LogsToAnalyze)
            {
                AnalyzeSource(Configuration, logSource, errorAnalysis, CodaClient, errorLogs);
                CleanLogFolder(Configuration, logSource);
            }
            ReportErrorsToCoda(Configuration, CodaClient, errorAnalysis, errorLogs);
            SaveCachedItems(Configuration, errorLogs);
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0004", "Updated message cache saved.", ErrorLogSeverityEnum.Debug);
            GenerateAnalysisReport(Configuration, errorAnalysis);
            ReportNodeExporterStats(Configuration, prometheusStats, errorAnalysis);
            SendMailNotices(Configuration, errorAnalysis, CodaClient);
            Configuration["analysis"]!["lastRunDate"] = DateTime.UtcNow;
        }

        private static void CleanLogFolder(JObject Configuration, JObject logSource)
        {
            var logSettings = (JObject)logSource["inputSpecs"]!;
            if (!logSettings.ContainsKey("cleanFolder"))
            {
                return;
            }
            if (Convert.ToBoolean(logSettings["cleanFolder"]))
            {
                var logFolder = Path.GetDirectoryName($"{logSettings["inputFile"]}");
                if (logFolder is not null)
                {
                    if (!logSettings.ContainsKey("cleanSettings"))
                    {
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CLG-0002", "ERROR: Log folder cleaning enabled, but not configured with 'cleanSettings' object", ErrorLogSeverityEnum.Error);
                    }
                    else
                    {
                        var cleanSettings = (JObject)logSettings["cleanSettings"]!;
                        if (!cleanSettings.ContainsKey("cleanFilePattern") || !cleanSettings.ContainsKey("cleanAgeDays"))
                        {
                            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CLG-0005", "ERROR: Malformed 'cleanSettings' does not contain cleanAgeDays or cleanFilePattern", ErrorLogSeverityEnum.Error);
                            return;
                        }
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CLG-0001", $"Cleaning log folder '{logFolder}'", ErrorLogSeverityEnum.Debug);
                        var di = new DirectoryInfo(logFolder);
                        var files = di.EnumerateFiles($"{cleanSettings["cleanFilePattern"]}");
                        foreach (var file in files)
                        {
                            var ts = DateTime.Now.Subtract(file.LastWriteTime);
                            if (ts.TotalDays > Convert.ToInt16(cleanSettings["cleanAgeDays"]))
                            {
                                try
                                {
                                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CLG-0003", $"--- Deleting file: {file.Name}...", ErrorLogSeverityEnum.Debug);
                                    file.Delete();
                                }
                                catch (Exception ex)
                                {
                                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CLG-0004", $"WARNING: Unable to delete file: {ex.Message}", ErrorLogSeverityEnum.Warning);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reports errors to CodaEA API
        /// </summary>
        /// <param name="codaClient"></param>
        /// <param name="errorAnalysis"></param>
        private static void ReportErrorsToCoda(
            JObject Configuration,
            Client CodaClient,
            SerializableDictionary<string, ErrorAnalysis> ErrorAnalysis,
            Dictionary<String, ErrorLogItem> ErrorLogs)
        {
            foreach (var sev in ErrorAnalysis.Keys)
            {
                foreach (var err in ErrorAnalysis[sev].Errors.Values)
                {
                    try
                    {
                        var result = CodaClient.ReportError(err.ErrorCode, err.Network, $"{Configuration["currentVersion"]}", err.NumberOccurrences, err.ErrorMessage);
                        if (result.ContainsKey("code"))
                        {
                            // Handle error
                            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "RPT-0001",
                                $"CodaEA did not process error {err.ErrorCode}, result: {result.ToString(Newtonsoft.Json.Formatting.None)}", ErrorLogSeverityEnum.Error);
                        }
                        else
                        {
                            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "RPT-0002",
                                $"Error {err.ErrorCode} recorded {err.NumberOccurrences} instances with CodaEA", ErrorLogSeverityEnum.Debug);
                            err.AcceptedMeaning = $"{result["acceptedMeaning"]}";
                            err.AcceptedSeverity = Convert.ToInt32(result["acceptedSeverity"]);
                            err.DateCached = DateTime.UtcNow;
                            ErrorAnalysis[sev].Errors[$"{err.Network}-{err.ErrorCode}"] = err;
                            ErrorLogs[$"{err.Network}-{err.ErrorCode}"] = err;
                        }

                    }
                    catch (Exception ex)
                    {
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "RPT-0003",
                            $"FATAL ERROR - COULD NOT CALL CODA API.  Error {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Sends email notices if any Sev 1 or Unanalyzed issues were found
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="ErrorAnalysis"></param>
        private static void SendMailNotices(JObject Configuration, SerializableDictionary<string, ErrorAnalysis> ErrorAnalysis, Client CodaClient)
        {
            var mailConfig = (JObject)Configuration["analysis"]!;
            if (!Convert.ToBoolean(mailConfig["notification"]))
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0005", "Mail notifications are not configured, skipping.", ErrorLogSeverityEnum.Warning);
                return;
            }
            if (ErrorAnalysis.ContainsKey("1") || ErrorAnalysis.ContainsKey("0"))
            {
                var subject = "Error Analysis Has Produced Known Issues";
                var body = new StringBuilder();
                body.Append("<h1>The following errors were found on your system in the latest run:</h1><br/><br/>");
                body.Append($"Source: {Environment.MachineName}<br/>");
                string errorTable = GenerateErrorTable(ErrorAnalysis);
                body.Append(errorTable);
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0006", "Queueing Analysis email on server...", ErrorLogSeverityEnum.Debug);
                var result = CodaClient.MailMe(subject, $"{body}");
                if (result.ContainsKey("code"))
                {
                    ShowErrorMessage(result, false);
                }
            }
            else
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0015", "No severe or unanalyzed errors identified", ErrorLogSeverityEnum.Warning);
            }
        }

        /// <summary>
        /// Generates HTML table to display errors found
        /// </summary>
        /// <param name="errorAnalysis"></param>
        /// <returns></returns>
        private static string GenerateErrorTable(Dictionary<string, ErrorAnalysis> errorAnalysis)
        {
            var sb = new StringBuilder();
            sb.Append(ClientUtilities.FillTextTemplate("Mail-notification-tablehead.txt", null, CodaRESTClient.Client.PathSeparator));
            foreach (var severity in errorAnalysis.Keys)
            {
                var sev = severity switch
                {
                    "1" => "Critical",
                    "2" => "Important",
                    "3" => "Nominal",
                    _ => "Unknown"
                };
                var sectionData = new JObject()
                {
                    ["Action"] = sev,
                };
                if (errorAnalysis.ContainsKey(severity))
                {
                    sb.Append(ClientUtilities.FillTextTemplate("Mail-notification-tablesectionhead.txt", sectionData, Client.PathSeparator));
                    foreach (var err in errorAnalysis[severity].Errors.Values)
                    {
                        var meaning = String.IsNullOrEmpty(err.AcceptedMeaning) ? err.AcceptedMeaning : "Unknown";
                        var errData = new JObject()
                        {
                            ["ErrorCode"] = err.ErrorCode,
                            ["Number"] = err.NumberOccurrences,
                            ["Details"] = meaning,
                        };
                        sb.Append(ClientUtilities.FillTextTemplate("Mail-notification-tablerow.txt", errData, Client.PathSeparator));
                    }
                }
            }
            sb.Append(ClientUtilities.FillTextTemplate("Mail-notification-tablefoot.txt", null, Client.PathSeparator));

            return sb.ToString();
        }

        /// <summary>
        /// Loads item cache
        /// </summary>
        /// <returns></returns>
        private static Dictionary<String, ErrorLogItem> GetCachedItems()
        {
            var appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cacheFile = $"{appdir}{Client.PathSeparator}ErrorLog.cache";
            if (File.Exists(cacheFile))
            {
                try
                {
                    var sr = new StreamReader(cacheFile);
                    var retVal = JsonConvert.DeserializeObject<Dictionary<String, ErrorLogItem>>(sr.ReadToEnd());
                    sr.Close();
                    return retVal!;
                }
                catch
                {
                    // On error, return new empty collection
                    return new Dictionary<String, ErrorLogItem>();
                }
            }
            else
            {
                // On file not exist, return new empty collection
                return new Dictionary<String, ErrorLogItem>();
            }
        }

        /// <summary>
        /// Saves item cache
        /// </summary>
        /// <param name="CachedItems"></param>
        private static void SaveCachedItems(JObject Configuration, Dictionary<String, ErrorLogItem> CachedItems)
        {
            var appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cacheFile = $"{appdir}{Client.PathSeparator}ErrorLog.cache";
            try
            {
                var dw = new StreamWriter(cacheFile);
                dw.Write(JsonConvert.SerializeObject(CachedItems));
                dw.Close();
            }
            catch (Exception ex)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CACHE-0001", $"Unable to save cached items to `{cacheFile}'.  Error {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
            }
        }

        private static void ReportNodeExporterStats(JObject Configuration, JObject PrometheusStats, SerializableDictionary<string, ErrorAnalysis> ErrorAnalysis)
        {
            if (String.IsNullOrEmpty($"{Configuration["prometheusFile"]}"))
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0006", "Skipping Prometheus Node Exporter stats, no file specified", ErrorLogSeverityEnum.Warning);
            }
            else
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0007", "Generating Node Exporter Stats...", ErrorLogSeverityEnum.Debug);
                CalculateNodeStats(PrometheusStats, ErrorAnalysis);
                var statReport = "# HELP codaea_errors_New Errors new to CodaEA database\n";
                statReport += "# TYPE codaea_errors_New gauge\n";
                statReport += $"codaea_errors_New{{app=\"{Configuration["network"]}\"}} {PrometheusStats["newErrors"]}\n";
                statReport += "# HELP codaea_errors_Sev1 Errors determined Critical by CodaEA Community\n";
                statReport += "# TYPE codaea_errors_Sev1 gauge\n";
                statReport += $"codaea_errors_Sev1{{app=\"{Configuration["network"]}\"}} {PrometheusStats["sev1Errors"]}\n";
                statReport += "# HELP codaea_errors_Sev2 Errors determined Important by CodaEA Community\n";
                statReport += "# TYPE codaea_errors_Sev2 gauge\n";
                statReport += $"codaea_errors_Sev2{{app=\"{Configuration["network"]}\"}} {PrometheusStats["sev2Errors"]}\n";
                statReport += "# HELP codaea_errors_Sev3 Errors determined Nominal by CodaEA Community\n";
                statReport += "# TYPE codaea_errors_Sev3 gauge\n";
                statReport += $"codaea_errors_Sev3{{app=\"{Configuration["network"]}\"}} {PrometheusStats["sev3Errors"]}\n";
                var sw = new StreamWriter($"{Configuration["prometheusFile"]}");
                sw.WriteLine(statReport);
                sw.Close();
            }
        }

        /// <summary>
        /// Collects stats from analysis
        /// </summary>
        /// <param name="PrometheusStats"></param>
        /// <param name="ErrorAnalysis"></param>
        private static void CalculateNodeStats(JObject PrometheusStats, Dictionary<string, ErrorAnalysis> ErrorAnalysis)
        {
            // We iterate through all error summaries and add them up in the categories we collect
            foreach (var severity in ErrorAnalysis.Keys)
            {
                foreach (var err in ErrorAnalysis[severity].Errors.Values)
                {
                    var pro = "newErrors";
                    if (Convert.ToInt16(severity) > 0)
                    {
                        pro = $"sev{severity}Errors";
                    }
                    var cnt = Convert.ToInt32(PrometheusStats[pro]);
                    PrometheusStats[pro] = cnt + err.NumberOccurrences;
                    if (!err.IsAnalyzed)
                    {
                        cnt = Convert.ToInt32(PrometheusStats["unAnalyzedErrors"]);
                        PrometheusStats["unAnalyzedErrors"] = cnt + err.NumberOccurrences;
                    }
                    if (!err.IsSolved)
                    {
                        cnt = Convert.ToInt32(PrometheusStats["unSolvedErrors"]);
                        PrometheusStats["unSolvedErrors"] = cnt + err.NumberOccurrences;
                    }
                }
            }
        }

        private static void GenerateAnalysisReport(JObject Configuration, SerializableDictionary<string, ErrorAnalysis> ErrorAnalysis)
        {
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0008", "Generating Analysis Report...", ErrorLogSeverityEnum.Debug);
            var reportFileCfg = $"{Configuration["reportPath"]}";
            var reportFileBase = Path.GetFileNameWithoutExtension(reportFileCfg);
            var reportExt = Path.GetFileNameWithoutExtension(reportFileCfg);
            var reportFile = $"{reportFileBase}_{DateTime.Now:yyyy-MM-dd-hh-mm}.{reportExt}";
            var fw = new StreamWriter(reportFile);
            fw.Write(JObject.Parse(JsonConvert.SerializeObject(ErrorAnalysis).ToString()).ToString(Newtonsoft.Json.Formatting.Indented));
            fw.Close();
        }

        /// <summary>
        /// Initializes the stats we want to report for Prometheus Node Exporter
        /// </summary>
        /// <returns></returns>
        private static JObject InitializePrometheusStats()
        {
            return new JObject()
            {
                { "newErrors", 0 },
                { "sev1Errors", 0 },
                { "sev2Errors", 0 },
                { "sev3Errors", 0 },
                { "unAnalyzedErrors", 0 },
                { "unSolvedErrors", 0 }
            };
        }

        /// <summary>
        /// Conducts analysis on a single log source
        /// </summary>
        /// <param name="Configuration">Client configuration</param>
        /// <param name="LogSource">Individual log specs to analyze</param>
        /// <param name="ErrorAnalysis">Collected error analysis</param>
        private static void AnalyzeSource(
            JObject Configuration,
            JObject LogSource,
            Dictionary<string, ErrorAnalysis> ErrorAnalysis,
            CodaRESTClient.Client CodaClient,
            Dictionary<String, ErrorLogItem> ErrorLogs)
        {
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0009", $"Analyzing source '{LogSource["name"]}'...", ErrorLogSeverityEnum.Debug);
            List<string> inputLines;
            string inputFile = $"{LogSource["inputSpecs"]!["inputFile"]}";
            if ($"{LogSource["input"]}" == "eventlog")
            {
                // Windows Event Log
                ProcessEventLog(Configuration, LogSource, ErrorAnalysis, CodaClient, ErrorLogs);
            }
            else if ($"{LogSource["input"]}" == "journal")
            {
                // Linux SysLog Journal
                ProcessJournal(Configuration, LogSource, ErrorAnalysis, CodaClient, ErrorLogs);
            }
            else if ($"{LogSource["input"]}".StartsWith("file/"))
            {
                // All "file/" methods indicate file-level processors
                ProcessFilePlugin(Configuration, inputFile, LogSource, CodaClient, ErrorAnalysis, ErrorLogs);
            }
            else
            {
                // All other methods indicate line-level processors
                inputLines = ReadInputFile(inputFile, Convert.ToInt32(LogSource["skipLines"]));
                ProcessLines(inputLines, Configuration, LogSource, ErrorAnalysis, CodaClient, ErrorLogs);
            }
        }

        /// <summary>
        /// Processes the Linux SysLog Journal
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logSource"></param>
        /// <param name="errorAnalysis"></param>
        /// <param name="codaClient"></param>
        /// <param name="errorLogs"></param>
        /// <exception cref="Exception"></exception>
        private static void ProcessJournal(
            JObject Configuration,
            JObject LogSource,
            Dictionary<string, ErrorAnalysis> ErrorAnalysis,
            Client CodaClient,
            Dictionary<string, ErrorLogItem> ErrorLogs)
        {
            if (Client.TargetOS.Contains("Windows"))
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PROC-0003", "journal Log Sources only supported on Linux platform.", ErrorLogSeverityEnum.Error);
                return;
            }
            var journalArgs = "--output-fields _SYSTEMD_UNIT,PRIORITY,SYSLOG_IDENTIFIER,SYSLOG_TIMESTAMP,MESSAGE,MESSAGE_ID,SYSLOG_IDENTIFIER";
            // Fields to return
            journalArgs += " -p \"emerg\"..\"err\" ";                                                   // Priorities to include - emerg, alert, crit, error
            journalArgs += $" -t \"{LogSource["inputSpecs"]!["process"]}\"";                             // Reporting process
            DateTime lastRun;
            try { lastRun = Convert.ToDateTime(Configuration["analysis"]!["lastRunDate"]); }
            catch { lastRun = DateTime.UtcNow.AddDays(-7); }
            journalArgs += $"-S \"{lastRun:yyyy-MM-dd HH:mm:ss}\"";                         // Query since last date
            journalArgs += " --utc";                                                                    // Show all in UTC time
            journalArgs += " -o json";																	//Output in JSON
            var log = ClientUtilities.Bash("/usr/bin/journalctl", journalArgs, true);
            var sr = new StringReader(log);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                var logItem = ParseItem_Journal(line, $"{Configuration["network"]}");
                ErrorLogItem codaRecord = GetCodaInfo(Configuration, logItem, CodaClient, ErrorLogs);
                if (codaRecord.Source is null)
                {
                    codaRecord.Source = new JObject()
                    {
                        ["logFile"] = $"Linux SysLog - {LogSource["inputSpecs"]!["process"]}",
                        ["source"] = line,
                    };
                }
                if (!ErrorAnalysis.ContainsKey($"{codaRecord.AcceptedSeverity}"))
                {
                    var ea = new ErrorAnalysis();
                    ErrorAnalysis.Add($"{codaRecord.AcceptedSeverity}", ea);
                }
                foreach (var severity in ErrorAnalysis.Keys)
                {
                    ErrorAnalysis[severity].LogError(codaRecord, severity == $"{codaRecord.AcceptedSeverity}");
                }
            }
        }

        /// <summary>
        /// Read Windows Event Log logs for given config
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="LogSource"></param>
        /// <param name="ErrorAnalysis"></param>
        /// <param name="CodaClient"></param>
        /// <param name="ErrorLogs"></param>
        private static void ProcessEventLog(JObject Configuration, JObject LogSource, Dictionary<string, ErrorAnalysis> ErrorAnalysis, Client CodaClient, Dictionary<string, ErrorLogItem> ErrorLogs)
        {
            if (!Client.TargetOS.Contains("Windows"))
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PROC-0001", "eventlog Log Sources only supported on Windows platform.", ErrorLogSeverityEnum.Error);
                return;
            }
#pragma warning disable CA1416 // Validate platform compatibility
            DateTime lastRun;
            try
            {
                lastRun = Convert.ToDateTime(Configuration["analysis"]!["lastRunDate"]);
            }
            catch
            {
                lastRun = DateTime.UtcNow.AddDays(-7);
            }
            var qry = $"*[System/Provider/@Name=\"{LogSource["inputSpecs"]!["source"]}\" and (System/TimeCreated/@SystemTime >= '{lastRun:yyyy-MM-ddTHH:mm:ss}') and System[({LevelQuery(LogSource)})]]";
            var eq = new EventLogQuery($"{LogSource["inputSpecs"]!["eventLog"]}", PathType.LogName, qry);
            var lr = new EventLogReader(eq);
            EventRecord evt;
            do
            {
                evt = lr.ReadEvent();
                if (evt != null)
                {
                    var logItem = ParseItem_EventLog(evt, $"{Configuration["network"]}");
                    ErrorLogItem codaRecord = GetCodaInfo(Configuration, logItem, CodaClient, ErrorLogs);
                    if (codaRecord.Source is null)
                    {
                        codaRecord.Source = new JObject()
                        {
                            ["logFile"] = $"Windows {LogSource["inputSpecs"]!["eventLog"]} Event Log",
                            ["bookmark"] = $"{evt.Bookmark}",
                        };
                    }
                    if (!ErrorAnalysis.ContainsKey($"{codaRecord.AcceptedSeverity}"))
                    {
                        var ea = new ErrorAnalysis();
                        ErrorAnalysis.Add($"{codaRecord.AcceptedSeverity}", ea);
                    }
                    foreach (var severity in ErrorAnalysis.Keys)
                    {
                        ErrorAnalysis[severity].LogError(codaRecord, severity == $"{codaRecord.AcceptedSeverity}");
                    }
                }
            } while (evt != null);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        /// <summary>
        /// Converts the Event Log entry into an ErrorLogItem
        /// </summary>
        /// <param name="LogItem"></param>
        /// <param name="Network"></param>
        /// <returns></returns>
        private static ReportLogItem ParseItem_EventLog(EventRecord LogItem, string Network)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var retVal = new ReportLogItem();
            // We need to convert the event entry to JSON for ease of processing
            var xml = new XmlDocument();
            xml.LoadXml(LogItem.ToXml());
            var json = JObject.Parse(JsonConvert.SerializeXmlNode(xml));
            retVal.Network = Network;
            retVal.ErrorCode = $"{json["Event"]!["System"]!["EventID"]!["#text"]}";
            retVal.ErrorMessage = LogItem.FormatDescription();
            return retVal;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        /// <summary>
        /// Builds a severity query for event log based on source specs
        /// </summary>
        /// <param name="LogSource"></param>
        /// <returns></returns>
        private static string LevelQuery(JObject LogSource)
        {
            var lvls = (JArray)LogSource["inputSpecs"]!["severity"]!;
            var retVal = String.Empty;
            foreach (string? lvl in lvls)
            {
                switch (lvl!)
                {
                    case "critical":
                        if (retVal.Length > 0)
                        {
                            retVal += " or ";
                        }
                        retVal += "Level=1";
                        break;
                    case "error":
                        if (retVal.Length > 0)
                        {
                            retVal += " or ";
                        }
                        retVal += "Level=2";
                        break;
                    case "warning":
                        if (retVal.Length > 0)
                        {
                            retVal += " or ";
                        }
                        retVal += "Level=3";
                        break;
                    case "information":
                        if (retVal.Length > 0)
                        {
                            retVal += " or ";
                        }
                        retVal += "Level=4";
                        break;
                    case "verbose":
                        if (retVal.Length > 0)
                        {
                            retVal += " or ";
                        }
                        retVal += "Level=5";
                        break;
                    default:
                        break;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Process whole-file processor plugin
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="InputFile"></param>
        /// <param name="LogSource"></param>
        /// <param name="ErrorAnalysis"></param>
        /// <param name="CodaClient"></param>
        /// <param name="ErrorLogs"></param>
        private static void ProcessFilePlugin(
            JObject Configuration,
            string InputFile,
            JObject LogSource,
            Client CodaClient,
            Dictionary<string, ErrorAnalysis> ErrorAnalysis,
            Dictionary<string, ErrorLogItem> ErrorLogs)
        {
            if (_Plugins is null)
            {
                return;
            }
            var idParts = $"{LogSource["input"]}".Split('/');
            if (!_Plugins.ContainsKey(idParts[1]))
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "FPLG-0001", $"ERROR - Plugin '{idParts[1]}' not found", ErrorLogSeverityEnum.Error);
                return;
            }
            var pluginAssembly = _Plugins[idParts[1]];
            if (!pluginAssembly.FileProcessor)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "FPLG-0002", $"ERROR - Plugin '{idParts[1]}' is not a File Processor", ErrorLogSeverityEnum.Error);
                return;
            }
            RunPlugin(pluginAssembly, Configuration, InputFile, LogSource, CodaClient, ErrorAnalysis, ErrorLogs);
        }

        private static void RunPlugin(
                ICodaPlugin PluginAssembly,
                JObject Configuration,
                string InputFile,
                JObject LogSource,
                Client CodaClient,
                Dictionary<string, ErrorAnalysis> ErrorAnalysis,
                Dictionary<string, ErrorLogItem> ErrorLogs)
        {
            if (PluginAssembly.OpenLogFile(InputFile, LogSource))
            {
                try
                {
                    // Get list of Severities (Msg Types) to filter on
                    var msgTypeList = (JArray)LogSource["messageType"]!["values"]!;
                    List<string>? msgTypes = null;
                    if (msgTypeList is not null)
                    {
                        msgTypeList.ToObject<List<string>>();
                    }
                    // Loop through the log file
                    while (!PluginAssembly.EndOfStream)
                    {
                        // Obtain log entry
                        var logitem = PluginAssembly.NextLogItem();
                        if (logitem is not null)
                        {
                            // Incorporate log entry into error logs
                            logitem["Network"] = $"{Configuration["network"]}";
                            var logItemEntry = JsonConvert.DeserializeObject<ReportLogItem>(logitem.ToString());
                            if (logItemEntry is not null)
                            {
                                if ((msgTypes is null) || (msgTypes.Contains(logItemEntry.Severity)))
                                {
                                    var codaRecord = GetCodaInfo(Configuration, logItemEntry, CodaClient, ErrorLogs);
                                    if (!ErrorAnalysis.ContainsKey($"{codaRecord.AcceptedSeverity}"))
                                    {
                                        var ea = new ErrorAnalysis();
                                        ErrorAnalysis.Add($"{codaRecord.AcceptedSeverity}", ea);
                                    }
                                    foreach (var severity in ErrorAnalysis.Keys)
                                    {
                                        ErrorAnalysis[severity].LogError(codaRecord, severity == $"{codaRecord.AcceptedSeverity}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "FPLG-0003", $"Exception occurred while processing file '{InputFile}', {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
                }
            }
        }


        /// <summary>
        /// Reads the contents of the file line by line into a List
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private static List<string> ReadInputFile(string inputFile, int SkipLines)
        {
            var retVal = new List<string>();
            int lineNumber = 0;
            using (var sr = new StreamReader(inputFile))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    lineNumber++;
                    if (lineNumber > SkipLines)
                    {
                        retVal.Add(line);
                    }
                }
                sr.Close();
            }
            return retVal;
        }

        /// <summary>
        /// Processes the lines read from the input file
        /// </summary>
        /// <param name="inputLines"></param>
        /// <param name="configuration"></param>
        /// <param name="logSource"></param>
        /// <param name="errorAnalysis"></param>
        /// <param name="prometheusStats"></param>
        private static void ProcessLines(
            List<string> InputLines,
            JObject Configuration,
            JObject LogSource,
            Dictionary<string, ErrorAnalysis> ErrorAnalysis,
            CodaRESTClient.Client CodaClient,
            Dictionary<String, ErrorLogItem> ErrorLogs)
        {
            int lineCount = 0;
            int skipLines = 0;
            try
            {
                skipLines = Convert.ToInt32(LogSource["inputSpecs"]!["skipLines"]);
            }
            catch { }
            foreach (var line in InputLines)
            {
                lineCount++;
                if (lineCount > skipLines)
                {
                    if ((lineCount % 1000) == 0)
                    {
                        Console.Write("..");
                    }
                    try
                    {
                        ProcessLine(line, Configuration, LogSource, ErrorAnalysis, CodaClient, ErrorLogs, lineCount);
                    }
                    catch (Exception ex)
                    {
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PROC-0002", $"Line {lineCount}, Error {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a single input line
        /// </summary>
        /// <param name="CodaClient"></param>
        /// <param name="Configuration"></param>
        /// <param name="ErrorAnalysis">Current error analysis results</param>
        /// <param name="ErrorLogs">Cached error logs</param>
        /// <param name="Line">Text line from source file to be parsed</param>
        /// <param name="LineCount">Line number from source file for reference</param>
        /// <param name="LogSource">Log Source specifications from config</param>
        private static void ProcessLine(
            string Line,
            JObject Configuration,
            JObject LogSource,
            Dictionary<string, ErrorAnalysis> ErrorAnalysis,
            CodaRESTClient.Client CodaClient,
            Dictionary<String, ErrorLogItem> ErrorLogs,
            int LineCount)
        {
            string network = $"{Configuration["network"]}".Trim().ToLower();
            DateTime? lastRunDate = null;
            try
            {
                lastRunDate = Convert.ToDateTime(Configuration["analysis"]!["lastRunDate"]);
            }
            catch { }
            if (lastRunDate is null)
            {
                // If never run, get last 7 days
                lastRunDate = DateTime.UtcNow.AddDays(-7);
            }
            Trace.WriteLine($"Processing line: {Line}");
            ReportLogItem? logItem = null;
            try
            {
                logItem = LogSource["input"]!.ToString() switch
                {
                    "text/csv" => ParseItem_TextCSV(Line, LogSource, network),
                    "text/fixed" => ParseItem_TextFixed(Line, LogSource, network),
                    "text/other" => ParseItem_TextOther(Line, LogSource, network),
                    "text/cardano" => ParseItem_JsonCardano(Line, network),
                    "text/regex" => ParseItem_RegEx(Line, LogSource, network),
                    _ => ProcessParsePlugin(Configuration, Line, LogSource, network)
                };
            }
            catch (Exception ex)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PARSE-0001", $"Error {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
            }
            if (logItem is not null)
            {
                ProcessLineItem(Configuration, LogSource, ErrorAnalysis, CodaClient, ErrorLogs, lastRunDate, logItem, LineCount);
            }
        }

        private static ReportLogItem? ParseItem_RegEx(string Line, JObject LogSource, string Network)
        {
            // TODO: RegEx parsing
            Trace.WriteLine($"{LogSource["input"]} - {Network}");
            Trace.WriteLine(Line);
            return null;
        }

        /// <summary>
        /// Determines if a standardized representation of an error log entry is worthy of attention
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="LogSource"></param>
        /// <param name="ErrorAnalysis"></param>
        /// <param name="CodaClient"></param>
        /// <param name="ErrorLogs"></param>
        /// <param name="lastRunDate"></param>
        /// <param name="logItem"></param>
        /// <param name="LineCount"></param>
        private static void ProcessLineItem(
            JObject Configuration,
            JObject LogSource,
            Dictionary<string, ErrorAnalysis> ErrorAnalysis,
            Client CodaClient,
            Dictionary<string, ErrorLogItem> ErrorLogs,
            DateTime? lastRunDate,
            ReportLogItem logItem,
            int LineCount)
        {
            // Skip if too old
            if (logItem.TimeOccurredUTC <= lastRunDate)
            {
                return;
            }
            // Filter on Severity list
            var sevs = (JArray)LogSource["messageType"]!["values"]!;
            var sevsList = sevs.ToObject<List<string>>()!;
            if (!sevsList.Contains(logItem.Severity))
            {
                return;
            }
            ErrorLogItem codaRecord = GetCodaInfo(Configuration, logItem, CodaClient, ErrorLogs);
            if (codaRecord.Source is null)
            {
                if ($"{LogSource["input"]}" == "journal")
                {
                    codaRecord.Source = new JObject()
                    {
                        { "logFile", "Linux System Journal" }
                    };
                }
                else
                {
                    codaRecord.Source = new JObject()
                    {
                        { "logFile", LogSource["inputSpecs"]!["inputFile"] },
                        { "lineNumber", LineCount }
                    };
                }
            }
            if (!ErrorAnalysis.ContainsKey($"{codaRecord.AcceptedSeverity}"))
            {
                var ea = new ErrorAnalysis();
                ErrorAnalysis.Add($"{codaRecord.AcceptedSeverity}", ea);
            }
            foreach (var severity in ErrorAnalysis.Keys)
            {
                ErrorAnalysis[severity].LogError(codaRecord, severity == $"{codaRecord.AcceptedSeverity}");
            }
        }

        /// <summary>
        /// Calls the ProcessLineItem() method of a plugin to return the ReportLogItem
        /// </summary>
        /// <param name="Configuration"></param>
        /// <param name="LogLine"></param>
        /// <param name="LogSource"></param>
        /// <param name="Network"></param>
        /// <returns></returns>
        private static ReportLogItem? ProcessParsePlugin(JObject Configuration, string LogLine, JObject LogSource, string Network)
        {
            if (_Plugins is null)
            {
                return null;
            }
            var idParts = $"{LogSource["input"]}".Split('/');
            if (_Plugins.ContainsKey(idParts[1]))
            {
                try
                {
                    var pluginAssembly = _Plugins[idParts[1]];
                    if (!pluginAssembly.LineProcessor)
                    {
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "LPLG-0002", $"ERROR - Plugin '{idParts[1]}' is not a text line processor", ErrorLogSeverityEnum.Error);
                        return null;
                    }
                    var logitem = pluginAssembly.ProcessLineItem(LogLine, LogSource);
                    if (logitem is not null)
                    {
                        logitem["Network"] = Network;
                        return JsonConvert.DeserializeObject<ReportLogItem>(logitem.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "LPLG-0003", $"Exception occurred while plugin processed line, {ex.HResult}: {ex.Message}", ErrorLogSeverityEnum.Error);
                    return null;
                }
            }
            else
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "LPLG-0001", $"ERROR - Plugin '{idParts[1]}' is not available", ErrorLogSeverityEnum.Error);
            }
            return null;
        }

        /// <summary>
        /// Retrieves associated CodEA info
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="codaClient"></param>
        /// <param name="ErrorLogs">The cached results from Coda</param>
        /// <returns></returns>
        private static ErrorLogItem GetCodaInfo(JObject Configuration, ReportLogItem logItem, Client codaClient, Dictionary<String, ErrorLogItem> ErrorLogs)
        {
            // First, find out if item is cached
            var errKey = $"{logItem.Network}-{logItem.ErrorCode}";
            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0010", $"Checking error entry {errKey}...", ErrorLogSeverityEnum.Debug);
            if (ErrorLogs.ContainsKey(errKey))
            {
                // It is cached; if cache is more than a few days old and not solved, check again; otherwise, use the cache
                if (ErrorLogs[errKey].IsSolved)
                {
                    LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0011", "Retrieved from cache", ErrorLogSeverityEnum.Debug);
                    return ErrorLogs[errKey];
                }
                else
                {
                    if (DateTime.UtcNow.Subtract(ErrorLogs[errKey].DateCached).Days <= 3)
                    {
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0012", "Retrieved from cache", ErrorLogSeverityEnum.Debug);
                        return ErrorLogs[errKey];
                    }
                    else
                    {
                        // Not solved and more than 3 days old, clear from cache and check again
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0013", "Cached item is >3 days old, refreshing...", ErrorLogSeverityEnum.Debug);
                        ErrorLogs.Remove(errKey);
                    }
                }
            }
            // It isn't cached, so we need to retrieve info from Coda, report 1 instance, cache and return that
            var retVal = new ErrorLogItem
            {
                ErrorMessage = logItem.ErrorMessage,
                ErrorCode = logItem.ErrorCode,
                Network = logItem.Network
            };
            var codadata = codaClient.GetError(retVal.ErrorCode, retVal.Network, true);
            if (codadata != null)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "INFO-0014", "Retrieved data from CodaEA server", ErrorLogSeverityEnum.Debug);
                retVal.AcceptedMeaning = $"{codadata["acceptedMeaning"]}";
                retVal.AcceptedSeverity = Convert.ToInt32(codadata["acceptedSeverity"]);
                retVal.DateCached = DateTime.UtcNow;
                var solutions = (JArray?)codadata["troubleshooting"];
                if (solutions is null)
                {
                    retVal.IsSolved = false;
                }
                else
                {
                    retVal.IsSolved = (solutions.Count > 0);
                }
            }
            else
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "CODA-0001", "Unable to retrieve data from CodaEA", ErrorLogSeverityEnum.Warning);
            }
            ErrorLogs.Add(errKey, retVal);
            return retVal;
        }

        private static ReportLogItem ParseItem_TextOther(string line, JObject logSource, string Network)
        {
            var retVal = new ReportLogItem();
            var delimiter = $"{logSource["delimiter"]}";
            var values = DelimParse(line, delimiter);

            int i = 0;
            var columnNames = new List<string>();
            foreach (JObject column in (JArray)logSource["inputSpecs"]!["columnSpec"]!)
            {
                columnNames.Add($"{column["columnName"]}");
                if (Convert.ToBoolean(column["trim"]))
                {
                    string? trimChars = column.ContainsKey("trimChars") ? $"{column["trimChars"]}" : null;
                    values[i] = CodaTrim(values[i], trimChars);
                }
                i++;
            }

            // Only process lines same # columns or longer
            if (columnNames.Count <= values.Length)
            {
                retVal = MapValuesToReportLog(columnNames, values, (JObject)logSource["inputSpecs"]!["messageType"]!, Network);
                retVal.OtherData = line;
                retVal.Network = Network;
            }

            return retVal;
        }

        /// <summary>
        /// Parses a JSON Cardano-Node error log entry
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Network"></param>
        /// <returns></returns>
        private static ReportLogItem? ParseItem_JsonCardano(string Line, string Network)
        {
            var retVal = new ReportLogItem();
            var logLine = JObject.Parse(Line);
            if ($"{logLine["sev"]}" == "Error")
            {
                retVal.TimeOccurredUTC = Convert.ToDateTime(logLine["at"]);
                retVal.Severity = $"{logLine["sev"]}";
                var errorData = (JObject)logLine["data"]!;
                if (errorData.ContainsKey("val"))
                {
                    errorData = (JObject)errorData["val"]!;
                    retVal.Network = Network;
                    retVal.ErrorCode = $"{errorData["kind"]}";
                    retVal.ErrorMessage = $"{errorData["reason"]!["kind"]}";
                    retVal.OtherData = errorData.ToString();
                }
                else
                {
                    retVal.Network = Network;
                    retVal.ErrorCode = $"{errorData["kind"]}";
                    retVal.ErrorMessage = $"{errorData["event"]}";
                    retVal.OtherData = errorData.ToString();
                }
            }
            else
            {
                retVal = null;
            }
            return retVal;
        }

        private static ReportLogItem MapValuesToReportLog(List<string> columnNames, string[] values, JObject mapSpecs, string Network)
        {
            var retVal = new ReportLogItem
            {
                TimeOccurredUTC = Convert.ToDateTime(values[columnNames.IndexOf($"{mapSpecs["timeColumn"]}")]),
                ErrorMessage = values[columnNames.IndexOf($"{mapSpecs["messageColumn"]}")],
                ErrorCode = values[columnNames.IndexOf($"{mapSpecs["codeColumn"]}")],
                Network = Network,
                Severity = values[columnNames.IndexOf($"{mapSpecs["columnName"]}")],
            };
            return retVal;
        }

        /// <summary>
        /// Splits a string with the given multicharacter delimiter
        /// </summary>
        /// <param name="line"></param>
        /// <param name="delim"></param>
        /// <returns></returns>
        private static string[] DelimParse(string line, string delim)
        {
            var delims = new List<string>
            {
                delim
            };
            return line.Split(delims.ToArray(), StringSplitOptions.None);
        }

        /// <summary>
        /// Parses a line using fixed-width text specs into a ReportLogItem
        /// </summary>
        /// <param name="line"></param>
        /// <param name="logSource"></param>
        /// <returns></returns>
        private static ReportLogItem ParseItem_TextFixed(string line, JObject logSource, string Network)
        {
            var retVal = new ReportLogItem();
            var columnNames = new List<string>();
            var values = new List<string>();
            foreach (JObject column in (JArray)logSource["inputSpecs"]!["columnSpec"]!)
            {
                columnNames.Add($"{column["columnName"]}");
                var start = Convert.ToInt32(column["startColumn"]);
                var len = Convert.ToInt32(column["length"]);
                if ((start + len) > line.Length)
                {
                    len = line.Length - start + 1;
                }
                string val = line.Substring(start - 1, len);
                if (Convert.ToBoolean(column["trim"]))
                {
                    string? trimChars = column.ContainsKey("trimChars") ? $"{column["trimChars"]}" : null;
                    val = CodaTrim(val, trimChars);
                }
                values.Add(val);
            }

            // Only process lines same # columns or longer
            if (columnNames.Count <= values.Count)
            {
                retVal = MapValuesToReportLog(columnNames, values.ToArray(), (JObject)logSource["inputSpecs"]!["messageType"]!, Network);
                retVal.OtherData = line;
                retVal.Network = Network;
            }

            return retVal;
        }

        /// <summary>
        /// Removes specified characters from beginning/end of string
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="TrimCharacters"></param>
        /// <returns></returns>
        private static string CodaTrim(string Value, string? TrimCharacters)
        {
            var retVal = Value.Trim();
            if (TrimCharacters is not null)
            {
                foreach (var c in TrimCharacters)
                {
                    while (retVal.StartsWith(c))
                    {
                        if (retVal.Length > 1)
                        {
                            retVal = retVal[1..];
                        }
                        else
                        {
                            retVal = String.Empty;
                        }
                    }
                    while (retVal.EndsWith(c))
                    {
                        if (retVal.Length > 1)
                        {
                            retVal = retVal[0..^1];
                        }
                        else
                        {
                            retVal = String.Empty;
                        }
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Parses a line by CSV into a ReportLogItem
        /// </summary>
        /// <param name="line"></param>
        /// <param name="logSource"></param>
        /// <returns></returns>
        private static ReportLogItem ParseItem_TextCSV(string line, JObject logSource, string Network)
        {
            var retVal = new ReportLogItem();
            var columnNames = new List<string>();
            foreach (JObject column in (JArray)logSource["inputSpecs"]!["columnSpec"]!)
            {
                columnNames.Add($"{column["columnName"]}");
            }
            var values = CSVParse(line);
            // Only process lines same # columns or longer
            if (columnNames.Count <= values.Length)
            {
                retVal = MapValuesToReportLog(columnNames, values, (JObject)logSource["inputSpecs"]!["messageType"]!, Network);
                retVal.OtherData = line;
                retVal.Network = Network;
            }

            return retVal;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parses a line using CSV standards into individual tokens; supports quote qualifiers and double-quote quote literals
        /// </summary>
        /// <param name="Line"></param>
        /// <returns></returns>
        private static string[] CSVParse(string Line)
        {
            var retVal = new List<string>();

            int pos = 0;
            bool quote = false; // Whether token is qualified with quotes
            string val = String.Empty; // Token
            while (pos < Line.Length)
            {
                if (Line[pos] == '"')
                {
                    if (pos > 0)
                    {
                        // 2 quotes in a row
                        if (Line[pos - 1] == '"')
                        {
                            val += '"';
                            quote = !quote;
                        }
                        else
                        {
                            quote = !quote;
                        }
                    }
                    else
                    {
                        quote = !quote;
                    }
                }
                else if (Line[pos] == ',')
                {
                    if (quote)
                    {
                        val += Line[pos];
                    }
                    else
                    {
                        // Trim double-quote
                        if (val[^2..] == "\"\"")
                        {
                            val = val.Remove(val.Length - 1);
                        }
                        retVal.Add(val);
                        val = String.Empty;
                    }
                }
                else
                {
                    val += Line[pos];
                }
                pos++;
            }
            // Trim double-quote
            if (val[^2..] == "\"\"")
            {
                val = val.Remove(val.Length - 1);
            }
            retVal.Add(val);

            return retVal.ToArray();
        }

        /// <summary>
        /// Converts a Linux SysLog entry to ReportLogItem
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Network"></param>
        /// <returns></returns>
        private static ReportLogItem ParseItem_Journal(string Line, string Network)
        {
            var severities = "Emergency,Alert,Critical,Error,Warning,Notice,Informational,Debug".Split(',');
            var lineObject = JObject.Parse(Line);
            var retVal = new ReportLogItem()
            {
                Network = Network,
                ErrorCode = $"{lineObject["MESSAGE_ID"]}",
                ErrorMessage = $"{lineObject["MESSAGE"]}",
                Severity = severities[Convert.ToInt32(lineObject["PRIORITY"])],
                TimeOccurredUTC = Convert.ToDateTime(lineObject["SYSLOG_TIMESTAMP"]),
                OtherData = Line,
                ReportingProcess = $"{lineObject["SYSLOG_IDENTIFIER"]}"
            };
            return retVal;
        }
    }
}
