'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { SessionStatus } from '../services/powershellservice';

export function statusBarItemMenu() {
    return vscode.commands.registerCommand('poshProTools.statusBarMenu', async () => {
        if (!Container.IsInitialized()) return;

        if (Container.PowerShellService.status === SessionStatus.Failed) {
            var retry = await vscode.window.showErrorMessage("PowerShell Pro Tools session failed to start. Please check the PowerShell Pro Tools extension output for more information.", "Retry");
            if (retry === "Retry") {
                Container.PowerShellService.Reconnect(() => { });
            }
            return;
        }

        if (Container.PowerShellService.status === SessionStatus.Initializing) {
            return;
        }

        let options = ["About PowerShell Pro Tools", "Documentation", "Forums", "Report an Issue"];


        const result = await vscode.window.showQuickPick(options);

        if (result === "About PowerShell Pro Tools") {
            vscode.env.openExternal(vscode.Uri.parse("https://docs.poshtools.com"))
        }
        else if (result === "Install License") {
            vscode.commands.executeCommand("poshProTools.openLicenseFile");
        }
        else if (result === "Documentation") {
            vscode.env.openExternal(vscode.Uri.parse("https://docs.poshtools.com"))
        }
        else if (result === "Forums") {
            vscode.env.openExternal(vscode.Uri.parse("https://forums.ironmansoftware.com"))
        }
        else if (result === "Report an Issue") {
            vscode.env.openExternal(vscode.Uri.parse("https://github.com/ironmansoftware/issues"))
        }
    });
}