﻿using ExpanseMod.LootSpawn;
using ExpanseMod.Models;
using ExpanseMod.Util;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExpanseMod.LootSpawn
{
    public class ConflictZone : ZoneType
    {
        private ZoneItemReward _reward { get; set; }

        public ConflictZone(Vector3D origin, Vector3D position, double radius) 
            : base("Conflict Zone", origin, position, radius, new TimeSpan(0, Utilities.Config.Zone_TimeToLiveMinutes, 0), true)
        {
            _reward = new ZoneItemReward()
            {
                ItemCount = Utilities.Config.Zone_MilitaryRewardCount,
                ItemDefinition = new MyDefinitionId(typeof(MyObjectBuilder_Component), Utilities.Config.Zone_MilitaryReward)
            };
        }

        public override ZoneUpdateResult Update(List<IMyPlayer> players)
        {
            try
            {
                //Check if we need to expire before thinking
                if (base.Update(players) == ZoneUpdateResult.Timeout)
                {
                    //Check if we have any winners
                    if (_lastZoneScan.ShipsFound > 0)
                        GiveItemToClosestShip();

                    //We've reached our win condition so this conflict zone is done!
                    return ZoneUpdateResult.Timeout;
                }
                
                return ZoneUpdateResult.Success;
            }
            catch(Exception ex)
            {
                Logger.Log($"An exception occured when updating Conflict Zone. Ex: {ex.Source} : {ex.Message} {ex.StackTrace}");
                return ZoneUpdateResult.Failure;
            }
        }

        private ZoneOutcome GiveItemToClosestShip()
        {
            //Check if we have any ships found in the last scan
            if (_lastZoneScan == null || _lastZoneScan.FoundShips.Count == 0)
                return null;

            //Find the closest ship to the center
            var closestShip = _lastZoneScan.FoundShips.OrderBy(s => s.Value.DistanceToCenter).FirstOrDefault();

            //Put reward in the first available cargo container
            foreach (var block in closestShip.Value.Ship.GetFatBlocks<MyCargoContainer>())
            {
                //Check if the cargo container isn't full
                if (!block.GetInventory().IsFull)
                {
                    Util.Utilities.InventoryAdd(block.GetInventory(), //The cargo container block
                                              _reward.ItemCount, //How many items to add
                                              _reward.ItemDefinition); //The exact item type and subtype

                    //Break out of the loop as we don't want to give any more rewards
                    break;
                }
            }

            //Return the winner
            return new ZoneOutcome()
            {
                Ships = new List<MyCubeGrid> { closestShip.Value.Ship }
            };
        }


        //This function is to be executed after a scan
        protected override void UpdateGPS(List<IMyPlayer> players)
        {
            var additionalText = string.Empty;

            if (_lastZoneScan.ShipsFound > 0)
                additionalText = $"[{_lastZoneScan.ShipsFound.ToString()}]";

            if (_expireTime != DateTime.MinValue)
            {
                var timeLeft = (_expireTime - DateTime.Now);
                //if (timeLeft.TotalSeconds < _minSecondsToShowCountdown)
                //{
                    var totalMinutes = Math.Round(timeLeft.TotalMinutes);
                    var totalSeconds = Math.Round(timeLeft.TotalSeconds);
                    var timeLeftDisplay = (totalSeconds >= 60 ? totalMinutes + "m" : totalSeconds + "s");
                    additionalText += (additionalText.Length > 0 ? " " : string.Empty) + $"T:-{timeLeftDisplay}";
                //}
            }

            if (!string.IsNullOrEmpty(additionalText))
                _GPS.Name = $"{_zoneName} {additionalText}";
            else
                _GPS.Name = _zoneName;

            //Modify existing GPS
            foreach (var player in players)
            {
                //Check if all players have the GPS coordinate
                var gpsList = MyAPIGateway.Session.GPS.GetGpsList(player.IdentityId);
                if(!gpsList.Any(g => g.Hash.Equals(_GPS.Hash)))
                    MyAPIGateway.Session.GPS.AddLocalGps(_GPS);
                else
                    MyAPIGateway.Session.GPS.ModifyGps(player.IdentityId, _GPS);
            }
        }
    }
}
