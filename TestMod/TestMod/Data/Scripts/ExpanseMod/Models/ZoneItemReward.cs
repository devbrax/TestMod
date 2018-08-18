using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ObjectBuilders;

namespace ExpanseMod.Models
{
    public class ZoneItemReward
    {
        public int ItemCount { get; set; }
        public MyDefinitionId ItemDefinition { get; set; }
        public string SubtypeName { get; set; }
    }
}
