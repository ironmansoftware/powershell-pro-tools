using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.Classification;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Test.Classification
{
    [TestClass]
    public class PowerShellParseUtilitiesUnitTests
    {
        [TestMethod]
        public void ShouldHaveParamBlockIfScriptHasParametersDefined()
        {
            var ast = GenerateAst(scriptWithParameters);
            ParamBlockAst paramBlock = null;
            bool result = PowerShellParseUtilities.HasParamBlock(ast, out paramBlock);
            Assert.AreEqual<bool>(true, result);

            ast = GenerateAst(scriptWithParameterSets);
            result = PowerShellParseUtilities.HasParamBlock(ast, out paramBlock);
            Assert.AreEqual<bool>(true, result);
        }

        [TestMethod]
        public void ShouldNotHaveParamBlockIfScriptHasNoParametersDefined()
        {
            var ast = GenerateAst(scriptWithoutParameters);
            ParamBlockAst paramBlock = null;
            bool result = PowerShellParseUtilities.HasParamBlock(ast, out paramBlock);

            Assert.AreEqual<bool>(false, result);
        }

        [TestMethod]
        public void ShouldParseParametersCorrectly()
        {
            ParamBlockAst paramBlock = GenerateParamBlockAst(scriptWithParameters);
            var model = PowerShellParseUtilities.ParseParameters(paramBlock);

            var expectedModel = new ParameterEditorModel(
                new ObservableCollection<ScriptParameterViewModel>()
                {
                    new ScriptParameterViewModel(new ScriptParameter()
                        {
                            Name = "StringType",
                            Type = DataTypeConstants.StringType,
                            DefaultValue = null
                        }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "CharType",
                        Type = DataTypeConstants.CharType,
                        DefaultValue = null
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "ByteType",
                        Type = DataTypeConstants.ByteType,
                        DefaultValue = null
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "IntType",
                        Type = DataTypeConstants.Int32Type,
                        DefaultValue = null
                    })
                },

                PowerShellParseUtilities.GenerateCommonParameters());

            Assert.AreEqual<bool>(true, CompareParameterEditorModels(expectedModel, model));
        }

        [TestMethod]
        public void ShouldParseParameterSetNamesCorrectly()
        {
            ParamBlockAst paramBlock = GenerateParamBlockAst(scriptWithParameterSets);
            var model = PowerShellParseUtilities.ParseParameters(paramBlock);

            var expectedModel = new ParameterEditorModel(
                new ObservableCollection<ScriptParameterViewModel>()
                {
                    new ScriptParameterViewModel(new ScriptParameter()
                        {
                            Name = "StringType",
                            Type = DataTypeConstants.StringType,
                            DefaultValue = null
                        }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "CharType",
                        Type = DataTypeConstants.CharType,
                        DefaultValue = "c"
                    })
                },

                PowerShellParseUtilities.GenerateCommonParameters(),

                new Dictionary<string, IList<ScriptParameterViewModel>>()
                {
                    {"Set1", new List<ScriptParameterViewModel>()
                                {
                                    new ScriptParameterViewModel(new ScriptParameter()
                                    {
                                        Name = "LongType",
                                        Type = DataTypeConstants.Int64Type,
                                        DefaultValue = 100000,
                                        ParameterSetName = "Set1"
                                    }),
                                    new ScriptParameterViewModel(new ScriptParameter()
                                    {
                                        Name = "BoolType",
                                        Type = DataTypeConstants.BoolType,
                                        DefaultValue = null,
                                        ParameterSetName = "Set1"
                                    })
                                 }
                     },

                     {"Set2", new List<ScriptParameterViewModel>()
                                {
                                    new ScriptParameterViewModel(new ScriptParameter()
                                    {
                                        Name = "SwitchType",
                                        Type = DataTypeConstants.SwitchType,
                                        ParameterSetName = "Set2",
                                    }),
                                    new ScriptParameterViewModel(new ScriptParameter()
                                    {
                                        Name = "DecimalType",
                                        Type = DataTypeConstants.DecimalType,
                                        ParameterSetName = "Set2"
                                    })
                                 }
                     }
                },

                new List<string>() { "Set1", "Set2" },

                "Set1");

            Assert.AreEqual<bool>(true, CompareParameterEditorModels(expectedModel, model));
        }

        private ParamBlockAst GenerateParamBlockAst(string script)
        {
            var ast = GenerateAst(script);
            ParamBlockAst paramBlock = null;
            if (PowerShellParseUtilities.HasParamBlock(ast, out paramBlock))
            {
                return paramBlock;
            }
            return null;
        }

        private Ast GenerateAst(string script)
        {
            Token[] tokens;
            ParseError[] errors;
            return Parser.ParseInput(script, out tokens, out errors);
        }

        private bool CompareParameterEditorModels(ParameterEditorModel model1, ParameterEditorModel model2)
        {
            if (model1 == model2)
            {
                return true;
            }

            if ((model1 == null && model2 != null) ||
                (model1 != null && model2 == null) ||
                (model1.ParameterSetToParametersDict == null && model2.ParameterSetToParametersDict != null) ||
                (model1.ParameterSetToParametersDict != null && model2.ParameterSetToParametersDict == null))
            {
                return false;
            }

            if (model1.Parameters.Count != model2.Parameters.Count ||
                model1.CommonParameters.Count != model2.CommonParameters.Count ||
                (model1.ParameterSetToParametersDict != null &&
                 model2.ParameterSetToParametersDict != null &&
                 model1.ParameterSetToParametersDict.Count != model2.ParameterSetToParametersDict.Count))
            {
                return false;
            }

            // Compare Parameters and CommonParameters
            int parametersCount = model1.Parameters.Count;
            var equalityComparer = new ScriptParameterViewModelEqualityComparer();
            bool equal = Enumerable.SequenceEqual<ScriptParameterViewModel>(model1.Parameters, model2.Parameters, equalityComparer) &&
                         Enumerable.SequenceEqual<ScriptParameterViewModel>(model1.CommonParameters, model2.CommonParameters, equalityComparer);

            if (!equal)
            {
                return false;
            }

            if (model1.SelectedParameterSetName != null)
            {
                // Compare SelectedParameterSetName and ParameterSetNames
                equal = equal &&
                        String.Equals(model1.SelectedParameterSetName, model2.SelectedParameterSetName, StringComparison.Ordinal) &&
                        Enumerable.SequenceEqual<string>(model1.ParameterSetNames, model2.ParameterSetNames, StringComparer.Ordinal);
                if (!equal)
                {
                    return false;
                }

                // Compare ParameterSetToParametersDict
                foreach (var set in model1.ParameterSetNames)
                {
                    equal = equal &&
                            Enumerable.SequenceEqual<ScriptParameterViewModel>(model1.ParameterSetToParametersDict[set],
                                                                               model2.ParameterSetToParametersDict[set],
                                                                               equalityComparer);
                    if (!equal)
                    {
                        return false;
                    }
                }
            }
            return equal;
        }

        private bool CompareListOfObjects(IEnumerable<object> objects1, IEnumerable<object> objects2)
        {
            if (objects1 == objects2)
            {
                return true;
            }

            if ((objects1 == null && objects2 != null) ||
                (objects1 != null && objects2 == null))
            {
                return false;
            }

            return Enumerable.SequenceEqual(objects1.OrderBy(t => t), objects2.OrderBy(t => t));
        }

        # region Scripts for test

        private static string scriptWithParameters = @"
