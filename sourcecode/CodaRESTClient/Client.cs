// This file contains core client functions, and functions pertaining to Accounts and Voting

using Newtonsoft.Json.Linq;
using RestSharp;
using System.ComponentModel;

namespace CodaRESTClient
{
    public enum CodaObjectTypeEnum
    {
        ErrorLogDiscussion,
        TroubleshootSolution,
        TroubleshootComment
    }

    public enum VoteTypeEnum
    {
        VoteUp,
        VoteDown,
        Report,
        ConfirmReport,
        DenyReport
    }

    /// <summary>
    /// Provides functional, synchronous access to CodaEA REST API servers
    /// </summary>
    public partial class Client : IDisposable
    {

        private bool disposed = false;
        private static string _PathSeparator = String.Empty;
        public string APIServerBaseAddress { get; set; }
        public string APIKey { get; set; }

        public static string TargetOS
        {
            get
            {
                return System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            }
        }

        public static string PathSeparator
        {
            get
            {
                if (String.IsNullOrEmpty(_PathSeparator))
                {
                    if (TargetOS.Contains("Windows"))
                    {
                        _PathSeparator = "\\";
                    }
                    else
                    {
                        _PathSeparator = "/";
                    }
                }
                return _PathSeparator;
            }
        }

        private readonly RestClient CodaClient;

#pragma warning disable IDE0044 // Add readonly modifier
        private Dictionary<long, JObject> Accounts;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning disable IDE0044 // Add readonly modifier
        private Dictionary<long, JObject> FullAccounts;
#pragma warning restore IDE0044 // Add readonly modifier

        public Client(string WithAPIServer, string WithAPIKey)
        {
            APIServerBaseAddress = WithAPIServer;
            APIKey = WithAPIKey;
            CodaClient = new RestClient(APIServerBaseAddress);
            Accounts = new Dictionary<long, JObject>();
            FullAccounts = new Dictionary<long, JObject>();
        }

        #region "Utility Functions"
        /// <summary>
        /// Creates a request with standard headers
        /// </summary>
        /// <param name="RequestPath"></param>
        /// <param name="RequestMethod"></param>
        /// <param name="Full"></param>
        /// <returns></returns>
        private RestRequest NewRequest(string RequestPath, Method RequestMethod, bool? Full = null)
        {
            var request = new RestRequest(RequestPath, RequestMethod);
            request.AddHeader("apikey", APIKey);
            if (Full != null)
            {
                request.AddHeader("full", Full.ToString()!.ToLower());
            }
            return request;
        }

        /// <summary>
        /// Wrapper for a complete handling of request response return
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        private JObject GetResponse(RestRequest Request)
        {
            var response = CodaClient.ExecuteAsync(Request).Result;
            JObject rsp;
            if (!String.IsNullOrEmpty(response.Content))
            {
                try
                {
                    rsp = JObject.Parse(response.Content.Replace('\\', ' '));
                }
                catch
                {
                    rsp = new JObject()
                    {
                        ["code"] = "error",
                        ["result"] = "Unexpected Response",
                        ["message"] = response.Content
                    };
                }
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    rsp = new JObject()
                    {
                        ["status"] = "Success",
                    };
                }
                else
                {
                    rsp = new JObject
                    {
                        ["code"] = $"{response.StatusCode}",
                        ["message"] = "No data from server",
                    };
                }
            }
            return rsp;
        }

        /// <summary>
        /// Retrieves an Error Log Discussion item
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public JObject GetDiscussion(long DiscussionId)
        {
            var request = NewRequest($"/api/discussion/{DiscussionId}", Method.Get);
            return GetResponse(request);
        }

        /// <summary>
        /// Wrapper for a complete handling of request response return
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        private JArray GetResponseArray(RestRequest Request)
        {
            var response = CodaClient.ExecuteAsync(Request).Result;
            JArray rsp;
            if (!String.IsNullOrEmpty(response.Content))
            {
                try
                {
                    rsp = JArray.Parse(response.Content);

                }
                catch
                {
                    var rsp2 = new JObject()
                    {
                        ["code"] = "error",
                        ["result"] = "Unexpected Response",
                        ["response"] = response.Content
                    };
                    rsp = new JArray()
                    {
                        rsp2
                    };
                }
            }
            else
            {
                rsp = new JArray
                {
                    "No data from server"
                };
            }
            return rsp;
        }

