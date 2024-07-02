using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace PowerShellToolsPro
{
	[Export(typeof(IVisualStudio))]
	public class VisualStudio : IVisualStudio
	{
		private readonly DTE _dte;
		private readonly IVsActivityLog _log;

		public VisualStudio()
		{
			_dte = (DTE)Package.GetGlobalService(typeof(DTE));
			_log = Package.GetGlobalService(typeof(SVsActivityLog)) as IVsActivityLog;
		}

		public void Log(LogLevel level, string message)
		{
			_log?.LogEntry((uint)level, "PowerShellProTools", message);
		}

		public IVisualStudioProjects Projects => new VisualStudioProjects(_dte);
		public IVisualStudioFile GetFile(string fileName)
		{
			var projectItem = _dte.Solution.FindProjectItem(fileName);
			return new VisualStudioFile(projectItem);
		}

	    public IVisualStudioProject ActiveWindowProject
	    {
	        get { return new VisualStudioProject(_dte.ActiveWindow.Project); }
	    }

		public IVisualStudioFile ActiveFile
		{
			get 
			{ 
				if (_dte.ActiveDocument?.ProjectItem == null)
                {
					return null;
                }
				return new VisualStudioFile(_dte.ActiveDocument.ProjectItem); 
			}
		}
	}

	public class VisualStudioProjects : IVisualStudioProjects
	{
		private readonly DTE _dte;

		internal VisualStudioProjects(DTE dte)
		{
			_dte = dte;
		}

		public IVisualStudioProject GetProjectContainingFile(string fileName)
		{
			var projectItem = _dte.Solution.FindProjectItem(fileName);
			var containingProject = projectItem.ContainingProject;
			return new VisualStudioProject(containingProject);
		}

	    public IEnumerable<IVisualStudioProject> Projects
	    {
	        get
	        {
	            var projects = new List<VisualStudioProject>();
	            foreach (Project project in _dte.Solution.Projects)
	            {
	                projects.Add(new VisualStudioProject(project));
                }

	            return projects;
	        }
	    }
	}

	public class VisualStudioProject : IVisualStudioProject
	{
		private readonly Project _project;

		internal VisualStudioProject(Project project)
		{
			_project = project;
		}

		public void AddReference(string referenceName)
		{
		    var proj = _project.GetPropertyValue<object>("Project");
            var vsproject = proj.GetPrivatePropertyValue<VSProject>("VSProject");

            var identity = referenceName.Split(',').First().Trim();
            
            foreach(Reference reference in vsproject.References) {
                if (reference.Identity == identity) return;
            }

		    vsproject.References.Add(referenceName);
		}

	    public IEnumerable<IVisualStudioFile> Files
	    {
	        get
	        {
				if (_project == null) return new IVisualStudioFile[0];

	            return GetFilesFromProjectItems(_project.ProjectItems);
	        }
	    }

	    private List<IVisualStudioFile> GetFilesFromProjectItems(ProjectItems projectItems)
	    {
	        var files = new List<IVisualStudioFile>();
	        foreach (ProjectItem item in projectItems)
	        {
	            //Folder
	            if (item.Kind.Equals("{6bb5f8ef-4483-11d3-8bcf-00c04f8ec28c}", StringComparison.OrdinalIgnoreCase))
	            {
	                var moreFiles = GetFilesFromProjectItems(item.ProjectItems);
                    files.AddRange(moreFiles);
	            }

                //File
	            if (item.Kind.Equals("{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}", StringComparison.OrdinalIgnoreCase))
	            {
	                files.Add(new VisualStudioFile(item));
                }
	        }
	        return files.OrderBy(m => m.FileName).ToList();
        }

	    public string FullName
	    {
	        get { return _project.FullName; }
	    }
	}

	public class VisualStudioFile : IVisualStudioFile
	{
		private readonly ProjectItem _projectItem;

		internal VisualStudioFile(ProjectItem projectItem)
		{
			_projectItem = projectItem;
		}

	    public string FileName
	    {
	        get
	        {
	            return _projectItem.FileNames[0];
	        }
	    }

		public bool IsPowerShellScript
        {
			get
            {
				return FileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase);
            }
        }

	    public void InsertAtCaret(string text)
	    {
	        var textDoc = (TextDocument)_projectItem.Document.Object("TextDocument");
	        var editPoint = textDoc.Selection.ActivePoint.CreateEditPoint();
	        editPoint.Insert(text);
	    }

        public void InsertAtBeginningOfDocument(string text)
		{
			var textDoc = (TextDocument)_projectItem.Document.Object("TextDocument");
			var editPoint = textDoc.StartPoint.CreateEditPoint();
			editPoint.Insert(text);
			_projectItem.Document.Save();
		}

        public void ReplaceSelection(string text)
        {
            var textDoc = (TextDocument)_projectItem.Document.Object("TextDocument");

            var editPoint = textDoc.Selection.ActivePoint.CreateEditPoint();
            editPoint.ReplaceText(textDoc.Selection.BottomPoint, text, 0);
            _projectItem.Document.Save();
        }
    }
}
