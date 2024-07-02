import * as vscode from 'vscode';
import { Container } from '../container';

export abstract class TreeViewProvider implements vscode.TreeDataProvider<vscode.TreeItem> {

	private _onDidChangeTreeData: vscode.EventEmitter<vscode.TreeItem | undefined> = new vscode.EventEmitter<vscode.TreeItem | undefined>();
	readonly onDidChangeTreeData: vscode.Event<vscode.TreeItem | undefined> = this._onDidChangeTreeData.event;

	constructor() {
		let command = this.getRefreshCommand();
		if (command)
			vscode.commands.registerCommand(command, () => this.refresh());
	}

	abstract requiresLicense(): boolean;
	abstract getRefreshCommand(): string;

	refresh(node?: vscode.TreeItem): void {
		this._onDidChangeTreeData.fire(node);
	}

	getTreeItem(element: vscode.TreeItem): vscode.TreeItem {

		if (element instanceof ParentTreeItem) {
			const node = element as ParentTreeItem;

			return node.getTreeItem();
		}

		return element;
	}

	async getChildren(element?: vscode.TreeItem): Promise<vscode.TreeItem[]> {
		try {
			return await this.getChildrenImpl(element);
		}
		catch (err) {
			//vscode.window.showErrorMessage(err.message);
			return []
		}
	}

	abstract getNodes(): Promise<vscode.TreeItem[]>;

	async getChildrenImpl(element?: vscode.TreeItem): Promise<vscode.TreeItem[]> {
		if (element == null) {
			var nodes = [];

			let implNodes = await this.getNodes();
			implNodes.forEach(node => nodes.push(node));

			return nodes;
		}

		if (element instanceof ParentTreeItem) {
			var parentTreeItem = element as ParentTreeItem;
			return parentTreeItem.getChildren();
		}
	}

}


export class Node extends vscode.TreeItem {
	constructor(label: string, icon: string, tooltip?: string) {
		super(label);

		this.iconPath = new vscode.ThemeIcon(icon);
		this.contextValue = 'help';
		this.tooltip = tooltip;
		this.command = {
			command: 'poshProTools.help',
			arguments: [this],
			title: 'Help'
		}
	}
}

export abstract class ParentTreeItem extends vscode.TreeItem {
	constructor(label: string, state: vscode.TreeItemCollapsibleState) {
		super(label, state)
	}

	getTreeItem(): vscode.TreeItem {
		return this;
	}

	abstract getChildren(): Thenable<vscode.TreeItem[]>
}