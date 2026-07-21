import * as vscode from 'vscode';
import { Container } from '../container';
import { load } from '../settings';
import { SessionStatus } from './powershellservice';

export class PerformanceService {
    private statusBarItem: vscode.StatusBarItem;

    constructor(context: vscode.ExtensionContext) {
        const settings = load();

        if (!settings.statusBar.performanceVisibility) {
            return;
        }

        const alignment = settings.statusBar.performanceAlignment === "left" ? vscode.StatusBarAlignment.Left : vscode.StatusBarAlignment.Right;

        this.statusBarItem =
            vscode.window.createStatusBarItem(
                alignment,
                1);

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

        const refreshInterval = settings.statusBar.performanceRefreshInterval * 1000;

        const interval = setInterval(async () => {
            if (Container.PowerShellService.status !== SessionStatus.Connected) {
                return;
            }

            try {
                const performance = await Container.PowerShellService.GetPerformance();
                perfService.statusBarItem.text = `$(dashboard) MEM ${performance.Memory}, CPU ${performance.Cpu}`
            }
            catch (error) {
                Container.Log("Failed to update PowerShell performance. " + error);
            }
        }, refreshInterval);

        this.statusBarItem.tooltip = "PowerShell Performance";

        context.subscriptions.push(this.statusBarItem);
        context.subscriptions.push({
            dispose: () => {
                clearInterval(interval);
            }
        });
    }
}