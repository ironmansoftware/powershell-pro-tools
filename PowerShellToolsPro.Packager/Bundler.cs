
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using PowerShellToolsPro.Packager.Config;

namespace PowerShellToolsPro.Packager
{
	public class Bundler
	{
		public event EventHandler<string> Output;

		private void OnOutput(string value)
		{
			if (Output != null) Output(this, value);
		}

		private PsPackConfig _config;

		public Bundler(PsPackConfig config)
		{
			_config = config;
		}


		public BundleResult Bundle(string root)
		{
			if (!File.Exists(root))
			{
				return new BundleResult(BundleStatus.Failed, new []{$"Root file {root} not found."});
			}

			OnOutput($"Parsing file {root}.");

			Token[] tokens;
			ParseError[] errors;
			var contents = File.ReadAllText(root, Encoding.UTF8);
			var ast = Parser.ParseInput(contents, out tokens, out errors);

			var replacements = new Dictionary<Ast, string>();
			var bundleResult = new BundleResult();

			// Bundle other scripts
			var bareWordDotSourced = ast.FindAll(m => m is CommandAst, true).Cast<CommandAst>().Where(x => x.CommandElements.OfType<StringConstantExpressionAst>().Any(m => m.StringConstantType == StringConstantType.BareWord && m.Value.StartsWith(".") && m.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)));
			FindReplacements(root, bareWordDotSourced, bundleResult, replacements);

			var expandableString = ast.FindAll(m => m is CommandAst, true).Cast<CommandAst>().Where(x => x.InvocationOperator == TokenKind.Dot && x.CommandElements.OfType<ExpandableStringExpressionAst>().Any(m => m.StringConstantType == StringConstantType.DoubleQuoted && m.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)));
			FindReplacements(root, expandableString, bundleResult, replacements);

