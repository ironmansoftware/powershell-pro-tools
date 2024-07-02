import * as vscode from 'vscode';
import { Container } from '../container';
var fs = require('fs')
import * as path from 'path';
import net = require('net');

var server: net.Server = null;

function init(panel: vscode.WebviewPanel) {
    server = net.createServer(function (stream) {
        stream.on('data', (c) => {
            const str = c.toString();
            const msg = JSON.parse(str);
            panel.webview.postMessage(msg)
            stream.end();
        });
    });

    var terminal = vscode.window.terminals.find(x => x.name === "PowerShell Extension");
    if (terminal == null) {
        throw "PowerShell Extension not found.";
    }

    terminal.processId.then(x => {
        var pipePath = `/tmp/PPTPipePerformance${x}`;
        if (process.platform === "win32") {
            pipePath = `\\\\.\\pipe\\PPTPipePerformance${x}`;
        }

        server.listen(pipePath);
    });
}

export function showDataGrid(context: vscode.ExtensionContext) {
    return vscode.commands.registerCommand('poshProTools.dataGrid', async () => {
        if (!Container.IsInitialized()) return;

        const cssUri = vscode.Uri.file(path.join(context.extensionPath, 'resources', 'datatables.min.css')).with({ scheme: 'vscode-resource' });
        const jsUri = vscode.Uri.file(path.join(context.extensionPath, 'resources', 'datatables.min.js')).with({ scheme: 'vscode-resource' });

        const panel = vscode.window.createWebviewPanel(
            'poshProToolsDataGrid',
            'PowerShell Grid View',
            vscode.ViewColumn.One,
            {
                enableScripts: true
            }
        );

        init(panel);

        const resourcesHead = `<link rel="stylesheet" href="${cssUri}">
        <script src="${jsUri}"></script>`

        var text = getText();
        panel.webview.html = text.replace('${head}', resourcesHead);

        panel.onDidDispose(x => {
            server.close();
        })
    })
}

function getText() {
    var welcomePath = Container.context.asAbsolutePath(path.join('resources', 'grid-view.html'));
    return fs.readFileSync(welcomePath, 'utf8');
}