using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Language;
using PowerShellTools.Commands.UserInterface;
using PowerShellTools.Common;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Utilities help parse parameters in Param block in a script
    /// </summary>
    internal static class PowerShellParseUtilities
    {
        private const string ValidateSetTypeName = "ValidateSet";
        private const string ParameterSetArgumentName = "ParameterSetName";
        private const string MandatoryArgumentName = "Mandatory";
        private static readonly Version CurrentPowershellVersion = DependencyUtilities.GetInstalledPowerShellVersion();
        private static readonly Version RequiredVersionForInformationCommonParams = new Version(5, 0);

        /// <summary>
        /// Try to find a Param block on the top level of an AST.
        /// </summary>
        /// <param name="ast">The targeting AST.</param>
        /// <param name="paramBlockAst">Wanted Param block.</param>
        /// <returns>True if there is one. Otherwise, false.</returns>
        public static bool HasParamBlock(Ast ast, out ParamBlockAst paramBlockAst)
        {
            paramBlockAst = (ParamBlockAst)ast.Find(p => p is ParamBlockAst, false);

            return paramBlockAst != null;
        }

        /// <summary>
        /// Try to parse a Param block and form them into a model containing parameters related data.
        /// </summary>
        /// <param name="paramBlockAst">The targeting Param block.</param>
        /// <returns>A model containing parameters related data.</returns>
        public static ParameterEditorModel ParseParameters(ParamBlockAst paramBlockAst)
        {
            ObservableCollection<ScriptParameterViewModel> scriptParameters = new ObservableCollection<ScriptParameterViewModel>();
            IDictionary<string, IList<ScriptParameterViewModel>> parameterSetToParametersDict = new Dictionary<string, IList<ScriptParameterViewModel>>();
            IList<string> parameterSetNames = new List<string>();

            if (paramBlockAst != null)
            {
                // First, filter all parameter types not supported yet.
                var parametersList = paramBlockAst.Parameters.
                    Where(p => !DataTypeConstants.UnsupportedDataTypes.Contains(p.StaticType.FullName)).
                    ToList();

                foreach (var p in parametersList)
                {
                    HashSet<object> allowedValues = new HashSet<object>();
                    bool isParameterSetDefined = false;
                    string parameterSetName = null;
                    bool isMandatory = false;

                    foreach (var a in p.Attributes)
                    {
                        // Find if there defines attribute ValidateSet
                        if (a.TypeName.FullName.Equals(ValidateSetTypeName, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (ConstantExpressionAst pa in ((AttributeAst)a).PositionalArguments)
                            {
                                allowedValues.Add(pa.Value);
                            }
                        }

                        // Find if there defines attribute ParameterNameSet
                        // Find if there defines attribute Mandatory
                        if (a is AttributeAst)
                        {
                            foreach (var arg in ((AttributeAst)a).NamedArguments)
                            {
                                if (!isParameterSetDefined)
                                {
                                    isParameterSetDefined = arg.ArgumentName.Equals(ParameterSetArgumentName, StringComparison.OrdinalIgnoreCase);
                                    if (isParameterSetDefined)
                                    {
                                        parameterSetName = ((StringConstantExpressionAst)arg.Argument).Value;
                                    }
                                }

                                if (!isMandatory)
                                {
                                    bool isMandatoryDefined = arg.ArgumentName.Equals(MandatoryArgumentName, StringComparison.OrdinalIgnoreCase);
                                    if (isMandatoryDefined)
                                    {
                                        if (arg.Argument is VariableExpressionAst)
                                        {
                                            isMandatory = ((VariableExpressionAst)arg.Argument).VariablePath.UserPath.Equals("true", StringComparison.OrdinalIgnoreCase);
                                        }

                                        if (arg.Argument is ConstantExpressionAst)
                                        {
                                            isMandatory = Convert.ToBoolean(((ConstantExpressionAst) arg.Argument).Value);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Get parameter type
                    string type = p.StaticType.FullName;
                    if (type.EndsWith(DataTypeConstants.ArrayType, StringComparison.OrdinalIgnoreCase))
                    {
                        type = DataTypeConstants.ArrayType;
                    }

                    // Get parameter name
                    string name = p.Name.VariablePath.UserPath;

                    // Get paramter default value, null it is if there is none specified 
                    object defaultValue = null;
                    if (p.DefaultValue != null)
                    {
                        if (p.DefaultValue is ConstantExpressionAst)
                        {
                            defaultValue = ((ConstantExpressionAst)p.DefaultValue).Value;
                        }
                        else if (p.DefaultValue is VariableExpressionAst)
                        {
                            defaultValue = ((VariableExpressionAst)p.DefaultValue).VariablePath.UserPath;
                        }
                    }

                    // Construct a ScriptParameterViewModel based on whether a parameter set is found
                    ScriptParameterViewModel newViewModel = null;
                    if (isParameterSetDefined && parameterSetName != null)
                    {
                        newViewModel = new ScriptParameterViewModel(new ScriptParameter(name, type, defaultValue, allowedValues, parameterSetName, isMandatory));
                        IList<ScriptParameterViewModel> existingSets;
                        if (parameterSetToParametersDict.TryGetValue(parameterSetName, out existingSets))
                        {
                            existingSets.Add(newViewModel);
                        }
                        else
                        {
                            parameterSetNames.Add(parameterSetName);
                            parameterSetToParametersDict.Add(parameterSetName, new List<ScriptParameterViewModel>() { newViewModel });
                        }
                    }
                    else
                    {
                        newViewModel = new ScriptParameterViewModel(new ScriptParameter(name, type, defaultValue, allowedValues, isMandatory));
                        scriptParameters.Add(newViewModel);
                    }
                }
            }
            // Construct the actual model
            ParameterEditorModel model = new ParameterEditorModel(scriptParameters,
                                                                  GenerateCommonParameters(),
                                                                  parameterSetToParametersDict.Count > 0 ? parameterSetToParametersDict : null,
                                                                  parameterSetNames.Count > 0 ? parameterSetNames : null,
                                                                  parameterSetNames.FirstOrDefault());

            return model;
        }

        internal static IList<ScriptParameterViewModel> GenerateCommonParameters()
        {
            List<ScriptParameterViewModel> commonParameters = new List<ScriptParameterViewModel>()
            {
                // Debug
                new ScriptParameterViewModel(
                    new ScriptParameter("Debug", DataTypeConstants.SwitchType, null, new HashSet<object>())
                    ),

                // ErrorAction
                new ScriptParameterViewModel(
                    new ScriptParameter("ErrorAction", DataTypeConstants.EnumType, String.Empty, new HashSet<object>
                        {String.Empty, "SilentlyContinue", "Stop", "Continue", "Inquire", "Ignore", "Suspend"})
                    ),

                // ErrorVariable
                new ScriptParameterViewModel(
                    new ScriptParameter("ErrorVariable", DataTypeConstants.StringType, null, new HashSet<object>())
                    ),

                // OutBuffer
                new ScriptParameterViewModel(
                    new ScriptParameter("OutBuffer", DataTypeConstants.StringType, null, new HashSet<object>())
                    ),

                // OutVariable
                new ScriptParameterViewModel(
                    new ScriptParameter("OutVariable", DataTypeConstants.StringType, null, new HashSet<object>())
                    ),

                // PipelineVariable
                new ScriptParameterViewModel(
                    new ScriptParameter("PipelineVariable", DataTypeConstants.StringType, null, new HashSet<object>())
                    ),

                //Verbose
                new ScriptParameterViewModel(
                    new ScriptParameter("Verbose", DataTypeConstants.SwitchType, null, new HashSet<object>())
                    ),

                // WarningAction
                new ScriptParameterViewModel(
                    new ScriptParameter("WarningAction", DataTypeConstants.EnumType, String.Empty, new HashSet<object>()
                        {String.Empty, "SilentlyContinue", "Stop", "Continue", "Inquire", "Ignore", "Suspend"})
                    ),

                // WarningVariable
                new ScriptParameterViewModel(
                    new ScriptParameter("WarningVariable", DataTypeConstants.StringType, null, new HashSet<object>())
                    )
            };

            if(CurrentPowershellVersion >= RequiredVersionForInformationCommonParams)
            {
                // InformationAction
                commonParameters.Add(new ScriptParameterViewModel(
                    new ScriptParameter("InformationVariable", DataTypeConstants.StringType, null, new HashSet<object>())
                    ));

                // InformationVariable
                commonParameters.Add(new ScriptParameterViewModel(
                    new ScriptParameter("InformationAction", DataTypeConstants.EnumType, String.Empty, new HashSet<object>()
                        {String.Empty, "SilentlyContinue", "Stop", "Continue", "Inquire", "Ignore", "Suspend"})
                    ));
            }

            return commonParameters;
        }
    }
}
