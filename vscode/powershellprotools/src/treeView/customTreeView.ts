import * as vscode from 'vscode';
import { Container } from '../container';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';
import { CustomTreeItem, CustomTreeView } from '../types';

export class CustomTreeViewProvider extends TreeViewProvider {
	requiresLicense(): boolean {
		return false;
	}

	getRefreshCommand(): string {
		return "customView.refresh";
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		var treeViews = await Container.PowerShellService.GetTreeViews();
		return treeViews.map(x => new CustomTreeViewParentNode(x));
	}
}

export class CustomTreeViewParentNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		var treeItems = await Container.PowerShellService.LoadChildren(this.treeView.Label, "");
		return treeItems.map(x => x.HasChildren ? new CustomTreeItemParentNode(x) : new CustomTreeItemNode(x));
	}

	constructor(public readonly treeView: CustomTreeView) {
		super(treeView.Label, vscode.TreeItemCollapsibleState.Collapsed);

		if (treeView.Icon)
			this.iconPath = new vscode.ThemeIcon(treeView.Icon)

		this.description = treeView.Description;
		this.tooltip = treeView.Tooltip;
	}
}

export class CustomTreeItemNode extends vscode.TreeItem {

	public path: string;
	public treeViewId: string;

	constructor(treeItem: CustomTreeItem) {
		super(treeItem.Label);

		this.path = treeItem.Path;
		this.treeViewId = treeItem.TreeViewId;
		this.contextValue = treeItem.CanInvoke ? "customViewItem" : null;

		this.tooltip = treeItem.Tooltip;
		this.description = treeItem.Tooltip;
		if (treeItem.Icon)
			this.iconPath = new vscode.ThemeIcon(treeItem.Icon);
	}
}

export class CustomTreeItemParentNode extends ParentTreeItem {
	public path: string;
	public treeViewId: string;

	async getChildren(): Promise<vscode.TreeItem[]> {
		var treeItems = await Container.PowerShellService.LoadChildren(this.treeItem.TreeViewId, this.treeItem.Path);
		return treeItems.map(x => x.HasChildren ? new CustomTreeItemParentNode(x) : new CustomTreeItemNode(x));
	}

	constructor(private readonly treeItem: CustomTreeItem) {
		super(treeItem.Label, vscode.TreeItemCollapsibleState.Collapsed);

		this.path = treeItem.Path;
		this.treeViewId = treeItem.TreeViewId;
		this.contextValue = treeItem.CanInvoke ? "customViewItem" : null;

		this.tooltip = treeItem.Tooltip;
		this.description = treeItem.Tooltip;
		if (treeItem.Icon)
			this.iconPath = new vscode.ThemeIcon(treeItem.Icon);
	}
}