using System;
using System.IO;
using PowerShellToolsPro.Packager.Config;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace PowerShellToolsPro.Packager
{
    public class PackageProcess
    {
        private readonly Stage[] _stages;

        public PackageProcess()
        {
			Config = new PsPackConfig();
            _stages = new Stage[]
            {
                new ValidateStage(),
                new OutputPathStage(),
                new BundleStage(),
                new CompileStage(),
                new UpdateHostStage(),
                new ObfucatorStage()
            };

	        foreach (var stage in _stages)
	        {
				stage.OnMessage += Stage_OnMessage;
		        stage.OnErrorMessage += Stage_OnErrorMessage;
                stage.OnWarningMessage += Stage_OnWarningMessage;
	        }
        }

		private void Stage_OnMessage(object sender, string e)
		{
			if (OnMessage != null)
				OnMessage(this, e);
		}

	    private void Stage_OnErrorMessage(object sender, string e)
	    {
		    if (OnErrorMessage != null)
			    OnErrorMessage(this, e);
	    }

        private void Stage_OnWarningMessage(object sender, string e)
        {
            if (OnWarningMessage != null)
                OnWarningMessage(this, e);
        }

        public StageResult Execute()
        {
            StageResult result = null;
            StageResult previousStage = null;
            foreach (var stage in _stages)
            {
                if (stage.ShouldExecute(this, previousStage))
                {
                    previousStage = stage.Execute(this, previousStage);
                    result += previousStage;

                    if (!result.Success)
                    {
                        return result;
                    }
                }
            }

            if (OnMessage != null && result.Success)
                OnMessage(this, "Output: " + result.OutputFileName);

            return result;
        }

	    public event EventHandler<string> OnMessage;
	    public event EventHandler<string> OnErrorMessage;
        public event EventHandler<string> OnWarningMessage;

        public PsPackConfig Config { get; set; }

	    public bool PackAsExecutable
	    {
		    get { return Config.Package.Enabled; }
			set { Config.Package.Enabled = value;  }
	    }
        public bool Bundle
        {
	        get { return Config.Bundle.Enabled; }
	        set { Config.Bundle.Enabled = value; }
        }
		public bool Obfuscate
		{
			get { return Config.Package.Obfuscate; }
			set { Config.Package.Obfuscate = value; }
		}

	    public string OutputDirectory
	    {
		    get { return Config.OutputPath; }
		    set { Config.OutputPath = value; }
	    }

	    public string Script
	    {
		    get { return Config.Root; }
			set { Config.Root = value; }
	    }

	    public bool HideConsoleWindow
	    {
		    get { return Config.Package.HideConsoleWindow; }
			set { Config.Package.HideConsoleWindow = value; }
	    }

        public string DotNetVersion
        {
            get { return Config.Package.DotNetVersion; }
            set { Config.Package.DotNetVersion = value; }
        }

        public string FileVersion
        {
            get { return Config.Package.FileVersion; }
            set { Config.Package.FileVersion = value; }
        }
    }

    public class ValidateStage : Stage
    {
        public string[] validPSVersions = new[] { "7.2.0", "7.2.1", "7.2.10", "7.2.12", "7.2.13", "7.2.14", "7.2.16", "7.2.17", "7.2.18", "7.2.2", "7.2.21", "7.2.22", "7.2.23", "7.2.24", "7.2.3", "7.2.4", "7.2.5", "7.2.6", "7.2.7", "7.2.8", "7.2.9", "7.3.0", "7.3.1", "7.3.10", "7.3.11", "7.3.2", "7.3.3", "7.3.4", "7.3.5", "7.3.6", "7.3.7", "7.3.8", "7.3.9", "7.4.0", "7.4.1", "7.4.2", "7.4.3", "7.4.4", "7.4.5", "7.4.6", "7.4.7", "7.5.0" };
        public string[] validNetVersionsForSystemDefault = new[] { "net462", "net470", "net471", "net472", "net480", "net481" };
        public string[] validNetVersionsForNew = new[] { "netcoreapp31", "net50", "net60", "net70", "net80", "net81", "net90" };
        public string[] validNetVersions = new[] { "net462", "net470", "net471", "net472", "net480", "net481", "netcoreapp31", "net50", "net60", "net70", "net80", "net90" };

        public override StageResult Execute(PackageProcess process, StageResult previousStage)
        {
            ValidateDotNetVersion(process);
            ValidatePowerShellVersion(process);

            return new StageResult(true);
        }

        private void ValidateDotNetVersion(PackageProcess process)
        {
            if (string.IsNullOrEmpty(process.Config.Package.DotNetVersion)) return;

            var dotNetVersion = process.Config.Package.DotNetVersion.Trim('v').Replace(".", string.Empty);

            if (!dotNetVersion.StartsWith("net")) dotNetVersion = "net" + dotNetVersion;

            bool found = false;
            foreach(var version in validNetVersions)
            {
                if (version == dotNetVersion)
                {
                    found = true;
                }
            }

            if (!found)
            {
                WriteWarningMessage($"Unvalidated .NET Version specified. Valid values are: {validNetVersions.Aggregate((x, y) => x + ", " + y)}");
            }

            found = false;
            if (process.Config.Package.PowerShellVersion == null || process.Config.Package.PowerShellVersion == "Windows PowerShell")
            {
                foreach (var version in validNetVersionsForSystemDefault)
                {
                    if (version == dotNetVersion) return;
                }

                if (!found)
                {
                    WriteWarningMessage($"Unvalidated .NET Version specified for selected PowerShell version. Valid values are: {validNetVersionsForSystemDefault.Aggregate((x, y) => x + ", " + y)}");
                }
            }

            found = false;
            if (process.Config.Package.PowerShellVersion != null)
            {
                foreach (var version in validNetVersionsForNew)
                {
                    if (version == dotNetVersion) return;
                }

                if (!found)
                {
                    WriteWarningMessage($"Unvalidated .NET Version specified for selected PowerShell version. Valid values are: {validNetVersionsForNew.Aggregate((x, y) => x + ", " + y)}");
                }
            }
        }

        private void ValidatePowerShellVersion(PackageProcess process)
        {
            if (string.IsNullOrEmpty(process.Config.Package.PowerShellVersion)) return;
            if (process.Config.Package.PowerShellVersion.Equals("Windows PowerShell", StringComparison.OrdinalIgnoreCase)) return;
            if (process.Config.Package.PowerShellVersion.Contains("preview"))
            {
                WriteDebugMessage("Preview version specified. Skipping version check.");
                return;
            }

            if (!Version.TryParse(process.Config.Package.PowerShellVersion, out Version version))
            {
                throw new Exception($"Invalid version specified. This parameter supports 'Windows PowerShell', {validPSVersions.Aggregate((x, y) => x + ", " + y)}");
            }

            foreach (var supportedVersionStr in validPSVersions)
            {
                var supportedVersion = Version.Parse(supportedVersionStr);
                if (supportedVersion == version) return;
            }

            WriteWarningMessage($"Invalid version specified. This parameter supports 'Windows PowerShell', {validPSVersions.Aggregate((x, y) => x + ", " + y)}");
        }

        public override bool ShouldExecute(PackageProcess process, StageResult previousStage)
        {
            return true;
        }
    }

    public class OutputPathStage : Stage
    {
        public override bool ShouldExecute(PackageProcess process, StageResult previousStage)
        {
            return true;
        }

        public override StageResult Execute(PackageProcess process, StageResult previousStage)
        {
	        if (string.IsNullOrEmpty(process.Config.OutputPath))
	        {
				throw new Exception("Output path must be specified");
	        }

            var outputDirectory = process.Config.OutputPath;
            if (!Path.IsPathRooted(outputDirectory))
            {
                outputDirectory = Path.Combine(Environment.CurrentDirectory, outputDirectory);
            }

	        WriteDebugMessage(string.Format("OutputPath is {0}", outputDirectory));

			if (!Directory.Exists(outputDirectory))
			{
				WriteDebugMessage("OutputPath does not exist. Creating directory.");
				Directory.CreateDirectory(outputDirectory);
            }

            process.Config.OutputPath = outputDirectory;

            return Success(process.Script);
        }
    }

    public class BundleStage : Stage
    {
		public override bool ShouldExecute(PackageProcess process, StageResult previousStage)
        {
            return process.Config.Bundle.Enabled;
        }

		public override StageResult Execute(PackageProcess process, StageResult previousStage)
		{
			if (string.IsNullOrEmpty(process.Config.Root))
			{
				throw new Exception("Root must be specified.");
			}

			if (!File.Exists(process.Config.Root))
			{
				throw new Exception($"File {process.Config.Root} does not exist.");
			}

			WriteDebugMessage(string.Format("Bundling {0}", process.Config.Root));

			var bundler = new Bundler(process.Config);

            bundler.Output += (sender, str) => WriteDebugMessage(str);

			var result = bundler.Bundle(process.Config.Root);
			if (result.Status == BundleStatus.Failed)
			{
				foreach (var error in result.Errors)
				{
					WriteErrorMessage(error);
				}

				return Failure();
			}

			if (result.Status == BundleStatus.SuccessWithWarnings)
			{
				foreach (var warning in result.Warnings)
				{
					WriteWarningMessage(warning);
				}
			}

			var tempFile = Path.GetTempPath();
			var fileInfo = new FileInfo(process.Config.Root);
			tempFile = Path.Combine(tempFile, fileInfo.Name);

			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}

			WriteText(tempFile, result.Value);

            if (!process.PackAsExecutable)
            {
                var tempFileInfo = new FileInfo(tempFile);
                var destinationFile = Path.Combine(process.Config.OutputPath, tempFileInfo.Name);
                File.Copy(tempFile, destinationFile, true);

                WriteDebugMessage("Bundle result: " + destinationFile);

                return Success(destinationFile);
            }

            return Success(tempFile, new Dictionary<string, object> { { "Modules", result.Modules?.ToList()  } });
        }

		private void WriteText(string path, string text)
		{
            using (var streamWriter = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate), new UTF8Encoding(true)))
			{
				streamWriter.Write(text);
			}
		}
	}

    public class CompileStage : Stage
    {
		public override bool ShouldExecute(PackageProcess process, StageResult previousStage)
        {
            return process.Config.Package.Host == PowerShellHosts.Default && process.PackAsExecutable;
        }

        public override StageResult Execute(PackageProcess process, StageResult previousStage)
        {
            var compiler = new Compiler();
            compiler.ErrorOutput += (e, s) =>
            {
                WriteErrorMessage(s);
            };

            compiler.BuildOutput += (e, s) =>
            {
                WriteDebugMessage(s);
            };

	        WriteDebugMessage(string.Format("Packaging {0}", previousStage.OutputFileName));

			try
            {
                var resultingFile =
                    compiler.BuildSingleFileExecutable(previousStage, process.Config.OutputPath, process.Config.Package, process.Config.Root);

                return Result(resultingFile, !string.IsNullOrEmpty(resultingFile));
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex.Message);
                return Failure();
            }
        }
    }

    public class ObfucatorStage : Stage
    {
		public override bool ShouldExecute(PackageProcess process, StageResult previousStage)
        {
            return process.Config.Package.Obfuscate && process.Config.Package.Enabled;
        }

        public override StageResult Execute(PackageProcess process, StageResult previousStage)
        {
            var obfuscator = new Obfuscator();
            obfuscator.Output += (sender, s) =>
            {
                WriteDebugMessage(s);
            };

	        WriteDebugMessage(string.Format("Obfuscating {0}", previousStage.OutputFileName));

			var fileName = obfuscator.Obfuscate(previousStage.OutputFileName, process.Config.OutputPath, process.Config.Package.PowerShellVersion);

            return new StageResult(fileName, true);
        }
    }

    public abstract class Stage
    {
	    public event EventHandler<string> OnMessage;
	    public event EventHandler<string> OnErrorMessage;
        public event EventHandler<string> OnWarningMessage;

        public abstract bool ShouldExecute(PackageProcess process, StageResult previousStage);
        public abstract StageResult Execute(PackageProcess process, StageResult previousStage);

        protected void WriteDebugMessage(string message)
        {
	        if (OnMessage != null)
		        OnMessage(this, message);
        }

        protected void WriteErrorMessage(string message)
        {
			if (OnErrorMessage != null)
				OnErrorMessage(this, message);
		}

        protected void WriteWarningMessage(string message)
        {
            if (OnWarningMessage != null)
                OnWarningMessage(this, message);
        }

        protected StageResult Result(string outputFileName, bool success)
        {
            return new StageResult(outputFileName, success);
        }

        protected StageResult Success(string outputFileName)
        {
            return new StageResult(outputFileName, true);
        }

        protected StageResult Success(string outputFileName, IDictionary<string, object> data)
        {
            return new StageResult(outputFileName, true, data);
        }

        protected StageResult Success()
        {
            return new StageResult(true);
        }

        protected StageResult Failure()
        {
            return new StageResult(false);
        }
    }

    public class StageResult
    {
        public StageResult(bool success)
        {
            OutputFileName = null;
            Success = success;
            Data = new Dictionary<string, object>();
        }

        public StageResult(string outputFileName, bool success, IDictionary<string, object> data)
        {
            OutputFileName = outputFileName;
            Success = success;
            Data = data;
        }

        public StageResult(string outputFileName, bool success)
        {
            OutputFileName = outputFileName;
            Success = success;
            Data = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Data { get; }
        public string OutputFileName { get; private set; }
        public bool Success { get; }
        public static StageResult operator +(StageResult c1, StageResult c2)
        {
            if (c1 == null) return c2;
            if (c2 == null) return c1;

            var result = new StageResult(c1.Success && c2.Success);
            result.OutputFileName = c2.OutputFileName;

            return result;
        }

    }
}

