using ExpanseMod.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseMod.LootSpawn
{
    public class ZoneOutcomeResolver
    {
        private ZoneScanResults _scanResults { get; set; }
        private ZoneOutcomeResolverOptions _options { get; set; }

        public ZoneOutcomeResolver(ZoneScanResults scanResults, ZoneOutcomeResolverOptions options)
        {
            _options = options;
            _scanResults = scanResults;
        }

        public ZoneOutcome Resolve()
        {
            return _options.Resolve();
        }
    }
}
