using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseMod.LootSpawn.Models
{
    public class ZoneScanResult
    {
        public List<long> Players { get; set; }
        public Dictionary<long,MyCubeGrid> Grids { get; set; }
    }
}
