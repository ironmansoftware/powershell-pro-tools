using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal struct CommandFormatterOptions
    {
        public CommandFormatStyle FormateStyle { get; set; }
        public string ParameterSet { get; set; }
    }
}
