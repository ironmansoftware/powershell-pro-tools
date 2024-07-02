using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Utilities;
using PowerShellProTools.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.Intellisense
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("PowerShell Quick Info Provider")]
    [ContentType("PowerShell")]
    [Order]
    internal sealed class PowerShellQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            // This ensures only one instance per textbuffer is created
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new PowerShellQuickInfoSource(textBuffer));
        }
    }

    internal sealed class PowerShellQuickInfoSource : IAsyncQuickInfoSource
    {
        //private static readonly ImageId _icon = KnownMonikers.AbstractCube.ToImageId();

        private ITextBuffer _textBuffer;

        public PowerShellQuickInfoSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            return Task.FromResult<QuickInfoItem>(null);

            //var l = new LicenseManager();
            //if (l.GetInstalledLicenseStatus() == LicenseStatus.Licensed)
            //{
            //    return Task.FromResult<QuickInfoItem>(null);
            //}
            //var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);

            //if (triggerPoint != null)
            //{
            //    var line = triggerPoint.Value.GetContainingLine();
            //    var lineNumber = triggerPoint.Value.GetContainingLine().LineNumber;
            //    var lineSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);
            //    var lineNumberElm = new ContainerElement(
            //        ContainerElementStyle.Wrapped,
            //        new ClassifiedTextElement(
            //            new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, "PowerShell Pro Tools is unlicensed. Enhanced hover is not enabled. Get a license at ironmansoftware.com.")
            //        ));

            //    return Task.FromResult(new QuickInfoItem(lineSpan, lineNumberElm));
            //}

            //return Task.FromResult<QuickInfoItem>(null);
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}
