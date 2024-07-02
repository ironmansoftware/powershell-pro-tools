using System;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using PowerShellTools.Project;

namespace PowerShellTools.Workspace
{
	[ExportLaunchDebugTarget(
        ProviderType,
        new[] { PowerShellConstants.PS1File }
    )]
    class PowerShellLaunchDebugTargetProvider : ILaunchDebugTargetProvider {
        private const string ProviderType = "219820a8-5fa0-4627-afdb-cbf27ef1ffba";

        public const string LaunchTypeName = "powershell";

        // Set by the workspace, not by our users
        private const string ScriptNameKey = "target";

        public const string ScriptArgumentsKey = "scriptArguments";
        public const string WorkingDirectoryKey = "workingDirectory";

        public const string DefaultInterpreterValue = "(default)";

        public const string JsonSchema = @"{
  ""definitions"": {
    ""powershell"": {
      ""type"": ""object"",
      ""properties"": {
        ""type"": {""type"": ""string"", ""enum"": [ ""powershell"" ]},
        ""scriptArguments"": { ""type"": ""string"" },
        ""workingDirectory"": { ""type"": ""string"" }
      }
    },
    ""powershellFile"": {
      ""allOf"": [
        { ""$ref"": ""#/definitions/default"" },
        { ""$ref"": ""#/definitions/powershell"" }
      ]
    }
  },
    ""defaults"": {
        "".ps1"": { ""$ref"": ""#/definitions/powershell"" }
    },
    ""configuration"": ""#/definitions/powershellFile""
}";

        public void LaunchDebugTarget(IWorkspace workspace, IServiceProvider serviceProvider, DebugLaunchActionContext debugLaunchActionContext) {
            var settings = debugLaunchActionContext.LaunchConfiguration;
            var scriptName = settings.GetValue(ScriptNameKey, string.Empty);
			var args = settings.GetValue(ScriptArgumentsKey, string.Empty);
            var debug = !settings.GetValue("noDebug", false);



			if (string.IsNullOrEmpty(scriptName))
			{
				throw new InvalidOperationException("Debug script is missing!");
			}

			new PowerShellProjectLauncher().LaunchFile(scriptName, debug, args);
		}

        public bool SupportsContext(IWorkspace workspace, string filePath) {
            throw new NotImplementedException();
        }
    }
}