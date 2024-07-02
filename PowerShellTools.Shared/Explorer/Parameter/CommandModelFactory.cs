using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal static class CommandModelFactory
    {
        private static readonly string AllParameterSets = "__AllParameterSets";

        internal static CommandModel GenerateCommandModel(IPowerShellCommandMetadata metadata)
        {
            if (metadata == null)
            {
                return null;
            }

            var commandName = metadata.Name;
            var parameters = GetParameters(metadata.Parameters);
            var parameterSets = GetParameterSets(metadata.Parameters);

            return new CommandModel(commandName, parameters, parameterSets);
        }

        internal static List<ParameterModel> GetParameters(List<IPowerShellParameterMetadata> metadata)
        {
            var parameters = new List<ParameterModel>();

            if (metadata == null)
            {
                return parameters;
            }

            foreach (IPowerShellParameterMetadata parameter in metadata)
            {
                var parameterName = parameter.Name;
                var parameterType = parameter.Type;

                foreach (IPowerShellParameterSetMetadata set in parameter.ParameterSets)
                {
                    var setName = set.Name;
                    var isMandatory = set.IsMandatory;
                    var helpMessage = set.HelpMessage;

                    parameters.Add(new ParameterModel(setName, parameterName, parameterType, isMandatory, helpMessage));
                }
            }

            return parameters;
        }

        internal static List<string> GetParameterSets(List<IPowerShellParameterMetadata> metadata)
        {
            var sets = new List<string>();

            if (metadata == null)
            {
                return sets;
            }

            foreach (IPowerShellParameterMetadata parameter in metadata)
            {
                foreach (IPowerShellParameterSetMetadata set in parameter.ParameterSets)
                {
                    if (set.Name != AllParameterSets && !sets.Contains(set.Name))
                    {
                        sets.Add(set.Name);
                    }
                }
            }

            return sets;
        }
    }
}
