using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Windows.Forms;

namespace CodaClientVSIX
{
    public partial class CodaOptionsForm : Form
    {
        public CodaOptionsForm()
        {
            InitializeComponent();
        }
        private long _AccountId;

        private void CodaOptionsForm_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(chkErrorEdits, "Accepted Meaning/Severity provided for a watched Error Code");
            toolTip1.SetToolTip(chkErrorReport, "An error in your communities is reported");
            toolTip1.SetToolTip(chkTroubleshoot, "Troubleshooting tips/solutions are posted");
            toolTip1.SetToolTip(chkTshootComment, "A comment is posted on a Troubleshootint tip");
            toolTip1.SetToolTip(chkVoteDown, "When your posts are voted down");
            toolTip1.SetToolTip(chkVoteUp, "When your posts are voted up");
            cboFrequency.Text = "Immediate";
            var myAccount = GetMyAccount();
            if (myAccount == null)
            {
                return;
            }
            if (myAccount.ContainsKey("code"))
            {
                MessageBox.Show($"{myAccount["message"]}", "An Error Occurred");
                return;
            }
            var options = (JObject)myAccount["options"];
            var notifications = (JObject)options["notifications"];
            cboFrequency.Text = $"{notifications["Schedule"]}";
            chkErrorEdits.Checked = Convert.ToBoolean(notifications["ErrorLogEdit"]);
            chkErrorReport.Checked = Convert.ToBoolean(notifications["ErrorLogReport"]);
            chkTroubleshoot.Checked = Convert.ToBoolean(notifications["Troubleshoot"]);
            chkTshootComment.Checked = Convert.ToBoolean(notifications["Comment"]);
            chkVoteUp.Checked = Convert.ToBoolean(notifications["VoteUp"]);
            chkVoteDown.Checked = Convert.ToBoolean(notifications["VoteDown"]);
        }


        private JObject GetMyAccount()
        {
            var apiServer = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("Server")}";
            if (String.IsNullOrEmpty(apiServer))
            {
                MessageBox.Show("Configure your CodaEA connection first.");
                return null;
            }
            var apiclient = new RestClient(apiServer);
            var apikey = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("APIKey")}";
            var request = new RestRequest("/api/account/getmy", RestSharp.Method.Get);
            request.AddHeader("apikey", apikey);
            request.AddHeader("full", "true");
            var response = apiclient.ExecuteAsync(request).Result;
            if (String.IsNullOrEmpty(response.Content))
            {
                return new JObject()
                {
                    ["code"] = "none",
                    ["message"] = "No data returned from API server"
                };
            }
            try
            {
                var acct = JObject.Parse(response.Content);
                _AccountId = Convert.ToInt64(acct["accountId"]);
                return acct;
            }
            catch
            {
                return new JObject()
                {
                    ["code"] = "none",
                    ["message"] = "Unknown data returend from API server"
                };
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var prefs = new JObject()
            {
                ["notifications"] = new JObject()
                {
                    ["Schedule"] = cboFrequency.Text,
                    ["ErrorLogEdit"] = chkErrorEdits.Checked,
                    ["ErrorLogReport"] = chkErrorReport.Checked,
                    ["Troubleshoot"] = chkTroubleshoot.Checked,
                    ["Comment"] = chkTshootComment.Checked,
                    ["VoteUp"] = chkVoteUp.Checked,
                    ["VoteDown"] = chkVoteDown.Checked
                }
            };
            var apiServer = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("Server")}";
            if (String.IsNullOrEmpty(apiServer))
            {
                MessageBox.Show("Configure your CodaEA connection first.");
                return;
            }
            var apiclient = new RestClient(apiServer);
            var apikey = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("APIKey")}";
            var request = new RestRequest($"/api/account/{_AccountId}/updateoptions", Method.Put);
            request.AddHeader("apikey", apikey);
            request.AddHeader("full", "true");
            request.AddBody(prefs.ToString(Newtonsoft.Json.Formatting.None), "text/json");
            var response = apiclient.ExecuteAsync(request).Result;
            JObject result = null;
            if (String.IsNullOrEmpty(response.Content))
            {
                result = new JObject()
                {
                    ["code"] = "none",
                    ["message"] = "No data returned from API server"
                };
            }
            else
            {
                try
                {
                    result = JObject.Parse(response.Content);
                }
                catch
                {
                    result = new JObject()
                    {
                        ["code"] = "none",
                        ["message"] = "Unknown data returend from API server"
                    };
                }
            }
            if (result.ContainsKey("code"))
            {
                MessageBox.Show($"Error - {result["message"]}", "CodaEA Error");
            }
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
