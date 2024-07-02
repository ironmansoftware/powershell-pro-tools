'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';

var decorationTypes = [];

export function clearProfiling() {
    return vscode.commands.registerCommand('powershell.clearProfiling', () => {
        decorationTypes.forEach(x => x.dispose());
        decorationTypes = [];
    });
}


export function profile() {
    return vscode.commands.registerCommand('powershell.profileScript', async () => {
        if (!Container.IsInitialized()) return;

        vscode.window.withProgress({
            location: vscode.ProgressLocation.Notification,
            title: "Executing PowerShell Profiler"
        }, async () => {
            decorationTypes.forEach(x => x.dispose());
            decorationTypes = [];

            const editor = vscode.window.activeTextEditor;

            var profileResult = await Container.PowerShellService.MeasureScript(editor.document.fileName);
            var timingForFile = profileResult.Timings.filter(x => x.SequencePoint.FromAst && !x.SequencePoint.Root);

            timingForFile.forEach(element => {
                var percentTime = (element.DurationMilliseconds / profileResult.TotalDuration) * 100;
                var decorationType = vscode.window.createTextEditorDecorationType({
                    after: {
                        color: '#939393',
                        fontWeight: '100',
                        margin: '0 0 0 1em',
                        textDecoration: 'none',
                        contentText: `${percentTime.toFixed(2)}% (${element.CallCount} calls, ${element.DurationMilliseconds}ms)`
                    },
                    rangeBehavior: vscode.DecorationRangeBehavior.OpenOpen
                });

                var startPos = editor.document.positionAt(element.SequencePoint.StartOffset);
                var endPos = editor.document.positionAt(element.SequencePoint.EndOffset);

                editor.setDecorations(decorationType, [new vscode.Range(startPos, endPos)]);

                decorationTypes.push(decorationType);
            });
        })


    });
}