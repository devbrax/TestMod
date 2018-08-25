using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using VRage.ModAPI;
using VRage.Voxels;
using VRageMath;
using ExpanseMod.Models;

namespace ExpanseMod.Util
{
    public static class Utilities
    {
        public static Config Config;

        public static bool InventoryAdd(IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amount, PhysicalContent = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, inventoryItem.PhysicalContent, -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }

        public static Config SaveConfig(string configFileName)
        {
            var cfg = MyAPIGateway.Utilities.WriteFileInGlobalStorage(configFileName);
            var data = MyAPIGateway.Utilities.SerializeToXML(Config);
            cfg.Write(data);
            cfg.Flush();
            cfg.Close();

            return Config;
        }

        public static Config ReadConfigFile(string configFileName)
        {
            //If the config file doesn't exist, create it
            if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(configFileName))
            {
                //Initialize a default config
                Config = new Config();
                return SaveConfig(configFileName);
            }

            var configXml = MyAPIGateway.Utilities.ReadFileInGlobalStorage(configFileName);
            Config = MyAPIGateway.Utilities.SerializeFromXML<Config>(configXml.ReadToEnd());

            return Config;
        }

        public static void RemoveGPS(IMyGps gps)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (var player in players)
            {
                MyAPIGateway.Session.GPS.RemoveGps(player.IdentityId, gps);
            }
        }

        public static IMyGps CreateGPS(Vector3D position, string gpsName)
        {
            var GPS = MyAPIGateway.Session.GPS.Create(gpsName,
                                                     "GPS",
                                                     position,
                                                     true,false);

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (var player in players)
            {
                MyAPIGateway.Session.GPS.AddGps(player.IdentityId, GPS);
            }

            return GPS;
        }

        public static ZoneScanResults Scan(List<IMyPlayer> players, Vector3D position, BoundingSphereD bounds)
        {
            var scanResults = new ZoneScanResults(position);
            //Copy the zone bounds as we need to pass it by ref
            var allEntities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounds);

            //Get all cube grids
            var gridsInZone = allEntities.Where(e => e is MyCubeGrid);

            //Get all characters
            var playersInZone = allEntities.Where(e => e is IMyCharacter).Select(e => (IMyCharacter)e);

            //Add players to the found list
            foreach (var player in playersInZone)
            {
                var character = player.GetObjectBuilder() as MyObjectBuilder_Character;
                if (character != null && character.OwningPlayerIdentityId.HasValue)
                    scanResults.FoundPlayer(character.OwningPlayerIdentityId.Value, character);
            }

            //Add ships to the found list
            foreach (MyCubeGrid grid in gridsInZone)
            {
                var foundPlayer = players.FirstOrDefault(p => p.Controller?.ControlledEntity?.Entity?.GetTopMostParent()?.EntityId == grid.EntityId);
                if (foundPlayer != null)
                    scanResults.FoundPilotedShip(foundPlayer.IdentityId, grid);
            }

            return scanResults;
        }
    }
}
