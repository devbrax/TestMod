using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using Sandbox.Definitions;
using VRageMath;
using ExpanseMod.Models;
using Sandbox.Game;

namespace ExpanseMod.Util
{
    public static class Utilities
    {
        public static string ConfigFileName = "TheExpanseZones.cfg";
        public static ServerConfig ServerConfig;
        public static ClientConfig ClientConfig;

        public static float ToGridLength(this MyCubeSize cubeSize)
        {
            return MyDefinitionManager.Static.GetCubeSize(cubeSize);
        }


        public static void CreateAndSyncEntities(this List<MyObjectBuilder_EntityBase> entities)
        {
            MyAPIGateway.Entities.RemapObjectBuilderCollection(entities);
            entities.ForEach(item => MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item));
            MyAPIGateway.Multiplayer.SendEntitiesCreated(entities);
        }

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

        public static ServerConfig SaveServerConfig()
        {
            var cfg = MyAPIGateway.Utilities.WriteFileInGlobalStorage(ConfigFileName);
            var data = MyAPIGateway.Utilities.SerializeToXML(ServerConfig);
            cfg.Write(data);
            cfg.Flush();
            cfg.Close();

            return ServerConfig;
        }

        public static ClientConfig SaveClientConfig()
        {
            var cfg = MyAPIGateway.Utilities.WriteFileInGlobalStorage(ConfigFileName);
            var data = MyAPIGateway.Utilities.SerializeToXML(ClientConfig);
            cfg.Write(data);
            cfg.Flush();
            cfg.Close();

            return ClientConfig;
        }

        public static ServerConfig ReadServerConfigFile()
        {
            //If the config file doesn't exist, create it
            if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(ConfigFileName))
            {
                //Initialize a default config
                ServerConfig = new ServerConfig();
                return SaveServerConfig();
            }

            var configXml = MyAPIGateway.Utilities.ReadFileInGlobalStorage(ConfigFileName);

            try
            {
                ServerConfig = MyAPIGateway.Utilities.SerializeFromXML<ServerConfig>(configXml.ReadToEnd());
            }
            catch(Exception ex)
            {
                Logger.Log($"Unable to load config! Ex: {ex.Message} {ex.StackTrace}");
            }

            return ServerConfig;
        }

        public static ClientConfig ReadClientConfigFile()
        {
            //If the config file doesn't exist, create it
            if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(ConfigFileName))
            {
                //Initialize a default config
                ClientConfig = new ClientConfig();
                return SaveClientConfig();
            }

            var configXml = MyAPIGateway.Utilities.ReadFileInGlobalStorage(ConfigFileName);

            try
            {
                ClientConfig = MyAPIGateway.Utilities.SerializeFromXML<ClientConfig>(configXml.ReadToEnd());
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to load config!");
            }

            return ClientConfig;
        }

        public static void RemoveGPS(IMyGps gps)
        {
            var players = GetPlayers();
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

            var players = Utilities.GetPlayers();
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
                    scanResults.FoundPilotedShip(foundPlayer, grid);
            }

            return scanResults;
        }

        public static void DisplayToAllPlayers(string msg, string color = "White")
        {
            MyVisualScriptLogicProvider.ShowNotificationToAll(msg, 10000, color);
        }

        public static void DisplayMessageToPlayer(long playerId, string msg, string color = "White")
        {
            MyVisualScriptLogicProvider.ShowNotification(msg, 10000, color, playerId);
        }

        public static void DisplayMessageToLocalPlayer(string msg, string color = "White")
        {
            MyVisualScriptLogicProvider.ShowNotification(msg, 10000, color);
        }

        public static Color VectorToColor(Vector3D vector)
        {
            return new Color((int)vector.X, (int)vector.Y, (int)vector.Z);
        }

        public static List<IMyPlayer> GetPlayers()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            return players;
        }
    }
}
