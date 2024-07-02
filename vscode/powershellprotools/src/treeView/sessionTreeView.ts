import * as vscode from 'vscode';
import { Container } from '../container';
import { Session } from '../types';
import { TreeViewProvider } from './treeViewProvider';

export class SessionTreeViewProvider extends TreeViewProvider {
	requiresLicense(): boolean {
		return false;
	}

	getRefreshCommand(): string {
		return "sessionsView.refresh";
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		var history = await Container.PowerShellService.GetSessions();
		return history.map(x => new SessionTreeItem(x));
	}
}

export class SessionTreeItem extends vscode.TreeItem {
	constructor(public readonly session: Session) {
		super(session.Name);

		this.description = session.ComputerName;

		this.tooltip = session.Name;
		this.iconPath = new vscode.ThemeIcon('vm-connect');
	}

	contextValue = 'session';
}
