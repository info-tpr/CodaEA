using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodaClientVSIX
{
    public partial class CodaClientConfigForm : Form
    {
        public CodaClientConfigForm()
        {
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            lblServer.Text = "https://prod.codaea.io";
            lblWebServer.Text = "https://www.codaea.io";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            lblServer.Text = "https://test.codaea.io";
            lblWebServer.Text = "https://www.codaea.io";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            lblServer.Text = "http://dev.codaea.io:50687";
            lblWebServer.Text = "https://dev.codaea.io:7227";
        }

        private void label3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.codaea.io/Account/SignUp");
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.codaea.io/Account/SignUp?net=Visual%20Studio");
        }

        /// <summary>
        /// About
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.codaea.io");
        }

        private bool KeyExists(RegistryKey baseKey, string subKeyName)
        {
            RegistryKey ret = baseKey.OpenSubKey(subKeyName);

            return ret != null;
        }

        private async void CodaClientConfigForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (!KeyExists(Registry.CurrentUser, "CodaEA"))
                {
                    InitializeEnvironment();
                }
                var codaKey = Registry.CurrentUser.OpenSubKey("CodaEA");
                var env = codaKey.GetValue("Environment");
                switch (env)
                {
                    case 0:
                        radioButton1.Checked = true;
                        break;
                    case 1:
                        radioButton2.Checked = true;
                        break;
                    default:
                        radioButton3.Checked = true;
                        break;
                }
                txtAPIKey.Text = $"{codaKey.GetValue("APIKey")}";

                var codaDeploymentStatus = await GetURLDocumentAsync("https://raw.githubusercontent.com/info-tpr/CodaEA/main/CodaStatus.txt");
                btnMoreInfo.Text = $"CodaEA Status: {codaDeploymentStatus} ({btnMoreInfo.Text})";
                switch(codaDeploymentStatus.ToUpper())
                {
                    case "PRODUCTION":
                        radioButton1.Checked = true;
                        btnMoreInfo.Visible = false;
                        break;
                    case "TEST":
                        radioButton1.Visible = false;
                        break;
                    case "DEVELOPMENT":
                        radioButton1.Visible = false;
                        radioButton2.Visible = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: CVSIX-0001 {ex.Message}.  Do you wish to see this on CodaEA?", "CodaEA", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                // TODO: If Yes, show on CodaEA
            }
        }

        private static string CleanBytes(byte[] Bytes)
        {
            var retVal = System.Text.Encoding.Default.GetString(Bytes);
            for (int x = 0; x < Bytes.Length; x++)
            {
                if (retVal[x] == '\n' || retVal[x] == '\0')
                {
                    return retVal.Substring(0, x);
                }
            }
            return retVal;
        }

        private static async Task<string> GetURLDocumentAsync(string URL)
        {
            var webclient = new HttpClient();
            var result = await webclient.GetAsync(URL);
            Stream str = await result.Content.ReadAsStreamAsync();
            byte[] bytes = new byte[50];
            await str.ReadAsync(bytes, 0, 49);
            return CleanBytes(bytes);
        }


        private static void InitializeEnvironment()
        {
            var codaKey = Registry.CurrentUser.CreateSubKey("CodaEA");
            codaKey.CreateSubKey("Environment", true);
            codaKey.SetValue("Environment", 1);
            codaKey.SetValue("Server", "https://test.codaea.io");
            codaKey.SetValue("APIKey", "");
            codaKey.SetValue("WebServer", "https://www.codaea.io");
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            var codaKey = Registry.CurrentUser.CreateSubKey("CodaEA");
            if (radioButton1.Checked)
            {
                codaKey.SetValue("Environment", 0);
            }
            else if (radioButton2.Checked)
            {
                codaKey.SetValue("Environment", 1);
            }
            else
            {
                codaKey.SetValue("Environment", 2);
            }
            codaKey.SetValue("APIKey", txtAPIKey.Text);
            codaKey.SetValue("Server", lblServer.Text);
            codaKey.SetValue("WebServer", lblWebServer.Text);
            this.Hide();
        }

        private void btnMoreInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("When CodaEA is in Test mode, you will only be able to access the Test environment.  Once we migrate to Production, you will be able to access either." +
                "  All data from Test will be moved to Production for the initial deployment, but anything entered in Test will no longer be migrated after we move to Production.");
        }
    }
}
