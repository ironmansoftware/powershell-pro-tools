import * as vscode from 'vscode';
import { Container } from './../container';
export let PowerShellLanguageId = "poshProTools";

export function signOnSave() {
    vscode.workspace.onDidSaveTextDocument(async x => {
        var settings = Container.GetSettings();
        if (!settings.signOnSave || !x.fileName.toLocaleLowerCase().endsWith('.ps1')) return;

        var selectedCert = settings.signOnSaveCertificate;

        if (selectedCert === '')
        {
            var certs = await Container.PowerShellService.GetCodeSigningCerts();
            var certStrings = certs.map((x, i) => `${i} - ${x.Subject} | ${x.Expiration} | ${x.Thumbprint}`)
            var result = await vscode.window.showQuickPick(certStrings);
            if (result)
            {
                var certIndex = Number.parseInt(result.split('-')[0].trimEnd())
                selectedCert = certs[certIndex].Path;
                const configuration: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(PowerShellLanguageId);
                await configuration.update('signOnSaveCertificate', selectedCert);
            }
        }

        await Container.PowerShellService.SignScript(x.fileName, selectedCert);
    })
}