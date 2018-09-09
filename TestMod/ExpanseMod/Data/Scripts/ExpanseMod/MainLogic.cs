using Sandbox.Game;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using Sandbox.ModAPI;
using ExpanseMod.LootSpawn;
using ExpanseMod.Util;
using System;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using VRageMath;
using System.Linq;

namespace ExpanseMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MainLogic : MySessionComponentBase
    {
        public static bool mpActive => MyAPIGateway.Multiplayer.MultiplayerActive;
        public static bool isServer => MyAPIGateway.Multiplayer.IsServer;
        public static bool isDedicatedServer => MyAPIGateway.Utilities.IsDedicated;
        

        public ZoneManager _zoneManager { get; set; }

        public bool _isInitialized { get; set; }

        public MainLogic()
        {
            
        }

        public override void BeforeStart()
        {
            Logger.Init();
            PlayerGPSManager.Init();
            base.BeforeStart();
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!_isInitialized && MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
                {
                    if (!MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE) && MyAPIGateway.Multiplayer.IsServer && !MyAPIGateway.Utilities.IsDedicated)
                        InitServer();
                    Init();
                }
                if (!_isInitialized && MyAPIGateway.Utilities != null && MyAPIGateway.Multiplayer != null
                    && MyAPIGateway.Session != null && MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
                {
                    InitServer();
                    return;
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"A fatal exception was thrown during UpdateBeforeSimulation initialization. {ex.Message} {ex.StackTrace}");
                return;
            }

            try
            {
                base.UpdateBeforeSimulation();

                if (MyAPIGateway.Session == null)
                    return;


                if (isServer || isDedicatedServer)
                {
                    _zoneManager.Update();
                }
                else
                {
                    PlayerGPSManager.Update();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"A fatal exception was thrown during game logic. {ex.TargetSite} {ex.Message} {ex.StackTrace}");
            }
        }

        private void Init()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateBeforeSimulation().
            Logger.Log("Expanse Zones Client Initialized");

            Utilities.ReadClientConfigFile();
        }

        private void InitServer()
        {
            _isInitialized = true;
            
            MyVisualScriptLogicProvider.PlayerConnected += PlayerConnected;
            MyVisualScriptLogicProvider.PlayerDropped += PlayerDropped;
            MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;

            //Load configuration. This must be done before anything is initialized
            Utilities.ReadServerConfigFile();
            _zoneManager = new ZoneManager(Utilities.ServerConfig.Zone_SpawnAreas, Utilities.ServerConfig.Zone_MaxZones, new TimeSpan(0, Utilities.ServerConfig.Zone_SpawnTimeoutMinutes, 0));

            Logger.Log("Expanse Zones Server Initialized");
        }

        private void PlayerConnected(long playerId)
        {
            //Logger.Log("Player connected with ID " + playerId + ". Sending missing GPS coordinates");

            //var players = new List<IMyPlayer>();
            //MyAPIGateway.Players.GetPlayers(players);
            //var player = players.FirstOrDefault(p => p.IdentityId == playerId);

            //if(player != null)
            //{
            //    ServerGPSManager.Server_SendAllGPSToPlayer(player.SteamUserId, _zoneManager);
            //}
        }
            
        private void PlayerDropped(string itemTypeName, string itemSubTypeName, long playerId, int amount)
        {
            //Logger.Log("Player dropped with ID " + playerId);
        }

        private void PlayerDisconnected(long playerId)
        {
            //Logger.Log("Player disconnected with ID " + playerId);
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
        }

        protected override void UnloadData()
        {
            Logger.Close();
        }
    }
}
