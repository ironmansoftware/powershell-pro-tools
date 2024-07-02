import * as vscode from 'vscode';
import { Container } from '../container';
import { TreeViewProvider } from './treeViewProvider';

export class QuickScriptViewProvider extends TreeViewProvider {
	getRefreshCommand(): string {
		return "quickScriptView.refresh";
	}

	requiresLicense(): boolean {
		return false;
	}

	static Instance: QuickScriptViewProvider;

	constructor() {
		super();
		QuickScriptViewProvider.Instance = this;
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		return Container.QuickScriptService.getScripts().map(x => new QuickScript(x.Name, x.File));
	}
}


export class QuickScript extends vscode.TreeItem {
	constructor(
		public readonly name: string,
		public readonly file: string
	) {
		super(name, vscode.TreeItemCollapsibleState.None);
	}

	public getTreeItem(): vscode.TreeItem {
		return {
			tooltip: this._tooltip,
			contextValue: this.contextValue,
			collapsibleState: this.collapsibleState,
			label: this.label,
			description: this._description
		};
	}

	get _description(): string {
		return this.file;
	}

	get _tooltip(): string {
		return this.file;
	}

	contextValue = "quickscript";
}
