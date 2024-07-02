import * as vscode from 'vscode';
import { Container } from '../container';
import { ParentTreeItem, TreeViewProvider } from './treeViewProvider';
import * as path from 'path';

export class AstTreeViewProvider extends TreeViewProvider {
	requiresLicense(): boolean {
		return false;
	}
	getRefreshCommand(): string {
		return "astView.refresh";
	}

	editor: vscode.TextEditor;

	constructor() {
		super();
		this.editor = vscode.window.activeTextEditor;
	}

	async getNodes(): Promise<vscode.TreeItem[]> {
		return [new Ast()];
	}
}


export class Ast extends ParentTreeItem {

	constructor() {
		super("AST", vscode.TreeItemCollapsibleState.Collapsed);
		this.Editor = vscode.window.activeTextEditor;
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
		if (this.Editor && this.Editor.document.languageId === "powershell") {
			return path.basename(this.Editor.document.fileName);
		}
		return "";
	}

	get _tooltip(): string {
		if (this.Editor && this.Editor.document.languageId === "powershell") {
			return this.Editor.document.fileName;
		}
		return "";
	}

	public File: string;
	private Editor: vscode.TextEditor;

	async getChildren(): Promise<vscode.TreeItem[]> {
		if (this.Editor == null) {
			return [new vscode.TreeItem("No file open.")]
		}

		if (this.Editor.document.languageId !== "powershell") {
			return [new vscode.TreeItem("File is not PowerShell.")]
		}

		this.File = this.Editor.document.fileName;

		var ast = await Container.PowerShellService.GetAst(this.Editor.document.fileName);

		return [new AstNode(ast, this)];
	}

	contextValue = "ast";
}

export class AstNode extends ParentTreeItem {
	private ast: any;
	public parentAst: Ast;

	get StartOffset(): number {
		return this.ast.StartOffset;
	}

	get EndOffset(): number {
		return this.ast.EndOffset;
	}

	constructor(ast: any, parentAst: Ast) {
		super(ast.AstType, vscode.TreeItemCollapsibleState.Collapsed);
		this.ast = ast;
		this.parentAst = parentAst;
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
		return `[${this.ast.StartOffset}-${this.ast.EndOffset}]`
	}

	get _tooltip(): string {
		return this.ast.AstContent;
	}

	async getChildren(): Promise<vscode.TreeItem[]> {
		var children = await Container.PowerShellService.GetAstByHashcode(this.ast.HashCode);

		if (!Array.isArray(children)) {
			children = [children]
		}

		return children.map(x => new AstNode(x, this.parentAst));
	}

	contextValue = 'astNode';
}
