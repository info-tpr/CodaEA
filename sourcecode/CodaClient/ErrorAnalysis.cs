using CodaRESTClient;
using Newtonsoft.Json.Linq;
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
        /// <param name="ReferenceData">Reference to original log data to add to the references: date=log entry date, type=File/Journal/EventLog/etc., file=path, line=line #)</param>
        public void LogError(ErrorLogItem LogItem, bool IsMatchingSeverity, JObject ReferenceData)
        {
            var errorKey = $"{LogItem.Network}-{LogItem.ErrorCode}";
            if (IsMatchingSeverity)
            {
                if (Errors.ContainsKey(errorKey))
                {
                    Errors[errorKey].NumberOccurrences += 1;
                    var refdata = Errors[errorKey].Source;
                    if (refdata is null)
                    {
                        refdata = new JObject()
                        {
                            ["logReferences"] = new JArray()
                            {
                                { ReferenceData }
                            }
                        };
                    }
                    else
                    {
                        var refs = refdata.ContainsKey("logReferences") switch
                        {
                            true => (JArray)refdata["logReferences"]!,
                            false => new JArray()
                        };
                        refs.Add(ReferenceData);
                        refdata["logReferences"] = refs;
                    }
                    Errors[errorKey].Source = refdata;
                }
                else
                {
                    LogItem.NumberOccurrences = 1;
                    var refdata = new JObject()
                    {
                        ["logReferences"] = new JArray()
                            {
                                { ReferenceData }
                            }
                    };
                    LogItem.Source = refdata;
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
