'use strict';
import * as vscode from 'vscode';
import { Container } from '../container';
import { ICommand } from './command';
import { SessionTreeItem } from '../treeView/sessionTreeView';
import { Session } from '../types';

export class SessionCommands implements ICommand {

    private _pinnedTextEditors: Map<vscode.Uri, Session>;

    private _runspacePushed: boolean;
    private _statusBarItem: vscode.StatusBarItem;

    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.EnterSession());
        context.subscriptions.push(this.RemoveSession());
        context.subscriptions.push(this.ExitSession());
        context.subscriptions.push(this.PinSession());
        context.subscriptions.push(this.UnpinSession());

        this._pinnedTextEditors = new Map<vscode.Uri, Session>();
    }

    EnterSession() {
        return vscode.commands.registerCommand('poshProTools.enterSession', async (node: SessionTreeItem) => {
            if (!Container.IsInitialized()) return;

            this._runspacePushed = true;

            Container.PowerShellService.EnterSession(node.session.Id);
        })
    }

    RemoveSession() {
        return vscode.commands.registerCommand('poshProTools.removeSession', async (node: SessionTreeItem) => {
            if (!Container.IsInitialized()) return;

            Container.PowerShellService.RemoveSession(node.session.Id);

            await vscode.commands.executeCommand('sessionsView.refresh')
        })
    }

    ExitSession() {
        return vscode.commands.registerCommand('poshProTools.exitSession', async () => {
            if (!Container.IsInitialized()) return;

            this._runspacePushed = false;

            Container.PowerShellService.ExitSession();
        })
    }

    UnpinSession() {
        return vscode.commands.registerCommand('poshProTools.unpinSession', async () => {
            this._pinnedTextEditors.delete(vscode.window.activeTextEditor.document.uri);
            this._statusBarItem.hide();
        });
    }

    PinSession() {

        this._statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 1);

        return vscode.commands.registerCommand('poshProTools.pinSession', async () => {
            if (!Container.IsInitialized()) return;

            const sessions = await Container.PowerShellService.GetSessions();

            if (sessions.length === 0 || (sessions.length === 1 && sessions[0] == null)) {
                vscode.window.showWarningMessage("No sessions to pin.");
                return;
            }

            const result = await vscode.window.showQuickPick(sessions.map(x => x.Name));
            const session = sessions.find(x => x.Name === result);

            this._pinnedTextEditors.set(vscode.window.activeTextEditor.document.uri, session);

            if (this._runspacePushed) {
                this._runspacePushed = false;
                await Container.PowerShellService.ExitSession();
            }

            this._runspacePushed = true;
            await Container.PowerShellService.EnterSession(session.Id);
            this._statusBarItem.text = `$(pinned) Document pinned to ${session.Name}`;
            this._statusBarItem.show();

            vscode.window.onDidChangeActiveTextEditor(async (editor) => {
                if (this._runspacePushed) {
                    this._runspacePushed = false;
                    await Container.PowerShellService.ExitSession();
                    this._statusBarItem.hide();
                }

                if (this._pinnedTextEditors.has(editor.document.uri)) {
                    const session = this._pinnedTextEditors.get(editor.document.uri);
                    await Container.PowerShellService.EnterSession(session.Id);

                    this._statusBarItem.text = `$(pinned) Document pinned to ${session.Name}`;
                    this._statusBarItem.show();

                    this._runspacePushed = true;
                }
            });
        })
    }
}