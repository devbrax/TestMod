using ExpanseMod.Util;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseMod.Models
{
    public class ZoneOutcomeResolverOptions
    {
        public delegate ZoneOutcome ResolveDelegate(ZoneScanResults scanResults);
        public delegate void GiveRewardsDelegate(ZoneOutcome playersAndShips);

        private ZoneScanResults _scanResults { get; set; }
        private ResolveDelegate _resolveDelegate;
        private GiveRewardsDelegate _rewardDelegate;

        private ZoneOutcome DefaultResolve(ZoneScanResults scanResults)
        {
            if (scanResults == null || scanResults.FoundShips.Count == 0)
                return null;

            Logger.Log("Using DefaultResolve...");
            //By default we give the closest ship to the center the reward
            var closestShip = scanResults.FoundShips.OrderBy(s => s.Value.DistanceToCenter).FirstOrDefault();

            Logger.Log($"Found ship {closestShip.Value.DistanceToCenter} to center");

            return new ZoneOutcome()
            {
                Ships = new List<MyCubeGrid> { closestShip.Value.Ship }
            };
        }

        private void DefaultReward(ZoneOutcome playersAndShips)
        {
            //TODO: Put item in not full cargo container
            Logger.Log("Default reward");
        }

        public ZoneOutcomeResolverOptions(ZoneScanResults scanResults)
        {
            _resolveDelegate = DefaultResolve;
            _rewardDelegate = DefaultReward;
            _scanResults = scanResults;
        }


        public ZoneOutcomeResolverOptions(ResolveDelegate resolveDelegate, ZoneScanResults scanResults, GiveRewardsDelegate rewardDelegate = null)
        {
            _resolveDelegate = resolveDelegate;
            _rewardDelegate = rewardDelegate;
            _scanResults = scanResults;
        }

        public ZoneOutcome Resolve()
        {
            var outcome = _resolveDelegate.Invoke(_scanResults);

            if(_rewardDelegate != null && outcome != null)
            {
                _rewardDelegate.Invoke(outcome);
            }

            return outcome;
        }
    }
}
