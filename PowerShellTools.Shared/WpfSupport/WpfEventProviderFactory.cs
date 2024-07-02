using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Windows.Design.Host;
using PowerShellProTools.Host;

namespace PowerShellToolsPro.WpfSupport
{
	[Export]
	public class WpfEventProviderFactory
	{
		[ImportingConstructor]
		public WpfEventProviderFactory()
		{
		}

		[Export("WpfEventProviderFactory")]
		public EventBindingProvider GetProvider(Func<IWpfTextView> xamlTextView, Func<IWpfTextView> textView)
		{
			return new WpfEventBindingProvider(xamlTextView, textView);
		}
	}
}
