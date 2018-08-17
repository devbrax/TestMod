using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace ExpanseMod.Util
{
    public static class Config
    {
        public static int Zone_TimeToLiveMinutes = 10;

        public static int Zone_SpawnTimeoutMinutes = 30;

        public static int Zone_MaxZones = 3;

        public static int Zone_IndustryRadius = 1000;
        public static int Zone_MilitaryRadius = 1000;
        public static int Zone_ScienceRadius = 1000;

        public static int Zone_SpawnScanRadiusMeters = 500;

        public static int Zone_SpawnRandomOffset = 50000;
        public static int Zone_SpawnMaxAttempts = 5;


        public static string Zone_MilitaryReward = "Stone";//"MilitaryOre";
        public static string Zone_IndustryReward = "Uranium";//"IndustryOre";
        public static string Zone_ScienceReward = "Iron";//"ScienceOre";

        public static List<Vector3D> Zone_SpawnAreas = new List<Vector3D>
        {
            new Vector3D(0,0,0),
            new Vector3D(5000, 0, 0),
            new Vector3D(10000, 0, 0),
            new Vector3D(15000, 0, 0),
            new Vector3D(20000, 0, 0),
            new Vector3D(25000, 0, 0),
            new Vector3D(30000, 0, 0)
        };




        public static void LoadConfig()
        {
            //TODO: Load config from file and set values
        }
    }
}
