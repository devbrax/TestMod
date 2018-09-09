using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ExpanseMod.Models
{
    public class ZoneScanResultItem
    {
        public IMyPlayer Player { get; set; }
        public long PlayerIdentityId { get; set; }
        public MyObjectBuilder_Character Character { get; set; }
        public MyCubeGrid Ship { get; set; }
        public double DistanceToCenter { get; set; }
        public bool IsLargeShip { get; set; }
        public int ShipBlockCount { get; set; }
    }
}
