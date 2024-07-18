export class RefactorTextEdit {
    Start: TextPosition;
    End: TextPosition;
    Cursor: TextPosition;
    Content: string;
    FileName: string;
    Type: TextEditType;
    Uri: string;
}

export class RefactorInfo {
    Name: string;
    Type: RefactorType;
}

export enum TextEditType {
    none,
    replace,
    newFile,
    insert
}

export class TextEditorState {
    content: string;
    fileName: string;
    uri: string;
    selectionStart: TextPosition;
    selectionEnd: TextPosition;
    documentEnd: TextPosition;
}

export class TextPosition {
    Line: number;
    Character: number;
}

export class RefactorRequest {
    type: RefactorType;
    editorState: TextEditorState
    properties: Array<RefactoringProperty>
}

export class RefactoringProperty {
    type: RefactorProperty;
    value: string;
}

export enum RefactorType {
    extractFile,
    exportModuleMember,
    convertToSplat,
    extractFunction,
    convertToMultiLine,
    generateFunctionFromUsage,
    introduceVariableForSubstring,
    wrapDotNetMethod,
    reorder,
    SplitPipe,
    IntroduceUsingNamespace,
    ConvertToPSItem,
    ConvertToDollarUnder,
    GenerateProxyFunction,
    ExtractVariable
}

export enum RefactorProperty {
    fileName,
    name
}

export class Hover {
    Markdown: string;
}

export class ReferenceLocation {
    FileName: string;
    Line: number;
    Character: number;
}

export class FunctionDefinition {
    Name: string;
    FileName: string;
    Line: number;
    Character: number;
    References: Array<ReferenceLocation>;
}

export class Certificate {
    Path: string;
    Thumbprint: string;
    Subject: string;
    Expiration: string;
}

export class PSAssembly {
    Name: string;
    Version: string;
    Path: string;
}

export class PSType {
    Name: string;
    AssemblyName: string;
    FullTypeName: string;
}

export class PSMethod {
    Name: string;
    AssemblyName: string;
    TypeName: string;
    Display: string;
}

export class PSField {
    Name: string;
    AssemblyName: string;
    TypeName: string;
    FieldType: string;
}

export class PSProperty {
    Name: string;
    AssemblyName: string;
    TypeName: string;
    PropertyType: string;
}

export class PSNamespace {
    Name: string;
    FullName: string;
}

export class CustomTreeItem {
    TreeViewId: string;
    Label: string;
    Description: string;
    Tooltip: string;
    Icon: string;
    HasChildren: boolean;
    Path: string;
    CanInvoke: boolean;
}

export class CustomTreeView {
    Label: string;
    Description: string;
    Tooltip: string;
    Icon: string;
}

export class Session {
    Name: string;
    Id: number;
    ComputerName: string;
}

export class Job {
    Id: number;
    Name: string;
    Type: string;
    State: string;
    HasMoreData: boolean;
    Location: string;
}

export class Performance {
    Memory: number;
    Cpu: number;
}