import * as vscode from 'vscode';
import { Container } from '../container';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';

export class VariableViewProvider extends TreeViewProvider {
	getRefreshCommand(): string {
		return "variableView.refresh";
	}

	requiresLicense(): boolean {
		return true;
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		var variables = await Container.PowerShellService.GetVariables();

		return variables.map(x => new VariableDetails(x));
	}
}


export class VariableDetails extends ParentTreeItem {

	async getChildren(): Promise<vscode.TreeItem[]> {
		var children = await Container.PowerShellService.ExpandVariable(this.variableDetails.Path);

		return children.map(x => {
			return new VariableDetails(x);
		})
	}

	constructor(
		public readonly variableDetails: any
	) {
		super(variableDetails.VarName, variableDetails.HasChildren ? vscode.TreeItemCollapsibleState.Collapsed : vscode.TreeItemCollapsibleState.None);
	}

	public getTreeItem(): vscode.TreeItem {
		return {
			tooltip: this._tooltip,
			contextValue: this.contextValue,
			collapsibleState: this.collapsibleState,
			label: this.label,
			description: this._description || ""
		};
	}

	get _description(): string {
		return this.variableDetails.VarValue;
	}

	get _tooltip(): string {
		return this.variableDetails.Type;
	}

	contextValue = "variable";
}