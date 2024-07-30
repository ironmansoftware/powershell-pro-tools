'use strict';
import * as vscode from 'vscode';
import { ICommand } from './command';
import axios from 'axios';
import { load } from '../settings';

export class NewsTreeViewCommands implements ICommand {
    register(context: vscode.ExtensionContext) {
        context.subscriptions.push(this.Open());
    }

    Open() {
        return vscode.commands.registerCommand('newsView.open', async (url) => {
            vscode.env.openExternal(vscode.Uri.parse(url));
        })
    }
}

export async function notifyAboutNews(context: vscode.ExtensionContext) {
    var settings = load();
    if (settings.disableNewsNotification) {
        return;
    }

    var result = await axios({ url: 'https://www.ironmansoftware.com/news/api' });
    var news = result.data;

    if (news.length > 0) {
        var latestNews = news[0];
        var id = context.globalState.get("PSPNewsId");
        if (id == latestNews.id) {
            return;
        }

        vscode.window.showInformationMessage("Ironman Software News - " + latestNews.title + " - " + latestNews.description, "View").then((result) => {
            if (result == "View") {
                vscode.env.openExternal(vscode.Uri.parse(latestNews.url));
                context.globalState.update("PSPNewsId", latestNews.id);
            }
        });
    }
}