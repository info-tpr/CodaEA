using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Web;
using Task = System.Threading.Tasks.Task;

namespace CodaClientVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CodaAnalyzeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ca9027a9-dbcc-41de-8cf3-3f6f28faf2d3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodaAnalyzeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CodaAnalyzeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CodaAnalyzeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CodaAnalyzeCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CodaAnalyzeCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var codaClient = InitializeClient(sender, e);

            EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(SDTE));
            var errorList = dte.ToolWindows.ErrorList as IErrorList;
            var selected = errorList.TableControl.SelectedEntry;
            object code = null;
            object desc = null;
            if (selected != null)
            {
                selected.TryGetValue("errorcode", out code);
                selected.TryGetValue("text", out desc);
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "No Error selected - select an error and try this action again.",
                    "CodaEA",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK, 
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            // Obtain a web login token
            var token = GetCodaWebToken();
            if (string.IsNullOrEmpty(token))
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "Invalid response from CodaEA API Server - We apologize for the inconvenience.",
                    "CodaEA",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            string vsver = Registry.CurrentUser.OpenSubKey("CodaEA-VSVER").GetValue("vsver").ToString();
            string server = Registry.CurrentUser.OpenSubKey("CodaEA").GetValue("WebServer").ToString();
            string url = $"{server}/CodaEA/ShowError/{HttpUtility.UrlEncode("visualstudio")}/";
            url += $"{HttpUtility.UrlEncode($"{code}")}/{HttpUtility.UrlEncode(vsver)}/";
            url += $"{HttpUtility.UrlEncode($"{desc}")}/?sessionToken={token}";
            Process.Start(url);

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

        private bool KeyExists(RegistryKey baseKey, string subKeyName)
        {
            RegistryKey ret = baseKey.OpenSubKey(subKeyName);

            return ret != null;
        }

        private JObject InitializeClient(object sender, EventArgs e)
        {
            if (!KeyExists(Registry.CurrentUser, "CodaEA"))
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "CodaEA is not configured; please choose 'Configure CodaEA' first",
                    "CodaEA",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return null;
            }
            var retVal = new JObject();
            retVal.Add("server", $"{Registry.CurrentUser.GetValue("CodaEA\\Server")}");
            return retVal;
        }

    }
}
