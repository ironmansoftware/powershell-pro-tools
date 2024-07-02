using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.Windows.Design.Host;
using PowerShellTools.Common.Logging;

namespace PowerShellToolsPro.WpfSupport
{
	public class WpfEventBindingProvider : EventBindingProvider
	{
		private readonly Func<IWpfTextView> _textView;
		private readonly Func<IWpfTextView> _xamlTextView;
		private static readonly ILog Log = LogManager.GetLogger("WpfEventBindingProvider");

		public WpfEventBindingProvider(Func<IWpfTextView> xamlTextView, Func<IWpfTextView> textView)
		{
			if (xamlTextView == null)
			{
				Log.Error("xamlTextView is null");
				throw new ArgumentNullException("xamlTextView");
			}

			if (textView == null)
			{
				Log.Error("textView is null");
				throw new ArgumentNullException("textView");
			}

			_textView = textView;
			_xamlTextView = xamlTextView;
		}

		public override bool AddEventHandler(EventDescription eventDescription, string objectName, string methodName)
        {
            return true;
        }

		public override bool AllowClassNameForMethodName()
		{
			return true;
		}

		public override void AppendStatements(EventDescription eventDescription, string methodName, string statements, int relativePosition)
		{
			throw new NotImplementedException();
		}

		public override string CodeProviderLanguage => "PowerShell";

		private Ast GetAst()
		{
			var textView = _textView();

			if (textView == null)
			{
				Log.Error("_textView returned a null textView");
				return null;
			}

			var textBuffer = textView.TextBuffer;

			if (textBuffer == null)
			{
				Log.Error("textView.TextBuffer is null.");
				return null;
			}

			var snapshotText = textBuffer.CurrentSnapshot.GetText();

			if (snapshotText == null)
			{
				Log.Error("snapshotText is null");
				return null;
			}

			Token[] tokens;
			ParseError[] errors;
			//
			//	We are reparsing ourself here. PoshTools proper will do this at some point and time but
			//  it's async and we want the AST now.
			//	If this becomes heavy, we need to wait on the parsing somehow and then grab the AST off
			//  the buffer: return textBuffer.Properties["PSAst"] as Ast;
			//
			return Parser.ParseInput(snapshotText, out tokens, out errors);
		}

		public override bool CreateMethod(EventDescription eventDescription, string methodName, string initialStatements)
		{
			Log.DebugFormat("CreateMethod: {0}, {1}", methodName, initialStatements);

			var textView = _textView();

			if (textView == null)
			{
				Log.Error("textView is null");
				return false;
			}

			var textBuffer = textView.TextBuffer;

			if (textBuffer == null)
			{
				Log.Error("textBuffer is null");
				return false;
			}

			var controlName = GetSelectedControlName() ?? SetSelectedControlName();

            if (controlName == null)
            {
                return false;
            }

            CreateXamlEdit(eventDescription, controlName);
			CreateControlVariable(controlName);

			var method = FindMethod(methodName);
			if (method == null)
			{
                CreateEventHandler(eventDescription, methodName, controlName);

				var ast = GetAst();
				var offset = FindOffsetBeforeShowDialog(ast);
				using (var edit = textBuffer.CreateEdit())
				{
					var text = BuildMethod(
						eventDescription,
						methodName,
						string.Empty,
						textView.Options.IsConvertTabsToSpacesEnabled() ?
							textView.Options.GetIndentSize() :
							-1);

					edit.Insert(offset, text);
					edit.Apply();
				}
			}

			ShowMethod(eventDescription, methodName);

			return true;
		}

		private void CreateErrorComment(string comment)
        {
			var textView = _textView();
			var textBuffer = textView.TextBuffer;

			var loadXamlFunction = FindMethod("Import-Xaml");
			using (var edit = textBuffer.CreateEdit())
			{
				var functionDefinition = $"\r\n#ERROR: {comment}\r\n";
				edit.Insert(loadXamlFunction.Extent.EndOffset + 1, functionDefinition);
				edit.Apply();
			}
		}

