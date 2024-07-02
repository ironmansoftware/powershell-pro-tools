using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Service;

namespace PowerShellTools.Classification
{
    internal class PowerShellClassifier : Classifier
    {
#pragma warning disable 169, 649
        [BaseDefinition(PredefinedClassificationTypeNames.SymbolDefinition), Name(Classifications.PowerShellAttribute), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition attributeTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.SymbolDefinition), Name(Classifications.PowerShellCommand), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition commandFormatTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Literal), Name(Classifications.PowerShellCommandArgument), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition commandArgumentTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellCommandParameter), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition commandParameterTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Comment), Name(Classifications.PowerShellComment), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition commentTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellGroupEnd), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition groupEndTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellGroupStart), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition groupStartTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Keyword), Name(Classifications.PowerShellKeyword), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition keywordTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellLineContinuation), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition lineContinuationTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellLoopLabel), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition loopLabelTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellMember), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition memberTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.WhiteSpace), Name(Classifications.PowerShellNewLine), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition newLineTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Number), Name(Classifications.PowerShellNumber), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition numberTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellOperator), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition operatorTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.WhiteSpace), Name(Classifications.PowerShellPosition), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition positionTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellStatementSeparator), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition statementSeparatorTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.String), Name(Classifications.PowerShellString), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition stringTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.SymbolDefinition), Name(Classifications.PowerShellType), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition typeTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Identifier), Name(Classifications.PowerShellVariable), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition variableTypeDefinition;

        [BaseDefinition(PredefinedClassificationTypeNames.Operator), Name(Classifications.PowerShellUnknown), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition unknownTypeDefinition;

        [BaseDefinition("text"), Name("PowerShell TokenInBreakpoint"), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition tokenInBreakpoinTypeDefinition;

        [BaseDefinition("text"), Name("PS1ScriptGaps"), Export(typeof(ClassificationTypeDefinition))]
        private static ClassificationTypeDefinition scriptGapsTypeDefinition;

        private static Dictionary<PSTokenType, IClassificationType> _tokenClassificationTypeMap;
#pragma warning restore 169, 649

        private IClassificationType _scriptGaps;

        internal PowerShellClassifier(ITextBuffer bufferToClassify, AnalysisWatcher analysisWatcher)
            : base(bufferToClassify)
        {
            _scriptGaps = ClassificationTypeRegistryService.GetClassificationType("PS1ScriptGaps");

            IPowerShellTokenizationService psts = new PowerShellTokenizationService(bufferToClassify, analysisWatcher);
            bufferToClassify.PostChanged += (o, args) => psts.StartTokenization();
            bufferToClassify.Properties.AddProperty(BufferProperties.PowerShellTokenizer, psts);
        }

        #region Static helper methods

        /// <summary>
        /// Get the classification type VS recognizes from the PowerShell token type.
        /// </summary>
        /// <param name="tokenType">The PowerShell token type.</param>
        /// <returns>The mapped classification type.</returns>
        internal static IClassificationType GetClassificationType(PSTokenType tokenType)
        {
            IClassificationType result;
            return TokenClassificationTypeMap.TryGetValue(tokenType, out result) ? result : null;
        }

        private static IClassificationTypeRegistryService ClassificationTypeRegistryService
        {
            get
            {
                return EditorImports.ClassificationTypeRegistryService;
            }
        }

        private static Dictionary<PSTokenType, IClassificationType> TokenClassificationTypeMap
        {
            get
            {
                CreateClassificationTypeMap();
                return _tokenClassificationTypeMap;
            }
        }

        private static void CreateClassificationTypeMap()
        {
            if (_tokenClassificationTypeMap == null || !_tokenClassificationTypeMap.Any())
            {
                _tokenClassificationTypeMap = new Dictionary<PSTokenType, IClassificationType>();
                _tokenClassificationTypeMap[PSTokenType.Attribute] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellAttribute);
                _tokenClassificationTypeMap[PSTokenType.Command] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellCommand);
                _tokenClassificationTypeMap[PSTokenType.CommandArgument] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellCommandArgument);
                _tokenClassificationTypeMap[PSTokenType.CommandParameter] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellCommandParameter);
                _tokenClassificationTypeMap[PSTokenType.Comment] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellComment);
                _tokenClassificationTypeMap[PSTokenType.GroupEnd] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellGroupEnd);
                _tokenClassificationTypeMap[PSTokenType.GroupStart] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellGroupStart);
                _tokenClassificationTypeMap[PSTokenType.Keyword] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellKeyword);
                _tokenClassificationTypeMap[PSTokenType.LineContinuation] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellLineContinuation);
                _tokenClassificationTypeMap[PSTokenType.LoopLabel] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellLoopLabel);
                _tokenClassificationTypeMap[PSTokenType.Member] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellMember);
                _tokenClassificationTypeMap[PSTokenType.NewLine] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellNewLine);
                _tokenClassificationTypeMap[PSTokenType.Number] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellNumber);
                _tokenClassificationTypeMap[PSTokenType.Operator] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellOperator);
                _tokenClassificationTypeMap[PSTokenType.Position] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellPosition);
                _tokenClassificationTypeMap[PSTokenType.StatementSeparator] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellStatementSeparator);
                _tokenClassificationTypeMap[PSTokenType.String] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellString);
                _tokenClassificationTypeMap[PSTokenType.Type] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellType);
                _tokenClassificationTypeMap[PSTokenType.Unknown] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellUnknown);
                _tokenClassificationTypeMap[PSTokenType.Variable] = ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellVariable);
            }
        }

        #endregion

        protected override IList<ClassificationSpan> VirtualGetClassificationSpans(SnapshotSpan span)
        {
            var list = new List<ClassificationSpan>();
            if (span.Snapshot == null || span.Snapshot.Length == 0)
            {
                return list;
            }

            AddTokenClassifications(TextBuffer, span, list, null, _scriptGaps);
            FillBeginningAndEnd(span, list, TextBuffer.CurrentSnapshot, _scriptGaps);
            return list;
        }

        private void FillClassificationGap(List<ClassificationSpan> classifications, Span? lastClassificationSpan, Span newClassificationSpan, ITextSnapshot currentSnapshot, IClassificationType classificationType)
        {
            if (lastClassificationSpan.HasValue && newClassificationSpan.Start > lastClassificationSpan.Value.Start + lastClassificationSpan.Value.Length)
            {
                classifications.Add(new ClassificationSpan(new SnapshotSpan(currentSnapshot, lastClassificationSpan.Value.Start + lastClassificationSpan.Value.Length, newClassificationSpan.Start - (lastClassificationSpan.Value.Start + lastClassificationSpan.Value.Length)), classificationType));
            }
        }
        
        private void FillBeginningAndEnd(SnapshotSpan span, List<ClassificationSpan> classifications, ITextSnapshot currentSnapshot, IClassificationType classificationType)
        {
            if (classifications.Count == 0)
            {
                classifications.Add(new ClassificationSpan(new SnapshotSpan(currentSnapshot, span.Start, span.Length), classificationType));
                return;
            }

            var classificationSpan = classifications[0];
            if (span.Start < classificationSpan.Span.Start)
            {
                var item = new ClassificationSpan(new SnapshotSpan(currentSnapshot, span.Start, classificationSpan.Span.Start - span.Start), classificationType);
                classifications.Insert(0, item);
            }

            var classificationSpan2 = classifications[classifications.Count - 1];
            if (classificationSpan2.Span.End < span.End)
            {
                classifications.Add(new ClassificationSpan(new SnapshotSpan(currentSnapshot, classificationSpan2.Span.End, span.End - classificationSpan2.Span.End), classificationType));
            }
        }

        private void AddTokenClassifications(ITextBuffer buffer, SnapshotSpan span, List<ClassificationSpan> classifications, Span? lastClassificationSpan, IClassificationType gapType)
        {
            if (!buffer.Properties.TryGetProperty(BufferProperties.TokenSpans, out List<ClassificationInfo> spans) || spans == null)
            {
                return;
            }

            foreach (var current in spans)
            {
                if (current.Start + current.Length < span.Start) continue;

                if (current.Start > span.End)
                {
                    break;
                }

                if (current.Start + current.Length > buffer.CurrentSnapshot.Length)
                    continue;

                var snapshotSpan = new SnapshotSpan(span.Snapshot, current.Start, current.Length);
                var classificationSpan = new ClassificationSpan(snapshotSpan, current.ClassificationType);

                FillClassificationGap(classifications, lastClassificationSpan, snapshotSpan, buffer.CurrentSnapshot, gapType);
                lastClassificationSpan = snapshotSpan;

                classifications.Add(classificationSpan);
            }
        }
    }
}
