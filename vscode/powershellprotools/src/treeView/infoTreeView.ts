import * as vscode from 'vscode';
import { Node, TreeViewProvider } from './treeViewProvider';

export class InfoViewProvider extends TreeViewProvider {
    getRefreshCommand(): string {
        return null;
    }

    requiresLicense(): boolean {
        return false;
    }

    async getNodes(): Promise<vscode.TreeItem[]> {
        return [
            new Node("Documentation", "book"),
            new Node("Forums", "account"),
            new Node("GitHub", "github")
        ];
    }
}