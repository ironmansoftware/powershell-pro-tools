'use strict';
import * as vscode from 'vscode';
import { packageAsExe } from './commands/packageAsExe';
import { convertToUdElement } from './commands/convertToUdElement';
import { profile, clearProfiling } from './commands/profile';
import { showWinFormDesigner } from './commands/formsDesigner';
import { generateWinForm } from './commands/generateWinForm';
import { PowerShellService, SessionStatus } from './services/powershellservice';
import { Container } from './container';
import { showDataGrid } from './commands/showDataGrid';
import { statusBarItemMenu } from './commands/statusBarItemMenu';
import { InstallPoshToolsModule } from './commands/installPoshToolsModule';
import { generateTool } from './commands/generateTool';
import { AstTreeViewProvider } from './treeView/astTreeView';
import { HostProcessViewProvider } from './treeView/hostProcessView';
import { ModuleViewProvider } from './treeView/moduleTreeView';
import { ProviderViewProvider } from './treeView/providerTreeView';
import { QuickScriptViewProvider } from './treeView/quickScriptView';
import { VariableViewProvider } from './treeView/variableTreeView';
import { InfoViewProvider } from './treeView/infoTreeView';
import help from './commands/help';
import { ReflectionViewProvider } from './treeView/reflectionTreeView';
import { CustomTreeViewProvider } from './treeView/customTreeView';
import { HistoryTreeViewProvider } from './treeView/historyTreeView';
import { SessionTreeViewProvider } from './treeView/sessionTreeView';
import { JobTreeViewProvider } from './treeView/jobTreeView';
import { PowerShellRenameProvider } from './services/renameProvider';
import { registerWelcomeCommands } from './commands/welcomeCommand';
import { TreeViewProvider } from './treeView/treeViewProvider';

const version = "PowerShellProTools.Version";

var powerShellService;

export interface IExternalPowerShellDetails {
    exePath: string;
    version: string;
    displayName: string;
    architecture: string;
}

export interface IPowerShellExtensionClient {
    registerExternalExtension(id: string, apiVersion?: string): string;
    unregisterExternalExtension(uuid: string): boolean;
    getPowerShellVersionDetails(uuid: string): Promise<IExternalPowerShellDetails>;
    waitUntilStarted(uuid: string): Promise<void>;
    getStorageUri(): vscode.Uri;
}


export async function activate(context: vscode.ExtensionContext) {
    var service = powerShellService = new PowerShellService(context);
    context.subscriptions.push(showDataGrid(context));
    context.subscriptions.push(packageAsExe());
    context.subscriptions.push(convertToUdElement());
    context.subscriptions.push(profile());
    context.subscriptions.push(clearProfiling());
    context.subscriptions.push(showWinFormDesigner(context));
    context.subscriptions.push(generateWinForm());
    context.subscriptions.push(generateTool());
    context.subscriptions.push(InstallPoshToolsModule());
    context.subscriptions.push(help());
    context.subscriptions.push(statusBarItemMenu());
    Container.initialize(context, powerShellService);

    var extension = vscode.extensions.getExtension<IPowerShellExtensionClient>("ms-vscode.PowerShell");
    if (!extension) {
        extension = vscode.extensions.getExtension<IPowerShellExtensionClient>("ms-vscode.PowerShell-Preview");
    }

    if (!extension) {
        vscode.window.showErrorMessage("PowerShell Pro Tools requires the Microsoft PowerShell or PowerShell Preview extension.");
        return;
    }

    const configuration: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration("terminal.integrated");
    var persistentSessions = configuration.get<boolean>("enablePersistentSessions");
    if (persistentSessions) {
        var result = await vscode.window.showErrorMessage("PowerShell Pro Tools requires the terminal.integrated.enablePersistentSessions setting to be disabled.", "Disable", "Learn About Persistent Sessions");
        if (result == "Learn About Persistent Sessions") {
            vscode.env.openExternal(vscode.Uri.parse("https://code.visualstudio.com/docs/terminal/advanced#_persistent-sessions"));
        }
        else if (result == "Disable") {
            await configuration.update("enablePersistentSessions", false, true);
            vscode.window.showInformationMessage("Persistent sessions have been disabled. Please reload the window for the changes to take effect.");
        }
        else {
            vscode.window.showErrorMessage("PowerShell Pro Tools did not start because Persistent Terminal Sessions is enabled.");
            service.setSessionStatus(SessionStatus.Disabled);
        }

        return;
    }

    await extension.activate();
    const powerShellExtensionClient = extension!.exports as IPowerShellExtensionClient;
    const id = powerShellExtensionClient.registerExternalExtension("ironmansoftware.powershellprotools");
    context.subscriptions.push({
        dispose: () => powerShellExtensionClient.unregisterExternalExtension(id)
    });
    await powerShellExtensionClient.waitUntilStarted(id);

    await finishActivation(context);
}

