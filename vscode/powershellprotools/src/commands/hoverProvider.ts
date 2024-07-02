import * as vscode from 'vscode';
import { Container } from '../container';
import { TextEditorState } from '../types';

export class PowerShellHoverProvider {
    constructor() {
        vscode.languages.registerHoverProvider('powershell', {
            async provideHover(document, position, token) {
                var editorState = new TextEditorState();
                editorState.content = document.getText()
                editorState.fileName = document.fileName
                editorState.documentEnd = {
                    Character: document.lineAt(document.lineCount - 1).range.end.character,
                    Line: document.lineCount - 1
                }

                var startColumn = position.character;
                var startLine = position.line;

                do {
                    var line = document.lineAt(startLine);
                    if (!line.isEmptyOrWhitespace)
                    {
                        break;
                    }
                    startColumn = 0;
                    startLine++;
                } while (startLine < document.lineCount);

                editorState.selectionStart = {
                    Line: startLine,
                    Character: startColumn
                };

                editorState.selectionEnd = editorState.selectionStart;

                let hover = await Container.PowerShellService.GetHover(editorState);
                if (hover == null) return null;
                let hovers = hover.Markdown.split('/n');

                var ms = new vscode.MarkdownString('', true);
                ms.isTrusted = true;
                for(let textHover of hovers)
                {
                    ms.appendMarkdown(textHover);
                    ms.appendText("\r\n");
                }
                return new vscode.Hover(ms);
            }
        });
    }
}