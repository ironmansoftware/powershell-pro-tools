using System.Collections.Generic;

namespace PowerShellTools.TestAdapter.Helpers
{
	public interface ISolution
	{
		IEnumerable<IProject> Projects { get; }
	}
}
