'use strict';
import * as vscode from 'vscode';
import path = require("path");
import fs = require("fs");
import { Container } from '../container';
import { load } from '../settings';

function makeDefaultPackageConfig(rootPath: string) {

    const settings = load();
    const outputPath = path.join(rootPath, 'out')

    if (settings.defaultPackagePsd1 && settings.defaultPackagePsd1 !== '') {
        const buffer = fs.readFileSync(settings.defaultPackagePsd1);
        var defaultPackageManifest = buffer.toString();
        defaultPackageManifest = defaultPackageManifest.replace('$root', vscode.window.activeTextEditor.document.fileName);
        return defaultPackageManifest.replace('$outputPath', outputPath);
    }
    else {
        return `@{
    Root = '${vscode.window.activeTextEditor.document.fileName}'
    OutputPath = '${outputPath}'
    Package = @{
        Enabled = $true
        Obfuscate = $false
        HideConsoleWindow = $false
        DotNetVersion = 'v4.6.2'
        FileVersion = '1.0.0'
        FileDescription = ''
        ProductName = ''
        ProductVersion = ''
        Copyright = ''
        RequireElevation = $false
        ApplicationIconPath = ''
        PackageType = 'Console'
    }
    Bundle = @{
        Enabled = $true
        Modules = $true
        # IgnoredModules = @()
    }
}
        `
    }
}

export function packageAsExe() {
    return vscode.commands.registerCommand('powershell.packageAsExe', (resource) => {
        if (!Container.IsInitialized()) return;

        var rootPath;

        if (!vscode.workspace.workspaceFolders && vscode.window.activeTextEditor) {
            rootPath = path.dirname(vscode.window.activeTextEditor.document.uri.fsPath);
        }
        else if (resource) {
            rootPath = path.dirname(resource.fsPath);

            var files = fs.readdirSync(rootPath);
            var file = files.find(x => {
                return path.basename(x).toLowerCase() === "package.psd1";
            });

            if (!file) {
                if (vscode.workspace.workspaceFolders) {
                    rootPath = vscode.workspace.workspaceFolders[0].uri.fsPath;
                }
            }
        }
        else if (vscode.workspace.workspaceFolders) {
            rootPath = vscode.workspace.workspaceFolders[0].uri.fsPath;
        }
        else {
            vscode.window.showWarningMessage("Open a PS1 file before packaging.");
            return;
        }

        var files = fs.readdirSync(rootPath);
        var file = files.find(x => {
            return path.basename(x).toLowerCase() === "package.psd1";
        });

        var packageManifest = path.join(rootPath, 'package.psd1')
        if (file == null) {
            if (!vscode.window.activeTextEditor) {
                vscode.window.showWarningMessage("Open a file to package.");
                return;
            }

            if (!vscode.window.activeTextEditor.document.uri.fsPath.toLowerCase().endsWith(".ps1")) {
                vscode.window.showWarningMessage("Open a PS1 file to package.");
                return;
            }

            const defaultPackageConfig = makeDefaultPackageConfig(rootPath);
            fs.writeFileSync(packageManifest, defaultPackageConfig);
            vscode.window.showInformationMessage(`Created default package manifest at ${packageManifest} `);
        }

        Container.PowerShellService.Package(packageManifest);
    });
}