using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace ExpanseMod.Models
{
    [ProtoContract]
    public class ZoneRewardLogEntry : LogEntry
    {
        [ProtoMember(1)]
        public long PlayerID { get; set; }
        [ProtoMember(2)]
        public ulong PlayerSteamID { get; set; }

        [ProtoMember(2)]
        public Vector3D Position { get; set; }


        public ZoneRewardLogEntry()
        {

        }
    }
}
