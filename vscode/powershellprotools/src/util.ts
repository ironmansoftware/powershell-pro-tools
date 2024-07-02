'use strict';
import * as vscode from 'vscode';

const NET_VERSION = 'NET_VERSION';

export function documentRange(): vscode.Range {
    const editor = vscode.window.activeTextEditor;

    var firstLine = editor.document.lineAt(0);
    var lastLine = editor.document.lineAt(editor.document.lineCount - 1);
    return new vscode.Range(0,
        firstLine.range.start.character,
        editor.document.lineCount - 1,
        lastLine.range.end.character);
}

export function updateNetVersion(storage: vscode.Memento, state: number) {
    storage.update(NET_VERSION, state);
}

export function getNetVersion(storage: vscode.Memento): number {
    return storage.get(NET_VERSION, 0);
}