        #endregion


        #region "Account Functions"

        /// <summary>
        /// Determines whether or not the badge is assigned to the account
        /// </summary>
        /// <param name="Account"></param>
        /// <param name="BadgeCode"></param>
        /// <returns></returns>
#pragma warning disable CA1822 // Mark members as static
        public bool AccountHasBadge(JObject Account, string BadgeCode, string Network)
#pragma warning restore CA1822 // Mark members as static
        {
            foreach (JObject badge in (JArray)Account["badges"]!)
            {
                if ($"{badge["badge"]}" == BadgeCode)
                {
                    if (String.IsNullOrEmpty(Network) || (Network == "*"))
                    {
                        return true;
                    }
                    else if ($"{badge["network"]}" == Network)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Validates a Temp Key
        /// </summary>
        /// <param name="ValidationKey"></param>
        /// <returns></returns>
        public JObject ValidateEmailLink(string ValidationKey)
        {
            var request = NewRequest("/api/admin/validatetoken", Method.Get);
            request.AddHeader("validationKey", ValidationKey);
            return GetResponse(request);
        }

        /// <summary>
        /// Marks an account as Inactive
        /// </summary>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        public JObject DeactivateAccount(long AccountId)
        {
            var request = NewRequest($"/api/account/{AccountId}/deactivate", Method.Patch);
            return GetResponse(request);
        }

        /// <summary>
        /// Retrieves info about a member account
        /// </summary>
        /// <param name="AccountId"></param>
        /// <param name="Full">Use 'true' to retrieve all realted info if you are an organization who manages this account</param>
        /// <returns>Server response</returns>
        public JObject GetAccountInfo(long AccountId, bool Full = false)
        {
            if (Full)
            {
                if (FullAccounts.ContainsKey(AccountId))
                {
                    return FullAccounts[AccountId];
                }
            }
            else
            {
                if (Accounts.ContainsKey(AccountId))
                {
                    return Accounts[AccountId];
                }
            }
            var request = NewRequest($"/api/account/{AccountId}", Method.Get, Full);
            var rsp = GetResponse(request);
            if (Full)
            {
                FullAccounts.Add(AccountId, rsp);
            }
            else
            {
                Accounts.Add(AccountId, rsp);
            }
            return rsp;
        }

        /// <summary>
        /// Retrieves your own account info
        /// </summary>
        /// <param name="Full">Use 'true' to retrieve all related info</param>
        /// <returns>Server response</returns>
        public JObject GetMyAccountInfo(bool Full = false)
        {
            var request = NewRequest("/api/account/getmy", Method.Get, Full);
            return GetResponse(request);
        }

        public JArray GetOwnedAccountList(long OwningAccountId, bool Full = false)
        {
            var request = NewRequest($"/api/account/{OwningAccountId}/getmylist", Method.Get, Full);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Performs an update on an account record - set fields to change, leave unchanged fields as null
        /// </summary>
        /// <param name="AccountId"></param>
        /// <param name="Name"></param>
        /// <param name="Email"></param>
        /// <param name="WalletAddress"></param>
        /// <returns>Server response</returns>
        public JObject UpdateAccountInfo(long AccountId, string? Name = null, string? Email = null, string? WalletAddress = null)
        {
            var updateInfo = new JObject();
            if (Name != null)
            {
                updateInfo.Add("name", Name);
            }
            if (Email != null)
            {
                updateInfo.Add("email", Email);
            }
            if (WalletAddress != null)
            {
                updateInfo.Add("walletAddress", WalletAddress);
            }
            if (updateInfo.Count > 0)
            {
                var request = NewRequest($"/api/account/{AccountId}", Method.Patch);
                request.AddBody(updateInfo.ToString(), "text/json");
                return GetResponse(request);
            }
            else
            {
                var result = new JObject
                {
                    { "message", "Request not sent because no data was changed" }
                };
                return result;
            }
        }

        public JObject UpdateAccountOptions(long AccountId, JObject NewOptions)
        {
            var request = NewRequest($"/api/account/{AccountId}/updateoptions", Method.Put);
            request.AddBody(NewOptions.ToString(), "text/json");
            return GetResponse(request);
        }

        /// <summary>
        /// Generates a new API key for the given account.  BEWARE: All previous API keys are invalidated.
        /// </summary>
        /// <param name="AccountId"></param>
        /// <returns>New API key</returns>
        public JObject GenerateNewAPIKey(long AccountId)
        {
            var request = NewRequest($"/api/account/{AccountId}/genapi", Method.Get);
            return GetResponse(request);
        }

        /// <summary>
        /// Creates a new Account (note:  only Administrators can do this)
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Email"></param>
        /// <param name="WalletAddress"></param>
        /// <returns>Newly created account record (API and recovery keys are emailed to the given address)</returns>
        public JObject AddNewAccount(string Name, string Email, string WalletAddress)
        {
            var request = NewRequest("/api/account", Method.Post);
            var data = new JObject
            {
                { "accountName", Name },
                { "email", Email },
                { "walletAddress", WalletAddress }
            };
            request.AddBody(data.ToString(), "text/json");
            return GetResponse(request);
        }

        /// <summary>
        /// Assigns a badge to an account in context of a network
        /// </summary>
        /// <param name="AccountId"></param>
        /// <param name="BadgeCode"></param>
        /// <param name="Network"></param>
        /// <returns></returns>
        public JObject AssignBadge(long AccountId, string BadgeCode, string Network)
        {
            if (String.IsNullOrEmpty(Network))
            {
                Network = "---";
            }
            var request = NewRequest($"/api/account/{AccountId}/assignbadge/{BadgeCode}/{Network}", Method.Put);
            return GetResponse(request);
        }

        public JObject UnassignBadge(long AccountId, string BadgeCode, string Network)
        {
            if (String.IsNullOrEmpty(Network))
            {
                Network = "---";
            }
            var request = NewRequest($"/api/account/{AccountId}/unassignbadge/{BadgeCode}/{Network}", Method.Put);
            return GetResponse(request);
        }

        public JObject SendActivationLink(string Email)
        {
            var request = NewRequest($"/api/account/{Email}/sendactivation", Method.Get);
            return GetResponse(request);
        }

        /// <summary>
        /// Gets a list of all badges
        /// </summary>
        /// <param name="Full"></param>
        /// <returns></returns>
        public JArray GetBadges(bool Full = false)
        {
            var request = NewRequest("/api/badges", Method.Get);
            request.AddHeader("full", Full);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Gets a list of all networks
        /// </summary>
        /// <returns></returns>
        public JArray GetNetworks()
        {
            var request = NewRequest("/api/networks", Method.Get);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Retrieves all info for a given Network
        /// </summary>
        /// <param name="Network"></param>
        /// <returns></returns>
        public JObject GetNetwork(string Network)
        {
            var request = NewRequest($"/api/networks/{Network}", Method.Get);
            request.AddHeader("full", true);
            return GetResponse(request);
        }

        #endregion

        /// <summary>
        /// Gets a list of available Networks in CodaEA
        /// </summary>
        /// <returns></returns>
        public JArray GetListOfNetworks()
        {
            var request = NewRequest("/api/networks", Method.Get);
            return GetResponseArray(request);
        }

        /// <summary>
        /// Gets full info on a specific network
        /// </summary>
        /// <param name="Network"></param>
        /// <returns></returns>
        public JObject GetNetworkInfo(string Network)
        {
            var request = NewRequest($"/api/networks/{Network}", Method.Get);
            return GetResponse(request);
        }

        /// <summary>
        /// Adds a Network to the list
        /// </summary>
        /// <param name="Network"></param>
        /// <param name="DisplayName"></param>
        /// <param name="IconFile"></param>
        /// <returns></returns>
        public bool AddNetwork(string Network, string DisplayName, string IconFile)
        {
            var request = NewRequest($"/api/networks/{Network}", Method.Post);
            request.AddHeader("displayName", DisplayName);
            request.AddHeader("iconFile", IconFile);
            var response = CodaClient.ExecuteAsync(request).Result;
            return (response.StatusCode == System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Sends an email to the account associated with the API key
        /// </summary>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public JObject MailMe(string Subject, string Body)
        {
            var request = NewRequest("/api/account/mailme", Method.Put);
            request.AddHeader("subject", Subject);
            var bodyData = new JObject()
            {
                ["body"] = Body,
            };
            request.AddBody(bodyData.ToString(Newtonsoft.Json.Formatting.None), "text/json");
            return GetResponse(request);
        }

        #region "Destructor"
        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;
            }
        }
        #endregion
    }
}