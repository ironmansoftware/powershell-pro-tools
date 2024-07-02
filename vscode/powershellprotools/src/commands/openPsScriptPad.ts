import * as vscode from 'vscode';
import { Container } from '../container';

export function openPsScriptPad() {
    return vscode.commands.registerCommand('poshProTools.openPsScriptPad', (resource) => {
        if (!Container.IsInitialized()) return;

        Container.PowerShellService.OpenPsScriptPad(resource.fsPath);
    });
}