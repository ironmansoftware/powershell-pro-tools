using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal class CommonParameterModel : ParameterModel
    {
        private string _value =  string.Empty;

        public CommonParameterModel(string name, HashSet<string> choices, ParameterType type, string helpMesssage)
            :base ("CommonParameters", name, type, false, helpMesssage)
        {
            Choices = choices;
        }

        public HashSet<string> Choices { get; private set; }

        public override bool HasErrors
        {
            get
            {
                return false;
            }
        }

        internal static List<CommonParameterModel> GetCommonParameters()
        {
            List<CommonParameterModel> parameters = new List<CommonParameterModel>();

            parameters.Add(new CommonParameterModel("Debug", new HashSet<string>(), ParameterType.Switch, null));
            parameters.Add(new CommonParameterModel("ErrorAction", new HashSet<string>() { string.Empty, "SilentlyContinue", "Stop", "Continue", "Inquire", "Ignore", "Suspend" }, ParameterType.Choice, null));
            parameters.Add(new CommonParameterModel("ErrorVariable", new HashSet<string>(), ParameterType.String, null));
            parameters.Add(new CommonParameterModel("OutBuffer", new HashSet<string>(), ParameterType.String, null));
            parameters.Add(new CommonParameterModel("OutVariable", new HashSet<string>(), ParameterType.String, null));
            parameters.Add(new CommonParameterModel("PipelineVariable", new HashSet<string>(), ParameterType.String, null));
            parameters.Add(new CommonParameterModel("Verbose", new HashSet<string>(), ParameterType.Switch, null));
            parameters.Add(new CommonParameterModel("WarningAction", new HashSet<string>() { string.Empty, "SilentlyContinue", "Stop", "Continue", "Inquire", "Ignore", "Suspend" }, ParameterType.Choice, null));
            parameters.Add(new CommonParameterModel("WarningVariable", new HashSet<string>(), ParameterType.String, null));

            return parameters;
        }
    }
}
