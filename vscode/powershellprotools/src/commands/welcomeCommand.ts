import * as vscode from 'vscode';
import WelcomePanel from '../webviews/welcome';

export const registerWelcomeCommands = (context: vscode.ExtensionContext) => {
    vscode.commands.registerCommand('poshProTools.welcome', Welcome);
}

export const Welcome = async (context: vscode.ExtensionContext) => {
    var extension = vscode.extensions.getExtension('ironmansoftware.powershellprotools');
    if (extension) {
        WelcomePanel.createOrShow(extension.extensionUri)
    }
}