using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    internal static class DataTypeConstants
    {
        // Supported Parameter types
        public const string BoolType = "System.Boolean";
        public const string SwitchType = "System.Management.Automation.SwitchParameter";
        public const string Int32Type = "System.Int32";
        public const string Int64Type = "System.Int64";
        public const string StringType = "System.String";
        public const string SingleType = "System.Single";
        public const string DoubleType = "System.Double";
        public const string DecimalType = "System.Decimal";
        public const string CharType = "System.Char";
        public const string ByteType = "System.Byte";
        public const string EnumType = "System.Enum";

        public const string ArrayType = "[]";

        // TODO: Unsupported parameter types
        public const string SecureStringType = "System.Security.SecureString";
        public const string PSCredentialType = "System.Management.Automation.PSCredential";

        public static HashSet<string> DataTypesSet = new HashSet<string>(new[] { BoolType, SwitchType });
        public static HashSet<string> UnsupportedDataTypes = new HashSet<string>(new[] { SecureStringType, PSCredentialType });
    }
}
