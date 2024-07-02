using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.Classification;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Test.Commands
{
    [TestClass]
    public class ParameterEditorViewModelUnitTests
    {
        [TestMethod]
        public void ShouldDetectInputErrors()
        {
            var model = new ParameterEditorModel(
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

            var viewModel = new ParameterEditorViewModel(model);
            model.Parameters[1].Value = "MoreThanOneChar";
            Assert.AreEqual<bool>(false, viewModel.IsSaveButtonEnabled);

            model.Parameters[1].Value = "C";
            Assert.AreEqual<bool>(true, viewModel.IsSaveButtonEnabled);

            model.Parameters[2].Value = (byte)122;
            Assert.AreEqual<bool>(true, viewModel.IsSaveButtonEnabled);

            model.Parameters[2].Value = -1;
            Assert.AreEqual<bool>(false, viewModel.IsSaveButtonEnabled);

            model.Parameters[2].Value = "StringShouldNotWork";
            Assert.AreEqual<bool>(false, viewModel.IsSaveButtonEnabled);
        }

        [TestMethod]
        public void ShouldDetectMantoryParametersNotSet()
        {
            var model = new ParameterEditorModel(
                new ObservableCollection<ScriptParameterViewModel>()
                {
                    new ScriptParameterViewModel(new ScriptParameter()
                        {
                            Name = "StringType",
                            Type = DataTypeConstants.StringType,
                            DefaultValue = null,
                            IsMandatory = true
                        }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "CharType",
                        Type = DataTypeConstants.CharType,
                        DefaultValue = null,
                        IsMandatory = true
                        
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "ByteType",
                        Type = DataTypeConstants.ByteType,
                        DefaultValue = null,
                        IsMandatory = true
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "IntType",
                        Type = DataTypeConstants.Int32Type,
                        DefaultValue = null,
                        IsMandatory = true
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "BoolType",
                        Type = DataTypeConstants.BoolType,
                        DefaultValue = null,
                        IsMandatory = true
                    })
                },

                PowerShellParseUtilities.GenerateCommonParameters());

            var viewModel = new ParameterEditorViewModel(model);
            model.Parameters[0].Value = null;
            Assert.AreEqual<bool>(true, viewModel.Parameters[0].HasValidationResult);
            Assert.AreEqual<bool>(true, viewModel.Parameters[0].ValidationResult.IsWarning);

            model.Parameters[0].Value = string.Empty;
            Assert.AreEqual<bool>(true, viewModel.Parameters[1].HasValidationResult);
            Assert.AreEqual<bool>(true, viewModel.Parameters[1].ValidationResult.IsWarning);

            model.Parameters[1].Value = null;
            Assert.AreEqual<bool>(true, viewModel.Parameters[1].HasValidationResult);
            Assert.AreEqual<bool>(true, viewModel.Parameters[1].ValidationResult.IsWarning);

            model.Parameters[2].Value = null;
            Assert.AreEqual<bool>(true, viewModel.Parameters[2].HasValidationResult);
            Assert.AreEqual<bool>(true, viewModel.Parameters[2].ValidationResult.IsWarning);

            model.Parameters[3].Value = null;
            Assert.AreEqual<bool>(true, viewModel.Parameters[3].HasValidationResult);
            Assert.AreEqual<bool>(true, viewModel.Parameters[3].ValidationResult.IsWarning);

            model.Parameters[4].Value = null;
            Assert.AreEqual<bool>(true, viewModel.Parameters[4].HasValidationResult);
            Assert.AreEqual<bool>(true, viewModel.Parameters[4].ValidationResult.IsWarning);
        }

        [TestMethod]
        public void ShouldCollapseParameterSetNamesWhenThereIsNone()
        {
            var model = new ParameterEditorModel(
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

            var viewModel = new ParameterEditorViewModel(model);
            Assert.AreEqual<string>(null, viewModel.SelectedParameterSetName);
            Assert.AreEqual<IList<string>>(null, viewModel.ParameterSetNames);
            Assert.AreEqual<bool>(false, viewModel.HasParameterSets);
        }

        [TestMethod]
        public void ShouldIncludeParametersWithinDefaultSet()
        {
            var model = new ParameterEditorModel(
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

            var viewModel = new ParameterEditorViewModel(model);
            Assert.AreEqual<bool>(true, viewModel.HasParameterSets);

            var expectedParameters = new ObservableCollection<ScriptParameterViewModel>()
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
                    }),
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
                };

            var equalityComparer = new ScriptParameterViewModelEqualityComparer();
            bool equal = Enumerable.SequenceEqual<ScriptParameterViewModel>(expectedParameters, model.Parameters, equalityComparer);

            // Parameters should inclulde the parameters within default Set.
            Assert.AreEqual<bool>(true, equal);
        }

        [TestMethod]
        public void ShouldUpdateParametersWithParameterSetChanged()
        {
            var model = new ParameterEditorModel(
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

            var viewModel = new ParameterEditorViewModel(model);
            viewModel.SelectedParameterSetName = "Set2";

            var expectedParameters = new ObservableCollection<ScriptParameterViewModel>()
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
                    }),
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
                };

            var equalityComparer = new ScriptParameterViewModelEqualityComparer();
            bool equal = Enumerable.SequenceEqual<ScriptParameterViewModel>(expectedParameters, model.Parameters, equalityComparer);

            // Parameters should inclulde the parameters within default Set.
            Assert.AreEqual<bool>(true, equal);
        }
    }
}
