
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;

namespace PowerShellTools.Workspace {
    [ExportLaunchConfigurationProvider(
        ProviderType,
        new[] { PowerShellConstants.PS1File },
        PowerShellLaunchDebugTargetProvider.LaunchTypeName,
        PowerShellLaunchDebugTargetProvider.JsonSchema
    )]
    class PowerShellLaunchConfigurationProvider : ILaunchConfigurationProvider {
        public const string ProviderType = "3d5f99fc-5f7a-43c8-9e84-03d932726080";

        public bool IsDebugLaunchActionSupported(DebugLaunchActionContext debugLaunchActionContext) {
            return true;
        }

        public void CustomizeLaunchConfiguration(DebugLaunchActionContext debugLaunchActionContext, IPropertySettings launchSettings) {
            launchSettings[PowerShellLaunchDebugTargetProvider.ScriptArgumentsKey] = string.Empty;
        }
    }
}