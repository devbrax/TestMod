using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseMod.Models
{
    [ProtoContract]
    public abstract class LogEntry
    {
        [ProtoMember(1)]
        public string Type { get; set; }
        [ProtoMember(2)]
        public DateTime Time { get; set; }

        public LogEntry()
        {

        }
    }
}