			var doubleQuotedString = ast.FindAll(m => m is CommandAst, true).Cast<CommandAst>().Where(x => x.InvocationOperator == TokenKind.Dot && x.CommandElements.OfType<StringConstantExpressionAst>().Any(m => (m.StringConstantType == StringConstantType.DoubleQuoted || m.StringConstantType == StringConstantType.SingleQuoted) && m.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)));
			FindReplacements(root, doubleQuotedString, bundleResult, replacements);

			var joinPath = ast.FindAll(m => m is CommandAst, true)
				.Cast<CommandAst>()
				.Where(x => x.InvocationOperator == TokenKind.Dot &&
				            x.CommandElements.FirstOrDefault(y => y is ParenExpressionAst) != null &&
				            x.CommandElements.First()
					            .FindAll(y => y is StringConstantExpressionAst, true)
					            .OfType<StringConstantExpressionAst>()
					            .Any(m => m.Value.Equals("Join-Path", StringComparison.OrdinalIgnoreCase)));

			FindReplacements(root, joinPath, bundleResult, replacements);

			if (_config.Bundle.Modules)
            {
				var wow64Value = IntPtr.Zero;

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					Wow64DisableWow64FsRedirection(ref wow64Value);

				try
                {
					var modules = BundleModules(_config.Package, ast, replacements, _config.Bundle.IgnoredModules).ToArray();
					bundleResult.Modules.AddRange(modules);
				}
				finally
                {
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						Wow64RevertWow64FsRedirection(wow64Value);
                }
            }

			try
            {
				BundleXaml(root, ast, replacements, bundleResult);
			}
			catch (Exception ex)
            {
				OnOutput("Failure bundling XAML." + ex.Message);
				bundleResult.Errors.Add(ex.Message);
			}

            //BundleResources(root, ast, replacements);
            BundleAssemblyLoad(root, ast, replacements, bundleResult);

            var offset = 0;
			var astContents = ast.ToString();
			foreach (var replacement in replacements.OrderBy(m => m.Key.Extent.StartOffset))
			{
				var startOffset = replacement.Key.Extent.StartOffset + offset;
				var endOffset = replacement.Key.Extent.EndOffset + offset;

				astContents = astContents.Remove(startOffset, endOffset - startOffset);
				astContents = astContents.Insert(startOffset, replacement.Value);
				offset += replacement.Value.Length - (endOffset - startOffset);
			}

			bundleResult.Value = astContents;
			return bundleResult;
		}

		private void FindReplacements(string root, IEnumerable<CommandAst> bareWordDotSourced, BundleResult bundleResult,
			Dictionary<Ast, string> replacements)
		{
			foreach (var node in bareWordDotSourced)
			{
				var dotSourcedFileName = ExpandFilePath(root, node);
				string resolvedFileName;
				if (!ResolveDotSourcedFileName(root, dotSourcedFileName, out resolvedFileName))
				{
					bundleResult.Warnings.Add($"Failed to resolve file name {dotSourcedFileName}");
					bundleResult.Status = BundleStatus.SuccessWithWarnings;
					continue;
				}

                if (!File.Exists(resolvedFileName))
                {
                    bundleResult.Warnings.Add($"File '{dotSourcedFileName}' does not exist. Bundled script may not function.");
                    bundleResult.Status = BundleStatus.SuccessWithWarnings;
                    continue;
                }

				var fileContents = Bundle(resolvedFileName);

				bundleResult.Errors.AddRange(fileContents.Errors);
				bundleResult.Warnings.AddRange(fileContents.Warnings);
				if (fileContents.Status == BundleStatus.SuccessWithWarnings)
				{
					bundleResult.Status = fileContents.Status;
				}

				replacements.Add(node, fileContents.Value);
			}
		}

		private static string ExpandFilePath(string root, CommandAst commandAst)
		{
			var rootFileInfo = new FileInfo(root);

			var commandName = commandAst.GetCommandName();
			if (string.IsNullOrEmpty(commandName))
			{
				var expandableString = commandAst.CommandElements.FirstOrDefault(m => m is ExpandableStringExpressionAst) as ExpandableStringExpressionAst;
				if (expandableString != null)
				{
                    return ExpandFilePath(root, expandableString);
				}

				var joinPath = commandAst.CommandElements.FirstOrDefault(m => m is ParenExpressionAst) as ParenExpressionAst;
				if (joinPath != null)
				{
					var pipeline = joinPath.Pipeline as PipelineAst;
					if (pipeline != null)
					{
                        return ExpandFilePath(root, pipeline);
					}
				}
			}

			return commandName;
		}

        private static string ExpandFilePath(string root, Ast expression)
        {
            if (expression == null) return null;

            var rootFileInfo = new FileInfo(root);

            var constantString = expression as StringConstantExpressionAst;

            if (constantString != null)
            {
                return constantString.Value;
            }

            var expandableString = expression as ExpandableStringExpressionAst;

            if (expandableString != null)
            {
                if (expandableString.Value.ToLower().Contains("$psscriptroot"))
                {
                    return expandableString.Value.ToLower().Replace("$psscriptroot", rootFileInfo.DirectoryName);
                }
            }

            var pipeline = expression as PipelineAst;
            if (pipeline != null)
            {
                var firstElement = pipeline.PipelineElements.FirstOrDefault() as CommandAst;
                if (firstElement != null)
                {
                    var commandFirstElement = firstElement.CommandElements.FirstOrDefault() as StringConstantExpressionAst;
                    if (commandFirstElement != null &&
                        commandFirstElement.Value.Equals("Join-Path", StringComparison.OrdinalIgnoreCase))
                    {
                        var elementsList = firstElement.CommandElements.ToList();
                        var pathIndex = elementsList.FindIndex(m => m.ToString().Equals("-path", StringComparison.OrdinalIgnoreCase));
                        var childPath = elementsList.FindIndex(m => m.ToString().Equals("-childpath", StringComparison.OrdinalIgnoreCase));

                        CommandElementAst secondElement;
                        CommandElementAst thirdElement;
                        if (pathIndex != -1 && childPath != -1)
                        {
                            secondElement = elementsList[pathIndex + 1];
                            thirdElement = elementsList[childPath + 1];
                        }
                        else
                        {
                            secondElement = firstElement.CommandElements.Skip(1).FirstOrDefault();
                            if (secondElement == null) return null;
                            thirdElement = firstElement.CommandElements.Skip(2).FirstOrDefault();
                            if (thirdElement == null) return null;
                        }

                        var rootPath = secondElement.ToString().Trim('(', ')');
                        if (rootPath.IndexOf("$PSScriptRoot", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
							rootPath = Regex.Replace(rootPath, $"{Regex.Escape("$")}PSScriptRoot", rootFileInfo.DirectoryName, RegexOptions.IgnoreCase);
                        }

                        if (string.IsNullOrEmpty(rootPath)) return null;
                        return Path.Combine(rootPath, thirdElement.ToString().Trim('\'', '"', ' '));
                    }
                }
            }

            return null;
        }

		private static bool ResolveDotSourcedFileName(string root, string fileName, out string resolvedFileName)
		{
			if (Path.IsPathRooted(fileName))
			{
				resolvedFileName = fileName;
				return true;
			}

			var rootFileInfo = new FileInfo(root);
			var rootFileDirectory = rootFileInfo.DirectoryName;

			if (string.IsNullOrEmpty(rootFileDirectory))
			{
				resolvedFileName = null;
				return false;
			}

			fileName = fileName.TrimStart('.', '\\');
			var combinedPath = Path.Combine(rootFileDirectory, fileName);
			if (!File.Exists(combinedPath))
			{
				resolvedFileName = null;
				return false;
			}

			resolvedFileName = combinedPath;
			return true;
		}

		private void BundleXaml(string root, Ast fileAst, Dictionary<Ast, string> replacements, BundleResult bundleResult)
		{
			var getContentCalls = fileAst.FindAll(m => m is CommandAst, true).Cast<CommandAst>()
				.Where(m => m.GetCommandName()?.Equals("Get-Content", StringComparison.OrdinalIgnoreCase) == true);

			var fileInfo = new FileInfo(root);
			foreach (var getContentCall in getContentCalls)
            {
				var xamlPath = string.Empty;
				for (int i = 0; i < getContentCall.CommandElements.Count; i++)
				{
					var element = getContentCall.CommandElements[i];
					if (element is CommandParameterAst parameter &&
						parameter.ParameterName.Equals("Path", StringComparison.OrdinalIgnoreCase) &&
						i < getContentCall.CommandElements.Count)
					{
						xamlPath = getContentCall.CommandElements[i + 1].ToString().Trim('\"');
						break;
					}
				}

				if (string.IsNullOrEmpty(xamlPath))
				{
					xamlPath = getContentCall.CommandElements.Last().ToString().Trim('\"');
				}

				xamlPath = xamlPath.Replace("$PSScriptRoot", fileInfo.DirectoryName);

				if (xamlPath.StartsWith("function:", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				try
				{
					OnOutput($"Checking path {xamlPath} for XAML.");
					var xamlFileInfo = new FileInfo(xamlPath);

					if (!xamlFileInfo.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					if (!xamlFileInfo.Exists)
					{
						OnOutput("Failed to locate file: " + xamlPath);
						continue;
					}

					OnOutput($"Embedding XAML file " + xamlPath);

					var xamlContent = ReadText(xamlFileInfo.FullName);
					var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xamlContent));
					base64 = string.Format("[System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('{0}'))\r\n[System.Reflection.Assembly]::LoadWithPartialName('PresentationFramework') | Out-Null", base64);
					replacements.Add(getContentCall, base64);
				}
				catch (Exception ex)
				{
					OnOutput("Failure checking XAML file. " + ex.Message);
					bundleResult.Errors.Add(ex.Message);
				}
			}
		}

        private static void BundleAssemblyLoad(string root, Ast fileAst, Dictionary<Ast, string> replacements, BundleResult bundleResult)
        {
            var invokeMemberExpressions = fileAst.FindAll(m => m is InvokeMemberExpressionAst, true).Cast<InvokeMemberExpressionAst>();

            foreach(var invokeMemberExpression in invokeMemberExpressions)
            {
                var typeExpressionAst = invokeMemberExpression.Expression as TypeExpressionAst;
                if (typeExpressionAst == null ||
                    (!typeExpressionAst.TypeName.FullName.Equals("System.Reflection.Assembly", StringComparison.OrdinalIgnoreCase) &&
                    !typeExpressionAst.TypeName.FullName.Equals("Reflection.Assembly", StringComparison.OrdinalIgnoreCase))
                    ) continue;

                var member = invokeMemberExpression.Member as StringConstantExpressionAst;
                if (member == null || !member.Value.Equals("LoadFrom", StringComparison.OrdinalIgnoreCase)) continue;

                var argument = invokeMemberExpression.Arguments.First();

                var filePath = ExpandFilePath(root, argument);

                BundleAssembly(filePath, replacements, bundleResult, invokeMemberExpression);
            }

            var addTypeWithPath = fileAst.FindAll(m => m is CommandAst commandAst &&
                                                       commandAst.GetCommandName()?.Equals("Add-Type", StringComparison.OrdinalIgnoreCase) == true &&
                                                       commandAst.CommandElements.Any(x => x is CommandParameterAst parameterAst && parameterAst.ParameterName.Equals("Path", StringComparison.OrdinalIgnoreCase)), true);
            foreach(CommandAst addTypeCommand in addTypeWithPath)
            {
                var nextIsValue = false;
                foreach (var commandElement in addTypeCommand.CommandElements)
                {
                    if (nextIsValue)
                    {
                        Ast ast = commandElement;
                        if (ast is ParenExpressionAst parenExpressionAst)
                        {
                            ast = parenExpressionAst.Pipeline as PipelineAst;
                        }

                        var filePath = ExpandFilePath(root, ast);
                        BundleAssembly(filePath, replacements, bundleResult, addTypeCommand);

                        break;
                    }

                    if (commandElement is CommandParameterAst parameter)
                    {
                        nextIsValue = parameter.ParameterName.Equals("path", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }

        private static void BundleAssembly(string filePath, Dictionary<Ast, string> replacements, BundleResult bundleResult, Ast ast)
        {
            if (!File.Exists(filePath))
            {
                bundleResult.Warnings.Add($"File '{filePath}' does not exist. Bundled script may not function.");
                if (bundleResult.Status == BundleStatus.Success)
                {
                    bundleResult.Status = BundleStatus.SuccessWithWarnings;
                }

                return;
            }

            var assemblyContent = File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(assemblyContent);
            replacements.Add(ast, $"[System.Reflection.Assembly]::Load([Convert]::FromBase64String('{base64}')) | Out-Null");
        }

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

		private IEnumerable<ModuleInfo> BundleModules(PackageConfig packageConfig, Ast fileAst, Dictionary<Ast, string> replacements, string [] ignoredModules)
		{
			var importModuleCall = fileAst.FindAll(m => m is CommandAst, true).Cast<CommandAst>()
                .Where(m => !string.IsNullOrWhiteSpace(m.GetCommandName()) && m.GetCommandName().Equals("Import-Module", StringComparison.OrdinalIgnoreCase));

			Runspace runspace;
			var iss = InitialSessionState.CreateDefault();
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				iss.AuthorizationManager = new AuthorizationManager("Microsoft.PowerShell");
				iss.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
			}

			var defaultRunspace = Runspace.DefaultRunspace;

			runspace = RunspaceFactory.CreateRunspace(iss);
			Runspace.DefaultRunspace = runspace;

			runspace.Open();

			foreach (var moduleCall in importModuleCall)
			{
				OnOutput($"Found module import '{moduleCall}' at {moduleCall.Extent.StartOffset}");
				IEnumerable<ModuleInfo> moduleInfos;
				Exception psException = null;
				using (var ps = PowerShell.Create())
				{
					var envVar = Environment.GetEnvironmentVariable("PSModulePath");
					OnOutput($"Packager PSModulePath: {envVar}");

					try
					{
						ps.AddScript(moduleCall + " -PassThru | % { [PowerShellToolsPro.Packager.ModuleInfo]$_ }");
						moduleInfos = ps.Invoke<ModuleInfo>();
					}
					catch (Exception ex)
					{
						OnOutput(string.Format("Failed to run '{0}'. " + ex.Message, moduleCall));
						continue;
					}

					if (ps.HadErrors && ps.Streams.Error.Any())
					{
						var firstError = ps.Streams.Error.FirstOrDefault();
						psException = firstError.Exception;

						OnOutput($"Error while import module: {psException}");
					}
				}

				var replacementText = new StringBuilder();

				if (!moduleInfos.Any())
				{
					OnOutput($"No module information was gathered. Not replacing module");
					replacementText.AppendLine(moduleCall.ToString());
				}

				if (ignoredModules?.Any(m => moduleInfos.Any(x => x.Name.Equals(m, StringComparison.OrdinalIgnoreCase))) == true)
				{
					OnOutput($"Module is ignored. Skipping module.");
					continue;
				}

				foreach (var moduleInfo in moduleInfos)
				{
					if (moduleInfo == null)
						throw new Exception(string.Format("Failed to load module using command {0}", importModuleCall), psException);

					var name = moduleInfo.Name;

					// Weird case where this module is returned when importing NTFSSecurity
					if (name == "NTFSSecurity.Init") continue;

					OnOutput($"Replacing module {name}");

					replacementText.AppendLine($"Import-Module -Name {name}");

					foreach (var requiredModule in moduleInfo.RequiredModules)
					{
						yield return requiredModule;
					}

					yield return moduleInfo;
				}

				replacements.Add(moduleCall, replacementText.ToString());
			}

			Runspace.DefaultRunspace = defaultRunspace;
			runspace.Dispose();
		}

		private static string InjectDotSourcedFiles(DotSourceFileTracker tracker, string moduleBase, string currentFile)
		{
			if (!Path.IsPathRooted(currentFile))
			{
				currentFile = Path.Combine(moduleBase, currentFile);
			}

			if (currentFile.StartsWith(tracker.SandboxDirectory, StringComparison.OrdinalIgnoreCase))
			{
				currentFile = currentFile.Remove(0, tracker.SandboxDirectory.Length);
				currentFile = currentFile.Trim('\\');
				currentFile = Path.Combine(moduleBase, currentFile);
			}

			var currentFileInfo = new FileInfo(currentFile);

			if (!currentFileInfo.Exists)
				throw new Exception("File not found");

			var fileContent = ReadText(currentFileInfo.FullName);
			var removedOffsets = new List<Tuple<int, int>>();
			var offset = 0;

			var sandboxItem = Path.Combine(tracker.SandboxDirectory, currentFileInfo.Name);

			foreach (var item in tracker.Replacements.Where(m => m.FileName.Equals(sandboxItem, StringComparison.OrdinalIgnoreCase)))
			{
				var startOffset = item.OriginStartOffset + offset;
				var endOffset = item.OriginEndOffset + offset;

				var dotSourcedFile = InjectDotSourcedFiles(tracker, moduleBase, item.TargetPath);
				if (!removedOffsets.Any(m => m.Item1 == item.OriginStartOffset && m.Item2 == item.OriginEndOffset))
				{
					removedOffsets.Add(new Tuple<int, int>(item.OriginStartOffset, item.OriginEndOffset));
					fileContent = fileContent.Remove(startOffset, endOffset - startOffset);
					offset -= (endOffset - startOffset);
					fileContent = fileContent.Insert(startOffset, dotSourcedFile);
					//if (offset < 0)
					//	offset = 0;
				}
				else
				{
					fileContent = fileContent.Insert(endOffset, dotSourcedFile);
				}

				offset += dotSourcedFile.Length;
			}

			return fileContent;
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			// If the source directory does not exist, throw an exception.
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory does not exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}


			// Get the file contents of the directory to copy.
			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files)
			{
				// Create the path to the new copy of the file.
				string temppath = Path.Combine(destDirName, file.Name);

				// Copy the file.
				file.CopyTo(temppath, true);
			}

			// If copySubDirs is true, copy the subdirectories.
			if (copySubDirs)
			{

				foreach (DirectoryInfo subdir in dirs)
				{
					// Create the subdirectory.
					string temppath = Path.Combine(destDirName, subdir.Name);

					// Copy the subdirectories.
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		private static void WriteText(string path, string text)
		{
			using(var streamWriter = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate), new UTF8Encoding(true)))
			{
				streamWriter.Write(text);
			}
		}

		private static string ReadText(string path)
		{
			using (var reader = new StreamReader(new FileStream(path, FileMode.Open), Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}

	public class DotSourceFileTracker
	{
		private readonly Action<string> _processFile;

		public DotSourceFileTracker(Action<string> processFile, string sandboxDirectory)
		{
			Replacements = new List<FileDotSourceReplacements>();
			_processFile = processFile;
			SandboxDirectory = sandboxDirectory;
		}

		public string SandboxDirectory { get; set; }

		public List<FileDotSourceReplacements> Replacements { get; set; }

		public void AddTarget(string fileName, int originStartOffset, int originEndOffset, string path)
		{
			Replacements.Add(new FileDotSourceReplacements
			{
				FileName = fileName,
				OriginEndOffset = originEndOffset ,
				OriginStartOffset = originStartOffset,
				TargetPath = path
			});
			_processFile(path);
		}
	}

	public class FileDotSourceReplacements
	{
		public string FileName { get; set; }
		public int OriginStartOffset { get; set; }
		public int OriginEndOffset { get; set; }
		public string TargetPath { get; set; }
	}

	public class BundleResult
	{
		public BundleResult()
		{
			Status = BundleStatus.Success;
			Errors = new List<string>();
			Warnings = new List<string>();
            Modules = new List<ModuleInfo>();
		}

		public BundleResult(BundleStatus status, string value = null)
		{
			Status = status;
			Errors = new List<string>();
			Warnings = new List<string>();
			Value = value;
            Modules = new List<ModuleInfo>();
        }

		public BundleResult(BundleStatus status, IEnumerable<string> errors)
		{
			Status = status;
			Errors = errors.ToList();
			Warnings = new List<string>();
            Modules = new List<ModuleInfo>();
        }

		public BundleStatus Status { get; set; }
		public List<string> Errors { get; }
		public List<string> Warnings { get; }
		public string Value { get; set; }
        public List<ModuleInfo> Modules { get; set; }
	}

	public enum BundleStatus
	{
		Success,
		SuccessWithWarnings,
		Failed
	}
}