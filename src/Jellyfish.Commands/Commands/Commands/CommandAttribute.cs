using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string CommandGroup { get; set; }
        public string CommandName { get; set; }

        public CommandAttribute(string commandGroup)
        {
            CommandGroup = commandGroup;
        }
    }
}