		private void CreateEventHandler(EventDescription eventDescription, string methodName, string controlName)
		{
			var textView = _textView();
			var textBuffer = textView.TextBuffer;

			var setEventHandlersFunction = FindMethod("Set-EventHandler");
			if (setEventHandlersFunction == null)
			{
				var loadXamlFunction = FindMethod("Import-Xaml");
				using (var edit = textBuffer.CreateEdit())
				{
					var functionDefinition = "\r\nfunction Set-EventHandler {\r\n}";
					edit.Insert(loadXamlFunction.Extent.EndOffset + 1, functionDefinition);
					edit.Apply();
				}

				using (var edit = textBuffer.CreateEdit())
				{
					var ast = GetAst();
					var offsetBeforeShowDialog = FindOffsetBeforeShowDialog(ast);
					var functionDefinition = "Set-EventHandler\r\n";
					edit.Insert(offsetBeforeShowDialog, functionDefinition);
					edit.Apply();
				}

				setEventHandlersFunction = FindMethod("Set-EventHandler");
			}

			var paramBlock = "param(";
			foreach (var parameter in eventDescription.Parameters)
			{
				paramBlock += $"[{parameter.TypeName.Replace("<", "[").Replace(">", "]")}]${parameter.Name},";
			}

			paramBlock = paramBlock.TrimEnd(',');
			paramBlock += $")\r\n\t\t{methodName}";

			foreach (var parameter in eventDescription.Parameters)
			{
				paramBlock += $" -{parameter.Name} ${parameter.Name}";
			}

			var lastStatement = setEventHandlersFunction.Body.EndBlock.Statements.LastOrDefault();
			var endOffset = lastStatement?.Extent.EndOffset ?? setEventHandlersFunction.Body.EndBlock.Extent.StartOffset + 1;

			using (var edit = textBuffer.CreateEdit())
			{
				var eventText = $"\r\n\t${controlName}.add_{eventDescription.Name}({{\r\n\t\t{paramBlock}\r\n\t}})";
				edit.Insert(endOffset, eventText);
				edit.Apply();
			}
		}

        private IEnumerable<string> GetEventHandlers(string variableName)
        {
            var setEventHandlersFunction = FindMethod("Set-EventHandler");

            var eventHandlers = setEventHandlersFunction.Body.EndBlock.FindAll(m => m is InvokeMemberExpressionAst, true).OfType<InvokeMemberExpressionAst>();

            foreach (var eventHandler in eventHandlers)
            {

                var variable = eventHandler.Expression.ToString().Replace("$", string.Empty);
                if (variableName.Equals(variable, StringComparison.OrdinalIgnoreCase))
                {
                    yield return eventHandler.Member.ToString().Replace("add_", string.Empty);
                }
            }
        }

		private string GetSelectedControlName()
		{
            try
            {
                var xamlTextView = _xamlTextView();
                var selectionIndex = xamlTextView.Selection.ActivePoint.Position.Position;
                var currentXamlText = xamlTextView.TextBuffer.CurrentSnapshot.GetText();

                var xamlUtilities = new XamlUtilities();
                var controlName = xamlUtilities.GetNameOfSelectedControl(currentXamlText, selectionIndex);

                return controlName;
            }
            catch (Exception ex)
            {
				Log.Error("Failed to get XAML Text VIew.", ex);
            }

            return null;
        }

        private string SetSelectedControlName()
        {
            try
            {
                var xamlTextView = _xamlTextView();
                var selectionIndex = xamlTextView.Selection.ActivePoint.Position.Position;

				var random = new Random();
                var id = random.Next(1, 1000);
                var controlName = "ctrl" + id;
                var edit = xamlTextView.TextBuffer.CreateEdit(EditOptions.DefaultMinimalChange, 0, "controlName");
                edit.Insert(selectionIndex, $"x:Name=\"{controlName}\"");
                edit.Apply();

				return controlName;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get XAML Text VIew.", ex);
            }

            return null;
        }

