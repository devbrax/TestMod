using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

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
        public double X { get; set; }

        [ProtoMember(4)]
        public double Y { get; set; }

        [ProtoMember(5)]
        public double Z { get; set; }

        [ProtoMember(6)]
        public string Name { get; set; }

        [ProtoMember(7)]
        public int Hash { get; set; }

        [ProtoMember(8)]
        public double SecondsToLive { get; set; }

        [ProtoMember(9)]
        public DateTime ExpireTime { get; set; }

        [ProtoMember(10)]
        public int ColorR { get; set; }

        [ProtoMember(11)]
        public int ColorG { get; set; }

        [ProtoMember(12)]
        public int ColorB { get; set; }

        public Color Color
        {
            get
            {
                return new Color(ColorR, ColorG, ColorB);
            }
        }

        public GPSPacket() { } // empty ctor is required for deserialization

        public GPSPacket(double x, double y, double z, string name, double secondsToLive, int colorR, int colorG, int colorB)
        {
            X = x;
            Y = y;
            Z = z;
            Name = name;
            SecondsToLive = secondsToLive;

            ColorR = colorR;
            ColorG = colorG;
            ColorB = colorB;
        }

        public GPSPacket(ulong sender, long entityId, double x, double y, double z, string name, int hash, double secondsToLive)
        {
            Sender = sender;
            EntityId = entityId;
            X = x;
            Y = y;
            Z = z;
            Name = name;
            Hash = hash;
            SecondsToLive = secondsToLive;
        }
    }
}
