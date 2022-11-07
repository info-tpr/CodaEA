using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace codaclient.classes
{

    public static class ClientUtilities
    {
        static private Dictionary<string, string> TemplateCache = new Dictionary<string, string>();

        /// <summary>
        /// Launches an external process that is expected to be interactive within the shell window
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <param name="runOnLinux"></param>
        public static void BashInteractive(this string cmd, string args, bool runOnLinux = false)
        {
            var escapedArgs = runOnLinux switch
            {
                true => $"-c \"{args.Replace("\"", "\\\"")}\"",
                false => args
            };
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd, //"/bin/bash",
                    Arguments = escapedArgs,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            //string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return;
        }

        /// <summary>
        /// Launches a process which is expected to have no interaction with the user in the shell; stdout is returned
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <param name="runOnLinux"></param>
        /// <returns>StdOut from process</returns>
        public static string Bash(this string cmd, string args, bool runOnLinux = false)
        {
            var escapedArgs = runOnLinux switch
            {
                true => $"-c \"{args.Replace("\"", "\\\"")}\"",
                false => args
            };
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd, //"/bin/bash",
                    Arguments = escapedArgs,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }

        /// <summary>
        /// Reads a text file from the Templates folder and fills variables with the TemplateData field values
        /// </summary>
        /// <param name="TemplateFileName"></param>
        /// <param name="TemplateData"></param>
        /// <param name="PathSeparator">OS-specific path separator</param>
        /// <returns></returns>
        public static string FillTextTemplate(string TemplateFileName, JObject? TemplateData, string PathSeparator)
        {
            string retVal;
            if (TemplateCache.ContainsKey(TemplateFileName))
            {
                retVal = TemplateCache[TemplateFileName];
            }
            else
            {
                var templateFile = $"{AppContext.BaseDirectory}{PathSeparator}templates{PathSeparator}{TemplateFileName}";
                var sr = new StreamReader(templateFile);
                retVal = sr.ReadToEnd();
                sr.Close();
                TemplateCache.Add(TemplateFileName, retVal);
            }
            if (TemplateData is not null)
            {
                foreach (var item in TemplateData)
                {
                    var tokenName = $"[{item.Key}]";
                    var tokenValue = $"{item.Value}";
                    retVal = retVal.Replace(tokenName, tokenValue);
                }
            }
            return retVal;
        }

    }
}
