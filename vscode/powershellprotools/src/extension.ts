'use strict';
import * as vscode from 'vscode';
import { packageAsExe } from './commands/packageAsExe';
import { convertToUdElement } from './commands/convertToUdElement';
import { profile, clearProfiling } from './commands/profile';
import { showWinFormDesigner } from './commands/formsDesigner';
import { generateWinForm } from './commands/generateWinForm';
import { PowerShellService } from './services/powershellservice';
import { Container } from './container';
import { showDataGrid } from './commands/showDataGrid';
import { statusBarItemMenu } from './commands/statusBarItemMenu';
import { openPsScriptPad } from './commands/openPsScriptPad';
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
    powerShellService = new PowerShellService(context);
    context.subscriptions.push(showDataGrid(context));
    context.subscriptions.push(packageAsExe());
    context.subscriptions.push(convertToUdElement());
    context.subscriptions.push(profile());
    context.subscriptions.push(clearProfiling());
    context.subscriptions.push(showWinFormDesigner(context));
    context.subscriptions.push(generateWinForm());
    context.subscriptions.push(generateTool());
    context.subscriptions.push(openPsScriptPad());
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

    if (!extension.isActive) {
        await extension.activate();
        const powerShellExtensionClient = extension!.exports as IPowerShellExtensionClient;
        const id = powerShellExtensionClient.registerExternalExtension(context.extension.id);
        await powerShellExtensionClient.waitUntilStarted(id);
    }

    await finishActivation(context);
}

async function finishActivation(context: vscode.ExtensionContext) {

    Container.Log("Finishing extension activation.");

    let terminal = null;
    do {
        terminal = vscode.window.terminals.find(x => x.name.startsWith("PowerShell Extension"));
    } while (!terminal)

    if (terminal == null) {
        throw "PowerShell Extension not found.";
    }

    const powershellProTools = vscode.extensions.getExtension("ironmansoftware.powershellprotools")!;
    const currentVersion = powershellProTools.packageJSON.version;

    Container.Log("Connecting to PowerShell Editor Services.");

    powerShellService.Connect(() => {
        Container.Log("Creating tree views.");

        vscode.window.createTreeView<vscode.TreeItem>('astView', { treeDataProvider: new AstTreeViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('hostProcessView', { treeDataProvider: new HostProcessViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('moduleView', { treeDataProvider: new ModuleViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('providerView', { treeDataProvider: new ProviderViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('quickScriptView', { treeDataProvider: new QuickScriptViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('variableView', { treeDataProvider: new VariableViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('infoView', { treeDataProvider: new InfoViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('reflectionView', { treeDataProvider: new ReflectionViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('customView', { treeDataProvider: new CustomTreeViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('historyView', { treeDataProvider: new HistoryTreeViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('sessionsView', { treeDataProvider: new SessionTreeViewProvider() });
        vscode.window.createTreeView<vscode.TreeItem>('jobView', { treeDataProvider: new JobTreeViewProvider() });

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