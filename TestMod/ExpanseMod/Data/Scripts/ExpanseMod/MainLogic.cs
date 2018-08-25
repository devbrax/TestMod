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

namespace ExpanseMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MainLogic : MySessionComponentBase
    {
        private string _configFileName = "TheExpanseZones.cfg";
        public static bool mpActive => MyAPIGateway.Multiplayer.MultiplayerActive;
        public static bool isServer => MyAPIGateway.Multiplayer.IsServer;
        public static bool isDedicatedServer => MyAPIGateway.Utilities.IsDedicated;

        public ZoneManager _zoneManager { get; set; }
        int _counter = 0;


        public bool _isInitialized { get; set; }

        public MainLogic()
        {
            
        }

        public override void BeforeStart()
        {
            PlayerGPSManager.Init();
            Logger.Log("GPS Manager initiated");
            base.BeforeStart();
        }



        public override void UpdateBeforeSimulation()
        {
            try
            {
                base.UpdateBeforeSimulation();

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
                    base.UpdateBeforeSimulation();
                    return;
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"A fatal exception was thrown during UpdateBeforeSimulation initialization. {ex.Message} {ex.StackTrace}");
                base.UpdateBeforeSimulation();
                return;
            }

            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                if (_counter % 50 == 0)
                {
                    var players = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(players);

                    _zoneManager.Update(players);

                    _counter = 0;
                }

                _counter++;
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
        }

        private void InitServer()
        {
            _isInitialized = true;

            MyVisualScriptLogicProvider.PlayerConnected += PlayerConnected;
            MyVisualScriptLogicProvider.PlayerDropped += PlayerDropped;
            MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;

            //Load configuration. This must be done before anything is initialized
            Utilities.ReadConfigFile(_configFileName);

            _zoneManager = new ZoneManager(Utilities.Config.Zone_SpawnAreas, Utilities.Config.Zone_MaxZones, new TimeSpan(0, Utilities.Config.Zone_SpawnTimeoutMinutes, 0));
        }

        private void PlayerConnected(long playerId)
        {

        }
            
        private void PlayerDropped(string itemTypeName, string itemSubTypeName, long playerId, int amount)
        {

        }

        private void PlayerDisconnected(long playerId)
        {

        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
        }
    }
}
