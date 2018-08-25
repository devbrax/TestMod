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

        protected Vector3D _zonePosition { get; set; }
        protected BoundingSphereD _zoneBounds { get; set; }
        protected bool _hasGPS { get; set; }
        protected IMyGps _GPS { get; set; }
        protected TimeSpan _timeToLive; //Default of 10 minutes

        public Vector3D _zoneOrigin { get; set; }
        public string _zoneName { get; set; }
        public DateTime _expireTime { get; set; }
        public ZoneScanResults _lastZoneScan { get; set; }

        protected abstract void UpdateGPS(List<IMyPlayer> players);

        private void init(string zoneName, Vector3D origin, Vector3D position, double radius, TimeSpan timeToLive, bool createGPS = true)
        {
            _zoneName = zoneName;
            _zoneOrigin = origin;
            _zonePosition = position;
            _zoneBounds = new BoundingSphereD(_zonePosition, (double)radius);
            _lastZoneScan = new ZoneScanResults(_zonePosition);
            _hasGPS = createGPS;

            _expireTime = DateTime.Now.Add(timeToLive);
            _timeToLive = timeToLive;

            if (createGPS)
            {
                _GPS = MyAPIGateway.Session.GPS.Create(zoneName,
                                                     "GPS",
                                                     position,
                                                     true, false);

                PlayerGPSManager.Server_AddGlobalGPS(_GPS, _expireTime);
            }
        }

        public ZoneType(string zoneName, Vector3D origin, Vector3D position, double radius, TimeSpan timeToLive, bool createGPS = true)
        {
            init(zoneName, origin, position, radius, timeToLive, createGPS);
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
                    PlayerGPSManager.Server_RemoveGlobalGPS(_GPS);

                return ZoneUpdateResult.Timeout;
            }

            Scan(players);

            if (_hasGPS)
                UpdateGPS(players);

            return ZoneUpdateResult.Success;
        }
    }
}
