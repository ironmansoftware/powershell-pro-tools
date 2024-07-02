using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Set, "VSCodeTextEditorDecoration")]
    public class SetTextEditorDecorationCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public VsCodeTextEditor TextEditor { get; set; }

        [Parameter(Mandatory = true)]
        public VsCodeRange Range { get; set; }
        [Parameter(Mandatory = true)]
        public string Key { get; set; }

        [Parameter()]
        public string BackgroundColor { get; set; }
        [Parameter()]
        public string Border { get; set; }
        [Parameter()]
        public string BorderColor { get; set; }
        [Parameter()]
        public string BorderRadius { get; set; }
        [Parameter()]
        public string BorderStyle { get; set; }
        [Parameter()]
        public string BorderWidth { get; set; }
        [Parameter()]
        public string Color { get; set; }
        [Parameter()]
        public string Cursor { get; set; }
        [Parameter()]
        public string FontStyle { get; set; }
        [Parameter()]
        public string FontWeight { get; set; }
        [Parameter()]
        public SwitchParameter IsWholeLine { get; set; }
        [Parameter()]
        public string LetterSpacing { get; set; }
        [Parameter()]
        public string Opacity { get; set; }
        [Parameter()]
        public string Outline { get; set; }
        [Parameter()]
        public string OutlineColor { get; set; }
        [Parameter()]
        public string OutlineStyle { get; set; }
        [Parameter()]
        public string OutlineWidth { get; set; }
        [Parameter()]
        [ValidateSet("ClosedClosed", "ClosedOpen", "OpenClosed", "OpenOpen")]
        public string RangeBehavior { get; set; }

        [Parameter()]
        public string TextDecoration { get; set; }

        [Parameter()]
        public DecorationAttachment After { get; set; }
        [Parameter()]
        public DecorationAttachment Before { get; set; }
        protected override void ProcessRecord()
        {
            SendCommand($"vscode.TextEditor.setDecoration", new { 
                filePath = TextEditor.Document.FileName,
                range = Range,
                key = Key, 
                backgroundColor = BackgroundColor, 
                border = Border, 
                borderColor = BorderColor,
                borderRadius = BorderRadius,
                borderStyle = BorderStyle, 
                borderWidth = BorderWidth, 
                color = Color, 
                cursor = Cursor,
                fontStyle = FontStyle, 
                fontWeight = FontWeight, 
                isWholeLine = IsWholeLine.IsPresent, 
                letterSpacing = LetterSpacing, 
                opacity = Opacity,
                outline = Outline, 
                outlineColor = OutlineColor, 
                outlineStyle = OutlineStyle, 
                outlineWidth = OutlineWidth, 
                rangeBehavior = RangeBehavior,
                textDecoration = TextDecoration,
                before = Before,
                after = After
            });
        }
    }

    [Cmdlet(VerbsCommon.Clear, "VSCodeDecoration")]
    public class ClearDecorationCommand : VSCodeCmdlet
    {
        [Parameter()]
        public string Key { get; set; }
        protected override void ProcessRecord()
        {
            SendCommand($"vscode.TextEditor.clearDecoration", new { Key= Key });
        }
    }

    [Cmdlet(VerbsCommon.New, "VSCodeDecorationAttachment")]
    public class NewDecorationAttachmentCommand : PSCmdlet 
    {
        [Parameter()]
        public string BackgroundColor { get; set; }

        [Parameter()]
        public string Border { get; set; }

        [Parameter()]
        public string BorderColor { get; set; }
        [Parameter()]
        public string Color { get; set; }
        [Parameter()]
        public string ContentText { get; set; }
        [Parameter()]
        public string FontStyle { get; set; }
        [Parameter()]
        public string FontWeight { get; set; }
        [Parameter()]
        public string Height { get; set; }
        [Parameter()]
        public string Margin { get; set; }
        [Parameter()]
        public string TextDecoration { get; set; }
        [Parameter()]
        public string Width { get; set; }

        protected override void BeginProcessing() 
        {
            
        }
    }
}
