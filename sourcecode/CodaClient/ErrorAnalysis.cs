using CodaRESTClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codaclient.classes
{
    /// <summary>
    /// Encapsulates an Error Analysis
    /// </summary>
    [Serializable()]
    public class ErrorAnalysis
    {
        /// <summary>
        /// List of unique errors reported
        /// </summary>
        public SerializableDictionary<string, ErrorLogItem> Errors { get; set; }

        public ErrorAnalysis()
        {
            Errors = new  SerializableDictionary<string, ErrorLogItem>();
        }

        /// <summary>
        /// Adds, updates or removes entry from the list
        /// </summary>
        /// <param name="LogItem">The log item entry</param>
        /// <param name="IsMatchingSeverity">If matches, adds to/updates the list; if not matches, removes from list</param>
        public void LogError(ErrorLogItem LogItem, bool IsMatchingSeverity)
        {
            var errorKey = $"{LogItem.Network}-{LogItem.ErrorCode}";
            if (IsMatchingSeverity)
            {
                if (Errors.ContainsKey(errorKey))
                {
                    Errors[errorKey].NumberOccurrences += 1;
                }
                else
                {
                    LogItem.NumberOccurrences = 1;
                    Errors.Add(errorKey, LogItem);
                }
            }
            else
            {
                if (Errors.ContainsKey(errorKey))
                {
                    Errors.Remove(errorKey);
                }
            }
        }
    }
}
