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
    public class GPS
    {
        [ProtoMember(1)]
        public double X { get; set; }

        [ProtoMember(2)]
        public double Y { get; set; }

        [ProtoMember(3)]
        public double Z { get; set; }

        [ProtoMember(4)]
        public string  Name { get; set; }

        [ProtoMember(5)]
        public int Hash { get; set; }


        public GPS() { } // empty ctor is required for deserialization

        public GPS(double x, double y, double z, string name, int hash)
        {
            X = x;
            Y = y;
            Z = z;
            Name = name;
            Hash = hash;
        }

    }
}
