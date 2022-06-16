using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codaclient.classes
{
    /// <summary>
    /// Used to encapsulate a log entry from one of the supported log sources
    /// </summary>
    public class ReportLogItem
    {
        /// <summary>
        /// The date/time the error occurred
        /// </summary>
        public DateTime TimeOccurredUTC { get; set; }
        /// <summary>
        /// Severity as reported by the application
        /// </summary>
        public String Severity { get; set; }
        /// <summary>
        /// App/Network to which this error applies
        /// </summary>
        public String Network { get; set; }
        /// <summary>
        /// Reporting process in the OS
        /// </summary>
        public String ReportingProcess { get; set; }
        /// <summary>
        /// Error Code as reported by the application
        /// </summary>
        public String ErrorCode { get; set; }
        /// <summary>
        /// Text of the error message
        /// </summary>
        public String ErrorMessage { get; set; }
        /// <summary>
        /// Any other data as necessary
        /// </summary>
        public String OtherData { get; set; }

        public ReportLogItem()
        {
            TimeOccurredUTC = DateTime.UtcNow;
            Severity = String.Empty;
            Network = String.Empty;
            ReportingProcess = String.Empty;
            ErrorCode = String.Empty;
            ErrorMessage = String.Empty;
            OtherData = String.Empty;
        }
    }
}
