using Newtonsoft.Json;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System.Linq;
using System.Management.Automation.Language;

namespace PowerShellProTools
{
    public class RemoteParser 
    {
        public static ParseErrorItem[] Parse(string script)
        {
            Parser.ParseInput(script, out Token[] tokens, out ParseError[] errors);
            return errors.Select(m => new ParseErrorItem(m.Message, m.Extent.StartOffset, m.Extent.EndOffset)).ToArray();
                
        }
    }

    public class ParseResult
    {
        public PSParseError[] Errors { get; set; }
        public PSToken[] Tokens { get; set; }
    }

    public class PSScriptPosition 
    {
        public PSScriptPosition() { }

        public PSScriptPosition(IScriptPosition scriptPosition) 
        {
            ColumnNumber = scriptPosition.ColumnNumber;
            File = scriptPosition.File;
            Line = scriptPosition.Line;
            LineNumber = scriptPosition.LineNumber;
            Offset = scriptPosition.Offset;
        }

        public int ColumnNumber { get; set; }
        public string File { get; set; }
        public string Line { get; set; }
        public int LineNumber { get; set; }
        public int Offset { get; set; }
    }

    public class PSScriptExtent 
    {
        public PSScriptExtent(PSScriptPosition startScriptPosition, PSScriptPosition endScriptPosition)
        {
            EndScriptPosition = endScriptPosition;
            StartScriptPosition = startScriptPosition;
        }
        public PSScriptExtent() { }
        public PSScriptExtent(IScriptExtent scriptExtent)
        {
            EndColumnNumber = scriptExtent.EndColumnNumber;
            EndLineNumber = scriptExtent.EndLineNumber;
            EndOffset = scriptExtent.EndOffset;
            EndScriptPosition = new PSScriptPosition(scriptExtent.EndScriptPosition);
            File = scriptExtent.File;
            StartColumnNumber = scriptExtent.StartColumnNumber;
            StartLineNumber = scriptExtent.StartLineNumber;
            StartOffset = scriptExtent.StartOffset;
            StartScriptPosition = new PSScriptPosition(scriptExtent.StartScriptPosition);
            Text = scriptExtent.Text;
        }


        public int EndColumnNumber { get; set; }
        public int EndLineNumber { get; set; }
        public int EndOffset { get; set; }
        public PSScriptPosition EndScriptPosition { get; set; }
        public string File { get; set; }
        public int StartColumnNumber { get; set; }
        public int StartLineNumber { get; set; }
        public int StartOffset { get; set; }
        public PSScriptPosition StartScriptPosition { get; set; }
        public string Text { get; set; }
    }

    public class PSParseError
    {
        public PSParseError(PSScriptExtent scriptExtent)
        {
            Extent = scriptExtent;
        }
        public PSParseError() { }
        public PSParseError(ParseError parseError)
        {
            ErrorId = parseError.ErrorId;
            Extent = new PSScriptExtent(parseError.Extent);
            IncompleteInput = parseError.IncompleteInput;
            Message = parseError.Message;
        }
        public string ErrorId { get; set; }
        public PSScriptExtent Extent { get; set; }
        public bool IncompleteInput { get; set; }
        public string Message { get; set; }
    }

    public class PSToken
    {
        public PSToken(PSScriptExtent scriptExtent) 
        {
            Extent = scriptExtent;
        }
        public PSToken() { }
        public PSToken(Token token)
        {
            Extent = new PSScriptExtent(token.Extent);
            HasError = token.HasError;
            Kind = token.Kind;
            Text = token.Text;
            TokenFlags = token.TokenFlags;
        }
        public PSScriptExtent Extent { get; set; }
        public bool HasError { get; set; }
        public TokenKind Kind { get; set; }
        public string Text { get; set; }
        public TokenFlags TokenFlags { get; set; }
    }
}
