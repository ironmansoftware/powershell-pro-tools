import * as vscode from 'vscode';
import net = require('net');
import { Container } from '../container';
var fs = require('fs')
import * as path from 'path';

export default class VSCodeService {

    server: net.Server;
    decorations: any;

    init() {
        this.decorations = {}
        var _this = this;

        var data = '';

        this.server = net.createServer(function (stream) {
            stream.on('connect', () => {
                console.log("connected");
            })
            stream.on('data', (c) => {
                const str = c.toString();

                if (!str.endsWith('!PS')) {
                    data += str;
                    return;
                }
                else {
                    data += str.substr(0, str.length - 3);
                }

                try {
                    const msg = JSON.parse(data);
                    _this.processMsg(msg, (s) => {
                        if (s && s !== '') {
                            stream.write(s);
                        }
                        stream.end();
                    });
                }
                catch (er) {
                    Container.Log(er);
                }
                finally {
                    data = ''
                }

            });
        });

        var terminal = vscode.window.terminals.find(x => x.name === "PowerShell Extension");
        if (terminal == null) {
            throw "PowerShell Extension not found.";
        }

        terminal.processId.then(x => {
            var pipePath = `/tmp/PPTPipeCode${x}`;
            if (process.platform === "win32") {
                pipePath = `\\\\.\\pipe\\PPTPipeCode${x}`;
            }

            this.server.listen(pipePath);
        });
    }

    convertToPosition(position) {
        return new vscode.Position(position._line, position._character);
    }

    convertToRange(range) {
        const start = this.convertToPosition(range._start);
        const end = this.convertToPosition(range._end);
        return new vscode.Range(start, end);
    }

    outDataGridView(args: any, callback) {

        if (!args) return;

        const cssUri = vscode.Uri.file(path.join(Container.context.extensionPath, 'resources', 'datatables.min.css')).with({ scheme: 'vscode-resource' });
        const jsUri = vscode.Uri.file(path.join(Container.context.extensionPath, 'resources', 'datatables.min.js')).with({ scheme: 'vscode-resource' });

        const panel = vscode.window.createWebviewPanel(
            'poshProToolsDataGrid',
            args.Title,
            vscode.ViewColumn.One,
            {
                enableScripts: true
            }
        );

        var terminal = vscode.window.terminals.find(x => x.name === "PowerShell Extension");
        if (terminal == null) {
            throw "PowerShell Extension not found.";
        }

        const resourcesHead = `<link rel="stylesheet" href="${cssUri}">
        <script src="${jsUri}"></script>`

        var welcomePath = Container.context.asAbsolutePath(path.join('resources', 'grid-view.html'));
        var text = fs.readFileSync(welcomePath, 'utf8');

        // Generate header
        let thead = '<thead><tr>';

        args.DataTable.DataColumns.forEach(x => {
            thead += `<th>${x.Label}</th>`
        })

        thead += "</tr></thead>"

        // Generate body
        let tbody = '<tbody>'

        args.DataTable.Data.forEach(x => {
            tbody += '<tr>'
            Object.keys(x.Values).forEach(key => {
                tbody += `<td>${x.Values[key].DisplayValue}</td>`
            })
            tbody += '</tr>'
        })

        panel.webview.html = text.replace('${head}', resourcesHead).replace('${thead}', thead).replace('${tbody}', tbody);

        panel.onDidDispose(x => {
            callback();
        })
    }

