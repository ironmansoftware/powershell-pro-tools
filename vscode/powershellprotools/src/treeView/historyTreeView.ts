import * as vscode from 'vscode';
import { Container } from '../container';
import { TreeViewProvider } from './treeViewProvider';

export class HistoryTreeViewProvider extends TreeViewProvider {
    requiresLicense(): boolean {
        return false;
    }

    getRefreshCommand(): string {
        return "historyView.refresh";
    }

    async getNodes(): Promise<vscode.TreeItem[]> {
        var history = await Container.PowerShellService.GetHistory();
        return history.map(x => new HistoryTreeItem(x));
    }
}

export class HistoryTreeItem extends vscode.TreeItem {
    constructor(public readonly history: string) {
        super(history);

        if (history.length > 60) {
            this.label = history.substring(0, 60) + "...";
        }

        this.tooltip = history;
        this.iconPath = new vscode.ThemeIcon('history');
    }

    contextValue = 'historyItem';
}
