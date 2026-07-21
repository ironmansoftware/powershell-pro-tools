import * as vscode from 'vscode';
import net = require('net');
import path = require("path");
import { Container } from '../container';
import { RapidSenseOptions } from './rapidSenseOptions';
import { FunctionDefinition, Hover, RefactorInfo, RefactorRequest, RefactorTextEdit, TextEditorState, Certificate, PSAssembly, PSType, PSMethod, PSProperty, PSField, PSNamespace, CustomTreeView, CustomTreeItem, Session, Job, Performance } from '../types';
import { load } from '../settings';

const randomstring = require('randomstring')
const os = require('os')
const fs = require('fs');

export enum SessionStatus {
    Initializing,
    Failed,
    Connected,
    Disabled
}

export class PowerShellService {
    private pipeName: string;
    private statusBarItem: vscode.StatusBarItem;
    private logger: net.Socket;
    private sessionStatus: SessionStatus;
    private reconnectDepth: number = 0;
    private reconnectPromise: Promise<void>;

    get status() {
        return this.sessionStatus;
    }

    constructor(context: vscode.ExtensionContext, pipeName?: string) {
        const settings = load();
        if (settings.statusBar.statusVisibility) {
            this.statusBarItem =
                vscode.window.createStatusBarItem(
                    vscode.StatusBarAlignment.Right,
                    1);

            this.statusBarItem.command = "poshProTools.statusBarMenu";
            this.statusBarItem.show();

            vscode.window.onDidChangeActiveTextEditor((textEditor) => {
                if (textEditor === undefined
                    || textEditor.document.languageId !== "powershell") {
                    this.statusBarItem.hide();
                } else {
                    this.statusBarItem.show();
                }
            });

            context.subscriptions.push(this.statusBarItem)
        }

        this.pipeName = pipeName || this.generatePipeName();

        this.setSessionStatus(SessionStatus.Initializing);
    }

    private generatePipeName(): string {
        return randomstring.generate({
            length: 12,
            charset: 'alphabetic'
        }).toLowerCase();
    }

    private resetPipeName(): void {
        this.pipeName = this.generatePipeName();
        Container.Log(`Generated new PowerShell Pro Tools pipe name: ${this.pipeName}`);
    }

    private getPipePath(pipeName: string, suffix: string = ""): string {
        if (process.platform === "win32") {
            return `\\\\.\\pipe\\${pipeName}${suffix}`;
        }

        return path.join(os.tmpdir(), `CoreFxPipe_${pipeName}${suffix}`);
    }

