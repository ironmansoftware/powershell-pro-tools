import * as vscode from 'vscode';
import { Container } from '../container';
import { RefactorRequest, TextEditorState, TextEditType } from '../types';

export class PowerShellRenameProvider implements vscode.RenameProvider {
    public async provideRenameEdits(
        document: vscode.TextDocument, position: vscode.Position,
        newName: string, token: vscode.CancellationToken):
        Promise<vscode.WorkspaceEdit> {

        const request = this.createRefactorRequest(position, document);

        const workspaceFolder = vscode.workspace.getWorkspaceFolder(document.uri);

        var result = await Container.PowerShellService.RenameSymbol(newName, workspaceFolder.uri.fsPath, request);

        let we = new vscode.WorkspaceEdit();

        for (var edit of result) {
            let uri = vscode.Uri.file(edit.FileName);

            switch (edit.Type) {
                case TextEditType.replace:
                    var range = new vscode.Range(new vscode.Position(edit.Start.Line, edit.Start.Character), new vscode.Position(edit.End.Line, edit.End.Character))
                    we.replace(uri, range, edit.Content);
                    break;
            }

            if (edit.Cursor) {
                var position = new vscode.Position(edit.Cursor.Line, edit.Cursor.Character);
                vscode.window.activeTextEditor.selection = new vscode.Selection(position, position);
            }
        }

        return we;
    }

    createRefactorRequest(position: vscode.Position, document: vscode.TextDocument): RefactorRequest {
        var request = new RefactorRequest();
        request.editorState = new TextEditorState();
        request.editorState.content = document.getText()
        request.editorState.fileName = document.fileName
        request.editorState.documentEnd = {
            Character: document.lineAt(document.lineCount - 1).range.end.character,
            Line: document.lineCount - 1
        }

        var startColumn = position.character;
        var startLine = position.line;

        do {
            var line = document.lineAt(startLine);
            if (!line.isEmptyOrWhitespace && line.text[line.firstNonWhitespaceCharacterIndex] != '#') {
                break;
            }
            startColumn = 0;
            startLine++;
        } while (startLine < document.lineCount);

        request.editorState.selectionStart = {
            Line: startLine,
            Character: startColumn
        };

        var endColumn = position.character;
        var endLine = position.line;

        do {
            var line = document.lineAt(endLine);
            if (!line.isEmptyOrWhitespace) {
                break;
            }
            endColumn = 0;
            endLine--;
        } while (endLine > 0);

        request.editorState.selectionEnd = {
            Line: endLine,
            Character: endColumn
        };

        return request;
    }
}
