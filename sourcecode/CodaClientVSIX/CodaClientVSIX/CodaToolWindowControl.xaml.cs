using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Web;
using System;
using RestSharp;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Security.Cryptography;
using System.Diagnostics;

namespace CodaClientVSIX
{
    /// <summary>
    /// Interaction logic for CodaToolWindowControl.
    /// </summary>
    public partial class CodaToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodaToolWindowControl"/> class.
        /// </summary>
        public CodaToolWindowControl()
        {
            this.InitializeComponent();
            var vsver = Registry.CurrentUser.OpenSubKey("CodaEA-VSVER");
            VSVersion.Text = $"{vsver.GetValue("vsver")}";
        }


        public void options_Click(object sender, EventArgs e)
        {
            var options = new CodaOptionsForm();
            options.ShowDialog();
        }


        private string GetCodaWebToken()
        {
            var apiServer = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("Server")}";
            var apiclient = new RestClient(apiServer);
            var apikey = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("APIKey")}";
            var request = new RestRequest("/api/account/gentoken", RestSharp.Method.Get);
            request.AddHeader("apikey", apikey);
            var response = apiclient.ExecuteAsync(request).Result;
            if (String.IsNullOrEmpty(response.Content))
            {
                return String.Empty;
            }
            var token = JObject.Parse(response.Content);
            if (token.ContainsKey("token"))
            {
                return $"{token["token"]}";
            }
            else
            {
                return String.Empty;
            }
        }

        private JObject GetMyAccount()
        {
            var apiServer = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("Server")}";
            var apiclient = new RestClient(apiServer);
            var apikey = $"{Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("APIKey")}";
            var request = new RestRequest("/api/account/getmy", RestSharp.Method.Get);
            request.AddHeader("apikey", apikey);
            var response = apiclient.ExecuteAsync(request).Result;
            if (String.IsNullOrEmpty(response.Content))
            {
                return null;
            }
            var token = JObject.Parse(response.Content);
            return token;
        }


        private JObject GetErrorFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsText(TextDataFormat.Text))
                {
                    var clipText = Clipboard.GetText(TextDataFormat.Text);
                    var errorData = new JObject();
                    var sr = new StringReader(clipText);
                    var headers = sr.ReadLine().Split('\t');
                    var err = sr.ReadLine().Split('\t');
                    int idx = 0;
                    foreach (var header in headers)
                    {
                        errorData.Add(header.Trim(), err[idx]);
                        idx++;
                    }
                    if (!errorData.ContainsKey("Code"))
                    {
                        throw new System.Exception();
                    }
                    return errorData;
                }
                else
                {
                    MessageBox.Show("Select the error message and press CTRL-C first");
                    return null;
                }

            }
            catch
            {
                MessageBox.Show("Select the error message and press CTRL-C first");
                return null;
            }
        }

        private void getHelp_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/info-tpr/CodaEA/blob/main/CodaClient_VSIX.md");
        }

        private void viewAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var token = GetCodaWebToken();
                if (string.IsNullOrEmpty(token))
                {
                    VsShellUtilities.ShowMessageBox(
                        null,
                        "Invalid response from CodaEA API Server - We apologize for the inconvenience.",
                        "CodaEA",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }
                var acct = GetMyAccount();
                if (acct is null)
                {
                    VsShellUtilities.ShowMessageBox(
                        null,
                        "Invalid response from CodaEA API Server - We apologize for the inconvenience.",
                        "CodaEA",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }
                else if (acct.ContainsKey("code"))
                {
                    VsShellUtilities.ShowMessageBox(
                        null,
                        $"Error: {acct["message"]}",
                        "CodaEA",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }
                string vsver = Registry.CurrentUser.OpenSubKey("CodaEA-VSVER").GetValue("vsver").ToString();
                string server = Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("WebServer").ToString();
                string url = $"{server}/CodaEA/ShowAccount/{acct["accountId"]}?sessionToken={token}";
                Process.Start(url);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to launch URL, Error: {ex.Message}");
            }
        }
    }
}