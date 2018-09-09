using ExpanseMod.Models;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExpanseMod.Util
{
    public static class PlayerGPSManager
    {
        private static FastResourceLock _lock = new FastResourceLock();
        private static List<TrackedGPS> _localGPS { get; set; }
        private static DateTime _lastTick = DateTime.Now;
        
        private const int UpdateDelay = 10;
        public const ushort GPSMessageId = 51317;
        private const int MaxTooltipSpam = 50;

        public static void Init()
        {
            _localGPS = new List<TrackedGPS>();
            MyAPIGateway.Multiplayer.RegisterMessageHandler(GPSMessageId, GpsCoordsReceived);
        }

        private static void GpsCoordsReceived(byte[] bytes)
        {
            try
            {
                //Ensure the player is executing it
                if (MyAPIGateway.Session.Player.Character == null) return;

                GPSPacket packet = null;
                
                try
                {
                    packet = MyAPIGateway.Utilities.SerializeFromBinary<GPSPacket>(bytes);
                }
                catch(Exception ex)
                {
                    Logger.Log("An exception occured while deserializing! " + ex.Message + " " + ex.StackTrace);
                    return;
                }

                //Ensure we don't already have this point
                if (_localGPS.Any(l => l.gpsPacket.X == packet.X && l.gpsPacket.Y == packet.Y && l.gpsPacket.Z == packet.Z))
                {
                    Logger.Log($"Already have the GPS {packet.Name}");
                    return;
                }

                //var timeLeftInSeconds = (int)(packet.ExpireTime - DateTime.UtcNow).TotalSeconds;
                //MyVisualScriptLogicProvider.AddGPS(packet.Name, "GPS", new Vector3D(packet.X, packet.Y, packet.Z), packet.Color, timeLeftInSeconds);

                var newGPS = MyAPIGateway.Session.GPS.Create(packet.Name,
                                                        "GPS",
                                                        new Vector3D(packet.X, packet.Y, packet.Z),
                                                        true, false);

                Logger.Log($"Adding GPS: {packet.Name} Color: {packet.Color.R} {packet.Color.G} {packet.Color.B}");

                MyAPIGateway.Session.GPS.AddLocalGps(newGPS);
                MyVisualScriptLogicProvider.SetGPSColor(packet.Name, packet.Color);

                packet.ExpireTime = DateTime.Now.AddSeconds(packet.SecondsToLive);
                packet.Hash = newGPS.Hash;

                using (_lock.AcquireExclusiveUsing())
                {
                    _localGPS.Add(new TrackedGPS()
                    {
                        gpsPacket = packet,
                        SpawnedGPS = newGPS
                    });
                }

                //For the first x times a zone spawns for a player, let them know
                if (Utilities.ClientConfig.Zone_SpawnTooltipCount < MaxTooltipSpam)
                {
                    Utilities.DisplayMessageToLocalPlayer($"A new zone has spawned! Be the closest to the center to receive a reward.");
                    Utilities.ClientConfig.Zone_SpawnTooltipCount++;
                    Utilities.SaveClientConfig();
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"An exception occured in GpsCoordsReceived. Ex: {ex.Source} {ex.Message} {ex.StackTrace}");
            }
        }

        public static void Update()
        { 
            if ((DateTime.Now - _lastTick).TotalSeconds >= UpdateDelay)
            {
                _lastTick = DateTime.Now;

                List<TrackedGPS> _copyOfGPS = null;
                var gpsToRemove = new List<int>();

                using (_lock.AcquireSharedUsing())
                    _copyOfGPS = new List<TrackedGPS>(_localGPS);


                //Check if we have any GPS to expire
                foreach (var gps in _copyOfGPS)
                {
                    var timeLeft = (gps.gpsPacket.ExpireTime - DateTime.Now);

                    if (timeLeft.TotalSeconds <= 0)
                    {
                        gpsToRemove.Add(gps.SpawnedGPS.Hash);
                        MyAPIGateway.Session.GPS.RemoveLocalGps(gps.gpsPacket.Hash);
                    }
                    else
                    {
                        var totalMinutes = Math.Round(timeLeft.TotalMinutes);
                        var timeLeftDisplay = (totalMinutes > 0 ? totalMinutes + "m" : "<1m");
                        var newName = gps.gpsPacket.Name + $" T-{timeLeftDisplay}";

                        //Only refresh the GPS when we need to
                        if (gps.SpawnedGPS.Name != newName)
                        {
                            //Refresh GPS by removing an adding. Would be nice if modifygps worked here
                            MyAPIGateway.Session.GPS.RemoveLocalGps(gps.SpawnedGPS);

                            var newGPS = MyAPIGateway.Session.GPS.Create(newName,
                                                               "GPS",
                                                               new Vector3D(gps.gpsPacket.X, gps.gpsPacket.Y, gps.gpsPacket.Z),
                                                               true, false);

                            gps.SpawnedGPS = newGPS;
                            gps.gpsPacket.Hash = newGPS.Hash;

                            MyAPIGateway.Session.GPS.AddLocalGps(newGPS);
                            MyVisualScriptLogicProvider.SetGPSColor(newName, gps.gpsPacket.Color);
                        }

                        //Update color
                        MyVisualScriptLogicProvider.SetGPSColor(gps.SpawnedGPS.Name, gps.gpsPacket.Color);
                    }
                }

                using (_lock.AcquireExclusiveUsing())
                {
                    //Remove the expired GPS
                    _localGPS = _localGPS.Where(t => !gpsToRemove.Any(r => r == t.SpawnedGPS.Hash)).ToList();
                }
            }     
        }
    }
}
