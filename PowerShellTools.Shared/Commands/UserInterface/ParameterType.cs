using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    internal enum ParameterType
    {
        Unknown,
        Array,
        Float,
        Double,
        Decimal,
        Char,
        Boolean,
        Switch,
        Enum,
        Byte,
        Int32,
        Int64,
        String
    }
}
