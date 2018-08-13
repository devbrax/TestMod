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
        private List<Zone> _activeZones;
        private int _minZonesToMaintain;
        private string _zoneNamePrefix;
        private Random _rand;
        private double _zoneRadius;
        private TimeSpan _zoneTimeToLive;

        public ZoneManager(List<Vector3D> spawnAreas, string zoneNamePrefix, double zoneRadius, TimeSpan zoneTimeToLive, int minZonesToMaintain = 3)
        {
            _minZonesToMaintain = minZonesToMaintain;
            _activeZones = new List<Zone>();
            _spawnAreas = spawnAreas;
            _zoneNamePrefix = zoneNamePrefix;
            _rand = new Random();
            _zoneRadius = zoneRadius;
            _zoneTimeToLive = zoneTimeToLive;
        }

        public List<Zone> GetActiveZones()
        {
            return _activeZones;
        }

        public void Update(List<IMyPlayer> players)
        {
            var zonesToRemove = new List<Zone>();

            foreach(var zone in _activeZones)
            {
                //Update and check if the zone has expired
                if (!zone.Update(players))
                {
                    //Zone is expiring so process results and give rewards
                    //TODO: Seems redundant to send the scan results to both
                    var resolver = new ZoneOutcomeResolver(zone._lastZoneScan, new ZoneOutcomeResolverOptions(zone._lastZoneScan));
                    var outcome = resolver.Resolve();
                    zonesToRemove.Add(zone);
                }
            }

            //Remove flagged zones
            foreach(var zoneToRemove in zonesToRemove)
            {
                _activeZones.Remove(zoneToRemove);
            }

            //Check if we need to spawn any new zones
            if(_activeZones.Count < _minZonesToMaintain)
            {
                TrySpawnZone(
                    _zoneNamePrefix,
                    _spawnAreas[_rand.Next(0, _spawnAreas.Count)],
                    _zoneRadius);
            }
        }

        public bool TrySpawnZone(string zoneName, Vector3D position, double radius, double randomOffset = 50000, int maxAttempts = 5, bool createGPS = true)
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
                    Logger.Log("Found a place to spawn a zone!");

                    //Create the new zone
                    var zone = new Zone(zoneName, randX, randY, randZ, radius, _zoneTimeToLive, createGPS);

                    //Add to tracked zones
                    _activeZones.Add(zone);

                    return true;
                }

                attempts++;
                Logger.Log($"Couldn't spawn zone at {randX}, {randY}, {randZ} trying again. Attempt # {attempts}");
            } while (attempts < maxAttempts);

            return false;
        }
    }
}
