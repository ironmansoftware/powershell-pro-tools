using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    internal class ParameterEditorModel
    {
        public ParameterEditorModel(ObservableCollection<ScriptParameterViewModel> parameters,
                                    IList<ScriptParameterViewModel> commonParameters)
            : this(parameters, commonParameters, null, null, null)
        {

        }

        public ParameterEditorModel(ObservableCollection<ScriptParameterViewModel> parameters,
                                    IList<ScriptParameterViewModel> commonParameters,
                                    IDictionary<string, IList<ScriptParameterViewModel>> parameterSetToParametersDict,
                                    IList<string> parameterSetNames,
                                    string selectedParameterSetName)
        {
            this.Parameters = parameters;
            this.CommonParameters = commonParameters;
            this.ParameterSetToParametersDict = parameterSetToParametersDict;
            this.ParameterSetNames = parameterSetNames;
            this.SelectedParameterSetName = selectedParameterSetName;
        }

        public ObservableCollection<ScriptParameterViewModel> Parameters { get; set; }

        public IList<ScriptParameterViewModel> CommonParameters { get; set; }

        public IDictionary<string, IList<ScriptParameterViewModel>> ParameterSetToParametersDict { get; set; }

        public IList<string> ParameterSetNames { get; set; }

        public string SelectedParameterSetName { get; set; }
    }
}
