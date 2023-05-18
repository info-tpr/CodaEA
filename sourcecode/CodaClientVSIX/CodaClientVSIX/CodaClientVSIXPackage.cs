using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace CodaClientVSIX
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CodaClientVSIXPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(CodaToolWindow))]
    public sealed class CodaClientVSIXPackage : AsyncPackage
    {
        /// <summary>
        /// CodaClientVSIXPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "5ea4dc4a-e0e0-4e50-b882-6910c760a820";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            var codaKey = Registry.CurrentUser.CreateSubKey("CodaEA-VSVER");
            codaKey.CreateSubKey("vsver", true);
            codaKey.SetValue("vsver", dte.Version);
            await CodaToolWindowCommand.InitializeAsync(this);
            await CodaAnalyzeCommand.InitializeAsync(this);
            await CodaConfigCommand.InitializeAsync(this);

            var autoEvent = new AutoResetEvent(false);
            var statusChecker = new StatusChecker(60);
            adtimer = new Timer(statusChecker.CheckStatus, autoEvent, 60000, 1000);
        }

        private bool KeyExists(RegistryKey baseKey, string subKeyName)
        {
            RegistryKey ret = baseKey.OpenSubKey(subKeyName);

            return ret != null;
        }

        #endregion

        #region Advertising
        private Timer adtimer = null;

        #endregion
    }

    public class StatusChecker
    {
        private int invokeCount;
        private int maxCount;

        public StatusChecker(int count)
        {
            invokeCount = 0;
            maxCount = count;
        }

        // This method is called by the timer delegate.
        public void CheckStatus(Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            Trace.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} Checking status {++invokeCount}.");

            if (invokeCount == maxCount)
            {
                // TODO: Build workflow for checking for messages

                // Reset the counter and signal the waiting thread.
                // MessageBox.Show("Booya");
                invokeCount = 0;
                autoEvent.Set();
            }
        }
    }
}
