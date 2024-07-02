import * as vscode from 'vscode';
import { Container } from '../container';
import { PSField, PSMethod, PSNamespace, PSProperty, PSType } from '../types';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';

export class ReflectionViewProvider extends TreeViewProvider {
	getRefreshCommand(): string {
		return "reflectionView.refresh";
	}

	requiresLicense(): boolean {
		return false;
	}

	static Instance: ReflectionViewProvider;

	constructor() {
		super();
		ReflectionViewProvider.Instance = this;
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		const assemblies = await Container.PowerShellService.GetAssemblies()
		return assemblies.map(x => new AssemblyNode(x.Name, x.Version, x.Path));
	}
}

export class AssemblyNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		return (await Container.PowerShellService.GetNamespaces(this.name, "")).map(m => new NamespaceNode(m, this.name));
	}
	constructor(
		public readonly name: string,
		public readonly version: string,
		public readonly path: string
	) {
		super(name, vscode.TreeItemCollapsibleState.Collapsed);
	}

	public getTreeItem(): vscode.TreeItem {
		return {
			tooltip: this._tooltip,
			contextValue: this.contextValue,
			collapsibleState: this.collapsibleState,
			label: this.label,
			description: this._description,
			iconPath: new vscode.ThemeIcon('package')
		};
	}

	get _description(): string {
		return this.version;
	}

	get _tooltip(): string {
		return this.path;
	}

	contextValue = "assembly";
}

export class NamespaceNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		const nodes = new Array<vscode.TreeItem>();

		var namespaces = (await Container.PowerShellService.GetNamespaces(this.assembly, this.namespace.FullName)).map(m => new NamespaceNode(m, this.assembly));

		var types = await Container.PowerShellService.GetTypes(this.assembly, this.namespace.FullName);
		var typeNodes = types.map(x => new TypeNode(x));

		return nodes.concat(namespaces).concat(typeNodes);
	}
	constructor(
		public readonly namespace: PSNamespace,
		public readonly assembly: string
	) {
		super(namespace.Name, vscode.TreeItemCollapsibleState.Collapsed);

		this.iconPath = new vscode.ThemeIcon('symbol-namespace');
	}

	contextValue = "namespace";
}

export class TypeNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		return [
			new FieldsNode(this.type),
			new PropertiesNode(this.type),
			new MethodsNode(this.type)
		];
	}
	constructor(
		public readonly type: PSType
	) {
		super(type.Name, vscode.TreeItemCollapsibleState.Collapsed);

		this.iconPath = new vscode.ThemeIcon('symbol-class');
	}

	contextValue = "type";
}

export class PropertiesNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		const properties = await Container.PowerShellService.GetProperties(this.type.AssemblyName, this.type.Name);
		return properties.map(m => new PropertyNode(m));
	}
	constructor(
		public readonly type: PSType
	) {
		super('Properties', vscode.TreeItemCollapsibleState.Collapsed);

		this.iconPath = new vscode.ThemeIcon('symbol-property');
	}
}

export class PropertyNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		return [];
	}
	constructor(
		public readonly property: PSProperty
	) {
		super(property.Name, vscode.TreeItemCollapsibleState.None);

		this.iconPath = new vscode.ThemeIcon('symbol-property');
		this.description = property.PropertyType;
	}

	contextValue = "property";
}

export class FieldsNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		const properties = await Container.PowerShellService.GetFields(this.type.AssemblyName, this.type.Name);
		return properties.map(m => new FieldNode(m));
	}
	constructor(
		public readonly type: PSType
	) {
		super('Fields', vscode.TreeItemCollapsibleState.Collapsed);

		this.iconPath = new vscode.ThemeIcon('symbol-field');
	}
}

export class FieldNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		return [];
	}
	constructor(
		public readonly field: PSField
	) {
		super(field.Name, vscode.TreeItemCollapsibleState.None);

		this.iconPath = new vscode.ThemeIcon('symbol-field');
		this.description = field.FieldType;
	}

	contextValue = "field";
}


export class MethodsNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		const properties = await Container.PowerShellService.GetMethods(this.type.AssemblyName, this.type.Name);
		return properties.map(m => new MethodNode(m));
	}
	constructor(
		public readonly type: PSType
	) {
		super('Methods', vscode.TreeItemCollapsibleState.Collapsed);

		this.iconPath = new vscode.ThemeIcon('symbol-method');
	}
}

export class MethodNode extends ParentTreeItem {
	async getChildren(): Promise<vscode.TreeItem[]> {
		return [];
	}
	constructor(
		public readonly method: PSMethod
	) {
		super(method.Display, vscode.TreeItemCollapsibleState.None);

		this.iconPath = new vscode.ThemeIcon('symbol-method');
	}

	contextValue = "method";
}
