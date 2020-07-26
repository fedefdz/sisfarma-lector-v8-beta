using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace Lector.Sharp.Wpf.Helpers
{
    internal static class AppProcessHelper
    {
        private static Process process;
        public static Process GetProcess
        {
            get
            {
                return process ?? (process = new Process
                {
                    StartInfo =
                    {
                        FileName = GetShortcutPath(), UseShellExecute = true
                    }
                });
            }
        }

        public static string GetShortcutPath()
            => $@"{Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                    GetPublisher(),
                    //GetDeploymentInfo().Name.Replace(".application", ""))}.appref-ms";
                    "SisFarma Lector")}.appref-ms";

        private static ActivationContext ActivationContext
           => AppDomain.CurrentDomain.ActivationContext;

        public static string GetPublisher()
        {
            XDocument xDocument;
            using (var memoryStream = new MemoryStream(ActivationContext.DeploymentManifestBytes))
            using (var xmlTextReader = new XmlTextReader(memoryStream))
                xDocument = XDocument.Load(xmlTextReader);

            if (xDocument.Root == null)
                return null;

            return xDocument.Root
                            .Elements().First(e => e.Name.LocalName == "description")
                            .Attributes().First(a => a.Name.LocalName == "publisher")
                            .Value;
        }

        public static ApplicationId GetDeploymentInfo()
            => (new ApplicationSecurityInfo(ActivationContext)).DeploymentId;

        private static Mutex instanceMutex;
        public static bool SetSingleInstance()
        {
            bool createdNew;
            instanceMutex = new Mutex(true, @"Local\" + Process.GetCurrentProcess().MainModule.ModuleName, out createdNew);
            return createdNew;
        }

        public static bool ReleaseSingleInstance()
        {
            if (instanceMutex == null) return false;

            instanceMutex.Close();
            instanceMutex = null;

            return true;
        }

        private static bool isRestartDisabled;
        private static bool canRestart;

        public static void BeginReStart()
        {
            // make sure we have the process before we start shutting down
            var proc = GetProcess;
            
            // Note that we can restart only if not
            canRestart = !isRestartDisabled;
            
            // Start the shutdown process
            Application.Current.Shutdown();
        }

        public static void PreventRestart(bool state = true)
        {
            isRestartDisabled = state;
            if (state) canRestart = false;
        }

        public static void RestartIfRequired(int exitCode = 0)
        {
            // make sure to release the instance
            ReleaseSingleInstance();
            
            if (canRestart && process != null)
                //app is restarting...
                process.Start();
            else
                // app is stopping...
                Application.Current.Shutdown(exitCode);
        }
    }
}
