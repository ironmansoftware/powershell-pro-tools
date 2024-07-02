using System.Management.Automation.Language;
using Microsoft.VisualStudio.Text.Editor;
using PowerShellToolsPro.Test.Mocking;
using PowerShellToolsPro.WpfSupport;
using Xunit;
using Microsoft.Windows.Design.Host;
using System;
using PowerShellProTools.Host;

namespace PowerShellToolsPro.Test.WpfSupport
{
	public class WpfEventBindingProviderTest
	{
		private const string XamlResourceRoot = "PowerShellToolsPro.Test.WpfSupport.Resources.Xaml.";
		private const string PowerShellResourceRoot = "PowerShellToolsPro.Test.WpfSupport.Resources.PowerShell.";

        [Fact(Skip ="Test")]
		public void ShouldBePowerShellLanguage()
		{
            var xamlTextView = new TextEditor(
        TestUtilities.GetEmbeddedResource(XamlResourceRoot + "XamlWithNamedButton.xaml"), 601);
            var powerShellTextView = new TextEditor(
                TestUtilities.GetEmbeddedResource(PowerShellResourceRoot + "BlankWpfCodeBehind.ps1"), 0);

            var provider = new WpfEventBindingProvider(() => xamlTextView.TextView, () => powerShellTextView.TextView);
            Assert.Equal("PowerShell", provider.CodeProviderLanguage);
		}

        [Fact(Skip = "Need to fix")]
        public void ShouldAddControlVariableFunction()
		{
			var xamlTextView = new TextEditor(
				TestUtilities.GetEmbeddedResource(XamlResourceRoot + "XamlWithNamedButton.xaml"), 601);
			var powerShellTextView = new TextEditor(
				TestUtilities.GetEmbeddedResource(PowerShellResourceRoot + "BlankWpfCodeBehind.ps1"), 0);

			var provider = new WpfEventBindingProvider(() => xamlTextView.TextView, () => powerShellTextView.TextView);

			var eventDescription = new EventDescription("Click", "void");

			provider.CreateMethod(eventDescription, "OnClick", "");

			var ast = GetAstFromEditor(powerShellTextView);
			Assert.NotNull(ast.FindFunctionDefinitionAst("Add-ControlVariables"));
		}

		[Fact(Skip = "Need to fix")]
		public void ShouldPlaceAddControlVariablesFunctionAtBeginningOfFile()
		{
			var xamlTextView = new TextEditor(
				TestUtilities.GetEmbeddedResource(XamlResourceRoot + "XamlWithNamedButton.xaml"), 601);
			var powerShellTextView = new TextEditor(
				TestUtilities.GetEmbeddedResource(PowerShellResourceRoot + "BlankWpfCodeBehind.ps1"), 0);

			var provider = new WpfEventBindingProvider(() => xamlTextView.TextView, () => powerShellTextView.TextView);

			var eventDescription = new EventDescription("Click", "void");

			provider.CreateMethod(eventDescription, "OnClick", "");

			var ast = GetAstFromEditor(powerShellTextView);
			var functionDefinitionAst = ast.FindFunctionDefinitionAst("Add-ControlVariables");

			Assert.Equal(0, functionDefinitionAst.Extent.StartOffset);
		}

        [Fact(Skip = "Need to fix")]
        public void ShouldAddEventRemovalToLoadXaml()
		{
			var xamlTextView = new TextEditor(
				TestUtilities.GetEmbeddedResource(XamlResourceRoot + "XamlWithNamedButton.xaml"), 601);
			var powerShellTextView = new TextEditor(
				TestUtilities.GetEmbeddedResource(PowerShellResourceRoot + "BlankWpfCodeBehind.ps1"), 0);

			var provider = new WpfEventBindingProvider(() => xamlTextView.TextView, () => powerShellTextView.TextView);

			var eventDescription = new EventDescription("Click", "void");

			provider.CreateMethod(eventDescription, "OnClick", "");

			var ast = GetAstFromEditor(powerShellTextView);
			var functionDefinitionAst = ast.FindFunctionDefinitionAst("Import-Xaml");

			Assert.Contains("$xaml.SelectNodes(\"//*[@x:Name='Test']\", $manager)[0].RemoveAttribute('Click')", functionDefinitionAst.ToString());
		}

		private static Ast GetAstFromEditor(TextEditor editor)
		{
			var text = editor.TextView.TextBuffer.CurrentSnapshot.GetText();

			Token[] tokens;
			ParseError[] errors;
			return Parser.ParseInput(text, out tokens, out errors);
		}
	}
}
