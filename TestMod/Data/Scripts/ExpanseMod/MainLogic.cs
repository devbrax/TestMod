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
        public ZoneManager _zoneManager { get; set; }
        int _counter = 0;

        public bool _isInitialized { get; set; }

        public MainLogic()
        {
            Logger.Log("TestMod Created!");
        }

        public override void BeforeStart()
        {
            Logger.Log("TestMod:BeforeStart called");

            base.BeforeStart();
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!_isInitialized && MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
                {
                    //Debug = MyAPIGateway.Session.Player.IsExperimentalCreator();

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
                if (_counter % 50 == 0)
                {
                    var players = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(players);

                    _zoneManager.Update(players);
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"A fatal exception was thrown during game logic. {ex.Message} {ex.StackTrace}");
            }
            
            _counter++;

            base.UpdateBeforeSimulation();
        }

        private void Init()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateBeforeSimulation().
            Logger.Log("TestMod Client Initialized");

            var spawnAreas = new List<Vector3D>
            {
                new Vector3D(0,0,0),
                new Vector3D(5000, 0, 0),
                new Vector3D(10000, 0, 0),
                new Vector3D(15000, 0, 0),
                new Vector3D(20000, 0, 0),
                new Vector3D(25000, 0, 0),
                new Vector3D(30000, 0, 0)
            };

            _zoneManager = new ZoneManager(spawnAreas, "Conflict Zone", 500, 3, new TimeSpan(0,1,0));
        }

        private void InitServer()
        {
            _isInitialized = true;
            Logger.Log("Server started");

            MyVisualScriptLogicProvider.PlayerConnected += PlayerConnected;
            MyVisualScriptLogicProvider.PlayerDropped += PlayerDropped;
            MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;
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
            Logger.Log("TestMod:Init called");

            base.Init(sessionComponent);
        }
    }
}
