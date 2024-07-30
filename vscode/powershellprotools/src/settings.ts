import vscode = require("vscode");
export let PowerShellLanguageId = "poshProTools";

export interface ISettings {
    universalDashboardPreviewPort: number;
    showUpgradeNotification: boolean;
    debugModuleLoad: boolean;
    ignoredModules: string;
    ignoredAssemblies: string;
    ignoredTypes: string;
    ignoredCommands: string;
    ignoredVariables: string;
    ignoredPaths: string;
    checkForModuleUpdates: boolean;
    license: string;
    defaultPackagePsd1: string;
    signOnSave: boolean;
    signOnSaveCertificate: string;
    excludeAutomaticVariables: boolean;
    clearScreenAfterLoad: boolean;
    disableNewsNotification: boolean;
}

export function load(): ISettings {
    const configuration: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(PowerShellLanguageId);
    return {
        universalDashboardPreviewPort: configuration.get<number>("universalDashboardPreviewPort", 10000),
        debugModuleLoad: configuration.get<boolean>("debugModuleLoad", false),
        showUpgradeNotification: configuration.get<boolean>("showUpgradeNotification", true),
        ignoredModules: configuration.get<string>("ignoredModules", ""),
        ignoredAssemblies: configuration.get<string>("ignoredAssemblies", ""),
        ignoredTypes: configuration.get<string>("ignoredTypes", ".*AnonymousType.*;.*ImplementationDetails.*;_.*"),
        ignoredCommands: configuration.get<string>("ignoredCommands", ""),
        ignoredVariables: configuration.get<string>("ignoredVariables", ""),
        ignoredPaths: configuration.get<string>("ignoredPaths", ""),
        checkForModuleUpdates: configuration.get<boolean>("checkForModuleUpdates", false),
        license: configuration.get<string>("license", ""),
        defaultPackagePsd1: configuration.get<string>("defaultPackagePsd1Path", ""),
        signOnSave: configuration.get<boolean>("signOnSave", false),
        signOnSaveCertificate: configuration.get<string>("signOnSaveCertificate", ""),
        excludeAutomaticVariables: configuration.get<boolean>("excludeAutomaticVariables", false),
        clearScreenAfterLoad: configuration.get<boolean>("clearScreenAfterLoad", true),
        disableNewsNotification: configuration.get<boolean>("disableNewsNotification", false)
    }
}
