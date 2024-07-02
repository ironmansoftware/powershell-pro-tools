using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    public enum ParameterType
    {
        Unsupported,
        Object,
        Array,
        Float,
        Double,
        Decimal,
        Char,
        Boolean,
        Switch,
        Enum,
        Choice,
        Byte,
        Int32,
        Int64,
        String
    }
}
