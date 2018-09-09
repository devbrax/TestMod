using ExpanseMod.Util;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRageMath;

namespace ExpanseMod.ShipSpawn
{
    class ShipSpawnManager
    {
        public void SpawnShip(string prefabName, Vector3D position)
        {
            var prefabKvp = MyDefinitionManager.Static.GetPrefabDefinitions().FirstOrDefault(kvp => kvp.Key.Equals(prefabName, StringComparison.InvariantCultureIgnoreCase));
            MyPrefabDefinition prefab = null;

            if (prefabKvp.Value != null)
                prefab = prefabKvp.Value;


            if (prefab != null)
            {
                AddPrefab(prefabName, position, SyncCreatePrefabType.Pirate);
            }
        }

        public enum SyncCreatePrefabType : byte
        {
            Stock = 0,
            Wireframe = 1,
            Pirate = 2
        }

        private static bool AddPrefab(string prefabName, Vector3D position, SyncCreatePrefabType prefabType, ulong messageId = 0)
        {
            long piratePlayerId = 0;
            if (prefabType == SyncCreatePrefabType.Pirate)
            {
                var fc = MyAPIGateway.Session.Factions.GetObjectBuilder();
                var faction = fc.Factions.FirstOrDefault(f => f.Tag == "SPRT");
                if (faction != null)
                {
                    var pirateMember = faction.Members.FirstOrDefault();
                    piratePlayerId = pirateMember.PlayerId;
                }
            }


            var prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
            if (prefab.CubeGrids == null)
            {
                MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
            }

            if (prefab.CubeGrids.Length == 0)
                return false;


            // Use the cubeGrid BoundingBox to determine distance to place.
            Vector3I min = Vector3I.MaxValue;
            Vector3I max = Vector3I.MinValue;
            foreach (var b in prefab.CubeGrids[0].CubeBlocks)
            {
                min = Vector3I.Min(b.Min, min);
                max = Vector3I.Max(b.Min, max);
            }
            var size = new Vector3(max - min);

            // TODO: find a empty spot in space to spawn the prefab safely.

            var distance = (Math.Sqrt(size.LengthSquared()) * prefab.CubeGrids[0].GridSizeEnum.ToGridLength() / 2) + 2;
            var offset = position - prefab.CubeGrids[0].PositionAndOrientation.Value.Position;
            var tempList = new List<MyObjectBuilder_EntityBase>();

            // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
            foreach (var grid in prefab.CubeGrids)
            {
                var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(grid.PositionAndOrientation.Value.Position + offset, grid.PositionAndOrientation.Value.Forward, grid.PositionAndOrientation.Value.Up);

                if (prefabType == SyncCreatePrefabType.Wireframe)
                    foreach (var cube in gridBuilder.CubeBlocks)
                    {
                        cube.IntegrityPercent = 0.01f;
                        cube.BuildPercent = 0.01f;
                    }

                if (prefabType == SyncCreatePrefabType.Pirate)
                {
                    foreach (var cube in gridBuilder.CubeBlocks)
                    {
                        cube.Owner = piratePlayerId;
                        cube.ShareMode = MyOwnershipShareModeEnum.None;
                    }
                }

                tempList.Add(gridBuilder);
            }

            tempList.CreateAndSyncEntities();
            return true;
        }
    }
}
