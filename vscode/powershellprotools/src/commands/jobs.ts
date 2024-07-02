'use strict';
import * as vscode from 'vscode';
import { ICommand } from './command';
import { JobTreeItem } from '../treeView/jobTreeView';
import { Container } from '../container';

export class JobTreeViewCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.Stop());
        context.subscriptions.push(this.Debug());
        context.subscriptions.push(this.Receive());
        context.subscriptions.push(this.Remove());
    }

    Stop() {
        return vscode.commands.registerCommand('jobView.stop', async (node: JobTreeItem) => {
            if (!Container.IsInitialized()) return;
            await Container.PowerShellService.StopJob(node.job.Id);
            await vscode.commands.executeCommand('jobView.refresh');
        })
    }

    Debug() {
        return vscode.commands.registerCommand('jobView.debug', async (node: JobTreeItem) => {
            if (!Container.IsInitialized()) return;
            await Container.PowerShellService.DebugJob(node.job.Id);
        })
    }

    Receive() {
        return vscode.commands.registerCommand('jobView.receive', async (node: JobTreeItem) => {
            if (!Container.IsInitialized()) return;
            await Container.PowerShellService.ReceiveJob(node.job.Id);
        })
    }

    Remove() {
        return vscode.commands.registerCommand('jobView.remove', async (node: JobTreeItem) => {
            if (!Container.IsInitialized()) return;
            await Container.PowerShellService.RemoveJob(node.job.Id);
            await vscode.commands.executeCommand('jobView.refresh');
        })
    }
}