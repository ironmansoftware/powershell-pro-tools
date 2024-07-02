using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;

namespace PowerShellTools.TestAdapter.Helpers
{
	public class Solution : ISolution
	{
		public Solution(IVsSolution solution)
		{
			Projects = solution.EnumerateLoadedProjects(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION).OfType<IVsProject>().Select(m => new Project(m));
		}

		public IEnumerable<IProject> Projects { get; }
	}
}
