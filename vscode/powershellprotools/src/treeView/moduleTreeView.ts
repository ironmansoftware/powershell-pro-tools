import * as vscode from 'vscode';
import { Container } from '../container';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';
import * as path from 'path';
import Versioning from '../utilities/versioning';
import { load } from '../settings';

export class ModuleViewProvider extends TreeViewProvider {
	getRefreshCommand(): string {
		return "moduleView.refresh";
	}

	requiresLicense(): boolean {
		return true;
	}

	private highestVersion: any[] = [];

	public static Instance: ModuleViewProvider;

	constructor() {
		super();
		ModuleViewProvider.Instance = this;
		vscode.commands.registerCommand('moduleExplorer.updateModule', this.updateModule);
		vscode.commands.registerCommand('moduleExplorer.uninstallModule', this.uninstallModule);
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		var modules = await Container.PowerShellService.GetModules();
		return modules.map(m => {
			return new Module(m.Name, m.Versions.map(x => new ModuleVersion(x.Version, x.ModuleBase, m.FromRepository)), m.FromRepository);
		})
	}


	async updateModule(item: Module) {
		await item.setUpdating(true);
		await Container.PowerShellService.SendTerminalCommand(`Update-Module -Name '${item.label}' -RequiredVersion '${item.higherVersion}'`);
		await item.setUpdating(false);
	}

	async uninstallModule(item: ModuleVersion) {
		var _this = this;
		vscode.window.setStatusBarMessage(`Uninstalling module ${item.label} (${item.version})...`);
		await Container.PowerShellService.UninstallModule(item.label.toString(), item.version);
		vscode.window.showInformationMessage(`Uninstalled module ${item.label} (${item.version})`);
		vscode.window.setStatusBarMessage('');
		_this.refresh();
	}

	cacheHighestVersion(moduleVersion: any) {
		this.highestVersion.push(moduleVersion);
	}

	findHighestVersion(module: Module) {
		var highest = this.highestVersion.find(m => m.Name === module.label);
		if (highest != null) {
			return highest.version;
		}
		return null;
	}

}


export class Module extends ParentTreeItem {
	getChildren(): Thenable<vscode.TreeItem[]> {
		return Promise.resolve(this.versions);
	}

	foundHigherVersion: boolean = false;
	public higherVersion: string;

	public updating: boolean = false;

	constructor(
		public readonly label: string,
		public readonly versions: ModuleVersion[],
		public readonly fromGallery: boolean
	) {
		super(label, vscode.TreeItemCollapsibleState.Collapsed);

		this.iconPath = new vscode.ThemeIcon('package');

		var settings = load();
		if (settings.checkForModuleUpdates) {
			if (this.fromGallery) {
				vscode.window.setStatusBarMessage(`Checking for new version of module ${label}...`);
				var highestVersion = ModuleViewProvider.Instance.findHighestVersion(this);

				if (highestVersion == null) {
					Container.PowerShellService.FindModuleVersion(this.label).then(version => {
						this.checkVersion(version);
					})
				} else {
					this.checkVersion(highestVersion);
				}
				vscode.window.setStatusBarMessage('');
			}
		}
	}

	async setUpdating(updating: boolean) {
		if (updating) {
			this.updating = updating;
			this.contextValue = '';
			ModuleViewProvider.Instance.refresh(this);
		} else {
			var x = await Container.PowerShellService.GetModulePath(this.label, this.higherVersion);
			this.updating = updating;
			this.contextValue = '';
			this.foundHigherVersion = false;
			this.versions.push(new ModuleVersion(this.higherVersion, x, this.fromGallery))
			this.higherVersion = '';
			ModuleViewProvider.Instance.refresh(this);
		}
	}

	checkVersion(version) {
		var versioning = new Versioning();

		var uptodate = false;
		this.versions.forEach(x => {
			if (versioning.compare(x.version, version) >= 0) {
				uptodate = true;
			}
		})

		ModuleViewProvider.Instance.cacheHighestVersion({ Name: this.label, Version: version });

		if (!uptodate) {
			this.contextValue = 'update'
			this.higherVersion = version;
			this.foundHigherVersion = true;
			ModuleViewProvider.Instance.refresh(this);
		}
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
		return `${this.label}`;
	}

	get _description(): string {

		if (this.updating) {
			return `Updating to ${this.higherVersion}...`;
		}

		var updateAvailable = "";
		if (this.foundHigherVersion) {
			updateAvailable = `(Update Available - ${this.higherVersion})`
		}

		return updateAvailable;
	}
}

class ModuleVersion extends vscode.TreeItem {
	constructor(
		public readonly version: string,
		public readonly location: string,
		public readonly fromRepository: boolean
	) {
		super(version, vscode.TreeItemCollapsibleState.None);

		if (fromRepository) {
			this.contextValue = 'uninstall'
		}
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
		return this.location;
	}

	get _description(): string {
		return this.location;
	}
}

