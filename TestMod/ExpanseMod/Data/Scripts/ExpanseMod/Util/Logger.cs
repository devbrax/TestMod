using Sandbox.Game;
using Sandbox.ModAPI;
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
        public static void Log(string msg)
        {
            var whoAmI = MyAPIGateway.Session.IsServer ? "Server" : "Player ID: " + (MyAPIGateway.Session.Player != null ? MyAPIGateway.Session.Player.IdentityId : -1);
            
            MyLog.Default.WriteLineAndConsole($"[TESTMOD] [{whoAmI}] {msg}");
        }

        public static void Display(string msg)
        {
            MyVisualScriptLogicProvider.ShowNotificationToAll(msg, 5000, "White");
        }
    }
}
