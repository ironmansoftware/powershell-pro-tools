import * as vscode from 'vscode';
import { Container } from '../container';
import { Job } from '../types';
import { TreeViewProvider } from './treeViewProvider';

export class JobTreeViewProvider extends TreeViewProvider {
    requiresLicense(): boolean {
        return false;
    }

    getRefreshCommand(): string {
        return "jobView.refresh";
    }

    async getNodes(): Promise<vscode.TreeItem[]> {
        var history = await Container.PowerShellService.GetJobs();
        return history.map(x => new JobTreeItem(x));
    }
}

export class JobTreeItem extends vscode.TreeItem {
    constructor(public readonly job: Job) {
        super(job.Name);

        this.description = job.Type;

        switch (job.State) {
            case "NotStarted":
                this.iconPath = new vscode.ThemeIcon('circle-outline');
                this.contextValue = 'job';
                break;
            case 'Running':
                this.iconPath = new vscode.ThemeIcon('play-circle');
                this.contextValue = 'jobrunning';
                break;
            case 'Completed':
                this.iconPath = new vscode.ThemeIcon('check');
                this.contextValue = 'jobcompleted';
                break;
            case 'Failed':
                this.iconPath = new vscode.ThemeIcon('error');
                this.contextValue = 'jobcompleted';
                break;
            case 'Stopped':
            case 'Stopping':
            case 'Blocked':
            case 'Suspended':
            case 'Suspending':
                this.iconPath = new vscode.ThemeIcon('circle-slash');
                this.contextValue = 'jobcompleted';
                break;
            case 'Disconnected':
                this.iconPath = new vscode.ThemeIcon('debug-disconnect');
                this.contextValue = 'jobrunning';
                break;
            case 'AtBreakpoint':
                this.iconPath = new vscode.ThemeIcon('circle-filled');
                this.contextValue = 'jobrunning';
                break;
        }

        this.tooltip = `${job.State} on ${job.Location}`;
    }
}