		private void CreateXamlEdit(EventDescription eventDescription, string controlName)
		{
			var textView = _textView();
			var textBuffer = textView.TextBuffer;
			var loadXamlFunction = FindMethod("Import-Xaml");

			// Don't add this again if it's already there.
			var removeNodeText = $"$xaml.SelectNodes(\"//*[@x:Name='{controlName}']\", $manager)[0].RemoveAttribute('{eventDescription.Name}')";
			if (loadXamlFunction.Body.EndBlock.ToString().Contains(removeNodeText))
			{
				return;
			}

			// Add removal of a particular event handler
			var offset = FindOffsetBeforeXamlReaderInstantiation(loadXamlFunction);
			using (var edit = textBuffer.CreateEdit())
			{
				edit.Insert(offset, "\t" + removeNodeText + "\r\n");
				edit.Apply();
			}
		}

		private static bool ControlVariableAlreadyDeclared(FunctionDefinitionAst ast, string controlName)
		{
			var commands = ast.Body.EndBlock.Statements.Where(m => m is PipelineAst)
				.Cast<PipelineAst>()
				.Where(m => m.PipelineElements[0] is CommandAst)
				.Select(m => m.PipelineElements[0])
				.Cast<CommandAst>()
				.Where(m => m.GetCommandName() == "New-Variable");

			foreach (var command in commands)
			{
				var nextIsName = false;
				foreach (var commandElement in command.CommandElements)
				{
					if (nextIsName)
					{
						if (commandElement.ToString().Equals($"'{controlName}'", StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
					nextIsName = commandElement.ToString() == "-Name";

				}
			}

			return false;
		}

		private void CreateControlVariable(string controlName)
		{
			var textView = _textView();
			var textBuffer = textView.TextBuffer;

			var controlVariableFunction = FindMethod("Add-ControlVariables");
			if (controlVariableFunction == null)
			{
				using (var edit = textBuffer.CreateEdit())
				{
					var functionDefinition = "function Add-ControlVariables {\r\n}\r\n\r\n";
					edit.Insert(0, functionDefinition);
					edit.Apply();
				}

				using (var edit = textBuffer.CreateEdit())
				{
					var ast = GetAst();
					var offsetBeforeShowDialog = FindOffsetBeforeShowDialog(ast);
					var functionDefinition = "Add-ControlVariables\r\n";
					edit.Insert(offsetBeforeShowDialog, functionDefinition);
					edit.Apply();
				}

				controlVariableFunction = FindMethod("Add-ControlVariables");
			}

			if (ControlVariableAlreadyDeclared(controlVariableFunction, controlName))
			{
				return;
			}

			var lastStatement = controlVariableFunction.Body.EndBlock.Statements.LastOrDefault();
			var endOffset = lastStatement?.Extent.EndOffset ?? controlVariableFunction.Body.EndBlock.Extent.StartOffset + 1;
			using (var edit = textBuffer.CreateEdit())
			{
				var newVariable = $"\t\r\nNew-Variable -Name '{controlName}' -Value $window.FindName('{controlName}') -Scope 1 -Force";
				edit.Insert(endOffset, newVariable);
				edit.Apply();
			}
		}

		private static int FindOffsetBeforeShowDialog(Ast ast)
		{
			var showDialog = ast.FindAll(m => m is InvokeMemberExpressionAst, true)
				.Cast<InvokeMemberExpressionAst>()
				.FirstOrDefault(m => m.Member.Extent.Text == "ShowDialog");

			return showDialog?.Extent.StartOffset - 1 ?? ast.Extent.EndOffset;
		}

		private static int FindOffsetBeforeXamlReaderInstantiation(Ast ast)
		{
			var xamlLoad = ast.FindAll(m => m is AssignmentStatementAst, true)
				.Cast<AssignmentStatementAst>()
				.Where(m => m.Left is VariableExpressionAst)
				.Select(m => (VariableExpressionAst) m.Left)
				.First(m => m.ToString().Equals("$xamlReader", StringComparison.OrdinalIgnoreCase));

			return xamlLoad.Extent.StartOffset - 1;
		}

		private static string BuildMethod(EventDescription eventDescription, string methodName, string indentation, int tabSize)
		{
			var text = new StringBuilder();
			text.AppendLine(indentation);
			text.Append(indentation);
			text.Append("function ");
			text.Append(methodName);
			text.Append("\r\n{\r\n");
			text.Append("\tparam(");
			foreach (var param in eventDescription.Parameters)
			{
				text.Append("$" + param.Name);
				text.Append(", ");
			}

			text.Remove(text.Length - 2, 2);
			text.AppendLine(")\r\n}");
			text.AppendLine();

			return text.ToString();
		}

		public override string CreateUniqueMethodName(string objectName, EventDescription eventDescription)
		{
			var name = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", objectName, eventDescription.Name);
			int count = 0;
			while (IsExistingMethodName(eventDescription, name))
			{
				name = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}{2}", objectName, eventDescription.Name, ++count);
			}
			return name;
		}

		public override IEnumerable<string> GetCompatibleMethods(EventDescription eventDescription)
		{
			var ast = GetAst();
			var paramCount = eventDescription.Parameters.Count() + 1;
			return ast.FindAll(m => m is FunctionDefinitionAst && ((FunctionDefinitionAst)m).Parameters?.Count == paramCount, false).Cast<FunctionDefinitionAst>().Select(m => m.Name);
		}

		public override IEnumerable<string> GetMethodHandlers(EventDescription eventDescription, string objectName)
        {
            return Array.Empty<string>(); //GetEventHandlers(objectName).Where(m => m.Equals(eventDescription.Name));
        }

		public override bool IsExistingMethodName(EventDescription eventDescription, string methodName)
		{
			return FindMethod(methodName) != null;
		}

		private FunctionDefinitionAst FindMethod(string methodName)
		{
			var ast = GetAst();
			return ast.FindFunctionDefinitionAst(methodName);
		}

		public override bool RemoveEventHandler(EventDescription eventDescription, string objectName, string methodName)
		{
			var textView = _textView();
			var textBuffer = textView.TextBuffer;

			var method = FindMethod(methodName);
			if (method != null)
			{
				// appending a method adds 2 extra newlines, we want to remove those if those are still
				// present so that adding a handler and then removing it leaves the buffer unchanged.

				using (var edit = textBuffer.CreateEdit())
				{
					int start = method.Extent.StartOffset - 1;

					// eat the newline we insert before the method
					while (start >= 0)
					{
						var curChar = edit.Snapshot[start];
						if (!Char.IsWhiteSpace(curChar))
						{
							break;
						}
						else if (curChar == ' ' || curChar == '\t')
						{
							start--;
							continue;
						}
						else if (curChar == '\n')
						{
							if (start != 0)
							{
								if (edit.Snapshot[start - 1] == '\r')
								{
									start--;
								}
							}
							start--;
							break;
						}
						else if (curChar == '\r')
						{
							start--;
							break;
						}

						start--;
					}


					// eat the newline we insert at the end of the method
					int end = method.Extent.EndOffset;
					while (end < edit.Snapshot.Length)
					{
						if (edit.Snapshot[end] == '\n')
						{
							end++;
							break;
						}
						else if (edit.Snapshot[end] == '\r')
						{
							if (end < edit.Snapshot.Length - 1 && edit.Snapshot[end + 1] == '\n')
							{
								end += 2;
							}
							else
							{
								end++;
							}
							break;
						}
						else if (edit.Snapshot[end] == ' ' || edit.Snapshot[end] == '\t')
						{
							end++;
							continue;
						}
						else
						{
							break;
						}
					}

					// delete the method and the extra whitespace that we just calculated.
					edit.Delete(Span.FromBounds(start + 1, end));
					edit.Apply();
				}

				return true;
			}
			return false;
		}

		public override bool RemoveHandlesForName(string elementName)
		{
			throw new NotImplementedException();
		}

		public override bool RemoveMethod(EventDescription eventDescription, string methodName)
		{
			throw new NotImplementedException();
		}

		public override void SetClassName(string className)
		{
		}

		public override bool ShowMethod(EventDescription eventDescription, string methodName)
		{
			var textView = _textView();

			var method = FindMethod(methodName);
			if (method != null)
			{
				textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, method.Extent.StartOffset));
				textView.Caret.EnsureVisible();
				return true;
			}

			return false;
		}

		public override void ValidateMethodName(EventDescription eventDescription, string methodName)
		{
		}

		private static string GetEmbeddedResource(string resourceLocation)
		{
			using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation))
			{
				if (stream == null) return null;

				using (var streamReader = new StreamReader(stream))
				{
					return streamReader.ReadToEnd();
				}
			}
		}
	}
}
