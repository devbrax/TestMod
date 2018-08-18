using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace ExpanseMod.Util
{
    public static class Logger
    {
        private static bool _debugMode = true;

        public static void Log(string msg)
        {
            MyLog.Default.WriteLineAndConsole("[TESTMOD] [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff") + "] " + msg);
        }

        public static void Display(string msg)
        {
            MyVisualScriptLogicProvider.ShowNotificationToAll(msg, 5000, "White");
        }
    }
}
