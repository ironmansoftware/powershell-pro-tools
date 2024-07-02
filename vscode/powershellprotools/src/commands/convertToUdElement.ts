'use strict';
import * as vscode from 'vscode';
import * as utils from './../util';
const cheerio = require('cheerio')

export function convertToUdElement() {
    return vscode.commands.registerCommand('powershell.convertToUdElement', async () => {
        const editor = vscode.window.activeTextEditor;

        if (editor.selection.isEmpty) return;

        const html = editor.document.getText(editor.selection)
        const $ = cheerio.load(html)
        const rootNode = $('body')[0].firstChild;
        var script = '';
        script = createElement(rootNode, 0, script);

        editor.edit(x => {
            x.replace(editor.selection, script)
        });
    });
}

function createElement(element, depth: number, script: string): string {
    if (element == null) return script;

    if (element.type !== 'tag') {
        return createElement(element.nextSibling, depth, script);
    }

    script += makeTabs(depth)
    script += `New-UDElement -Tag '${element.tagName}'`

    if (Object.keys(element.attribs).length > 0) {
        script += ' -Attributes @{ '
        Object.keys(element.attribs).forEach(x => {

            var attribName = x;

            if (x === 'class') {
                attribName = 'className'
            }

            script += `${attribName} = '${element.attribs[x]}'; `
        });
        script += '} '
    }

    if (element.firstChild != null) {
        script += '-Content {\n'
        script = createElement(element.firstChild, depth + 1, script)
        script += makeTabs(depth) + '}\n'
    } else {
        script += '\n'
    }

    if (element.nextSibling != null) {
        script = createElement(element.nextSibling, depth, script)
    }

    return script
}

function makeTabs(depth: number): string {
    var script = '';

    for (var i = 0; i < depth; i++) {
        script += '\t';
    }

    return script;
}