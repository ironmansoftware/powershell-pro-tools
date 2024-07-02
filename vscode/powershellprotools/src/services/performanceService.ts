import * as vscode from 'vscode';
import { Container } from '../container';

export class PerformanceService {
    private statusBarItem: vscode.StatusBarItem;

    constructor(context: vscode.ExtensionContext) {
        this.statusBarItem =
            vscode.window.createStatusBarItem(
                vscode.StatusBarAlignment.Left,
                1);

        //this.statusBarItem.command = "poshProTools.statusBarMenu";

        this.statusBarItem.show();
        vscode.window.onDidChangeActiveTextEditor((textEditor) => {
            if (textEditor === undefined
                || textEditor.document.languageId !== "powershell") {
                this.statusBarItem.hide();
            } else {
                this.statusBarItem.show();
            }
        });

        var perfService = this;

        const interval = setInterval(async () => {
            const performance = await Container.PowerShellService.GetPerformance();
            perfService.statusBarItem.text = `$(dashboard) MEM ${performance.Memory}, CPU ${performance.Cpu}`
        }, 5000);

        this.statusBarItem.tooltip = "PowerShell Performance";

        context.subscriptions.push(this.statusBarItem);
        context.subscriptions.push({
            dispose: () => {
                clearInterval(interval);
            }
        });
    }
}