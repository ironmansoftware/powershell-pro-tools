using System.Collections.Generic;

namespace PowerShellTools.TestAdapter.Helpers
{
	public interface IProject
	{
		IEnumerable<string> Items { get; }
		bool IsPowerShellProject { get; }
	}
}
