using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseMod.Models
{
    public class ZoneOutcome
    {
        public List<long> Players { get; set; }
        public List<MyCubeGrid> Ships { get; set; }
    }
}
