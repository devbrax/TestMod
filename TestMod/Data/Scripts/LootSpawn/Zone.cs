using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.ModAPI;

namespace TestMod.LootSpawn
{
    public class Zone
    {
        private string _zoneName { get; set; }
        private BoundingSphereD _zoneBounds { get; set; }
        private bool _hasGPS { get; set; }
        private IMyGps _GPS { get; set; }
        public List<long> _lastFoundPlayers { get; set; }

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
            _GPS.Name = $"{_zoneName} - P:{_lastFoundPlayers.Count}";

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

            _hasGPS = createGPS;

            if (createGPS)
                CreateGPS(position, zoneName);
        }
        public Zone(string zoneName, double x, double y, double z, double radius, bool createGPS = false)
        {
            var position = new Vector3D(x, y, z);
            _zoneName = zoneName;
            _zoneBounds = new BoundingSphereD(position, (double)radius);

            _hasGPS = createGPS;

            if (createGPS)
                CreateGPS(position, zoneName);
        }

        public void Update()
        {
            var bounds = _zoneBounds;
            var allEntities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounds);
            var shipControllers = allEntities.Where(e => e is Sandbox.ModAPI.IMyShipController).Select(s => (Sandbox.ModAPI.IMyShipController)s);

            var pilots = shipControllers.Where(s =>
                s.IsUnderControl &&
                s.ControllerInfo.IsLocallyHumanControlled() &&
                s.ControllerInfo.ControllingIdentityId > 0)
                .Select(s => s.ControllerInfo.ControllingIdentityId);

            _lastFoundPlayers = pilots.ToList();

            if (_hasGPS)
                UpdateStatus();
        }
    }
}
