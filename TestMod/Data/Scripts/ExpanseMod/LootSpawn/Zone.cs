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
        //The amount of seconds left before the T:-xx seconds display is added to the GPS
        private const int _minSecondsToShowCountdown = 45;
        private string _zoneName { get; set; }
        private Vector3D _zonePosition { get; set; }
        private BoundingSphereD _zoneBounds { get; set; }
        private bool _hasGPS { get; set; }
        private IMyGps _GPS { get; set; }
        public DateTime _expireTime { get; set; }

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
            var additionalText = string.Empty;

            if(_lastZoneScan.ShipsFound > 0)
                additionalText = _lastZoneScan.ShipsFound.ToString();

            if (_expireTime != DateTime.MinValue)
            {
                var timeLeft = (_expireTime - DateTime.Now);
                if (timeLeft.Seconds < _minSecondsToShowCountdown)
                {
                    additionalText += (additionalText.Length > 0 ? " " : string.Empty) + $"T:-{timeLeft.Seconds}";
                }
            }

            if(!string.IsNullOrEmpty(additionalText))
                _GPS.Name = $"{_zoneName} - {additionalText}";
            else
                _GPS.Name = _zoneName;

            //Modify existing GPS
            foreach (var player in players)
                MyAPIGateway.Session.GPS.ModifyGps(player.IdentityId, _GPS);
        }

        private void init(string zoneName, double x, double y, double z, double radius, bool createGPS = true)
        {
            var position = new Vector3D(x, y, z);
            _zoneName = zoneName;
            _zonePosition = position;
            _zoneBounds = new BoundingSphereD(_zonePosition, (double)radius);
            _lastZoneScan = new ZoneScanResults(_zonePosition);
            _hasGPS = createGPS;
            _expireTime = DateTime.MinValue;

            if (createGPS)
                CreateGPS(position, zoneName);
        }

        public Zone(string zoneName, double x, double y, double z, double radius, bool createGPS = true)
        {
            init(zoneName, x, y, z, radius, createGPS);

        }

        public Zone(string zoneName, double x, double y, double z, double radius, TimeSpan timeToLive, bool createGPS = true)
        {
            init(zoneName, x, y, z, radius, createGPS);
            _expireTime = DateTime.Now.Add(timeToLive);
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

        public bool Update(List<IMyPlayer> players)
        {
            //Check if expireTime is defined and see if we need to expire
            if (_expireTime != DateTime.MinValue && DateTime.Now.CompareTo(_expireTime) > 0)
            {
                if(_hasGPS)
                    MyAPIGateway.Session.GPS.RemoveLocalGps(_GPS);
                return false;
            }

            Scan(players);

            if (_hasGPS)
                UpdateStatus(players);

            return true;
        }
    }
}