    private delay(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    private async waitForPowerShellExtensionTerminal(): Promise<vscode.Terminal> {
        for (let attempt = 0; attempt < 60; attempt++) {
            const terminal = vscode.window.terminals.find(x => x.name.startsWith("PowerShell Extension"));
            if (terminal) {
                return terminal;
            }

            Container.Log("PowerShell Extension terminal is not available yet.");
            await this.delay(1000);
        }

        throw "PowerShell Extension not found.";
    }

    setSessionStatus(status: SessionStatus): void {
        // Set color and icon for 'Running' by default
        let statusIconText = "$(sync) PowerShell Pro Tools";
        let statusColor = "#f3fc74";
        let toolTip = "PowerShell Pro Tools is connecting...";

        if (status === SessionStatus.Connected) {
            statusIconText = "$(link) PowerShell Pro Tools";
            statusColor = "#affc74";
            toolTip = "PowerShell Pro Tools is connected";
        } else if (status === SessionStatus.Failed) {
            statusIconText = "$(alert) PowerShell Pro Tools";
            statusColor = "#fcc174";
            toolTip = "PowerShell Pro Tools failed to connect. Check out the Output channel for PowerShell Pro Tools for more information.";
        } else if (status === SessionStatus.Disabled) {
            statusIconText = "$(circle-slash) PowerShell Pro Tools";
            statusColor = "#fcc174";
            toolTip = "PowerShell Pro Tools is disabled because Persistent Terminal Sessions is enabled. Please disable Persistent Terminal Sessions and reload the window.";
        }

        const settings = load();
        if (settings.statusBar.statusVisibility) {
            this.statusBarItem.color = statusColor;
            this.statusBarItem.text = statusIconText;
            this.statusBarItem.tooltip = toolTip;
        }

        this.sessionStatus = status;
    }

    Reconnect(callback): Promise<void> {
        if (this.status === SessionStatus.Disabled) {
            return Promise.reject("PowerShell Pro Tools is disabled.");
        }

        if (this.reconnectPromise) {
            Container.Log("Reconnect already in progress.");
            return this.reconnectPromise.then(() => callback());
        }

        this.reconnectPromise = this.reconnect(callback).then(() => {
            this.reconnectPromise = null;
        }, (error) => {
            this.reconnectPromise = null;
            throw error;
        });

        return this.reconnectPromise;
    }

    private async reconnect(callback): Promise<void> {
        this.setSessionStatus(SessionStatus.Initializing);
        this.logger?.destroy();

        for (this.reconnectDepth = 1; this.reconnectDepth <= 10; this.reconnectDepth++) {
            try {
                Container.Log(`Reconnect attempt ${this.reconnectDepth}.`);
                await this.Connect(callback, true);
                this.reconnectDepth = 0;
                return;
            }
            catch (error) {
                Container.Log(`Reconnect attempt ${this.reconnectDepth} failed. ${error}`);
                await this.delay(5000);
            }
        }

        Container.Log("Reconnect depth exceeded. Connection failed.");
        this.setSessionStatus(SessionStatus.Failed);
        throw "Reconnect depth exceeded.";
    }

    async Connect(callback, refreshPipeName: boolean = false): Promise<void> {
        if (this.status === SessionStatus.Connected) {
            Container.Log("Already connected to PowerShell process.");
            callback();
            return;
        }

        if (refreshPipeName) {
            this.resetPipeName();
        }

        this.setSessionStatus(SessionStatus.Initializing);

        var cmdletPath = path.join(vscode.extensions.getExtension("ironmansoftware.powershellprotools").extensionPath, "Modules", "PowerShellProTools.VSCode", "PowerShellProTools.VSCode.psd1");
        if (!fs.existsSync(cmdletPath)) {
            cmdletPath = path.join(vscode.extensions.getExtension("ironmansoftware.powershellprotools").extensionPath, "src", "Modules", "PowerShellProTools.VSCode", "PowerShellProTools.VSCode.psd1");
        }
        var poshToolsModulePath = path.join(vscode.extensions.getExtension("ironmansoftware.powershellprotools").extensionPath, "Modules", "PowerShellProTools", "PowerShellProTools.psd1");
        if (!fs.existsSync(poshToolsModulePath)) {
            poshToolsModulePath = path.join(vscode.extensions.getExtension("ironmansoftware.powershellprotools").extensionPath, "src", "Modules", "PowerShellProTools", "PowerShellProTools.psd1");
        }

        const terminal = await this.waitForPowerShellExtensionTerminal();
        Container.Log("Importing module in PowerShell and starting server.");
        terminal.sendText(`Import-Module '${cmdletPath}'`, true);
        terminal.sendText(`Import-Module '${poshToolsModulePath}'`, true);
        terminal.sendText(`Start-PoshToolsServer -PipeName '${this.pipeName}'`, true);

        const settings = load();

        if (settings.clearScreenAfterLoad) {
            terminal.sendText('Clear-Host', true);
        }

        await this.connectLogger(this.pipeName);
        await this.invokeMethodWithRetry("Connect", []);

        this.reconnectDepth = 0;
        this.setSessionStatus(SessionStatus.Connected);
        Container.FinishInitialize();
        Container.Log("Connected to PowerShell process.")
        callback();
    }

    private async connectLogger(pipeName: string): Promise<void> {
        let lastError = null;
        for (let attempt = 0; attempt < 60; attempt++) {
            try {
                await this.connectLoggerPipe(pipeName);
                return;
            }
            catch (error) {
                lastError = error;
                await this.delay(500);
            }
        }

        throw lastError || `Timed out connecting to log pipe for ${pipeName}.`;
    }

    private connectLoggerPipe(pipeName: string): Promise<void> {
        return new Promise((resolve, reject) => {
            Container.Log("Connecting log pipe to PoshTools server.");

            const pipePath = this.getPipePath(pipeName, "_log");
            const logger = net.connect(pipePath);
            let connected = false;
            const timeout = setTimeout(() => {
                if (!connected) {
                    logger.destroy();
                    reject(`Timed out connecting to log pipe ${pipePath}.`);
                }
            }, 30000);

            logger.once("connect", () => {
                connected = true;
                clearTimeout(timeout);
                this.logger = logger;
                resolve();
            });

            logger.on('data', (data) => {
                try {
                    let buff = Buffer.from(data.toString(), 'base64');
                    let text = buff.toString('utf-8');
                    Container.Log(text);
                }
                catch (e) {
                    Container.Log("Error parsing log data. " + e);
                }
            });

            logger.on("error", (e) => {
                if (!connected) {
                    clearTimeout(timeout);
                    logger.destroy();
                    reject(e);
                    return;
                }

                Container.Log(`Log pipe error. ${e}`);
            });
        });
    }

    private async invokeMethodWithRetry(method: string, args: Array<any>): Promise<any> {
        let lastError = null;
        for (let attempt = 0; attempt < 60; attempt++) {
            try {
                return await this.invokeMethod(method, args, false);
            }
            catch (error) {
                lastError = error;
                await this.delay(500);
            }
        }

        throw lastError || `Timed out invoking ${method}.`;
    }

    InvokePowerShell(command: string): Promise<any> {
        return new Promise((resolve, reject) => {
            this.invokeMethod("ExecutePowerShell", [command]).then(response => {
                if (response.error) {
                    Container.Log("Error executing PowerShell command: " + response.error);
                    vscode.window.showErrorMessage(response.error);
                    reject();
                }
                else {
                    resolve(response);
                }
            })
        });
    }

    invokeMethod(method: string, args: Array<any>, retryOnFailure: boolean = true): Promise<any> {
        return new Promise((resolve, reject) => {
            if (this.status === SessionStatus.Failed || this.status === SessionStatus.Disabled) {
                reject(`PowerShell Pro Tools is not connected. Status: ${this.status}.`);
                return;
            }

            var pipePath = this.getPipePath(this.pipeName);
            var client: net.Socket;
            var settled = false;
            const connectTimeout = setTimeout(() => {
                settled = true;
                client?.destroy();
                reject(`Timed out connecting to named pipe ${pipePath} for ${method}.`);
            }, 30000);

            client = net.connect(pipePath, function () {
                if (settled) {
                    return;
                }

                clearTimeout(connectTimeout);
                const request = {
                    method,
                    args
                };

                const json = JSON.stringify(request);
                let buf = Buffer.from(json);
                let encodedData = buf.toString('base64');

                client.write(encodedData + "\r\n");
            });

            client.on("error", (e) => {
                if (settled) {
                    return;
                }

                settled = true;
                clearTimeout(connectTimeout);
                Container.Log(`Invoke method: ${method} Error sending data on named pipe. ${e}`);
                client.destroy();
                if ((e as any).code === "ETIMEDOUT") {
                    reject(e);
                    return;
                }

                if (!retryOnFailure) {
                    reject(e);
                    return;
                }

                this.Reconnect(() => { }).then(() => {
                    return this.invokeMethod(method, args, false);
                }).then(any => resolve(any), error => reject(error));
            });

            client.on('data', (data) => {
                if (settled) {
                    return;
                }

                settled = true;
                clearTimeout(connectTimeout);
                try {
                    let buff = Buffer.from(data.toString(), 'base64');
                    let text = buff.toString('utf-8');
                    var response = JSON.parse(text);
                    resolve(response);
                }
                catch (error) {
                    reject(error);
                }

                client.destroy();
            });
        });
    }

    async Package(packageConfig: string): Promise<any> {
        this.SendTerminalCommand(`Merge-Script -ConfigFile '${packageConfig}' -Verbose`)
    }

    async OpenPsScriptPad(file: string): Promise<any> {
        var response = await this.invokeMethod("OpenPsScriptPad", [file]);
        return response.Data;
    }

    async DisablePsesIntelliSense(options: RapidSenseOptions): Promise<any> {
        var response = await this.invokeMethod("DisablePsesIntelliSense", [JSON.stringify(options)]);
        return response.Data;
    }

    async EnablePsesIntelliSense(): Promise<any> {
        var response = await this.invokeMethod("EnablePsesIntelliSense", []);
        return response.Data;
    }

    async RefreshCompletionCache(): Promise<any> {
        var response = await this.invokeMethod("RefreshCompletionCache", []);
        return response.Data;
    }


    async InstallPoshToolsModule(): Promise<any> {
        var response = await this.invokeMethod("InstallPoshToolsModule", []);
        return response.Data;
    }

    async GetVariables(): Promise<any> {
        const settings = load();
        var response = await this.invokeMethod("GetVariables", [settings.excludeAutomaticVariables]);
        return response.Data;
    }

    async ExpandVariable(path: string): Promise<any> {
        var response = await this.invokeMethod("ExpandVariable", [path]);
        return response.Data;
    }

    async GetAst(path: string): Promise<any> {
        var response = await this.invokeMethod("ParseAst", [path]);
        return response.Data;
    }

    async GetAstByHashcode(hashcode: string): Promise<any> {
        var response = await this.invokeMethod("GetAstChildren", [hashcode]);
        return response.Data;
    }

    async GetModulePath(name: string, version: string): Promise<string> {
        return this.invokeMethod("GetModulePath", [name, version]);
    }

    async FindModuleVersion(name: string): Promise<string> {
        var response = await this.invokeMethod("FindModuleVersion", [name]);
        return response.Data;
    }

    async UninstallModule(name: string, version: string): Promise<string> {
        return await this.invokeMethod("UninstallModule", [name, version]);
    }

    async UpdateModule(name: string): Promise<string> {
        return await this.invokeMethod("UpdateModule", [name]);
    }

    async GetModules(): Promise<any> {
        var response = await this.invokeMethod("GetModules", []);
        return response.Data;
    }

    async GetProviders(): Promise<string[]> {

        var response = await this.invokeMethod("GetProviders", []);
        return response.Data;
    }

    async GetProviderDrives(provider: string): Promise<string[]> {
        var response = await this.invokeMethod("GetProviderDrives", [provider]);
        return response.Data;
    }

    async GetContainers(parent: string): Promise<any[]> {
        var result = await this.invokeMethod("GetContainers", [parent]);
        return result.Data;
    }

    async GetItems(parent: string): Promise<any[]> {
        var result = await this.invokeMethod("GetItems", [parent]);
        return result.Data;
    }

    async MeasureScript(fileName: string): Promise<any> {
        var result = await this.invokeMethod("MeasureScript", [fileName]);
        return result.Data;
    }

    private updateShow: boolean;

    async GetPSHostProcess(): Promise<any> {
        var response = await this.invokeMethod("GetPSHostProcesses", []);
        return response.Data;
    }

    async GetRunspaces(processId: number): Promise<any> {
        var response = await this.invokeMethod("GetRunspaces", [processId]);
        return response.Data;
    }

    AttachToRunspace(processId: number, runspaceId: number) {
        vscode.debug.startDebugging(vscode.workspace.workspaceFolders[0], {
            name: "PowerShell Attach",
            type: "PowerShell",
            request: "attach",
            processId,
            runspaceId
        });
    }

    async Complete(trigger: string, currentLine: string, position: number): Promise<any> {
        var response = await this.invokeMethod("Complete", [trigger, currentLine, position]);
        return response.Data;
    }

    async GenerateWinForm(codeFilePath: string, formPath: string): Promise<any> {
        await this.invokeMethod("GenerateWinForm", [codeFilePath, formPath, false]);
    }

    async GenerateTool(codeFilePath: string, formPath: string): Promise<any> {
        await this.invokeMethod("GenerateWinForm", [codeFilePath, formPath, true]);
    }

    async ShowFormDesigner(codeFilePath: string, designerFilePath: string): Promise<any> {
        await this.invokeMethod("ShowWinFormDesigner", [designerFilePath, codeFilePath]);
    }


    async ShowWpfFormDesigner(codeFilePath: string): Promise<any> {
        await this.invokeMethod("ShowWpfFormDesigner", [codeFilePath]);
    }

    async ConvertToPowerShellFromFile(fileName: string): Promise<string> {
        var result = await this.invokeMethod("ConvertToPowerShellFromFile", [fileName]);
        return result.Data;
    }

    async ConvertToPowerShell(script: string): Promise<string> {
        var result = await this.invokeMethod("ConvertToPowerShell", [script]);
        return result.Data;
    }

    async ConvertToCSharpFromFile(fileName: string): Promise<string> {
        var result = await this.invokeMethod("ConvertToCSharpFromFile", [fileName]);
        return result.Data;
    }

    async ConvertToCSharp(script: string): Promise<string> {
        var result = await this.invokeMethod("ConvertToCSharp", [script]);
        return result.Data;
    }

    async GetItemProperty(path: string): Promise<string> {
        var result = await this.invokeMethod("GetItemProperty", [path]);
        return result.Data;
    }

    async ViewItems(path: string): Promise<string> {
        var result = await this.invokeMethod("ViewItems", [path]);
        return result.Data;
    }

    async GetPowerShellProcessPath(): Promise<string> {
        var result = await this.invokeMethod("GetPowerShellProcessPath", []);
        return result.Data;
    }

    async RenameSymbol(newName: string, workspaceRoot: string, request: RefactorRequest): Promise<Array<RefactorTextEdit>> {
        var result = await this.invokeMethod("RenameSymbol", [newName, workspaceRoot, JSON.stringify(request)]);
        return result.Data;
    }

    async Refactor(request: RefactorRequest): Promise<Array<RefactorTextEdit>> {
        var result = await this.invokeMethod("Refactor", [JSON.stringify(request)]);
        return result.Data;
    }

    async GetValidRefactors(request: RefactorRequest): Promise<Array<RefactorInfo>> {
        var result = await this.invokeMethod("GetValidRefactors", [JSON.stringify(request)]);
        return result.Data;
    }

    async GetHover(state: TextEditorState): Promise<Hover> {
        var result = await this.invokeMethod("GetHover", [JSON.stringify(state)]);
        return result.Data;
    }

    async AddWorkspaceFolder(root: string): Promise<any> {
        var result = await this.invokeMethod("AddWorkspaceFolder", [root]);
        return result.Data;
    }

    async AnalyzeWorkspaceFile(file: string): Promise<any> {
        var result = await this.invokeMethod("AnalyzeWorkspaceFile", [file]);
        return result.Data;
    }

    async RemoveWorkspaceFolder(root: string): Promise<any> {
        var result = await this.invokeMethod("RemoveWorkspaceFolder", [root]);
        return result.Data;
    }

    async GetFunctionDefinitions(file: string, root: string): Promise<Array<FunctionDefinition>> {
        var result = await this.invokeMethod("GetFunctionDefinitions", [file, root]);
        return result.Data;
    }

    async GetCodeSigningCerts(): Promise<Array<Certificate>> {
        var result = await this.invokeMethod("GetCodeSigningCerts", []);
        return result.Data;
    }

    async SignScript(filePath: string, certificatePath: string): Promise<any> {
        var result = await this.invokeMethod("SignScript", [filePath, certificatePath]);
        return result.Data;
    }

    async GetAssemblies(): Promise<Array<PSAssembly>> {
        var result = await this.invokeMethod("GetAssemblies", []);
        return result.Data;
    }

    async GetTypes(assembly: string, parentNamespace: string): Promise<Array<PSType>> {
        var result = await this.invokeMethod("GetTypes", [assembly, parentNamespace]);
        return result.Data;
    }

    async GetNamespaces(assembly: string, parentNamespace: string): Promise<Array<PSNamespace>> {
        var result = await this.invokeMethod("GetNamespaces", [assembly, parentNamespace]);
        return result.Data;
    }

    async GetMethods(assembly: string, typeName: string): Promise<Array<PSMethod>> {
        var result = await this.invokeMethod("GetMethods", [assembly, typeName]);
        return result.Data;
    }

    async GetProperties(assembly: string, typeName: string): Promise<Array<PSProperty>> {
        var result = await this.invokeMethod("GetProperties", [assembly, typeName]);
        return result.Data;
    }

    async GetFields(assembly: string, typeName: string): Promise<Array<PSField>> {
        var result = await this.invokeMethod("GetFields", [assembly, typeName]);
        return result.Data;
    }

    async FindType(typeName: string): Promise<PSType> {
        var result = await this.invokeMethod("FindType", [typeName]);
        return result.Data;
    }

    async DecompileType(assemblyName: string, fullTypeName: string): Promise<string> {
        var result = await this.invokeMethod("DecompileType", [assemblyName, fullTypeName]);
        return result.Data;
    }

    async LoadAssembly(assemblyPath: string): Promise<void> {
        var result = await this.invokeMethod("LoadAssembly", [assemblyPath]);
        return result.Data;
    }

    async GetTreeViews(): Promise<Array<CustomTreeView>> {
        var result = await this.invokeMethod("GetTreeViews", []);
        return result.Data;
    }

    async LoadChildren(treeViewId: string, path: string): Promise<Array<CustomTreeItem>> {
        var result = await this.invokeMethod("LoadChildren", [treeViewId, path]);
        return result.Data;
    }

    async InvokeChild(treeViewId: string, path: string): Promise<void> {
        await this.invokeMethod("InvokeChild", [treeViewId, path]);
    }

    async RefreshTreeView(treeViewId: string): Promise<void> {
        await this.invokeMethod("RefreshTreeView", [treeViewId]);
    }

    async GetHistory(): Promise<Array<string>> {
        const count = vscode.workspace.getConfiguration("terminal.integrated.shellIntegration").get("history") as Number;

        var result = await this.invokeMethod("GetHistory", [count]);
        return result.Data;
    }

    async GetSessions(): Promise<Array<Session>> {
        var result = await this.invokeMethod("GetSessions", []);
        return result.Data;
    }

    async EnterSession(id: number): Promise<void> {
        Container.PowerShellService.SendTerminalCommand(`Enter-PSSession ${id}`);
    }

    async ExitSession(): Promise<void> {
        Container.PowerShellService.SendTerminalCommand('Exit-PSSession');
    }

    async RemoveSession(id: number): Promise<void> {
        await this.invokeMethod("RemoveSession", [id]);
    }

    async GetJobs(): Promise<Array<Job>> {
        var result = await this.invokeMethod("GetJobs", []);
        return result.Data;
    }

    async DebugJob(id: number): Promise<void> {
        this.SendTerminalCommand(`Debug-Job -Id ${id}`);
    }

    async RemoveJob(id: number): Promise<void> {
        await this.invokeMethod("RemoveJob", [id]);
    }

    async ReceiveJob(id: number): Promise<void> {
        this.SendTerminalCommand(`Receive-Job -Id ${id}`);
    }

    async StopJob(id: number): Promise<void> {
        await this.invokeMethod("StopJob", [id]);
    }

    async FindModule(module: string): Promise<Array<string>> {
        var result = await this.invokeMethod("FindModule", [module]);
        return result.Data;
    }

    async InstallModule(module: string): Promise<void> {
        await this.invokeMethod("InstallModule", [module]);
    }

    async GetPerformance(): Promise<Performance> {
        var result = await this.invokeMethod("GetPerformance", []);
        return result.Data;
    }

    SendTerminalCommand(command: string) {
        var terminal = vscode.window.terminals.find(x => x.name.startsWith("PowerShell Extension"));
        if (terminal == null) {
            return;
        }

        terminal.sendText(command, true);
    }
}