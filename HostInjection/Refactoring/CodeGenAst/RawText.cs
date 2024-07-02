namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class RawText : ICodeGenAst
    {
        public string Text { get; set; }
        public void Generate(ICodeBuilder stringBuilder)
        {
            stringBuilder.Append(Text);
        }
    }
}
