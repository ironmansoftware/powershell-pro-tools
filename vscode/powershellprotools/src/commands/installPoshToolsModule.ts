'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';

export function InstallPoshToolsModule() {
    return vscode.commands.registerCommand('poshProTools.installModule', async (node) => {
        if (!Container.IsInitialized()) return;

        vscode.window.setStatusBarMessage("Installing PowerShell Pro Tools Module...")
        Container.PowerShellService.InstallPoshToolsModule().then(x => {
            vscode.window.setStatusBarMessage("")
            vscode.window.showInformationMessage("Successfully installed PowerShell Pro Tools module.");
        });
    })
}