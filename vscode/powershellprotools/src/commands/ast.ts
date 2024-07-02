'use strict';
import * as vscode from 'vscode';
import { AstNode } from '../treeView/astTreeView';
import { ICommand } from './command';

export class AstCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.highlightAst());
        context.subscriptions.push(this.clearAstSelect());
    }

    private decoration: vscode.TextEditorDecorationType;

    selectRegion(startOffset: number, endOffset: number) {

        if (this.decoration) {
            this.decoration.dispose();
        }

        var editor = vscode.window.activeTextEditor;

        var decorationType = vscode.window.createTextEditorDecorationType({
            backgroundColor: new vscode.ThemeColor("editor.selectionBackground"),
            rangeBehavior: vscode.DecorationRangeBehavior.OpenOpen
        });

        var startPos = editor.document.positionAt(startOffset);
        var endPos = editor.document.positionAt(endOffset);

        editor.setDecorations(decorationType, [new vscode.Range(startPos, endPos)]);

        this.decoration = decorationType;
    }

    clearAstSelect() {
        return vscode.commands.registerCommand('poshProTools.clearAstSelection', async () => {
            if (this.decoration) {
                this.decoration.dispose();
            }
        });
    }

    highlightAst() {
        return vscode.commands.registerCommand('poshProTools.selectAst', async (ast: AstNode) => {
            const editor = vscode.window.activeTextEditor;

            if (!editor) {
                vscode.window.showErrorMessage("No editor is open to highlight this AST.");
                return;
            }

            if (editor.document.fileName !== ast.parentAst.File) {
                vscode.window.showErrorMessage(`This AST does not match the active file. AST is from ${ast.parentAst.File} `);
                return;
            }

            this.selectRegion(ast.StartOffset, ast.EndOffset);
        });
    }
}
