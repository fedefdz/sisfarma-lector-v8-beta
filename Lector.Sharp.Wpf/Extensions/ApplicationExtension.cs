using Lector.Sharp.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows
{
    public static class ApplicationExtension
    {
        public static bool SetSingleInstance(this Application app)
            => AppProcessHelper.SetSingleInstance();

        public static bool ReleaseSingleInstance(this Application app)
            => AppProcessHelper.ReleaseSingleInstance();

        public static void BeginReStart(this Application app)
            => AppProcessHelper.BeginReStart();

        public static void PreventRestart(this Application app, bool state = true)
            => AppProcessHelper.PreventRestart(state);

        public static void RestartIfRequired(this Application app)
            => AppProcessHelper.RestartIfRequired();

        public static string Version(this Application app)
            => Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
