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
        public static int Zone_TimeToLiveMinutes = 15;

        public static int Zone_SpawnTimeoutMinutes = 30;

        public static int Zone_MaxZones = 3;

        public static int Zone_IndustryRadius = 1000;
        public static int Zone_MilitaryRadius = 1000;
        public static int Zone_ScienceRadius = 1000;

        public static int Zone_SpawnScanRadiusMeters = 1000;

        public static int Zone_SpawnRandomOffset = 5000;
        public static int Zone_SpawnMaxAttempts = 5;

        public static int Zone_MilitaryRewardCount = 75;
        public static int Zone_IndustryRewardCount = 75;
        public static int Zone_ScienceRewardCount = 75;


        public static string Zone_MilitaryReward = "MilitaryComponent";
        public static string Zone_IndustryReward = "IndustrialComponent";
        public static string Zone_ScienceReward = "ScienceComponent";

        public static List<Vector3D> Zone_SpawnAreas = new List<Vector3D>
        {
            new Vector3D(0,0,0), //UN Earth
            new Vector3D(8010239.55, 8007884.69, 155530.67), // OPA Ceres cluster
            new Vector3D(3815802.47, 1030097.81, 17298.92) //MCRN asteroid cluster
        };

        public static void LoadConfig()
        {
            //TODO: Load config from file and set values
        }
    }
}
