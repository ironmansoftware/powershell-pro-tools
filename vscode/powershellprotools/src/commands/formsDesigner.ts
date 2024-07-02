'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';

async function LaunchEditor(context: vscode.ExtensionContext) {
    if (vscode.window.activeTextEditor == null) {
        vscode.window.showErrorMessage("Open a PS1 or XAML file to edit with the form designer before running this command.");
        return;
    }


    var fsPath = vscode.window.activeTextEditor.document.fileName.toLowerCase();
    const codeFilePath = fsPath;

    if (fsPath.endsWith(".ps1")) {
        const designerFilePath = fsPath.replace('.ps1', '.designer.ps1');
        vscode.window.setStatusBarMessage("Starting form designer...", 3000);
        await Container.PowerShellService.ShowFormDesigner(codeFilePath, designerFilePath);
    }
    else if (fsPath.endsWith(".xaml")) {
        await Container.PowerShellService.ShowWpfFormDesigner(codeFilePath);
    }
    else {
        vscode.window.showErrorMessage("Open a PS1 or XAML file to edit with the form designer before running this command.");
    }
}

export function showWinFormDesigner(context: vscode.ExtensionContext) {
    return vscode.commands.registerCommand('powershell.showWinFormDesigner', () => {
        if (!Container.IsInitialized()) return;

        LaunchEditor(context);
    });
}