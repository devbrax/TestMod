using ExpanseMod.LootSpawn;
using ExpanseMod.Models;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExpanseMod.Util
{
    public static class ServerGPSManager
    {

        public static void Server_AddGlobalGPS(double x, double y, double z, string name, double secondsToLive, Color color)
        {
            try
            {
                Logger.Log($"Sending GPS to clients. {name} Color: {color.R} {color.G} {color.B} {color.A}");
                var newGPS = new GPSPacket(x, y, z, name, secondsToLive, color.R, color.G, color.B);
                var dataPacket = MyAPIGateway.Utilities.SerializeToBinary(newGPS);

                //Send to the clients the GPS
                MyAPIGateway.Multiplayer.SendMessageToOthers(PlayerGPSManager.GPSMessageId, dataPacket);
            }
            catch(Exception ex)
            {
                Logger.Log("An exception was thrown when trying to send GPS to players. Ex: " + ex.Message + " " + ex.StackTrace);
            }

        }

        //public static void Server_AddGPS(ulong playerSteamId, double x, double y, double z, string name, double secondsToLive)
        //{
        //    try
        //    {
        //        var newGPS = new GPSPacket(x, y, z, name, secondsToLive);
        //        var dataPacket = MyAPIGateway.Utilities.SerializeToBinary(newGPS);

        //        //Send to the client the GPS
        //        MyAPIGateway.Multiplayer.SendMessageTo(PlayerGPSManager.GPSMessageId, dataPacket, playerSteamId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("An exception was thrown when trying to send GPS to players. Ex: " + ex.Message + " " + ex.StackTrace);
        //    }
        //}


        //public static void Server_SendAllGPSToPlayer(ulong playerSteamId, ZoneManager zoneManager)
        //{
        //    var zones = zoneManager.GetActiveZones();
        //    foreach(var zone in zones)
        //    {
        //        var pos = zone.GetPosition();
        //        var expireTimeInSeconds = (zone._expireTime - DateTime.Now).TotalSeconds;
        //        Server_AddGPS(playerSteamId, pos.X, pos.Y, pos.Z, zone._zoneName, expireTimeInSeconds);
        //    }
        //}
    }
}
