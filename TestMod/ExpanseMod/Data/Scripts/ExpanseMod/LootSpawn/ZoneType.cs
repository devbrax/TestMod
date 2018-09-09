using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.ModAPI;
using ExpanseMod.Util;
using System;
using ExpanseMod.Models;
using System.Linq;

namespace ExpanseMod.LootSpawn
{
    public abstract class ZoneType
    {
        public enum ZoneTypes { Military, Industrial, Science };
        public enum ZoneUpdateResult { Success, Failure, Timeout };

        protected Vector3D _zonePosition { get; set; }
        protected BoundingSphereD _zoneBounds { get; set; }
        protected bool _hasGPS { get; set; }
        protected IMyGps _GPS { get; set; }
        protected TimeSpan _timeToLive; //Default of 10 minutes

        public Vector3D _zoneOrigin { get; set; }
        public string _zoneName { get; set; }
        public DateTime _expireTime { get; set; }
        public ZoneScanResults _lastZoneScan { get; set; }
        public Color Color { get; set; }
        public ZoneTypes Type { get; set; }

        private void init(ZoneTypes type, string zoneName, Vector3D origin, Vector3D position, double radius, TimeSpan timeToLive, Color color, bool createGPS = true)
        {
            _zoneName = zoneName;
            _zoneOrigin = origin;
            _zonePosition = position;
            _zoneBounds = new BoundingSphereD(_zonePosition, (double)radius);
            _lastZoneScan = new ZoneScanResults(_zonePosition);
            _hasGPS = createGPS;

            _expireTime = DateTime.Now.Add(timeToLive);
            _timeToLive = timeToLive;

            Type = type;

            if (createGPS)
            {
                ServerGPSManager.Server_AddGlobalGPS(position.X, position.Y, position.Z, zoneName, (_expireTime - DateTime.Now).TotalSeconds, color);
            }
        }

        public ZoneType(ZoneTypes type, string zoneName, Vector3D origin, Vector3D position, double radius, TimeSpan timeToLive, Color color, bool createGPS = true)
        {
            init(type, zoneName, origin, position, radius, timeToLive, color, createGPS);
        }
            
        //TODO: Don't require players
        public ZoneScanResults Scan(List<IMyPlayer> players)
        {
            var oldFoundShips = _lastZoneScan.FoundShips;

            _lastZoneScan = Utilities.Scan(players, _zonePosition, _zoneBounds);

            var newFoundShips = _lastZoneScan.FoundShips.Where(newPlayer => !oldFoundShips.ContainsKey(newPlayer.Key));
            if(newFoundShips.Count() > 0)
            {
                Logger.Log($"Found {newFoundShips.Count()} new players!");
                NewPlayersInZone(newFoundShips);
            }

            return _lastZoneScan;
        }

        public virtual void NewPlayersInZone(IEnumerable<KeyValuePair<long,ZoneScanResultItem>> newPlayers)
        {
            //TODO: Each Zone should have it's own way of handling this
            foreach(var player in newPlayers)
            {
                Utilities.DisplayMessageToPlayer(player.Key, $"You have entered a {_zoneName}! Be the closest to the center to gain resources.", "Red");
            }
        }

        public virtual ZoneUpdateResult Update(List<IMyPlayer> players)
        {
            //Check if expireTime is defined and see if we need to expire
            if (_expireTime != DateTime.MinValue && DateTime.Now.CompareTo(_expireTime) > 0)
                return ZoneUpdateResult.Timeout;

            Scan(players);

            return ZoneUpdateResult.Success;
        }

        public Vector3D GetPosition()
        {
            return _zonePosition;
        }
    }
}
