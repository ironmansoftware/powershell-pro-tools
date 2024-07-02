'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';

export function generateTool() {
    return vscode.commands.registerCommand('powershell.generateTool', async () => {
        if (!Container.IsInitialized()) return;

        if (vscode.window.activeTextEditor == null) {
            vscode.window.showErrorMessage("You must open a .ps1 file to generate a Windows Form.");
            return;
        }

        var fsPath = vscode.window.activeTextEditor.document.fileName;

        if (!fsPath.endsWith('.ps1')) {
            vscode.window.showErrorMessage("You must open a .ps1 file to generate a Windows Form.");
            return;
        }

        const codeFilePath = fsPath;
        const formPath = fsPath.replace('.ps1', '.form.ps1');

        await Container.PowerShellService.GenerateTool(codeFilePath, formPath);

        var exePath = fsPath.replace('.ps1', '.form.exe');
        var result = await vscode.window.showInformationMessage(`Generated application: ${exePath}`, 'Launch');
        if (result === 'Launch') {
            vscode.env.openExternal(vscode.Uri.file(exePath))
        }
    });
}