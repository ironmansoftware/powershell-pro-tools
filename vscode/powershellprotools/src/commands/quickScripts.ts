'use strict';
import * as vscode from 'vscode';
import { ICommand } from './command';
import { Container } from '../container';
import { QuickScript, QuickScriptViewProvider } from '../treeView/quickScriptView';

export class QuickScriptCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.addQuickScript());
        context.subscriptions.push(this.openQuickScript());
        context.subscriptions.push(this.removeQuickScript());
    }

    addQuickScript() {
        return vscode.commands.registerCommand('poshProTools.addQuickScript', async () => {
            if (!Container.IsInitialized()) return;

            const editor = vscode.window.activeTextEditor;

            if (!editor) {
                vscode.window.showErrorMessage("Please select a file to add as a Quick Script.");
                return;
            }

            var name = await vscode.window.showInputBox({
                prompt: "Please enter the name for this Quick Script"
            });

            if (!name || name === "") {
                return;
            }

            await Container.QuickScriptService.setScript(name, editor.document.fileName);

            vscode.window.showInformationMessage(`${name} was added to Quick Scripts`);

            QuickScriptViewProvider.Instance.refresh();
        });
    }

    removeQuickScript() {
        return vscode.commands.registerCommand('poshProTools.removeQuickScript', async (node: QuickScript) => {
            if (!Container.IsInitialized()) return;

            var name = "";
            if (node) {
                name = node.name;
            } else {
                name = await vscode.window.showInputBox({
                    prompt: "Enter the name of the Quick Script to remove"
                })
            }

            if (!name || name === "") return;

            await Container.QuickScriptService.removeScript(name);

            QuickScriptViewProvider.Instance.refresh();

            vscode.window.showInformationMessage(`${name} was removed from Quick Scripts`);
        });
    }

    openQuickScript() {
        return vscode.commands.registerCommand('poshProTools.openQuickScript', async (node: QuickScript) => {
            if (!Container.IsInitialized()) return;

            var name = '"'
            if (node) {
                name = node.name;
            }
            else {
                name = await vscode.window.showInputBox({
                    prompt: "Enter Quick Script name"
                })
            }

            if (!name || name === "") return;

            var script = Container.QuickScriptService.getScript(name);

            if (!script) {
                vscode.window.showErrorMessage(`Quick script ${name} not found`);
                return
            }

            var document = await vscode.workspace.openTextDocument(script.File);
            vscode.window.showTextDocument(document);
        });
    }
}
