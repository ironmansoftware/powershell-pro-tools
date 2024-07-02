import * as vscode from 'vscode';
import { Container } from '../container';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';

export class ProviderViewProvider extends TreeViewProvider {
	getRefreshCommand(): string {
		return "providerView.refresh";
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		var providers = await Container.PowerShellService.GetProviders();

		return providers.map(x => new Provider(x));
	}

	requiresLicense(): boolean {
		return true;
	}
}

class Provider extends ParentTreeItem {
	constructor(label: string) {
		super(label, vscode.TreeItemCollapsibleState.Collapsed)
	}

	async getChildren(): Promise<vscode.TreeItem[]> {
		var drives = await Container.PowerShellService.GetProviderDrives(this.label.toString());

		if (drives.map) {
			return drives.map(x => new PSContainer(`${x}:\\`, `${x}:\\`));
		}
		else {
			return [new PSContainer(`${drives}:\\`, `${drives}:\\`)];
		}
	}
}

class PSContainer extends ParentTreeItem {
	private path: string;

	constructor(
		name: string,
		path: string
	) {
		super(name, vscode.TreeItemCollapsibleState.Collapsed)
		this.path = path;
	}

	async getChildren(): Promise<vscode.TreeItem[]> {
		var containers = await Container.PowerShellService.GetContainers(this.path);
		var items = await Container.PowerShellService.GetItems(this.path);

		var containerNodes = containers.map ? containers.map(x => new PSContainer(x.Name, x.Path)) : [];
		var itemNodes = items.map ? items.map(x => new PSItem(x.Name, x.Value, x.Path, x.Tooltip)) : [];

		var treeNodes = [];
		treeNodes = treeNodes.concat(containerNodes);
		return treeNodes.concat(itemNodes);
	}

	contextValue = "providerContainer";
}

class PSItem extends vscode.TreeItem {

	private tooltipStr: string;
	private value: string;

	path: string;

	constructor(name: string, value: string, path: string, tooltipStr: string) {
		super(name, vscode.TreeItemCollapsibleState.None);
		this.tooltipStr = tooltipStr;
		this.value = value;
		this.path = path;
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

	get _tooltip(): string {
		return this.tooltipStr;
	}

	get _description(): string {
		return this.value;
	}

	contextValue = "providerItem";
}

