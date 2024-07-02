import * as vscode from 'vscode';
import { Container } from '../container';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';

export class HostProcessViewProvider extends TreeViewProvider {
	getRefreshCommand(): string {
		return "hostProcessView.refresh";
	}

	requiresLicense(): boolean {
		return false;
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		var providers = await Container.PowerShellService.GetPSHostProcess();

		return providers.filter(m => m.Process !== 'PowerShellProTools.Host').map(x => new PSHostProcess(x));
	}
}

class PSHostProcess extends ParentTreeItem {

	private process: any;

	constructor(process: any) {
		super(`${process.Process} (${process.ProcessId})`, vscode.TreeItemCollapsibleState.Collapsed);
		this.process = process;
	}

	async getChildren(): Promise<vscode.TreeItem[]> {
		const runspaces = await Container.PowerShellService.GetRunspaces(this.process.ProcessId);
		return runspaces.filter(m => m.Name !== "RemoteHost").map(m => new Runspace(m, this.process.ProcessId));
	}
}

class Runspace extends vscode.TreeItem {

	public ProcessId: number;
	public RunspaceId: number;

	constructor(runspace: any, processId: number) {
		super(`${runspace.Name} (${runspace.Id})`, vscode.TreeItemCollapsibleState.None);
		this.ProcessId = processId;
		this.RunspaceId = runspace.Id;
	}

	contextValue = "runspace";
}
