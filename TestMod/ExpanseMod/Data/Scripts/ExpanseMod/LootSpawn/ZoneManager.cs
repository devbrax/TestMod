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
        private enum ZoneTypes { Military, Industrial, Science };

        private List<ZoneTypes> _availableZoneTypes = new List<ZoneTypes>();
        private DateTime _lastTick = DateTime.Now;
        private List<Vector3D> _spawnAreas;
        private List<ZoneType> _activeZones;
        private int _minZonesToMaintain;
        private Random _rand;
        private DateTime _zoneLastSpawnTime = DateTime.MinValue;
        private TimeSpan _zoneSpawnDelay;
        private bool _initialSetupComplete = false;
        private const int UpdateDelay = 1;

        private static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(new Random().Next(v.Length));
        }

        private void InitZoneTypes()
        {
            _availableZoneTypes.Add(ZoneTypes.Military);
            _availableZoneTypes.Add(ZoneTypes.Industrial);
            _availableZoneTypes.Add(ZoneTypes.Science);
        }


        public ZoneManager(List<Vector3D> spawnAreas, int minZonesToMaintain = 3, TimeSpan? zoneSpawnDelay = null)
        {
            _minZonesToMaintain = minZonesToMaintain;
            _activeZones = new List<ZoneType>();
            _spawnAreas = spawnAreas;
            _rand = new Random();


            if (zoneSpawnDelay.HasValue)
                _zoneSpawnDelay = zoneSpawnDelay.Value;

            InitZoneTypes();
        }

        public List<ZoneType> GetActiveZones()
        {
            return _activeZones;
        }

        public void Update()
        {
            if ((DateTime.Now - _lastTick).TotalSeconds >= UpdateDelay)
            {
                _lastTick = DateTime.Now;

                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                var zonesToRemove = new List<ZoneType>();

                foreach (var zone in _activeZones)
                    //Update and check if the zone has expired
                    if (zone.Update(players) == ZoneType.ZoneUpdateResult.Timeout)
                        zonesToRemove.Add(zone);

                //Remove flagged zones
                foreach (var zoneToRemove in zonesToRemove)
                    _activeZones.Remove(zoneToRemove);

                //Check if we need to spawn any new zones and if we should wait
                if (_activeZones.Count < _minZonesToMaintain
                    && (_zoneLastSpawnTime.Add(_zoneSpawnDelay).CompareTo(DateTime.Now) < 0
                        || _initialSetupComplete == false))
                {
                    //Try and spawn a zone
                    var usedSpawnPoints = _activeZones.Where(z => !z._zoneOrigin.Equals(Vector3D.MinValue)).Select(z => z._zoneOrigin);
                    var availableSpawnAreas = _spawnAreas.Where(s => !usedSpawnPoints.Any(u => u.Equals(s))).ToList();

                    if (availableSpawnAreas.Count == 0)
                    {
                        Logger.Log("ERROR! Unable to find spawn location for zone.");
                        return;
                    }

                    var randIndex = _rand.Next(0, availableSpawnAreas.Count);
                    var origin = availableSpawnAreas[randIndex];

                    var position = GetAvailableSpawnZone(origin, Utilities.Config.Zone_SpawnScanRadiusMeters, Utilities.Config.Zone_SpawnRandomOffset, Utilities.Config.Zone_SpawnMaxAttempts);
                    if (!position.Equals(Vector3D.MinValue))
                    {
                        //Pick a random type of Zone
                        var zoneType = _availableZoneTypes[_rand.Next(0, _availableZoneTypes.Count)];

                        ZoneType newZone;

                        switch (zoneType)
                        {
                            case ZoneTypes.Military:
                                newZone = new ConflictZone(origin, position, Utilities.Config.Zone_MilitaryRadius);
                                break;

                            case ZoneTypes.Industrial:
                                newZone = new IndustrialZone(origin, position, Utilities.Config.Zone_IndustryRadius);
                                break;

                            case ZoneTypes.Science:
                                newZone = new ScienceZone(origin, position, Utilities.Config.Zone_ScienceRadius);
                                break;
                            default:
                                throw new Exception($"Unknown zone type {zoneType}");
                        }

                        //Add to tracked zones
                        _activeZones.Add(newZone);
                    }

                    //Complete initial setup if we have the minimum and keep track of the last time we spawned them
                    if (_activeZones.Count == _minZonesToMaintain)
                    {
                        _initialSetupComplete = true;

                        //Keep track of the last time we spawned enough zones
                        _zoneLastSpawnTime = DateTime.Now;
                    }
                }
            }
        }

        public Vector3D GetAvailableSpawnZone(Vector3D position, double radius, double randomOffset, int maxAttempts)
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
                
                allEntities = allEntities.Where(e => !(e is Sandbox.Game.Entities.MyVoxelBase)).ToList();

                if (allEntities.Count == 0)
                    return new Vector3D(randX, randY, randZ);

                attempts++;
                Logger.Log($"Couldn't spawn zone at {randX}, {randY}, {randZ} trying again. Attempt # {attempts}");
            } while (attempts < maxAttempts);

            return Vector3D.MinValue;
        }
    }
}
