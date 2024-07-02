using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal sealed class CommandFormatter
    {
        private const string DefaultHashTableName = "params";
        private const string AllParameterSets = "__AllParameterSets";
        private static readonly char[] ArraySplitTokens = new char[] { ',', ';' };

        internal static string FormatCommandModel(CommandModel model, CommandFormatterOptions options)
        {
            if (model == null)
            {
                return string.Empty;
            }

            var formatStyle = options.FormateStyle;
            var parameterSet = options.ParameterSet;
            var sb = new StringBuilder();

            switch (formatStyle)
            {
                case CommandFormatStyle.Inline:
                    FormatCommandModelAsInlne(sb, model, parameterSet);
                    break;
                case CommandFormatStyle.HashTable:
                    FormatCommandModelAsHashTable(sb, model, parameterSet);
                    break;
                default:
                    FormatCommandModelAsInlne(sb, model, parameterSet);
                    break;
            }

            return sb.ToString();
        }

        private static void FormatCommandModelAsInlne(StringBuilder sb, CommandModel model, string parameterSet)
        {
            // Add the command name
            sb.Append(model.Name);

            // Add all parameters
            FormatParametersAsInline(sb, model, parameterSet);
        }

        private static void FormatCommandModelAsHashTable(StringBuilder sb, CommandModel model, string parameterSet)
        {
            // Create a hashtable block
            FormatParametersAsHashTable(sb, model, parameterSet);

            // Add the command with the name of the hashtable
            sb.AppendLine(string.Format("{0} @{1}", model.Name, DefaultHashTableName));
        }

        private static void FormatParametersAsInline(StringBuilder sb, CommandModel model, string parameterSet)
        {
            foreach (ParameterModel parameter in model.Parameters)
            {
                if ((parameter.Set == parameterSet | parameter.Set == AllParameterSets) &&
                    !string.IsNullOrWhiteSpace(parameter.Value))
                {
                    var item = FormatParameter(parameter, false);
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        sb.Append(item);
                    }
                }
            }

            foreach (CommonParameterModel parameter in model.CommonParameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Value))
                {
                    var item = FormatParameter(parameter, false);
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        sb.Append(item);
                    }
                }
            }
        }

        private static void FormatParametersAsHashTable(StringBuilder sb, CommandModel model, string parameterSet)
        {
            sb.AppendLine("$params=@{");

            foreach (ParameterModel parameter in model.Parameters)
            {
                if ((parameter.Set == parameterSet | parameter.Set == AllParameterSets) &&
                    !string.IsNullOrWhiteSpace(parameter.Value))
                {
                    var item = FormatParameter(parameter, true);
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        sb.AppendLine(item);
                    }
                }
            }

            foreach (CommonParameterModel parameter in model.CommonParameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Value))
                {
                    var item = FormatParameter(parameter, true);
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        sb.AppendLine(item);
                    }
                }
            }

            sb.AppendLine("}");
        }

        private static string FormatParameter(ParameterModel parameter, bool hash)
        {
            var type = parameter.Type;
            var name = parameter.Name;
            var value = parameter.Value;

            switch (type)
            {
                case ParameterType.Unsupported:
                case ParameterType.Float:
                case ParameterType.Double:
                case ParameterType.Decimal:
                case ParameterType.Char:
                case ParameterType.Enum:
                case ParameterType.Byte:
                case ParameterType.Int32:
                case ParameterType.Int64:
                case ParameterType.String:
                case ParameterType.Choice:
                    return FormatString(name, value, hash);
                case ParameterType.Boolean:
                    return FormatBool(name, value, hash);
                case ParameterType.Switch:
                    return FormatSwitch(name, value, hash);
                case ParameterType.Array:
                    return FormatArray(name, value, hash);
                default:
                    return string.Empty;
            }
        }

        private static string FormatString(string name, string value, bool hash)
        {
            return hash ? string.Format("{0}={1};", name, QuotedString(value, true)) : string.Format(" -{0} {1}", name, QuotedString(value));
        }

        private static string FormatBool(string name, string value, bool hash)
        {
            bool set;
            if (bool.TryParse(value, out set) && set)
            {
                return hash ? string.Format("{0}={1};", name, "$true") : string.Format(" -{0} {1}", name, "$true");
            }

            return hash ? string.Format("{0}={1};", name, "$false") : string.Format(" -{0} {1}", name, "$false");
        }

        private static string FormatSwitch(string name, string value, bool hash)
        {
            bool set;
            if (bool.TryParse(value, out set) && set)
            {
                return hash ? string.Format("{0}={1};", name, "$true") : string.Format(" -{0}", name);
            }

            return string.Empty;
        }

        private static string FormatArray(string name, string value, bool hash)
        {
            var parts = GetArrayParts(value);
            var sb = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                var part = QuotedString(parts[i], true);
                sb.Append(part);

                if(i < parts.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            var arr = sb.ToString();

            return hash ? string.Format("{0}={1};", name, arr) : string.Format(" -{0} {1}", name, arr); ;
        }

        
        private static string[] GetArrayParts(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new string[] { string.Empty };
            }

            return value.Split(ArraySplitTokens, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string QuotedString(string value, bool forceQuotes = false)
        {
            value = value.Trim();

            if (value.Contains(' ') || forceQuotes)
            {
                return string.Format("\"{0}\"", value);
            }
            else
            {
                return value;
            }
        }
    }
}
