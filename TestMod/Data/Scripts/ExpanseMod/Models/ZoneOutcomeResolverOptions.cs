using ExpanseMod.Util;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ObjectBuilders;

namespace ExpanseMod.Models
{
    public class ZoneOutcomeResolverOptions
    {
        public delegate ZoneOutcome ResolveDelegate(ZoneScanResults scanResults);

        private ZoneScanResults _scanResults { get; set; }
        private ResolveDelegate _resolveDelegate;

        private ZoneOutcome DefaultResolve(ZoneScanResults scanResults)
        {
            if (scanResults == null || scanResults.FoundShips.Count == 0)
                return null;

            //By default we give the closest ship to the center the reward
            var closestShip = scanResults.FoundShips.OrderBy(s => s.Value.DistanceToCenter).FirstOrDefault();

            //Give default reward of placeholder stone now
            foreach (var block in closestShip.Value.Ship.GetFatBlocks<MyCargoContainer>())
            {
                if (!block.GetInventory().IsFull)
                {
                    BlocksHelper.InventoryAdd(
                        block.GetInventory(),
                        10,
                        new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Stone"));
                    break;
                }
            }
  
            return new ZoneOutcome()
            {
                Ships = new List<MyCubeGrid> { closestShip.Value.Ship }
            };
        }

        public ZoneOutcomeResolverOptions(ZoneScanResults scanResults)
        {
            _resolveDelegate = DefaultResolve;
            _scanResults = scanResults;
        }


        public ZoneOutcomeResolverOptions(ResolveDelegate resolveDelegate, ZoneScanResults scanResults)
        {
            _resolveDelegate = resolveDelegate;
            _scanResults = scanResults;
        }

        public ZoneOutcome Resolve()
        {
            return _resolveDelegate.Invoke(_scanResults);
        }
    }
}
