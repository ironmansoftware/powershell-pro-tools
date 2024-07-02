'use strict';
import * as vscode from 'vscode';
import { ICommand } from './command';
import { HistoryTreeItem } from '../treeView/historyTreeView';

export class HistoryTreeViewCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.Command());
    }

    Command() {
        return vscode.commands.registerCommand('historyView.insert', async (node: HistoryTreeItem) => {
            var terminal = vscode.window.terminals.find(x => x.name === "PowerShell Extension");
            if (terminal != null) {
                terminal.sendText(node.history, false);
            }
        })
    }
}