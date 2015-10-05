using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Version { get; set; }
        public int Port { get; set; }
        public string ServiceName { get; set; }

        public CommandAttribute(int port, string version)
        {
            Version = version;
            Port = port;
        }
    }
}
