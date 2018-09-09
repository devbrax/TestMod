using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExpanseMod.Models
{
    public class ZoneScanResults
    {
        public Dictionary<long,ZoneScanResultItem> FoundShips { get; set; }
        public Dictionary<long, ZoneScanResultItem> FoundPlayers { get; set; }

        public DateTime LastScan { get; set; }
        private Vector3D ZoneCenter { get; set; }
        public int CharactersFound { get; set; }
        public int ShipsFound { get; set; }

        public ZoneScanResults(Vector3D zoneCenter)
        {
            ZoneCenter = zoneCenter;
            FoundShips = new Dictionary<long, ZoneScanResultItem>();
            FoundPlayers = new Dictionary<long, ZoneScanResultItem>();
            LastScan = DateTime.Now;
        }

        public void FoundPlayer(long playerIdentityId, MyObjectBuilder_Character character)
        {
            var charPosition = character?.PositionAndOrientation?.Position;
            var playerPosition = new Vector3D(charPosition.Value.x, charPosition.Value.y, charPosition.Value.z);

            FoundPlayers[playerIdentityId] = new ZoneScanResultItem()
            {
                PlayerIdentityId = playerIdentityId,
                Character = character,
                DistanceToCenter = Vector3D.Distance(ZoneCenter, playerPosition)
            };

            CharactersFound = FoundPlayers.Count;
        }

        public void FoundPilotedShip(IMyPlayer player, MyCubeGrid ship)
        {
            var shipPosition = ship.PositionComp.GetPosition();

            var shipBlocks = ship.GetBlocks();
            var totalBlocks = shipBlocks.Count();
 
            FoundShips[player.IdentityId] = new ZoneScanResultItem() {
                PlayerIdentityId = player.IdentityId,
                Player = player,
                Ship = ship,
                DistanceToCenter = Vector3D.Distance(ZoneCenter, shipPosition)//,
                //IsLargeShip = (largeBlocks > smallBlocks),
                //ShipBlockCount = largeBlocks + smallBlocks //TODO: This should be probably based on large block count
            };

            ShipsFound = FoundShips.Count;
        }
    }
}
