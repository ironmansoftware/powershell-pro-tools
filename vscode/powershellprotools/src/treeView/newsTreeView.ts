import * as vscode from 'vscode';
import { TreeViewProvider } from './treeViewProvider';
import axios from 'axios';

export class NewsViewProvider extends TreeViewProvider {
    getRefreshCommand(): string {
        return "newsView.refresh";
    }

    requiresLicense(): boolean {
        return false;
    }

    async getNodes(): Promise<vscode.TreeItem[]> {
        var result = await axios({ url: 'https://www.ironmansoftware.com/news/api' });
        var news = result.data;
        return news.map((news: any) => {
            return {
                label: news.title,
                iconPath: new vscode.ThemeIcon('sparkle-filled'),
                tooltip: news.description,
                collapsibleState: vscode.TreeItemCollapsibleState.None,
                command: {
                    title: 'open',
                    command: 'newsView.open',
                    arguments: [news.url]
                }
            };
        });
    }
}