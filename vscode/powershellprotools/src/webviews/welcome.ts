import * as vscode from 'vscode';

function getWebviewOptions(extensionUri: vscode.Uri): vscode.WebviewOptions {
    return {
        // Enable javascript in the webview
        enableScripts: true,

        // And restrict the webview to only loading content from our extension's `media` directory.
        localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
    };
}

export default class WelcomePanel {
    /**
     * Track the currently panel. Only allow a single panel to exist at a time.
     */
    public static currentPanel: WelcomePanel | undefined;

    public static readonly viewType = 'psohtoolsWelcome';

    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        // If we already have a panel, show it.
        if (WelcomePanel.currentPanel) {
            WelcomePanel.currentPanel._panel.reveal(column);
            return;
        }

        // Otherwise, create a new panel.
        const panel = vscode.window.createWebviewPanel(
            WelcomePanel.viewType,
            'PowerShell Pro Tools - Welcome',
            column || vscode.ViewColumn.One,
            getWebviewOptions(extensionUri),
        );

        WelcomePanel.currentPanel = new WelcomePanel(panel, extensionUri);
    }

    public static revive(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        WelcomePanel.currentPanel = new WelcomePanel(panel, extensionUri);
    }

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;

        const bootstrapCss = panel.webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'css', 'bootstrap.min.css'));
        const js = panel.webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'js'));
        const images = panel.webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'images'));

        // Set the webview's initial html content
        const htmlPath = vscode.Uri.joinPath(this._extensionUri, 'resources', 'welcome.html');
        var fs = require('fs');

        var content = fs.readFileSync(htmlPath.fsPath, 'utf8');

        const format = (a: string, args: string[]) => {
            for (var k in args) {
                a = a.replace(new RegExp("\\{" + k + "\\}", 'g'), args[k]);
            }
            return a
        }

        let theme = panel.webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'css', 'theme.css'));
        if (vscode.window.activeColorTheme.kind == vscode.ColorThemeKind.Dark) {
            theme = panel.webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'css', 'theme.dark.css'));
        }

        panel.webview.html = format(content, [bootstrapCss.toString(), js.toString(), images.toString(), theme.toString()]);

        // Listen for when the panel is disposed
        // This happens when the user closes the panel or when the panel is closed programmatically
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }

    public dispose() {
        WelcomePanel.currentPanel = undefined;

        // Clean up our resources
        this._panel.dispose();

        while (this._disposables.length) {
            const x = this._disposables.pop();
            if (x) {
                x.dispose();
            }
        }
    }
}