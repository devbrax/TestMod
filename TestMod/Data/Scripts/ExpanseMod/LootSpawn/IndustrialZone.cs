using ExpanseMod.LootSpawn;
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
    public class IndustrialZone : ZoneType
    {
        private static TimeSpan _timeToLive = new TimeSpan(0, Config.Zone_TimeToLiveMinutes, 0); //Default of 10 minutes
        private ZoneItemReward _reward { get; set; }

        public IndustrialZone(Vector3D position, double radius) 
            : base("Resource Extraction Site", position, radius, _timeToLive, true)
        {
            //TODO: Make the reward configurable
            _reward = new ZoneItemReward()
            {
                ItemCount = 1,
                ItemDefinition = new MyDefinitionId(typeof(MyObjectBuilder_Ore), Config.Zone_IndustryReward)
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
                //TODO: Log exception
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
                if (timeLeft.TotalSeconds < _minSecondsToShowCountdown)
                {
                    additionalText += (additionalText.Length > 0 ? " " : string.Empty) + $"T:-{timeLeft.Seconds}";
                }
            }

            if (!string.IsNullOrEmpty(additionalText))
                _GPS.Name = $"{_zoneName} {additionalText}";
            else
                _GPS.Name = _zoneName;

            //Modify existing GPS
            foreach (var player in players)
                MyAPIGateway.Session.GPS.ModifyGps(player.IdentityId, _GPS);

            //TODO: We should check if we need to create new GPS? What happens to the above line when a player joins after the initial GPS is created?
        }
    }
}
