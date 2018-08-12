using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame;
using ExpanseMod.Util;
using Sandbox.Game.Entities;
using System;
using VRage.Game;
using ExpanseMod.Models;

namespace ExpanseMod.LootSpawn
{
    public class Zone
    {
        private string _zoneName { get; set; }
        private Vector3D _zonePosition { get; set; }
        private BoundingSphereD _zoneBounds { get; set; }
        private bool _hasGPS { get; set; }
        private IMyGps _GPS { get; set; }

        public ZoneScanResults _lastZoneScan { get; set; }

        private void CreateGPS(Vector3D position, string gpsName)
        {
            _GPS = MyAPIGateway.Session.GPS.Create(gpsName,
                                                     "GPS",
                                                     position,
                                                     true, true);

            MyAPIGateway.Session.GPS.AddLocalGps(_GPS);
        }

        private void UpdateStatus(List<IMyPlayer> players)
        {
            var foundShip = _lastZoneScan.FoundShips.FirstOrDefault();
         
            _GPS.Name = $"{_zoneName} - P:{_lastZoneScan.CharactersFound} S:{_lastZoneScan.ShipsFound} Dist:{(foundShip.Value != null ? foundShip.Value.DistanceToCenter : -1)}";

            //Modify existing GPS
            foreach (var player in players)
                MyAPIGateway.Session.GPS.ModifyGps(player.IdentityId, _GPS);
        }

        public Zone(string zoneName, Vector3D position, double radius, bool createGPS = false)
        {
            _zoneName = zoneName;
            _zonePosition = position;
            _zoneBounds = new BoundingSphereD(_zonePosition, (double)radius);
            _lastZoneScan = new ZoneScanResults(_zonePosition);
            _hasGPS = createGPS;

            if (createGPS)
                CreateGPS(position, zoneName);
        }

        public Zone(string zoneName, double x, double y, double z, double radius, bool createGPS = false)
        {
            var position = new Vector3D(x, y, z);
            _zoneName = zoneName;
            _zonePosition = position;
            _zoneBounds = new BoundingSphereD(_zonePosition, (double)radius);
            _lastZoneScan = new ZoneScanResults(_zonePosition);
            _hasGPS = createGPS;

            if (createGPS)
                CreateGPS(position, zoneName);
        }

        //TODO: Don't require players
        public ZoneScanResults Scan(List<IMyPlayer> players)
        {
            //Reset the last scan
            _lastZoneScan = new ZoneScanResults(_zonePosition);

            //Copy the zone bounds as we need to pass it by ref
            var bounds = _zoneBounds;
            var allEntities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounds);

            //Get all cube grids
            var gridsInZone = allEntities.Where(e => e is Sandbox.Game.Entities.MyCubeGrid);

            //Get all characters
            var playersInZone = allEntities.Where(e => e is IMyCharacter).Select(e => (IMyCharacter)e);

            //Add players to the found list
            foreach (var player in playersInZone)
            {
                var character = player.GetObjectBuilder() as MyObjectBuilder_Character;
                if (character != null && character.OwningPlayerIdentityId.HasValue)
                    _lastZoneScan.FoundPlayer(character.OwningPlayerIdentityId.Value, character);
            }

            //Add ships to the found list
            foreach (Sandbox.Game.Entities.MyCubeGrid grid in gridsInZone)
            {
                var foundPlayer = players.FirstOrDefault(p => p.Controller?.ControlledEntity?.Entity?.GetTopMostParent()?.EntityId == grid.EntityId);
                if (foundPlayer != null)
                    _lastZoneScan.FoundPilotedShip(foundPlayer.IdentityId, grid);
            }

            return _lastZoneScan;
        }

        public void Update()
        {
            //TODO: We can probably used a cached player list to speed this up
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            Logger.Log("Doing scan...");

            Scan(players);

            Logger.Log($"Scan complete. Found {_lastZoneScan.FoundShips.Count} ships");

            //TODO: Check if we should give reward

            //TODO: Seems redundant to send the scan results to both
            //Do the default behavior for now
            var resolver = new ZoneOutcomeResolver(_lastZoneScan, new ZoneOutcomeResolverOptions(_lastZoneScan));

            var outcome = resolver.Resolve();

            if (_hasGPS)
                UpdateStatus(players);
        }
    }
}
