using CodaClient.Plugin;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace CodaEA.SQLServer
{
    public class Plugin : ICodaPlugin
    {
        private bool _EndOfStream = true;
        private SqlConnection? _DBConnection;
        private JObject? _Settings;
        private DataTable? _DataTable;
        private int _CurrentDataRow = -1;

        /// <summary>
        /// Connects to db if not connected; returns open DB connection
        /// </summary>
        private SqlConnection DBConnection
        {
            get
            {
                if (_Settings is null)
                {
                    throw new Exception("Attempt to access SQL database before configuration provided");
                }
                if (_DBConnection is null)
                {
                    var sqlconn = $"Data Source={_Settings["inputSpecs"]!["server"]};Initial Catalog={_Settings["inputSpecs"]!["database"]};User Id={_Settings["inputSpecs"]!["user"]};Password={_Settings["inputSpecs"]!["password"]};Integrated Security={_Settings["inputSpecs"]!["integratedSecurity"]};";
                    _DBConnection = new SqlConnection(sqlconn);
                    _DBConnection!.Open();
                }
                else if (_DBConnection.State == System.Data.ConnectionState.Closed)
                {
                    var sqlconn = $"Data Source={_Settings["inputSpecs"]!["server"]};Initial Catalog={_Settings["inputSpecs"]!["database"]};User Id={_Settings["inputSpecs"]!["user"]};Password={_Settings["inputSpecs"]!["password"]};Integrated Security={_Settings["inputSpecs"]!["integratedSecurity"]};";
                    _DBConnection = new SqlConnection(sqlconn);
                    _DBConnection!.Open();
                }
                return _DBConnection;
            }
        }

        #region "Interface Properties"
        public bool EndOfStream { get { return _EndOfStream; } }

        public bool FileProcessor { get { return true; } }

        public bool LineProcessor { get { return false; } }

        public string Version { get { return "2022.1"; } }

        public string Name { get { return "Official CodaEA SQL Table Processor"; } }

        public string Description { get { return "Processes the contents of a SQL table into ReportLogItem format"; } }

        public DateTime? UTCLastRunDate { get; set; }
        #endregion

        public Plugin()
        {
            _EndOfStream = false;
        }

        public JObject? NextLogItem()
        {
            if (_Settings is null) return null;
            if (_EndOfStream) return null;
            if (_DataTable is null)
            {
                _EndOfStream = true;
                return null;
            }
            var fields = (JArray)_Settings["inputSpecs"]!["fields"]!;
            if (fields is null || fields.Count < 4)
            {
                throw new Exception("'fields' setting in inputSpecs must contain at least 4 field names");
            }
            var dr = _DataTable.Rows[++_CurrentDataRow];
            var retVal = new JObject()
            {
                  {"TimeOccurredUTC", Convert.ToDateTime(dr[$"{fields[0]}"]) },
                  {"Severity", $"{dr[$"{fields[1]}"]}" },
                  {"Network", "my-app" }, // Name of network or app reporting
                  {"ReportingProcess", "my-app.exe" }, // OS process reporting the error
                  {"ErrorCode", $"{dr[$"{fields[2]}"]}" }, // Unique Error code identifier
                  {"ErrorMessage", $"{dr[$"{fields[3]}"]}" }, // Error message text
                  {"OtherData", "" }, // Anything else you wish to include
            };
            _EndOfStream = (_CurrentDataRow == _DataTable.Rows.Count);
            return retVal;
        }

        public bool OpenLogFile(string FilePath, JObject ConfigOptions)
        {
            _Settings = ConfigOptions;
            // Open the database connection
            if (DBConnection.State != System.Data.ConnectionState.Open)
            {
                return false;
            }
            // Open query and get results into data table
            OpenQuery();
            if (_DataTable is null)
            {
                _DataTable = new();
            }
            _EndOfStream = !(_DataTable.Rows.Count > 0);
            return true;
        }

        /// <summary>
        /// Opens the query and returns results to the _DataTable
        /// </summary>
        private void OpenQuery()
        {
            if (_Settings is null)
            {
                _DataTable = new DataTable();
                return;
            }
            using (var sqlcmd = DBConnection.CreateCommand())
            {
                if ($"{_Settings["inputSpecs"]!["queryType"]}".ToLower() == "table")
                {
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = $"SELECT * FROM [{_Settings["inputSpecs"]!["text"]}]";
                }
                else if ($"{_Settings["inputSpecs"]!["queryType"]}".ToLower() == "text")
                {
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = $"{_Settings["inputSpecs"]!["text"]}";
                }
                else
                {
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    AddParameters(sqlcmd);
                    sqlcmd.CommandText = $"{_Settings["inputSpecs"]!["text"]}";
                }
                using (var sa = new SqlDataAdapter(sqlcmd))
                {
                    _DataTable = new();
                    sa.Fill(_DataTable);
                }
            }
        }

        /// <summary>
        /// Adds parameters with values to stored procedure query
        /// </summary>
        /// <param name="sqlcmd"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void AddParameters(SqlCommand sqlcmd)
        {
            if (_Settings is null)
            {
                throw new Exception("Cannot operate without job settings");
            }
            if (!_Settings.ContainsKey("inputSpecs"))
            {
                throw new Exception("inputSpecs are required");
            }
            var parameters = (JArray)_Settings["inputSpecs"]!["parameters"]!;
            foreach (JObject parameter in parameters)
            {
                var val = $"{parameter["value"]}";
                sqlcmd.Parameters.AddWithValue($"{parameter["name"]}", GetValue(val));
            }
        }

        private string GetValue(string Value)
        {
            string retVal = Value switch
            {
                "$LAST_RUN_DATE" => $"{UTCLastRunDate}",
                _ => Value,
            };
            return retVal;
        }

        public JObject ProcessLineItem(string LogLine, JObject ConfigOptions)
        {
            throw new NotImplementedException();
        }
    }
}