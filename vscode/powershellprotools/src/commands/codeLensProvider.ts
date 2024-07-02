import * as vscode from 'vscode';
import { Container } from '../container';

export class PowerShellCodeLensProvider implements vscode.CodeLensProvider {
	async provideCodeLenses(document: vscode.TextDocument): Promise<vscode.CodeLens[]> {
		let wsUri = vscode.workspace.getWorkspaceFolder(document.uri);
		let funcDefs = await Container.PowerShellService.GetFunctionDefinitions(document.uri.fsPath, wsUri.uri.fsPath);

		let codeLenses = new Array<vscode.CodeLens>();
		for(let def of funcDefs) {
			let funcPosition = new vscode.Range(def.Line, def.Character, def.Line, def.Character);
			let locations = def.References.map(m => new vscode.Location(vscode.Uri.parse(m.FileName), new vscode.Position(m.Line, m.Character)));

			let command = {
				title: def.References.length === 1 ? '1 reference????' : `${def.References.length} references???`,
				command: 'editor.action.showReferences',
				arguments: [vscode.Uri.file(def.FileName), new vscode.Position(def.Line, def.Character), locations]
			};

			codeLenses.push(new vscode.CodeLens(funcPosition, command));
		}
		
		return codeLenses;
	}
}