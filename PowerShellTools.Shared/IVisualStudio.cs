using System.Collections.Generic;

namespace PowerShellToolsPro
{
	public enum LogLevel
	{
		Error = 1,
		Warning = 2,
		Info = 3
	}

	public interface IVisualStudio
	{
		IVisualStudioProjects Projects { get; }
		IVisualStudioFile GetFile(string fileName);
	    IVisualStudioProject ActiveWindowProject { get; }
		IVisualStudioFile ActiveFile { get; }


		void Log(LogLevel level, string message);
	}

	public interface IVisualStudioProjects
	{
		IVisualStudioProject GetProjectContainingFile(string fileName);
	    IEnumerable<IVisualStudioProject> Projects { get; }

	}

	public interface IVisualStudioProject
	{
		void AddReference(string referenceName);
		IEnumerable<IVisualStudioFile> Files { get; }
	    string FullName { get; }

    }

	public interface IVisualStudioFile
	{
	    string FileName { get; }
        void ReplaceSelection(string text);
	    void InsertAtCaret(string text);
        void InsertAtBeginningOfDocument(string text);
		bool IsPowerShellScript { get; }
	}
}
