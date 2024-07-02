'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { ICommand } from './command';

export class ProviderCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.insertProviderPath());
        context.subscriptions.push(this.viewItemProperties());
        context.subscriptions.push(this.viewChildItems());
    }

    insertProviderPath() {
        return vscode.commands.registerCommand('poshProTools.insertProviderPath', async (item: any) => {
            var activeEditor = vscode.window.activeTextEditor;
            if (!activeEditor) return;

            activeEditor.edit(x => {
                x.insert(activeEditor.selection.active, item.path)
            })
        });
    }

    viewItemProperties() {
        return vscode.commands.registerCommand('poshProTools.viewItemProperties', async (item: any) => {
            if (!Container.IsInitialized()) return;

            Container.PowerShellService.GetItemProperty(item.path);
        });
    }


    viewChildItems() {
        return vscode.commands.registerCommand('poshProTools.viewItems', async (item: any) => {
            if (!Container.IsInitialized()) return;

            Container.PowerShellService.ViewItems(item.path);
        });
    }
}
