/* eslint-disable no-console */
const fs = require('fs');
const path = require('path');
const ts = require('typescript');

const root = path.resolve(__dirname, '..');
const repoRoot = path.resolve(root, '..', '..');
const dtsPath = require.resolve('@types/vscode/index.d.ts', { paths: [root] });
const vscodeTypesPackage = require(require.resolve('@types/vscode/package.json', { paths: [root] }));
const source = fs.readFileSync(dtsPath, 'utf8');
const sourceFile = ts.createSourceFile(dtsPath, source, ts.ScriptTarget.Latest, true);

const generatedTypes = new Set();
const interfaces = new Map();
const classes = new Map();
const namespaces = new Map();
const enums = new Map();
const metadata = {};

const reserved = new Set([
    'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char', 'checked',
    'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do', 'double', 'else',
    'enum', 'event', 'explicit', 'extern', 'false', 'finally', 'fixed', 'float', 'for',
    'foreach', 'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal', 'is', 'lock',
    'long', 'namespace', 'new', 'null', 'object', 'operator', 'out', 'override', 'params',
    'private', 'protected', 'public', 'readonly', 'ref', 'return', 'sbyte', 'sealed', 'short',
    'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch', 'this', 'throw', 'true',
    'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort', 'using', 'virtual',
    'void', 'volatile', 'while'
]);

function findVSCodeModule() {
    for (const statement of sourceFile.statements) {
        if (ts.isModuleDeclaration(statement) && statement.name && statement.name.text === 'vscode') {
            return statement.body;
        }
    }

    throw new Error('Could not find declare module "vscode".');
}

function hasExport(node) {
    return !node.modifiers || node.modifiers.some(m => m.kind === ts.SyntaxKind.ExportKeyword);
}

function text(node) {
    return node ? node.getText(sourceFile) : '';
}

