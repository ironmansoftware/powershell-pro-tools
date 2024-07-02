'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { ICommand } from './command';

export class DebuggingCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.AttachRunspace());
        context.subscriptions.push(this.RunInNewTerminal());
    }

    AttachRunspace() {
        return vscode.commands.registerCommand('poshProTools.attachRunspace', async (node) => {
            if (!Container.IsInitialized()) return;
        })
    }

    RunInNewTerminal() {
        return vscode.commands.registerCommand('poshProTools.runInNewTerminal', async () => {
            if (!Container.IsInitialized()) return;

            if (vscode.window.activeTextEditor == null || !vscode.window.activeTextEditor.document.fileName.endsWith(".ps1")) {
                return
            }

            const path = await Container.PowerShellService.GetPowerShellProcessPath();
            var terminal = vscode.window.createTerminal("PowerShell", path, ['-NoExit', '-File', `${vscode.window.activeTextEditor.document.uri.fsPath}`])
            terminal.show();
        })
    }
}