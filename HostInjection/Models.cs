using HostInjection;
using PowerShellToolsPro;
using System.Collections.Generic;

namespace PowerShellProTools.Host.Refactoring
{
    public class TextEdit
    {
        public TextPosition Start { get; set; }
        public TextPosition End { get; set; }
        public TextPosition Cursor { get; set; }
        public string Content { get; set; }
        public string FileName { get; set; }
        public TextEditType Type { get; set; }
        public string Uri { get; set; }


        public static TextEdit None
        {
            get
            {
                return new TextEdit { Type = TextEditType.None };
            }
        }

        public override bool Equals(object obj)
        {
            return obj is TextEdit edit &&
                   EqualityComparer<TextPosition>.Default.Equals(Start, edit.Start) &&
                   EqualityComparer<TextPosition>.Default.Equals(End, edit.End) &&
                   EqualityComparer<TextPosition>.Default.Equals(Cursor, edit.Cursor) &&
                   Content == edit.Content &&
                   FileName == edit.FileName &&
                   Type == edit.Type &&
                   Uri == edit.Uri;
        }

        public override int GetHashCode()
        {
            int hashCode = -739371025;
            hashCode = hashCode * -1521134295 + EqualityComparer<TextPosition>.Default.GetHashCode(Start);
            hashCode = hashCode * -1521134295 + EqualityComparer<TextPosition>.Default.GetHashCode(End);
            hashCode = hashCode * -1521134295 + EqualityComparer<TextPosition>.Default.GetHashCode(Cursor);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Content);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Uri);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }
    }

    public enum TextEditType
    {
        None,
        Replace,
        NewFile,
        Insert
    }

    public class TextEditorState
    {
        public string Content { get; set; }
        public string FileName { get; set; }
        public TextPosition SelectionStart { get; set; }
        public TextPosition SelectionEnd { get; set; }
        public TextPosition DocumentEnd { get; set; }
        public IPoshToolsServer Server { get; set; }
        public string Uri { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TextEditorState state &&
                   Content == state.Content &&
                   FileName == state.FileName &&
                   Uri == state.Uri &&
                   EqualityComparer<TextPosition>.Default.Equals(SelectionStart, state.SelectionStart) &&
                   EqualityComparer<TextPosition>.Default.Equals(SelectionEnd, state.SelectionEnd) &&
                   EqualityComparer<TextPosition>.Default.Equals(DocumentEnd, state.DocumentEnd) &&
                   EqualityComparer<IPoshToolsServer>.Default.Equals(Server, state.Server);
        }

        public override int GetHashCode()
        {
            int hashCode = -650796018;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Content);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + EqualityComparer<TextPosition>.Default.GetHashCode(SelectionStart);
            hashCode = hashCode * -1521134295 + EqualityComparer<TextPosition>.Default.GetHashCode(SelectionEnd);
            hashCode = hashCode * -1521134295 + EqualityComparer<TextPosition>.Default.GetHashCode(DocumentEnd);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Uri);
            hashCode = hashCode * -1521134295 + EqualityComparer<IPoshToolsServer>.Default.GetHashCode(Server);
            return hashCode;
        }
    }

    public class TextPosition
    {
        public int Line { get; set; }
        public int Character { get; set; }
        public int Index { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TextPosition position &&
                   Line == position.Line &&
                   Character == position.Character &&
                   Index == position.Index;
        }

        public override int GetHashCode()
        {
            int hashCode = 2028873650;
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Character.GetHashCode();
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            return hashCode;
        }
    }

    public class RefactorRequest
    {
        public RefactorType Type { get; set; }
        public TextEditorState EditorState { get; set; }
        public List<RefactoringProperty> Properties { get; set; }
    }

    public class RefactoringProperty
    {
        public RefactorProperty Type { get; set; }
        public string Value { get; set; }
    }

    public enum RefactorType
    {
        ExtractFile,
        ExportModuleMember,
        ConvertToSplat,
        ExtractFunction,
        ConvertToMultiline,
        GenerateFunctionFromUsage,
        IntroduceVariableForSubstring,
        WrapDotNetMethod,
        Reorder,
        SplitPipe,
        IntroduceUsingNamespace,
        ConvertToPSItem,
        ConvertToDollarUnder,
        GenerateProxyFunction,
        ExtractVariable
    }

    public enum RefactorProperty
    {
        FileName,
        Name
    }

    public class RefactorInfo
    {
        public RefactorInfo() { }

        public RefactorInfo(string name, RefactorType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public RefactorType Type { get; set; }
    }
}