    processMsg(msg: any, callback) {

        var _this = this;

        switch (msg.type) {
            case "Terminated":
                Container.Log("PowerShell Extension has exited. Will attempt to reconnect.");
                Container.PowerShellService.Reconnect(() => { });
            case "Out-GridView":
                this.outDataGridView(msg.args, callback);
                break;
            case "vscode.window.showErrorMessage":
                if (msg.args.items) {
                    vscode.window.showErrorMessage(msg.args.message, msg.args.options, ...msg.args.items).then(callback);
                }
                else {
                    vscode.window.showErrorMessage(msg.args.message, msg.args.options)
                    callback();
                }

                break;
            case "vscode.window.showInformationMessage":
                if (msg.args.items) {
                    vscode.window.showInformationMessage(msg.args.message, msg.args.options, ...msg.args.items).then(callback);
                }
                else {
                    vscode.window.showInformationMessage(msg.args.message, msg.args.options)
                    callback();
                }
                break;
            case "vscode.window.showWarningMessage":
                if (msg.args.items) {
                    vscode.window.showWarningMessage(msg.args.message, msg.args.options, ...msg.args.items).then(callback);
                }
                else {
                    vscode.window.showWarningMessage(msg.args.message, msg.args.options)
                    callback();
                }
                break;
            case "vscode.window.setStatusBarMessage":
                vscode.window.setStatusBarMessage(msg.args.message, msg.args.hideAfterTimeout);
                callback(null);
                break;
            case "vscode.window.showInputBox":
                vscode.window.showInputBox(msg.args).then(callback);
                break;
            case "vscode.window.showQuickPick":
                vscode.window.showQuickPick(msg.args.items, {
                    canPickMany: msg.args.canPickMany,
                    ignoreFocusOut: msg.args.ignoreFocusOut,
                    placeHolder: msg.args.placeHolder
                }).then(callback);
                break;
            case "vscode.window.openTextDocument":
                vscode.workspace.openTextDocument(msg.args).then(doc => {
                    vscode.window.showTextDocument(doc);
                    callback(JSON.stringify(doc));
                });
                break;
            case "vscode.workspace.textDocuments":
                callback(JSON.stringify(vscode.workspace.textDocuments));
                break;
            case "vscode.TextDocument.getText":
                const document = vscode.workspace.textDocuments.find(x => x.fileName === msg.args.fileName);

                if (msg.args.range) {
                    callback(document.getText(_this.convertToRange(msg.args.range)));
                }
                else {
                    callback(document.getText());
                }

                break;
            case "vscode.TextDocument.insert":
                const insertTextEditor = vscode.window.visibleTextEditors.find(x => x.document.fileName === msg.args.fileName);
                const position = _this.convertToPosition(msg.args.position);

                insertTextEditor.edit(x => {
                    x.insert(position, msg.args.text);
                }).then(success => {
                    if (!success) {
                        vscode.window.showErrorMessage("Failed to apply edit");
                    }
                    callback();
                })

                break;
            case "vscode.TextDocument.delete":
                const removeTextEditor = vscode.window.visibleTextEditors.find(x => x.document.fileName === msg.args.fileName);
                removeTextEditor.edit(x => {
                    x.delete(_this.convertToRange(msg.args.range));
                }).then(success => {
                    if (!success) {
                        vscode.window.showErrorMessage("Failed to apply edit");
                    }
                    callback();
                })

                break;
            case "vscode.TextEditor.clearDecoration":
                var _this = this;

                if (msg.args.key) {
                    _this.decorations[msg.args.key].dispose();
                    delete _this.decorations[msg.args.key];
                }
                else {
                    const keys = Object.keys(this.decorations);
                    keys.forEach(key => {
                        _this.decorations[key].dispose();
                        delete _this.decorations[key];
                    })
                }

                callback();
                break;
            case "vscode.TextEditor.setDecoration":
                const decorationEditor = vscode.window.visibleTextEditors.find(x => x.document.fileName === msg.args.filePath);
                const decorationRange = _this.convertToRange(msg.args.range);

                var backgroundColor = null;
                if (msg.args.backgroundColor) {
                    backgroundColor = new vscode.ThemeColor(msg.args.backgroundColor);
                }

                var borderColor = null;
                if (msg.args.borderColor) {
                    borderColor = new vscode.ThemeColor(msg.args.borderColor);
                }

                var color = null;
                if (msg.args.color) {
                    color = new vscode.ThemeColor(msg.args.color);
                }

                var outlineColor = null;
                if (msg.args.outlineColor) {
                    outlineColor = new vscode.ThemeColor(msg.args.outlineColor);
                }

                var rangeBehavior = null;
                switch (msg.args.rangeBehavior) {
                    case "ClosedClosed":
                        rangeBehavior = vscode.DecorationRangeBehavior.ClosedClosed;
                        break;
                    case "OpenClosed":
                        rangeBehavior = vscode.DecorationRangeBehavior.OpenClosed;
                        break;
                    case "OpenOpen":
                        rangeBehavior = vscode.DecorationRangeBehavior.OpenOpen;
                        break;
                    case "ClosedOpen":
                        rangeBehavior = vscode.DecorationRangeBehavior.ClosedOpen;
                        break;
                }

                var decorationType = vscode.window.createTextEditorDecorationType({
                    backgroundColor: backgroundColor,
                    border: msg.args.border,
                    borderColor: borderColor,
                    borderRadius: msg.args.borderRadius,
                    borderStyle: msg.args.borderStyle,
                    borderWidth: msg.args.borderWidth,
                    color: color,
                    cursor: msg.args.cursor,
                    fontStyle: msg.args.fontStyle,
                    fontWeight: msg.args.fontWeight,
                    isWholeLine: msg.args.isWholeLine,
                    letterSpacing: msg.args.letterSpacing,
                    opacity: msg.args.opacity,
                    outline: msg.args.outline,
                    outlineColor: outlineColor,
                    outlineStyle: msg.args.outlineStyle,
                    outlineWidth: msg.args.outlineWidth,
                    rangeBehavior: rangeBehavior,
                    textDecoration: msg.args.textDecoration,
                    //before = Before,
                    //after = After
                });

                this.decorations[msg.args.key] = decorationType;

                decorationEditor.setDecorations(decorationType, [decorationRange]);

                callback();

                break;
            case "vscode.window.visibleTextEditors":
                callback(JSON.stringify(vscode.window.visibleTextEditors.map(x => {
                    return {
                        "_document": x.document,
                        "_languageId": x.document.languageId
                    }
                })));
                break;
            case "vscode.TextEditor.hide":
                const editor = vscode.window.visibleTextEditors.find(x => x.document.fileName === msg.args.filePath);
                if (!editor) {
                    vscode.window.showErrorMessage(`Failed to find text editor for ${msg.args.filePath}`);
                    callback();
                }

                editor.hide();
                callback();
                break;
            case "vscode.window.terminals":
                callback(JSON.stringify(vscode.window.terminals));
                break;
            case "vscode.Terminal.sendText":
                var terminal = vscode.window.terminals.find(x => x.name.toLocaleLowerCase() === msg.args.name.toLocaleLowerCase());
                if (!terminal) {
                    vscode.window.showErrorMessage(`Failed to find terminal ${msg.args.name}`);
                    callback('{}');
                    return;
                }

                terminal.sendText(msg.args.text, msg.args.addNewLine);
                callback('{}');
                break;
        }
    }
} 