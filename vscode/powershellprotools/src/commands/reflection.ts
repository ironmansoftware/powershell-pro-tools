'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { ICommand } from './command';
import { TypeNode } from '../treeView/reflectionTreeView';
var fs = require('fs');

export class ReflectionCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.DecompileType());
        context.subscriptions.push(this.LoadAssembly());
    }

    DecompileType() {
        return vscode.commands.registerCommand('poshProTools.decompileType', async (node: TypeNode) => {
            if (!Container.IsInitialized()) return;

            const source = await Container.PowerShellService.DecompileType(node.type.AssemblyName, node.type.FullTypeName);
            if (!source || source === '') {
                vscode.window.showErrorMessage("Failed to decompile type.");
                return
            }

            fs.readFile(source, 'utf8', async (err, data) => {
                const doc = await vscode.workspace.openTextDocument({
                    language: "csharp",
                    content: data
                });

                vscode.window.showTextDocument(doc);
            });
        })
    }

    LoadAssembly() {
        return vscode.commands.registerCommand('poshProTools.loadAssembly', async () => {
            if (!Container.IsInitialized()) return;

            const result = await vscode.window.showOpenDialog({
                canSelectFiles: true,
                canSelectFolders: false,
                canSelectMany: true,
                filters: {
                    'Binaries': ['exe', 'dll']
                }
            });

            result.forEach(async x => {
                Container.PowerShellService.LoadAssembly(x.fsPath);
            });

            await vscode.commands.executeCommand('reflectionView.refresh')
        })
    }
}