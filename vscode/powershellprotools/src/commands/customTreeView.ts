'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { ICommand } from './command';
import { CustomTreeItemNode, CustomTreeItemParentNode } from '../treeView/customTreeView';

export class CustomTreeViewCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.AttachRunspace());
    }

    AttachRunspace() {
        return vscode.commands.registerCommand('customView.invoke', async (node: CustomTreeItemNode | CustomTreeItemParentNode) => {
            if (!Container.IsInitialized()) return;

            await Container.PowerShellService.InvokeChild(node.treeViewId, node.path);
        })
    }
}