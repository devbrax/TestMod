using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.ModAPI;
using ExpanseMod.Util;
using System;
using ExpanseMod.Models;

namespace ExpanseMod.LootSpawn
{
    public abstract class ZoneType
    {
        public enum ZoneUpdateResult { Success, Failure, Timeout };
        //The amount of seconds left before the T:-xx seconds display is added to the GPS
        protected const int _minSecondsToShowCountdown = 45;
        
        protected Vector3D _zonePosition { get; set; }
        protected BoundingSphereD _zoneBounds { get; set; }
        protected bool _hasGPS { get; set; }
        protected IMyGps _GPS { get; set; }

        public string _zoneName { get; set; }
        public DateTime _expireTime { get; set; }
        public ZoneScanResults _lastZoneScan { get; set; }


        protected abstract void UpdateGPS(List<IMyPlayer> players);

        private void init(string zoneName, Vector3D position, double radius, bool createGPS = true)
        {
            _zoneName = zoneName;
            _zonePosition = position;
            _zoneBounds = new BoundingSphereD(_zonePosition, (double)radius);
            _lastZoneScan = new ZoneScanResults(_zonePosition);
            _hasGPS = createGPS;
            _expireTime = DateTime.MinValue;

            if (createGPS)
                _GPS = Utilities.CreateGPS(position, zoneName);
        }

        public ZoneType(string zoneName, Vector3D position, double radius, TimeSpan timeToLive, bool createGPS = true)
        {
            init(zoneName, position, radius, createGPS);
            _expireTime = DateTime.Now.Add(timeToLive);
        }

        //TODO: Don't require players
        public ZoneScanResults Scan(List<IMyPlayer> players)
        {
            _lastZoneScan = Utilities.Scan(players, _zonePosition, _zoneBounds);
            return _lastZoneScan;
        }

        public virtual ZoneUpdateResult Update(List<IMyPlayer> players)
        {
            //Check if expireTime is defined and see if we need to expire
            if (_expireTime != DateTime.MinValue && DateTime.Now.CompareTo(_expireTime) > 0)
            {
                if (_hasGPS)
                    MyAPIGateway.Session.GPS.RemoveLocalGps(_GPS);

                return ZoneUpdateResult.Timeout;
            }

            //TODO: Set up how often the zone scans for
            Scan(players);

            if (_hasGPS)
                UpdateGPS(players);

            return ZoneUpdateResult.Success;
        }
    }
}
