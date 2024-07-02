using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace PowerShellTools.TestAdapter.Helpers
{
	public class Project : IProject
	{
		public Project(IVsProject project)
		{
			Items = VsSolutionHelper.GetProjectItems(project);
			string projectPath;
			if (project.GetMkDocument(VSConstants.VSITEMID_ROOT, out projectPath) == VSConstants.S_OK)
			{
				IsPowerShellProject = projectPath.EndsWith("pssproj", System.StringComparison.OrdinalIgnoreCase);
			}
		}

		public bool IsPowerShellProject { get; private set; }

		public IEnumerable<string> Items { get; }
	}
}
