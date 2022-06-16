/*-----------------------------------
 * Plugin handling
 */

using CodaClient.Plugin;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Diagnostics;
using CodaRESTClient;

namespace codaclient.classes
{
    partial class Program
    {
        private static Dictionary<string, ICodaPlugin>? _Plugins;
        private static JObject? _PluginList;

        /// <summary>
        /// Loads or re-loads plugins
        /// </summary>
        static void LoadPlugins(JObject Configuration)
        {
            try
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "START-0005", "Loading Plugins...", ErrorLogSeverityEnum.Debug);
                _Plugins = new Dictionary<string, ICodaPlugin>();
                _PluginList = LoadPluginList();
                foreach (var plugin in _PluginList)
                {
                    var assembly = LoadPlugin((JObject)plugin.Value!, Configuration);
                    if (assembly is not null)
                    {
                        try
                        {
                            _Plugins.Add(plugin.Key, CreateObject(Configuration, assembly, (JObject)plugin.Value!));
                        }
                        catch (Exception ex)
                        {
                            LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PLG-0001", $"Error loading Plugin {plugin.Key}: {ex.HResult} {ex.Message}", ErrorLogSeverityEnum.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PLG-0002", $"Error loading Plugins: {ex.HResult} {ex.Message}", ErrorLogSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Instantiates the Plugin
        /// </summary>
        /// <param name="PluginAssembly"></param>
        /// <param name="PluginInfo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static ICodaPlugin CreateObject(JObject Configuration, Assembly PluginAssembly, JObject PluginInfo)
        {
            string pluginClass = Path.GetExtension($"{PluginInfo["className"]}")[1..];
            foreach (Type assmType in PluginAssembly.GetTypes())
            {
                if (assmType.Name == pluginClass)
                {
                    ICodaPlugin? result = Activator.CreateInstance(assmType) as ICodaPlugin;
                    if (result is not null)
                    {
                        result.UTCLastRunDate = null;
                        try
                        {
                            result.UTCLastRunDate = Convert.ToDateTime(Configuration["analysis"]!["lastRunDate"]);
                        } catch { }
                        LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "START-0006", $"Loaded Plugin {result.Name} v {result.Version} ({result.Description})", ErrorLogSeverityEnum.Debug);
                        return result;
                    }
                }
            }
            throw new ApplicationException($"Cannot instantiate {PluginInfo["className"]} - check the assembly, namespace and interface class name");
        }

        private static Assembly? LoadPlugin(JObject Plugin, JObject Configuration)
        {
            try
            {
                string pluginNamespace = Path.GetFileNameWithoutExtension($"{Plugin["className"]}");
                string pluginClass = Path.GetExtension($"{Plugin["className"]}")[1..];
                string pluginLocation = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Client.PathSeparator}plugins{Client.PathSeparator}{pluginNamespace}.dll";
                Trace.WriteLine($"Namespace: {pluginNamespace}");
                Trace.WriteLine($"Class: {pluginClass}");
                Trace.WriteLine($"Location: {pluginLocation}");
                var loadContext = new PluginLoadContext(pluginLocation);
                return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));

            }
            catch (Exception ex)
            {
                LogMessage(Configuration, MethodBase.GetCurrentMethod()!.Name, "PLG-0002", $"Error loading Plugin {Plugin["className"]}: {ex.HResult} {ex.Message}", ErrorLogSeverityEnum.Error);
                return null;
            }
        }

        private static JObject LoadPluginList()
        {
            var pluginFile = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Client.PathSeparator}CodaClient.Plugins.json";
            var sr = new StreamReader(pluginFile);
            var json = sr.ReadToEnd();
            sr.Close();
            return JObject.Parse(json);
        }

        private static void ListPlugins(JObject MyAccount)
        {
            CConsole.WriteLine($"{"================= Active Plugin List =============[Current User:":cyan} {MyAccount["accountName"]:blue}{"]=":cyan}");
            if (_Plugins is null || _Plugins.Count == 0)
            {
                CConsole.WriteLine($"{"==== NO PLUGINS CONFIGURED ====":red}");
            }
            else
            {
                int count = 0;
                foreach (var plugin in _Plugins)
                {
                    CConsole.WriteLine($"{"---- #":yellow} {++count:yellow} {"----":yellow}");
                    CConsole.Write($"{"ID:":cyan} {plugin.Key}     ");
                    CConsole.Write($"{"Name:":cyan} {plugin.Value.Name}     ");
                    CConsole.WriteLine($"{"Version:":cyan} {plugin.Value.Version}");
                    CConsole.WriteLine($"{"Description:":cyan} {plugin.Value.Description}");
                    CConsole.WriteLine($"{"---------------------------------------------------------":cyan}");
                }
            }
        }

        private static void ManagePlugins(string Breadcrumbs, JObject Configuration, JObject MyAccount)
        {
            var menu = "D)isable/enable # plugin R)emove # [permanently] L)oad/refresh plugins Q)uit";
            string input;
            do
            {
                Console.Clear();
                ShowBreadcrumbs(Breadcrumbs);
                ListPlugins(MyAccount);
                input = ShowMenu(Configuration, menu);
                if (input.ToUpper().StartsWith("D"))
                {
                    try
                    {
                        var index = Convert.ToInt16(input[2..]);
                        DisablePlugin(Breadcrumbs + "/Unload Plugin", Configuration, MyAccount);
                    }
                    catch
                    {
                        Pause("Invalid number, press ENTER to continue");
                    }
                }
                else if (input.ToUpper().StartsWith("R"))
                {
                    try
                    {
                        var index = Convert.ToInt16(input[2..]);
                        RemovePlugin(Breadcrumbs + "/Remove Plugin", Configuration, MyAccount);
                    }
                    catch
                    {
                        Pause("Invalid number, press ENTER to continue");
                    }
                }
                else if (input.ToUpper() == "L")
                {
                    LoadPlugins(Configuration);
                }
            } while (input.ToUpper() != "Q");
        }

        private static void RemovePlugin(string v, JObject configuration, JObject myAccount)
        {
            throw new NotImplementedException();
        }

        private static void DisablePlugin(string v, JObject configuration, JObject myAccount)
        {
            throw new NotImplementedException();
        }
    }
}
