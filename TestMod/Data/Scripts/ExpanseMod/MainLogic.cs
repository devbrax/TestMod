using Sandbox.Game;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using Sandbox.ModAPI;
using ExpanseMod.LootSpawn;
using ExpanseMod.Util;

namespace ExpanseMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MainLogic : MySessionComponentBase
    {
        public Zone _zone { get; set; }
        int _counter = 0;
        bool _debugMode = true;

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
                return;
            }

            if(_counter % 10 == 0)
            {
                _zone.Update();
            }
            _counter++;

            base.UpdateBeforeSimulation();
        }

        private void Init()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateBeforeSimulation().
            Logger.Log("TestMod Client Initialized");

            _zone = new Zone("Test Zone", 0, 0, 0, 100, true);
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
