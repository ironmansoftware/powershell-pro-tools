using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PowerShellTools.Common;

namespace PowerShellTools.Common
{
    public static class ConversionFactory
    {
        public static List<IPowerShellModule> Convert(PSDataCollection<PSModuleInfo> collection)
        {
            return collection.Select(x => new PowerShellModule(x))
                .ToList<IPowerShellModule>();
        }

        public static List<IPowerShellCommand> Convert(PSDataCollection<CommandInfo> collection)
        {
            return collection.Select(x => new PowerShellCommand(x))
                .ToList<IPowerShellCommand>();
        }

        public static List<IPowerShellCommandMetadata> Convert(PSDataCollection<CommandMetadata> collection)
        {
            return collection.Select(x => new PowerShellCommandMetadata(x))
                .ToList<IPowerShellCommandMetadata>();
        }

        public static List<IPowerShellParameterMetadata> Convert(Dictionary<string, ParameterMetadata> collection)
        {
            return collection.Select(x => new PowerShellParameterMetadata(x.Key, x.Value))
                .ToList<IPowerShellParameterMetadata>();
        }

        public static List<IPowerShellParameterSetMetadata> Convert(Dictionary<string, ParameterSetMetadata> collection)
        {
            return collection.Select(x => new PowerShellParameterSetMetadata(x.Key, x.Value))
                .ToList<IPowerShellParameterSetMetadata>();
        }

        public static ParameterType MapParameterType(Type type)
        {
            type = GetBaseType(type);
            var typeName = type.Name.ToLowerInvariant();

            switch (typeName)
            {
                case "string":
                    return ParameterType.String;
                case "object":
                    return ParameterType.Object;
                case "char":
                    return ParameterType.Char;
                case "byte":
                    return ParameterType.Byte;
                case "uint32":
                case "int":
                    return ParameterType.Int32;
                case "uint64":
                case "long":
                    return ParameterType.Int64;
                case "float":
                    return ParameterType.Float;
                case "decimal":
                    return ParameterType.Decimal;
                case "double":
                    return ParameterType.Double;
                case "array":
                    return ParameterType.Array;
                case "bool":
                    return ParameterType.Boolean;
                case "switchparameter":
                    return ParameterType.Switch;
                case "enum":
                    return ParameterType.Enum;
                default:
                    return ParameterType.Unsupported;
            }
        }

        public static Type GetBaseType(Type type)
        {
            while (type.BaseType != null)
            {
                var baseTypeName = type.BaseType.Name.ToLowerInvariant();
                if (baseTypeName == "object" | baseTypeName == "valuetype")
                {
                    break;
                }

                type = type.BaseType;
            }

            return type;
        }
    }
}
