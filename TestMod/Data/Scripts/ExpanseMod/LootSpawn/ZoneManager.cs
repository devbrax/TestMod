using ExpanseMod.Models;
using ExpanseMod.Util;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExpanseMod.LootSpawn
{
    public class ZoneManager
    {
        private List<Vector3D> _spawnAreas;
        private List<ZoneType> _activeZones;
        private int _minZonesToMaintain;
        private string _zoneNamePrefix;
        private Random _rand;
        private double _zoneRadius;
        private DateTime _zoneLastSpawnTime = DateTime.MinValue;
        private TimeSpan _zoneSpawnDelay = new TimeSpan(0,10,0); // Default of 10 minutes
        private bool _initialSetupComplete = false;

        public ZoneManager(List<Vector3D> spawnAreas, string zoneNamePrefix, double zoneRadius, int minZonesToMaintain = 3, TimeSpan? zoneSpawnDelay = null)
        {
            _minZonesToMaintain = minZonesToMaintain;
            _activeZones = new List<ZoneType>();
            _spawnAreas = spawnAreas;
            _zoneNamePrefix = zoneNamePrefix;
            _rand = new Random();
            _zoneRadius = zoneRadius;

            if (zoneSpawnDelay.HasValue)
                _zoneSpawnDelay = zoneSpawnDelay.Value;
        }

        public List<ZoneType> GetActiveZones()
        {
            return _activeZones;
        }

        public void Update(List<IMyPlayer> players)
        {
            var zonesToRemove = new List<ZoneType>();

            foreach(var zone in _activeZones)
                //Update and check if the zone has expired
                if (zone.Update(players) == ZoneType.ZoneUpdateResult.Timeout)
                    zonesToRemove.Add(zone);

            //Remove flagged zones
            foreach(var zoneToRemove in zonesToRemove)
                _activeZones.Remove(zoneToRemove);

            //Check if we need to spawn any new zones and if we should wait
            if (_activeZones.Count < _minZonesToMaintain 
                && (_zoneLastSpawnTime.Add(_zoneSpawnDelay).CompareTo(DateTime.Now) < 0
                    || _initialSetupComplete == false))
            {
                //Try and spawn a zone
                var position = GetAvailableSpawnZone(_spawnAreas[_rand.Next(0, _spawnAreas.Count)], _zoneRadius);
                if (position != Vector3D.MinValue)
                {
                    //Create the new zone
                    var zone = new ConflictZone(position, _zoneRadius);

                    //Add to tracked zones
                    _activeZones.Add(zone);
                }
            }

            //Complete initial setup if we have 3
            if (_activeZones.Count == _minZonesToMaintain)
            {
                _initialSetupComplete = true;

                //Keep track of the last time we spawned enough zones
                _zoneLastSpawnTime = DateTime.Now;
            }
        }

        public Vector3D GetAvailableSpawnZone(Vector3D position, double radius, double randomOffset = 50000, int maxAttempts = 5)
        {
            var attempts = 0;

            do
            {
                //Check if the zone intersects with anything
                var randX = position.X + ((_rand.NextDouble() - .5) * (randomOffset * 2));
                var randY = position.Y + ((_rand.NextDouble() - .5) * (randomOffset * 2));
                var randZ = position.Z + ((_rand.NextDouble() - .5) * (randomOffset * 2));

                var bounds = new BoundingSphereD(new Vector3D(randX, randY, randZ), radius);
                var allEntities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounds);

                if (allEntities.Count == 0)
                {
                    return new Vector3D(randX, randY, randZ);
                }

                attempts++;
                Logger.Log($"Couldn't spawn zone at {randX}, {randY}, {randZ} trying again. Attempt # {attempts}");
            } while (attempts < maxAttempts);

            return Vector3D.MinValue;
        }
    }
}