function delay(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function waitForPowerShellExtensionTerminal() {
    for (let attempt = 0; attempt < 60; attempt++) {
        const terminal = vscode.window.terminals.find(x => x.name.startsWith("PowerShell Extension"));
        if (terminal) {
            return terminal;
        }

        await delay(1000);
    }

    return null;
}

async function finishActivation(context: vscode.ExtensionContext) {

    Container.Log("Finishing extension activation.");

    const treeViewProviders = createTreeViews(context);

    const terminal = await waitForPowerShellExtensionTerminal();
    if (terminal == null) {
        throw "PowerShell Extension not found.";
    }

    const powershellProTools = vscode.extensions.getExtension("ironmansoftware.powershellprotools")!;
    const currentVersion = powershellProTools.packageJSON.version;

    Container.Log("Connecting to PowerShell Editor Services.");

    await powerShellService.Connect(() => {
        treeViewProviders.forEach(provider => provider.refresh());

        Container.Log("Starting code analysis.");

        if (vscode.workspace.workspaceFolders) {
            for (let wsf of vscode.workspace.workspaceFolders) {
                Container.PowerShellService.AddWorkspaceFolder(wsf.uri.fsPath);
            }
        }

        vscode.workspace.onDidChangeWorkspaceFolders(e => {
            for (let add of e.added) {
                Container.PowerShellService.AddWorkspaceFolder(add.uri.fsPath);
            }

            for (let remove of e.removed) {
                Container.PowerShellService.RemoveWorkspaceFolder(remove.uri.fsPath);
            }
        });

        vscode.workspace.onDidSaveTextDocument(x => {
            Container.PowerShellService.AnalyzeWorkspaceFile(x.uri.fsPath);
        });

        vscode.workspace.onDidCreateFiles(x => {
            Container.PowerShellService.AnalyzeWorkspaceFile(x.files[0].fsPath);
        })

        vscode.workspace.onDidDeleteFiles(x => {
            Container.PowerShellService.AnalyzeWorkspaceFile(x.files[0].fsPath);
        })

        vscode.workspace.onDidRenameFiles(x => {
            Container.PowerShellService.AnalyzeWorkspaceFile(x.files[0].newUri.fsPath);
        })

        Container.Log("Started PowerShell Pro Tools process.");

        context.globalState.update(version, currentVersion);

        Container.VSCodeService.init();

        context.subscriptions.push(vscode.languages.registerRenameProvider({ scheme: 'file', language: 'powershell' }, new PowerShellRenameProvider()));
        registerWelcomeCommands(context);
    });



}

function createTreeViews(context: vscode.ExtensionContext): TreeViewProvider[] {
    Container.Log("Creating tree views.");

    return [
        registerTreeView(context, 'astView', new AstTreeViewProvider()),
        registerTreeView(context, 'hostProcessView', new HostProcessViewProvider()),
        registerTreeView(context, 'moduleView', new ModuleViewProvider()),
        registerTreeView(context, 'providerView', new ProviderViewProvider()),
        registerTreeView(context, 'quickScriptView', new QuickScriptViewProvider()),
        registerTreeView(context, 'variableView', new VariableViewProvider()),
        registerTreeView(context, 'infoView', new InfoViewProvider()),
        registerTreeView(context, 'reflectionView', new ReflectionViewProvider()),
        registerTreeView(context, 'customView', new CustomTreeViewProvider()),
        registerTreeView(context, 'historyView', new HistoryTreeViewProvider()),
        registerTreeView(context, 'sessionsView', new SessionTreeViewProvider()),
        registerTreeView(context, 'jobView', new JobTreeViewProvider())
    ];
}

function registerTreeView(context: vscode.ExtensionContext, id: string, provider: TreeViewProvider): TreeViewProvider {
    context.subscriptions.push(provider);
    context.subscriptions.push(vscode.window.createTreeView<vscode.TreeItem>(id, { treeDataProvider: provider }));

    return provider;
}
