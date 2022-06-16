using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codaclient.classes
{
    [Serializable()]
    public class ErrorLogItem
    {
        private string _Network = String.Empty;
        public String Network 
        { 
            get
            {
                return _Network;
            }
            set
            { 
                if (value.Length > 50)
                {
                    _Network = value[..49];
                }
                else
                {
                    _Network=value;
                }
            }
        }
        private string _ErrorCode = String.Empty;
        public String ErrorCode
        {
            get
            {
                return _ErrorCode;
            }
            set
            {
                if (value.Length > 50)
                {
                    _ErrorCode = value[..49];
                }
                else
                {
                    _ErrorCode = value;
                }
            }
        }
        public String ErrorMessage { get; set; }
        public int NumberOccurrences { get; set; }
        public int AcceptedSeverity { get; set; }
        public String AcceptedMeaning { get; set; }
        /// <summary>
        /// Information on the source of the error
        /// </summary>
        public JObject? Source { get; set; }
        public DateTime DateCached { get; set; }
        /// <summary>
        /// Whether or not the error has been analyzed bo CodaEA community
        /// </summary>
        public bool IsAnalyzed { get { return (!String.IsNullOrEmpty(AcceptedMeaning)); } }
        /// <summary>
        /// Whether or not CodaEA community has proposed Troubleshooting solutions
        /// </summary>
        public bool IsSolved { get; set; }

        public ErrorLogItem()
        {
            Source = null;
            Network = String.Empty;
            ErrorCode = String.Empty;
            ErrorMessage = String.Empty;
            NumberOccurrences = 0;
            AcceptedSeverity = 0;
            AcceptedMeaning = String.Empty;
        }
    }
}
