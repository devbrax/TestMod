using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace ExpanseMod.Models
{
    class TrackedGPS
    {
        public GPSPacket gpsPacket { get; set; }
        public IMyGps SpawnedGPS { get; set; }
    }
}
