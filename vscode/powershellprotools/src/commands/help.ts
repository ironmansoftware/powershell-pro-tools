import * as vscode from 'vscode';

const help = () => {
    return vscode.commands.registerCommand('poshProTools.help', async (item: vscode.TreeItem) => {

        let url = '';
        switch (item.label) {
            case "Documentation":
                url = "https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code"
                break;
            case "Forums":
                url = "https://forums.ironmansoftware.com"
                break;
            case "Support":
                url = "https://ironmansoftware.com/product-support"
                break;
        }

        if (url !== '') {
            vscode.env.openExternal(vscode.Uri.parse(url))
        }
    });
}

export default help;