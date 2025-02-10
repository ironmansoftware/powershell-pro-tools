import * as vscode from 'vscode';
import { ICommand } from '../commands/command';
import { Container } from '../container';
import { load, PowerShellLanguageId } from './../settings';

export class RapidSenseCommand implements ICommand {

    private rapidSenseEnabled: boolean = false;
    private enabling: boolean = false;
    private statusBarItem: vscode.StatusBarItem;

    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.toggleRapidSense());

        const settings = load();

        if (settings.statusBar.rapidSenseVisibility) {
            this.statusBarItem =
                vscode.window.createStatusBarItem(
                    vscode.StatusBarAlignment.Right,
                    1);

            this.statusBarItem.command = "poshProTools.toggleRapidSense";
            this.statusBarItem.text = "IntelliSense";
            this.statusBarItem.show();
        }

        const provider1 = vscode.languages.registerCompletionItemProvider('powershell', this, "$", ".", "-", "[", "\\");

        context.subscriptions.push(provider1);

        vscode.workspace.onDidChangeConfiguration(async (e) => {
            if (e.affectsConfiguration(PowerShellLanguageId) && this.rapidSenseEnabled && !this.enabling) {
                this.enabling = true;
                await Container.PowerShellService.EnablePsesIntelliSense();
                this.enableRapidSense("Settings changed. Refreshing RapidSense caches...");
            }
        })
    }

    async provideCompletionItems(document: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken, context: vscode.CompletionContext) {
        if (this.rapidSenseEnabled) {
            const line = document.lineAt(position.line).text;

            const items = await Container.PowerShellService.Complete(context.triggerCharacter, line, position.character);
            return items.map(item => new vscode.CompletionItem(item.InsertText, item.CompletionKind));
        }
        return [];
    }

    enableRapidSense(msg: string) {
        const settings = load();

        vscode.window.withProgress({
            location: vscode.ProgressLocation.Notification,
            title: msg
        }, async () => {
            await Container.PowerShellService.DisablePsesIntelliSense({
                ignoredAssemblies: settings.ignoredAssemblies,
                ignoredCommands: settings.ignoredCommands,
                ignoredModules: settings.ignoredModules,
                ignoredTypes: settings.ignoredTypes,
                ignoredVariables: settings.ignoredVariables,
                ignoredPaths: settings.ignoredPaths
            });

            this.rapidSenseEnabled = true;
            this.statusBarItem.text = "$(rocket) RapidSense";
            this.enabling = false;
        })
    }

    toggleRapidSense() {

        vscode.debug.onDidTerminateDebugSession(async () => {
            if (this.rapidSenseEnabled) {
                vscode.window.withProgress({
                    location: vscode.ProgressLocation.Notification,
                    title: "Refreshing RapidSense cache..."
                }, async () => {
                    await Container.PowerShellService.RefreshCompletionCache();
                });
            }
        })

        return vscode.commands.registerCommand('poshProTools.toggleRapidSense', async () => {
            if (!Container.IsInitialized()) return;

            if (this.rapidSenseEnabled) {
                this.rapidSenseEnabled = false;
                this.statusBarItem.text = "IntelliSense";
                this.statusBarItem.color = null;
                await Container.PowerShellService.EnablePsesIntelliSense();
            }
            else {
                this.enableRapidSense("Enabling RapidSense...");
            }
        });
    }
}