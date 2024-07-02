using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.DebugEngine.PromptUI;

namespace PowerShellTools.Test.DebugEngine.PromptUI
{
    [TestClass]
    public class ReadHostPromptForChoicesViewModelUnitTests
    {
        [TestMethod]
        public void ShouldInitializeViewModelCorrectly()
        {
            ReadHostPromptForChoicesViewModel viewModel = new ReadHostPromptForChoicesViewModel(
                "Caption",
                "Message",
                new ChoiceItem[] {
                    new ChoiceItem("Label_Apple", "This is an Apple"),
                    new ChoiceItem("Label_Banana", "This is a banana"),
                    new ChoiceItem("Label_Orange", "This is an orange")
                },
                1);

            Assert.AreEqual<string>("Caption", viewModel.Caption);
            Assert.AreEqual<string>("Message", viewModel.Message);
            Assert.AreEqual<bool>(false, viewModel.Choices[0].IsDefault);
            Assert.AreEqual<bool>(true, viewModel.Choices[1].IsDefault);
            Assert.AreEqual<bool>(false, viewModel.Choices[2].IsDefault);
        }

        [TestMethod]
        public void ShouldExecuteCommandCorrectly()
        {
            ReadHostPromptForChoicesViewModel viewModel = new ReadHostPromptForChoicesViewModel(
                "Caption",
                "Message",
                new ChoiceItem[] {
                    new ChoiceItem("Label_Apple", "This is an Apple"),
                    new ChoiceItem("Label_Banana", "This is a banana"),
                    new ChoiceItem("Label_Orange", "This is an orange")
                },
                1);

            Assert.AreEqual<bool>(false, viewModel.Choices[0].IsDefault);
            Assert.AreEqual<bool>(true, viewModel.Choices[1].IsDefault);
            Assert.AreEqual<bool>(false, viewModel.Choices[2].IsDefault);

            viewModel.Command.Execute("Label_Orange");
            Assert.AreEqual<int>(2, viewModel.UserChoice);
        }
    }
}
