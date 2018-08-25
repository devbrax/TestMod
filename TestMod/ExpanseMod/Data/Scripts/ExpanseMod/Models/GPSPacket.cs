using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace ExpanseMod.Models
{
    [ProtoContract]
    public class GPSPacket
    {
        [ProtoMember(1)]
        public long EntityId = 0;

        [ProtoMember(2)]
        public ulong Sender = 0;

        [ProtoMember(3)]
        public List<GPS> TrackedGPS = null;

        public GPSPacket() { } // empty ctor is required for deserialization

        public GPSPacket(ulong sender, long entityId, List<GPS> trackedGPS)
        {
            Sender = sender;
            EntityId = entityId;
            TrackedGPS = trackedGPS;
        }
    }
}