function nameOf(node) {
    if (!node || !node.name) {
        return null;
    }

    return node.name.getText(sourceFile).replace(/^['"]|['"]$/g, '');
}

function csIdentifier(name) {
    if (!name) {
        return name;
    }

    let clean = name.replace(/[^A-Za-z0-9_]/g, '_');
    if (/^[0-9]/.test(clean)) {
        clean = '_' + clean;
    }

    if (reserved.has(clean)) {
        return '@' + clean;
    }

    return clean;
}

function csTypeName(name) {
    return `VSCode${name.replace(/[^A-Za-z0-9_]/g, '')}`;
}

function constructorTypeName(name) {
    return `${csTypeName(name)}Constructor`;
}

function stripParens(type) {
    while (type && ts.isParenthesizedTypeNode(type)) {
        type = type.type;
    }

    return type;
}

function mapType(type) {
    type = stripParens(type);
    if (!type) {
        return 'object';
    }

    if (type.kind === ts.SyntaxKind.VoidKeyword) {
        return 'void';
    }

    if (type.kind === ts.SyntaxKind.StringKeyword) {
        return 'string';
    }

    if (type.kind === ts.SyntaxKind.NumberKeyword) {
        return 'double';
    }

    if (type.kind === ts.SyntaxKind.BooleanKeyword) {
        return 'bool';
    }

    if (type.kind === ts.SyntaxKind.AnyKeyword || type.kind === ts.SyntaxKind.UnknownKeyword) {
        return 'object';
    }

    if (ts.isArrayTypeNode(type)) {
        const element = mapType(type.elementType);
        return element === 'void' ? 'object[]' : `${element}[]`;
    }

    if (ts.isUnionTypeNode(type)) {
        const usable = type.types.filter(t => {
            const kind = stripParens(t).kind;
            return kind !== ts.SyntaxKind.NullKeyword && kind !== ts.SyntaxKind.UndefinedKeyword && kind !== ts.SyntaxKind.NeverKeyword;
        });

        if (usable.length === 1) {
            return mapType(usable[0]);
        }

        return 'object';
    }

    if (ts.isTypeReferenceNode(type)) {
        const typeName = text(type.typeName);
        const first = type.typeArguments && type.typeArguments[0];

        if (typeName === 'Thenable' || typeName === 'Promise' || typeName === 'ProviderResult') {
            return mapType(first);
        }

        if (typeName === 'Array' || typeName === 'ReadonlyArray') {
            const element = mapType(first);
            return element === 'void' ? 'object[]' : `${element}[]`;
        }

        const shortName = typeName.split('.').pop();
        if (generatedTypes.has(shortName)) {
            return csTypeName(shortName);
        }

        if (enums.has(shortName)) {
            return csTypeName(shortName);
        }

        return 'object';
    }

    return 'object';
}

function getMembers(node) {
    const members = [];
    if (!node.members) {
        return members;
    }

    for (const member of node.members) {
        const name = nameOf(member);
        if (!name) {
            continue;
        }

        if (ts.isPropertySignature(member) || ts.isPropertyDeclaration(member) || ts.isGetAccessor(member)) {
            members.push({ kind: 'property', name, type: mapType(member.type) });
        }
        else if (ts.isMethodSignature(member) || ts.isMethodDeclaration(member)) {
            members.push({ kind: 'method', name, type: mapType(member.type) });
        }
    }

    return members;
}

function mergeMembers(existing, incoming) {
    const seen = new Set(existing.map(m => `${m.kind}:${m.name}`));
    for (const member of incoming) {
        const key = `${member.kind}:${member.name}`;
        if (!seen.has(key)) {
            existing.push(member);
            seen.add(key);
        }
    }
}

function collectTopLevel(moduleBody) {
    for (const statement of moduleBody.statements) {
        if (!hasExport(statement)) {
            continue;
        }

        if ((ts.isInterfaceDeclaration(statement) || ts.isClassDeclaration(statement)) && statement.name) {
            generatedTypes.add(statement.name.text);
        }
        else if (ts.isEnumDeclaration(statement) && statement.name) {
            generatedTypes.add(statement.name.text);
            enums.set(statement.name.text, statement);
        }
    }

    for (const statement of moduleBody.statements) {
        if (!hasExport(statement)) {
            continue;
        }

        if (ts.isModuleDeclaration(statement) && statement.name && statement.body && ts.isModuleBlock(statement.body)) {
            const namespaceName = statement.name.text;
            const existing = namespaces.get(namespaceName) || [];
            collectNamespaceMembers(namespaceName, statement.body, existing);
            namespaces.set(namespaceName, existing);
        }
        else if (ts.isInterfaceDeclaration(statement) && statement.name) {
            const name = statement.name.text;
            const existing = interfaces.get(name) || [];
            mergeMembers(existing, getMembers(statement));
            interfaces.set(name, existing);
        }
        else if (ts.isClassDeclaration(statement) && statement.name) {
            const name = statement.name.text;
            const existing = classes.get(name) || { instance: [], statics: [], constructable: false };
            for (const member of statement.members) {
                if (ts.isConstructorDeclaration(member)) {
                    existing.constructable = true;
                    continue;
                }

                const memberName = nameOf(member);
                if (!memberName) {
                    continue;
                }

                const item = {
                    kind: (ts.isMethodDeclaration(member) || ts.isMethodSignature(member)) ? 'method' : 'property',
                    name: memberName,
                    type: mapType(member.type)
                };

                const isStatic = member.modifiers && member.modifiers.some(m => m.kind === ts.SyntaxKind.StaticKeyword);
                mergeMembers(isStatic ? existing.statics : existing.instance, [item]);
            }

            classes.set(name, existing);
        }
    }
}

function collectNamespaceMembers(namespaceName, moduleBlock, members) {
    for (const statement of moduleBlock.statements) {
        if (ts.isVariableStatement(statement)) {
            for (const declaration of statement.declarationList.declarations) {
                const name = nameOf(declaration);
                if (name) {
                    mergeMembers(members, [{ kind: 'property', name, type: mapType(declaration.type) }]);
                }
            }
        }
        else if (ts.isFunctionDeclaration(statement)) {
            const name = nameOf(statement);
            if (name) {
                mergeMembers(members, [{ kind: 'method', name, type: mapType(statement.type) }]);
            }
        }
        else if (ts.isModuleDeclaration(statement) && statement.name && statement.body && ts.isModuleBlock(statement.body)) {
            const nestedName = `${namespaceName}.${statement.name.text}`;
            const nested = namespaces.get(nestedName) || [];
            collectNamespaceMembers(nestedName, statement.body, nested);
            namespaces.set(nestedName, nested);
            mergeMembers(members, [{ kind: 'property', name: statement.name.text, type: csTypeName(nestedName.replace(/\./g, '')) }]);
        }
    }
}

function emitMethod(member) {
    const methodName = csIdentifier(member.name);
    if (member.type === 'void') {
        return [
            `        public void ${methodName}(params object[] arguments)`,
            '        {',
            `            Invoke("${member.name}", arguments);`,
            '        }'
        ].join('\n');
    }

    return [
        `        public ${member.type} ${methodName}(params object[] arguments)`,
        '        {',
        `            return Invoke<${member.type}>("${member.name}", arguments);`,
        '        }'
    ].join('\n');
}

function emitProperty(member) {
    return [
        `        public ${member.type} ${csIdentifier(member.name)}`,
        '        {',
        `            get { return Get<${member.type}>("${member.name}"); }`,
        '        }'
    ].join('\n');
}

function emitObjectClass(name, members, pathSegments) {
    const className = csTypeName(name.replace(/\./g, ''));
    const lines = [
        `    public sealed class ${className} : VSCodeObject`,
        '    {',
        `        internal ${className}(VSCodeProxyClient client, VSCodeProxyTarget target)`,
        '            : base(client, target)',
        '        {',
        '        }'
    ];

    for (const member of members) {
        lines.push('');
        lines.push(member.kind === 'method' ? emitMethod(member) : emitProperty(member));
    }

    lines.push('    }');
    metadata[pathSegments.join('.')] = members.map(m => m.name);
    return lines.join('\n');
}

function emitConstructorClass(name, info) {
    const className = constructorTypeName(name);
    const instanceType = csTypeName(name);
    const lines = [
        `    public sealed class ${className} : VSCodeObject`,
        '    {',
        `        internal ${className}(VSCodeProxyClient client, VSCodeProxyTarget target)`,
        '            : base(client, target)',
        '        {',
        '        }',
        '',
        `        public ${instanceType} @new(params object[] arguments)`,
        '        {',
        `            return Construct<${instanceType}>(arguments);`,
        '        }'
    ];

    for (const member of info.statics) {
        lines.push('');
        lines.push(member.kind === 'method' ? emitMethod(member) : emitProperty(member));
    }

    lines.push('    }');
    metadata[name] = info.statics.map(m => m.name);
    return lines.join('\n');
}

function emitEnum(name, declaration) {
    const lines = [`    public enum ${csTypeName(name)}`, '    {'];
    declaration.members.forEach((member, index) => {
        const memberName = csIdentifier(nameOf(member) || `Value${index}`);
        let value = index;
        if (member.initializer && ts.isNumericLiteral(member.initializer)) {
            value = Number(member.initializer.text);
        }

        lines.push(`        ${memberName} = ${value},`);
    });
    lines.push('    }');
    return lines.join('\n');
}

function emitRoot() {
    const lines = [
        '    public sealed class VSCodeApi : VSCodeObject',
        '    {',
        '        internal VSCodeApi(VSCodeProxyClient client)',
        '            : base(client, VSCodeProxyClient.Path())',
        '        {',
    ];

    for (const name of Array.from(namespaces.keys()).filter(n => !n.includes('.')).sort()) {
        lines.push(`            ${csIdentifier(name)} = new ${csTypeName(name)}(client, VSCodeProxyClient.Path("${name}"));`);
    }

    for (const name of Array.from(classes.keys()).sort()) {
        lines.push(`            ${csIdentifier(name)} = new ${constructorTypeName(name)}(client, VSCodeProxyClient.Path("${name}"));`);
    }

    lines.push('        }');

    for (const name of Array.from(namespaces.keys()).filter(n => !n.includes('.')).sort()) {
        lines.push('');
        lines.push(`        public ${csTypeName(name)} ${csIdentifier(name)} { get; }`);
    }

    for (const name of Array.from(classes.keys()).sort()) {
        lines.push('');
        lines.push(`        public ${constructorTypeName(name)} ${csIdentifier(name)} { get; }`);
    }

    lines.push('    }');
    return lines.join('\n');
}

collectTopLevel(findVSCodeModule());

const csharp = [
    '// <auto-generated />',
    'namespace PowerShellToolsPro.Cmdlets.VSCode',
    '{',
    emitRoot(),
    '',
    ...Array.from(enums.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([name, declaration]) => emitEnum(name, declaration)),
    ...Array.from(namespaces.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([name, members]) => emitObjectClass(name, members, name.split('.'))),
    ...Array.from(interfaces.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([name, members]) => emitObjectClass(name, members, [name])),
    ...Array.from(classes.entries()).sort(([a], [b]) => a.localeCompare(b)).flatMap(([name, info]) => [
        emitObjectClass(name, info.instance, [name]),
        emitConstructorClass(name, info)
    ]),
    '}',
    ''
].join('\n\n');

const csharpPath = path.resolve(repoRoot, 'HostInjection', 'Generated', 'VSCodeApi.g.cs');
fs.mkdirSync(path.dirname(csharpPath), { recursive: true });
fs.writeFileSync(csharpPath, csharp);

const metadataSource = [
    '// <auto-generated />',
    'export const vscodeApiMetadata: { version: string; members: { [path: string]: string[] } } = {',
    `    version: ${JSON.stringify(vscodeTypesPackage.version)},`,
    `    members: ${JSON.stringify(metadata, null, 4)}`,
    '};',
    ''
].join('\n');

const metadataPath = path.resolve(root, 'src', 'generated', 'vscodeApiMetadata.ts');
fs.mkdirSync(path.dirname(metadataPath), { recursive: true });
fs.writeFileSync(metadataPath, metadataSource);

console.log(`Generated VS Code API surface from @types/vscode ${vscodeTypesPackage.version}.`);
