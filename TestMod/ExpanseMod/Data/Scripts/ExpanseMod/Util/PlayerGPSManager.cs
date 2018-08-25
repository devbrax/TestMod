using ExpanseMod.Models;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace ExpanseMod.Util
{
    public static class PlayerGPSManager
    {
        public static List<GPS> TrackedGPS { get; set; }

        public const ushort GPSMessageId = 51317;

        public static void Init()
        {
            TrackedGPS = new List<GPS>();
            MyAPIGateway.Multiplayer.RegisterMessageHandler(GPSMessageId, GpsCoordsReceived);
        }

        private static bool GPSAreEqual(GPS gps, IMyGps gps2)
        {
            return (gps.X == gps2.Coords.X && gps.Y == gps2.Coords.Y && gps.Z == gps2.Coords.Z);
        }


        private static bool GPSAreEqual(GPS gps, GPS gps2)
        {
            return (gps.X == gps2.X && gps.Y == gps2.Y && gps.Z == gps2.Z);
        }

        private static void GpsCoordsReceived(byte[] bytes)
        {
            try
            {
                //Ensure the player is executing it
                if (MyAPIGateway.Session.Player.Character == null) return;

                GPSPacket packet = null;
                List<GPS> remoteGPS = new List<GPS>();
                try
                {
                    packet = MyAPIGateway.Utilities.SerializeFromBinary<GPSPacket>(bytes);
                    remoteGPS = packet.TrackedGPS;
                }
                catch(Exception ex)
                {
                    //If we got nothing that means no GPS are left
                }

                //Check if a GPS has been removed and do the same
                var toBeRemovedGps = TrackedGPS.Where(g => !remoteGPS.Any(t => GPSAreEqual(t,g))).ToList();

                if (toBeRemovedGps.Count > 0)
                {
                    foreach (var gps in toBeRemovedGps)
                    {
                        MyAPIGateway.Session.GPS.RemoveLocalGps(gps.Hash);
                        TrackedGPS.Remove(gps);
                    }
                }

                //Check if we're missing 
                var toBeAdded = remoteGPS.Where(t => !TrackedGPS.Any(g => GPSAreEqual(t, g))).ToList();

                if (toBeAdded.Count > 0)
                {
                    foreach (var gps in toBeAdded)
                    {
                        var newGPS = MyAPIGateway.Session.GPS.Create(gps.Name,
                                                             "GPS",
                                                             new VRageMath.Vector3D(gps.X, gps.Y, gps.Z),
                                                             true, false);

                        MyAPIGateway.Session.GPS.AddLocalGps(newGPS);
                        TrackedGPS.Add(gps);
                    }
                }


                //Update existing
                foreach (var gps in remoteGPS)
                {
                    var foundLocalGPS = TrackedGPS.FirstOrDefault(t => GPSAreEqual(t, gps));

                    var newGPS = MyAPIGateway.Session.GPS.Create(gps.Name,
                                                         "GPS",
                                                         new VRageMath.Vector3D(gps.X, gps.Y, gps.Z),
                                                         true, false);

                    MyAPIGateway.Session.GPS.RemoveLocalGps(foundLocalGPS.Hash);
                    foundLocalGPS.Hash = newGPS.Hash;
                    MyAPIGateway.Session.GPS.AddLocalGps(newGPS);
                    
                }

            }
            catch(Exception ex)
            {
                Logger.Log($"An exception occured in GpsCoordsReceived. Ex: {ex.Source} {ex.Message} {ex.StackTrace}");
            }
        }

        public static void Server_UpdateGPS(IMyGps updatedGPS)
        {
            var foundGps = TrackedGPS.FirstOrDefault(t => GPSAreEqual(t,updatedGPS));
            if (foundGps != null)
            {
                foundGps.Hash = updatedGPS.Hash;
                foundGps.Name = updatedGPS.Name;
            }

            var listGps = TrackedGPS.FirstOrDefault(t => GPSAreEqual(t, updatedGPS));
        }

        public static void Server_AddGlobalGPS(IMyGps GPS, DateTime expireTime)
        {
            var newGPS = new GPS(GPS.Coords.X, GPS.Coords.Y, GPS.Coords.Z, GPS.Name, GPS.Hash);
            TrackedGPS.Add(newGPS);
            MyAPIGateway.Session.GPS.AddLocalGps(GPS);
            Server_SyncGPS();
        }

        public static void Server_RemoveGlobalGPS(IMyGps GPS)
        {
            TrackedGPS = TrackedGPS.Where(t => !GPSAreEqual(t, GPS)).ToList();
            MyAPIGateway.Session.GPS.RemoveLocalGps(GPS);
            Server_SyncGPS();
        }


        public static void Server_SyncGPS()
        {
            try
            {
                var dataPacket = MyAPIGateway.Utilities.SerializeToBinary(new GPSPacket() { TrackedGPS = TrackedGPS });

                //Send to the clients the GPS
                MyAPIGateway.Multiplayer.SendMessageToOthers(GPSMessageId, dataPacket);
            }
            catch(Exception ex)
            {
                Logger.Log($"GPS Sync failed with an exception: {ex.TargetSite} {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
