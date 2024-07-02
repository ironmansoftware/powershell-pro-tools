'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';

export function generateWinForm() {
    return vscode.commands.registerCommand('powershell.generateWinForm', async () => {
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

        await Container.PowerShellService.GenerateWinForm(codeFilePath, formPath);
        vscode.workspace.openTextDocument(formPath).then(y => {
            vscode.window.showTextDocument(y);
        })
    });
}