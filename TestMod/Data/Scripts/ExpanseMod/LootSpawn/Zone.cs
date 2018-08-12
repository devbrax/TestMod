using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame;
using ExpanseMod.Util;
using Sandbox.Game.Entities;
using System;
using ExpanseMod.LootSpawn.Models;

namespace ExpanseMod.LootSpawn
{
    public class Zone
    {
        private string _zoneName { get; set; }
        private BoundingSphereD _zoneBounds { get; set; }
        private bool _hasGPS { get; set; }
        private IMyGps _GPS { get; set; }
        public ZoneScanResult _lastZoneScan { get; set; }

        private void CreateGPS(Vector3D position, string gpsName)
        {
            _GPS = MyAPIGateway.Session.GPS.Create(gpsName,
                                                     "GPS",
                                                     position,
                                                     true, true);

            MyAPIGateway.Session.GPS.AddLocalGps(_GPS);
        }

        private void UpdateStatus()
        {
            _GPS.Name = $"{_zoneName} - P:{_lastZoneScan.Players.Count}";

            //Get all players
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            //Modify existing GPS
            foreach (var player in players)
                MyAPIGateway.Session.GPS.ModifyGps(player.IdentityId, _GPS);
        }

        public Zone(string zoneName, Vector3D position, double radius, bool createGPS = false)
        {
            _zoneName = zoneName;
            _zoneBounds = new BoundingSphereD(position, (double)radius);
            _lastZoneScan = new ZoneScanResult() { Grids = new Dictionary<long, MyCubeGrid>(), Players = new List<long>() };
            _hasGPS = createGPS;

            if (createGPS)
                CreateGPS(position, zoneName);
        }

        public Zone(string zoneName, double x, double y, double z, double radius, bool createGPS = false)
        {
            var position = new Vector3D(x, y, z);
            _zoneName = zoneName;
            _zoneBounds = new BoundingSphereD(position, (double)radius);
            _lastZoneScan = new ZoneScanResult() { Grids = new Dictionary<long, MyCubeGrid>(), Players = new List<long>() };
            _hasGPS = createGPS;

            if (createGPS)
                CreateGPS(position, zoneName);
        }

        public void Update()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            var bounds = _zoneBounds;
            var allEntities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounds);
            
            var gridsInZone = allEntities.Where(e => e is Sandbox.Game.Entities.MyCubeGrid);

            //var playersInZone = players.Where(p => allEntities.Any(e => p.Character. )).Select(p => p.IdentityId);
            //_lastZoneScan.Players.AddRange(playersInZone);

            _lastZoneScan = new ZoneScanResult() { Grids = new Dictionary<long, MyCubeGrid>(), Players = new List<long>() };

            if (gridsInZone.Count() > 0)
            {
                foreach (Sandbox.Game.Entities.MyCubeGrid grid in gridsInZone)
                {
                    var foundPlayer = players.FirstOrDefault(p => p.Controller?.ControlledEntity?.Entity?.GetTopMostParent()?.EntityId == grid.EntityId);
                    if (foundPlayer != null)
                    {
                        _lastZoneScan.Grids[foundPlayer.IdentityId] = grid;
                        _lastZoneScan.Players.Add(foundPlayer.IdentityId);
                    }
                }
            }

            _lastZoneScan.Players = _lastZoneScan.Players.Distinct().ToList();

            if (_hasGPS)
                UpdateStatus();
        }
    }
}
