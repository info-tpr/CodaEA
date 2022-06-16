using CodaClient.Plugin;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

#pragma warning disable CA1822 // Mark members as static
namespace AcmeCorp.CodaPlugin
{
    public class Class1 : ICodaPlugin
    {
        private bool _EndOfStream;
        public bool EndOfStream { get { return _EndOfStream; } }
        // Whether or not your PlugIn supports the full File Processor members
        public bool FileProcessor { get { return true; } }
        // Whether or not your PlugIn supports the Line Processor member
        public bool LineProcessor { get { return true; } }

        public string Version { get { return "2022.1"; } }

        public string Name { get { return "AcmeCorp Log Processor Plugin"; } }

        public string Description { get { return "This plugin processes log files from AcmeCorp software"; } }

        public DateTime? UTCLastRunDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        // Set random # of lines to return up to 100
        private readonly int _LogLines = new Random().Next(100);
        private int _LogLine = 0;

        public JObject? NextLogItem()
        {
            if (_LogLine++ >= _LogLines)
            {
                // Set flag so CodaClient can stop reading
                _EndOfStream = true;
                return null;
            }
            // Return dummy log item
            return ProcessLineItem("", new JObject());
        }

        public bool OpenLogFile(string FilePath, JObject ConfigOptions)
        {
            try
            {
                _EndOfStream = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public JObject ProcessLineItem(string LogLine, JObject ConfigOptions)
        {
            var logItem = new JObject()
            {
                ["TimeOccurredUTC"] = GetLogTime(LogLine, ConfigOptions),
                ["Severity"] = GetSeverity(LogLine, ConfigOptions),
                ["Network"] = "acmecorp",
                ["ReportingProcess"] = GetProcess(LogLine, ConfigOptions),
                ["ErrorCode"] = GetHashCode(LogLine, ConfigOptions),
                ["ErrorMessage"] = GetMessage(LogLine, ConfigOptions),
                ["OtherData"] = LogLine,
            };
            return logItem;
        }

        private string GetMessage(string logLine, JObject configOptions)
        {
            Trace.WriteLine($"{configOptions["input"]}");
            Trace.WriteLine(logLine);
            return "An error has occurred, you need to fix it.";
        }

        private string GetHashCode(string logLine, JObject configOptions)
        {
            Trace.WriteLine($"{configOptions["input"]}");
            Trace.WriteLine(logLine);
            return "Err-123";
        }

        private string GetProcess(string logLine, JObject configOptions)
        {
            Trace.WriteLine($"{configOptions["input"]}");
            Trace.WriteLine(logLine);
            return "acme.exe";
        }

        private string GetSeverity(string logLine, JObject configOptions)
        {
            Trace.WriteLine($"{configOptions["input"]}");
            Trace.WriteLine(logLine);
            return "Error";
        }

        private JToken GetLogTime(string logLine, JObject configOptions)
        {
            Trace.WriteLine($"{configOptions["input"]}");
            Trace.WriteLine(logLine);
            return DateTime.Now;
        }
    }
}
#pragma warning restore CA1822 // Mark members as static