#
# ScriptWithParameters.ps1
#

Param(
    [parameter(Mandatory = $false)]
    [string]
    $StringType,

	[parameter(Mandatory = $false)]
    [char]
    $CharType,

    [Parameter(Mandatory=$false)] 
    [byte] 
    $ByteType,

	[Parameter(Mandatory=$false)] 
    [int] 
	[ValidateRange(21,65)] 
    $IntType
) 

Write-Host ""Script with Parameters""";


        private static string scriptWithParameterSets = @"
#
# ScriptWithParameterSets.ps1
#

Param(
    [parameter(Mandatory = $false)]
    [string]
    $StringType,

	[parameter(Mandatory = $false)]
    [char]
    $CharType = 'c',
	 
    [Parameter(ParameterSetName=""Set1"")]
    [long]
	$LongType = 100000,
	 
	[Parameter(ParameterSetName=""Set1"")]
    [bool]
	$BoolType,

    [Parameter(ParameterSetName=""Set2"")]  
    [Switch]
	$SwitchType,

	[Parameter(ParameterSetName=""Set2"")]
    [decimal]
	$DecimalType
) 

Write-Host ""Script with Parameters""";

        private static string scriptWithoutParameters = @"
<#
.Synopsis
   Short description
.DESCRIPTION
   Long description
.EXAMPLE
   Example of how to use this cmdlet
.EXAMPLE
   Another example of how to use this cmdlet
#>

function Verb-Noun
{
    [CmdletBinding()]
    [OutputType([int])]
    Param
    (
        # Param1 help description hello
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0
		 )]
        $Param1,

        # Param2 help description
        [Parameter('')]
        [int]
        $Param2

    )
}

function MyFunc()
{
    Write-Host
    Set-ExecutionPolicy -ExecutionPolicy Undefined 
}";
        #endregion
    }
}
