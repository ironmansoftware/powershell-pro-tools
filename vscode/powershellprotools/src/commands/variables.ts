'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { VariableDetails } from '../treeView/variableTreeView';
import { ICommand } from './command';

export class VariableCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.insertVariable());
    }

    insertVariable() {
        return vscode.commands.registerCommand('poshProTools.insertVariable', async (variable: VariableDetails) => {
            var activeEditor = vscode.window.activeTextEditor;
            if (!activeEditor) return;

            activeEditor.edit(x => {
                x.insert(activeEditor.selection.active, variable.variableDetails.Path)
            })
        });
    }

    viewType() {
        return vscode.commands.registerCommand('poshProTools.viewType', async (variable: VariableDetails) => {
            if (!Container.IsInitialized()) return;

            const type = Container.PowerShellService.FindType(variable.variableDetails.type);
            if (!type) {
                vscode.window.showWarningMessage(`Failed to find type ${variable.variableDetails.type}`);
                return;
            }


        });
    }
}
