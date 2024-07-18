'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { ICommand } from './command';
import { RefactorInfo, RefactoringProperty, RefactorProperty, RefactorRequest, RefactorTextEdit, RefactorType, TextEditorState, TextEditType } from '../types';

export class RefactoringCommands implements ICommand, vscode.CodeActionProvider {
    async provideCodeActions(document: vscode.TextDocument, range: vscode.Range | vscode.Selection, context: vscode.CodeActionContext, token: vscode.CancellationToken): Promise<(vscode.CodeAction | vscode.Command)[]> {
        if (!Container.IsInitialized(false)) return;

        var request = this.getRefactorRequest();

        var validRefactors = await Container.PowerShellService.GetValidRefactors(request);
        var codeActions = new Array<vscode.CodeAction>();

        for (var r of validRefactors) {
            request.type = r.Type;

            if (r.Type == RefactorType.ExtractVariable) {
                const newName = `newvariable${new Date().getMilliseconds().toString()}`;
                request.properties = [
                    { type: RefactorProperty.name, value: newName }
                ]
            }

            let we = await this.requestRefactor(request, false);
            if (we.entries().length == 0) continue;

            var action = new vscode.CodeAction(r.Name, vscode.CodeActionKind.RefactorExtract);
            action.edit = we;
            action.command = {
                title: 'Rename',
                command: 'editor.action.rename'
            }
            codeActions.push(action);
        }

        return codeActions;
    }

    // resolveCodeAction?(codeAction: vscode.CodeAction, token: vscode.CancellationToken): vscode.ProviderResult<vscode.CodeAction> {
    //     throw new Error('Method not implemented.');
    // }

    register(context: vscode.ExtensionContext) {

        context.subscriptions.push(
            vscode.languages.registerCodeActionsProvider('powershell', this, {
                providedCodeActionKinds: [vscode.CodeActionKind.Refactor, vscode.CodeActionKind.RefactorExtract]
            })
        );

        context.subscriptions.push(this.Refactor());
        context.subscriptions.push(this.MoveLeft());
        context.subscriptions.push(this.MoveRight());
    }

    async Move(direction: string) {
        var request = this.getRefactorRequest();
        request.type = RefactorType.reorder
        request.properties = [{
            type: RefactorProperty.name,
            value: direction
        }]

        await this.requestRefactor(request);
    }

    MoveLeft() {
        return vscode.commands.registerCommand('poshProTools.refactorMoveLeft', async (node) => {
            if (!Container.IsInitialized()) return;

            this.Move("Left");
        })
    }

    MoveRight() {
        return vscode.commands.registerCommand('poshProTools.refactorMoveRight', async (node) => {
            if (!Container.IsInitialized()) return;

            this.Move("Right");
        })
    }

    Refactor() {
        return vscode.commands.registerCommand('poshProTools.refactor', async (node) => {
            if (!Container.IsInitialized()) return;

            var request = this.getRefactorRequest();

            var validRefactors = await Container.PowerShellService.GetValidRefactors(request);
            var refactorName = await vscode.window.showQuickPick(validRefactors.map(m => m.Name));
            var refactor = validRefactors.find(m => m.Name === refactorName);

            if (!refactor) return;

            switch (refactor.Type) {
                case RefactorType.ExtractVariable:
                    await this.extractVariable(request);
                    break;
                case RefactorType.extractFile:
                    await this.extractFile(request);
                    break;
                case RefactorType.extractFunction:
                    await this.extractFunction(request);
                    break;
                default:
                    request.type = refactor.Type;
                    await this.requestRefactor(request);
                    break;
            }
        })
    }

    createRefactorRequest(selection: vscode.Selection, document: vscode.TextDocument): RefactorRequest {
        var request = new RefactorRequest();
        request.editorState = new TextEditorState();
        request.editorState.content = document.getText()
        request.editorState.fileName = document.fileName
        request.editorState.uri = document.uri.toString()
        request.editorState.documentEnd = {
            Character: document.lineAt(document.lineCount - 1).range.end.character,
            Line: document.lineCount - 1
        }

        var startColumn = selection.start.character;
        var startLine = selection.start.line;

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

        var endColumn = selection.end.character;
        var endLine = selection.end.line;

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

    getRefactorRequest(): RefactorRequest {
        var selection = vscode.window.activeTextEditor.selection;
        return this.createRefactorRequest(selection, vscode.window.activeTextEditor.document);
    }

    async extractVariable(request: RefactorRequest) {
        const name = await vscode.window.showInputBox({
            prompt: "Enter variable name"
        });

        request.properties = [{ type: RefactorProperty.name, value: name }];
        request.type = RefactorType.ExtractVariable

        this.requestRefactor(request);
    }

    async extractFunction(request: RefactorRequest) {
        const name = await vscode.window.showInputBox({
            prompt: "Enter function name"
        });

        request.properties = [{ type: RefactorProperty.name, value: name }];
        request.type = RefactorType.extractFunction

        this.requestRefactor(request);
    }

    async extractFile(request: RefactorRequest) {
        const fileName = await vscode.window.showInputBox({
            prompt: "Enter file name"
        });

        request.properties = [{ type: RefactorProperty.fileName, value: fileName }];
        request.type = RefactorType.extractFile

        this.requestRefactor(request);
    }

    async requestRefactor(request: RefactorRequest, apply: boolean = true): Promise<vscode.WorkspaceEdit> {
        var result = await Container.PowerShellService.Refactor(request);

        var filesToOpen = new Array<vscode.Uri>();

        let we = new vscode.WorkspaceEdit();

        for (var edit of result) {

            let uri = edit.Uri ? vscode.Uri.parse(edit.Uri) : vscode.Uri.file(edit.Uri);

            switch (edit.Type) {
                case TextEditType.replace:
                    var range = new vscode.Range(new vscode.Position(edit.Start.Line, edit.Start.Character), new vscode.Position(edit.End.Line, edit.End.Character))
                    we.replace(uri, range, edit.Content);
                    break;
                case TextEditType.insert:
                    var position = new vscode.Position(edit.Start.Line, edit.Start.Character);
                    we.insert(uri, position, edit.Content);
                    break;
                case TextEditType.newFile:
                    we.createFile(uri, {
                        overwrite: false,
                        ignoreIfExists: true
                    });

                    we.insert(uri, new vscode.Position(1, 1), edit.Content);
                    break;
            }

            filesToOpen.push(uri);

            if (edit.Cursor) {
                var position = new vscode.Position(edit.Cursor.Line, edit.Cursor.Character);
                vscode.window.activeTextEditor.selection = new vscode.Selection(position, position);
            }
        }

        if (apply && await vscode.workspace.applyEdit(we)) {
            filesToOpen.forEach(uri => vscode.workspace.openTextDocument(uri));
        }

        return we;
    }
}