using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.Classification
{
    public static class Classifications
    {
        public const string PowerShellAttribute = "PowerShellAttribute";
        public const string PowerShellCommand = "PowerShellCommand";
        public const string PowerShellCommandArgument = "PowerShellCommandArgument";
        public const string PowerShellCommandParameter = "PowerShellCommandParameter";
        public const string PowerShellComment = "PowerShellComment";
        public const string PowerShellKeyword = "PowerShellKeyword";
        public const string PowerShellNumber = "PowerShellNumber";
        public const string PowerShellOperator = "PowerShellOperator";
        public const string PowerShellString = "PowerShellString";
        public const string PowerShellType = "PowerShellType";
        public const string PowerShellVariable = "PowerShellVariable";
        public const string PowerShellMember = "PowerShellMember";
        public const string PowerShellGroupStart = "PowerShellGroupStart";
        public const string PowerShellGroupEnd = "PowerShellGroupEnd";
        public const string PowerShellLineContinuation = "PowerShellLineContinuation";
        public const string PowerShellLoopLabel = "PowerShellLoopLabel";
        public const string PowerShellNewLine = "PowerShellNewLine";
        public const string PowerShellPosition = "PowerShellPosition";
        public const string PowerShellStatementSeparator = "PowerShellStatementSeparator";
        public const string PowerShellUnknown = "PowerShellUnknown";
    }

    // The foreground colors specified in this file are the defaults if not overridden.
    // For Dark theme and High Contrast, see PowerShellToolsColors.pkgdef

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellAttribute)]
    [Name("PowerShell Attribute")]
    [DisplayName("PowerShell Attribute")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellAttributeFormat : ClassificationFormatDefinition
    {
        public PowerShellAttributeFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 191, 255);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellCommand)]
    [Name("PowerShell Command")]
    [DisplayName("PowerShell Command")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellCommandFormat : ClassificationFormatDefinition
    {
        public PowerShellCommandFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 255);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellCommandArgument)]
    [Name("PowerShell Command Argument")]
    [DisplayName("PowerShell Command Argument")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellCommandArgumentFormat : ClassificationFormatDefinition
    {
        public PowerShellCommandArgumentFormat()
        {
            ForegroundColor = Color.FromArgb(255, 138, 43, 226);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellCommandParameter)]
    [Name("PowerShell Command Parameter")]
    [DisplayName("PowerShell Command Parameter")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellCommandParameterFormat : ClassificationFormatDefinition
    {
        public PowerShellCommandParameterFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 128);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellComment)]
    [Name("PowerShell Comment")]
    [DisplayName("PowerShell Comment")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellCommentFormat : ClassificationFormatDefinition
    {
        public PowerShellCommentFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 100, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellKeyword)]
    [Name("PowerShell Keyword")]
    [DisplayName("PowerShell Keyword")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellKeywordFormat : ClassificationFormatDefinition
    {
        public PowerShellKeywordFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 139);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellNumber)]
    [Name("PowerShell Number")]
    [DisplayName("PowerShell Number")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellNumberFormat : ClassificationFormatDefinition
    {
        public PowerShellNumberFormat()
        {
            ForegroundColor = Color.FromArgb(255, 128, 0, 128);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellOperator)]
    [Name("PowerShell Operator")]
    [DisplayName("PowerShell Operator")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellOperatorsFormat : ClassificationFormatDefinition
    {
        public PowerShellOperatorsFormat()
        {
            ForegroundColor = Color.FromArgb(255, 169, 169, 169);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellString)]
    [Name("PowerShell String")]
    [DisplayName("PowerShell String")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellStringFormat : ClassificationFormatDefinition
    {
        public PowerShellStringFormat()
        {
            ForegroundColor = Color.FromArgb(255, 139, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellType)]
    [Name("PowerShell Type")]
    [DisplayName("PowerShell Types")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellTypeFormat : ClassificationFormatDefinition
    {
        public PowerShellTypeFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 128, 128);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellVariable)]
    [Name("PowerShell Variable")]
    [DisplayName("PowerShell Variable")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellVariablesFormat : ClassificationFormatDefinition
    {
        public PowerShellVariablesFormat()
        {
            ForegroundColor = Color.FromArgb(255, 255, 69, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellMember)]
    [Name("PowerShell Member")]
    [DisplayName("PowerShell Member")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellMemberFormat : ClassificationFormatDefinition
    {
        public PowerShellMemberFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellGroupStart)]
    [Name("PowerShell Group Start")]
    [DisplayName("PowerShell Group Start")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellGroupStartFormat: ClassificationFormatDefinition
    {
        public PowerShellGroupStartFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellGroupEnd)]
    [Name("PowerShell Group End")]
    [DisplayName("PowerShell Group End")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellGroupEndFormat : ClassificationFormatDefinition
    {
        public PowerShellGroupEndFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellLineContinuation)]
    [Name("PowerShell Line Continuation")]
    [DisplayName("PowerShell Line Continuation (Backtick)")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellLineContinuationFormat : ClassificationFormatDefinition
    {
        public PowerShellLineContinuationFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellLoopLabel)]
    [Name("PowerShell Loop Label")]
    [DisplayName("PowerShell Loop Label")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellLoopLabelFormat : ClassificationFormatDefinition
    {
        public PowerShellLoopLabelFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 139);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellStatementSeparator)]
    [Name("PowerShell Statement Separator")]
    [DisplayName("PowerShell Statement Separator (Semicolon)")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellStatementSeparatorFormat : ClassificationFormatDefinition
    {
        public PowerShellStatementSeparatorFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellUnknown)]
    [Name("PowerShell Unknown")]
    [DisplayName("PowerShell Unknown")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PowerShellUnknownFormat : ClassificationFormatDefinition
    {
        public PowerShellUnknownFormat()
        {
            ForegroundColor = Color.FromArgb(255, 0, 0, 0);
        }
    }
}