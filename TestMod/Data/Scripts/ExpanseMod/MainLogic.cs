﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Components.Session;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using ExpanseMod.LootSpawn;
using Sandbox.ModAPI;
using ProtoBuf;

using MyAPIGateway = Sandbox.ModAPI.MyAPIGateway;

namespace ExpanseMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MainLogic : MySessionComponentBase
    {
        int counter = 0;
        bool debugMode = true;
        ExpanseMod.LootSpawn.Zone z = new ExpanseMod.LootSpawn.Zone("Test Zone", 0, 0, 0, 100, true);

        public bool _isInitialized { get; set; }

        public MainLogic()
        {
            LogEntry("TestMod Created!");
        }

        public override void BeforeStart()
        {
            LogEntry("TestMod:BeforeStart called");

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


            if(counter % 10 == 0)
            {
                //z.Update();
                //var players = z._lastFoundPlayers;
                LogEntry("TestMod Tick");
            }
            counter++;

            base.UpdateBeforeSimulation();
        }

        private void Init()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateBeforeSimulation().
            LogEntry("TestMod Client Initialized");
        }

        private void InitServer()
        {
            _isInitialized = true;
            LogEntry("Server started");

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
            LogEntry("TestMod:Init called");

            base.Init(sessionComponent);
        }

        public void LogEntry(string argument)
        {
            if (argument.Contains("Debug") == true && debugMode == false)
                return;

            MyLog.Default.WriteLineAndConsole("TestMod: " + argument);

            if (debugMode == true)
                MyVisualScriptLogicProvider.ShowNotificationToAll(argument, 5000, "White");
        }
    }
}
