using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.Classification;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Test.Commands
{
    [TestClass]
    public class ParameterEditorHelperUnitTests
    {
        [TestMethod]
        public void ShouldGeneratedEmptyScriptArgsWhenNoValueIsGiven()
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

            string actualScriptArgs = ParameterEditorHelper.GenerateScripArgsFromModel(model);
            Assert.AreEqual<string>(String.Empty, actualScriptArgs);
        }

        [TestMethod]
        public void ShouldGeneratedCorrectScriptArgsBasedOnFullyGivenValues()
        {
            var model = new ParameterEditorModel(
                new ObservableCollection<ScriptParameterViewModel>()
                {
                    new ScriptParameterViewModel(new ScriptParameter()
                        {
                            Name = "StringType",
                            Type = DataTypeConstants.StringType,
                            DefaultValue = "DefaultString"
                        }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "CharType",
                        Type = DataTypeConstants.CharType,
                        DefaultValue = "C"
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "ByteType",
                        Type = DataTypeConstants.ByteType,
                        DefaultValue = (byte)255
                    }),
                    new ScriptParameterViewModel(new ScriptParameter()
                    {
                        Name = "IntType",
                        Type = DataTypeConstants.Int32Type,
                        DefaultValue = 1111
                    })
                },

                PowerShellParseUtilities.GenerateCommonParameters());

            string actualScriptArgs = ParameterEditorHelper.GenerateScripArgsFromModel(model);
            string expectedScriptArgs = " -StringType \"DefaultString\" -CharType \"C\" -ByteType 255 -IntType 1111";
            Assert.AreEqual<string>(expectedScriptArgs, actualScriptArgs);
        }

        [TestMethod]
        public void ShouldGeneratedCorrectScriptArgsBasedOnPartiallyGivenValues()
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
                        DefaultValue = "C"
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
                        DefaultValue = 1111
                    })
                },

                PowerShellParseUtilities.GenerateCommonParameters());

            string actualScriptArgs = ParameterEditorHelper.GenerateScripArgsFromModel(model);
            string expectedScriptArgs = " -CharType \"C\" -IntType 1111";
            Assert.AreEqual<string>(expectedScriptArgs, actualScriptArgs);
        }
    }
}
