using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;


namespace ExpanseMod.Util
{
    public class ServerConfig
    {
        public int Zone_TimeToLiveMinutes = 7;

        public int Zone_SpawnTimeoutMinutes = 15;

        public int Zone_MaxZones = 3;

        public int Zone_IndustryRadius = 1000;
        public int Zone_MilitaryRadius = 1000;
        public int Zone_ScienceRadius = 1000;

        public int Zone_SpawnScanRadiusMeters = 1000;

        public int Zone_SpawnRandomOffset = 5000;
        public int Zone_SpawnMaxAttempts = 5;

        public int Zone_MilitaryRewardCount = 75;
        public int Zone_IndustryRewardCount = 75;
        public int Zone_ScienceRewardCount = 75;


        public string Zone_MilitaryReward = "MilitaryComponent";
        public string Zone_IndustryReward = "IndustrialComponent";
        public string Zone_ScienceReward = "ScienceComponent";

        public Vector3D Zone_IndustryColor = new Vector3D(244, 216, 11);
        public Vector3D Zone_MilitaryColor = new Vector3D(222, 90, 12);
        public Vector3D Zone_ScienceColor = new Vector3D(70, 84, 249);

        public List<Vector3D> Zone_SpawnAreas = new List<Vector3D>
        {
            new Vector3D(158120.12, 76418.06, 84605.13), //UN Earth
            new Vector3D(8010239.55, 8007884.69, 155530.67), // OPA Ceres cluster
            new Vector3D(750943.33, -17280.94, -5985.41) //MCRN asteroid cluster
        };
    }
}
