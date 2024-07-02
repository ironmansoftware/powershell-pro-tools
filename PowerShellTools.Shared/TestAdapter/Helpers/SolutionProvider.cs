using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace PowerShellTools.TestAdapter.Helpers
{
	[Export(typeof(ISolutionProvider))]
	public class SolutionProvider : ISolutionProvider
	{
		private readonly IServiceProvider _serviceProvider;

		[ImportingConstructor]
		public SolutionProvider([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public ISolution GetLoadedSolution()
		{
			var solution = (IVsSolution)_serviceProvider.GetService(typeof(SVsSolution));

			if (solution == null) return null;
			return new Solution(solution);
		}
	}
}
