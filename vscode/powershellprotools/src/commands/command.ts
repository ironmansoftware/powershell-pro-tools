import * as vscode from 'vscode';

export interface ICommand {
    register(context : vscode.ExtensionContext);
}