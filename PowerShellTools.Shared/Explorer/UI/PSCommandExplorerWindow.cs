using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Common;
using PowerShellTools.Explorer.Search;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("dd9b7693-1385-46a9-a054-06566904f861")]
    public class PSCommandExplorerWindow : ToolWindowPane, IHostWindow
    {
        private readonly IDataProvider _dataProvider;
        private readonly PSCommandExplorer _commandExplorer;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public PSCommandExplorerWindow() :
            base(null)
        {
            _dataProvider = new DataProvider();
            _commandExplorer = new PSCommandExplorer(this, _dataProvider);

            // Set the window title reading it from the resources.
            this.Caption = Resources.ExplorerWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = new HostControl();
            ShowCommandExplorer();
        }

        public HostControl ContentHost
        {
            get
            {
                return (HostControl)base.Content;
            }
            set
            {
                ((HostControl)base.Content).Content = value;
            }
        }

        public void ShowCommandExplorer()
        {
            SetCaption(string.Empty);
            ContentHost.Content = _commandExplorer;
        }

        public void ShowParameterEditor(IPowerShellCommand command)
        {
            var view = new PSParameterEditor(this, _dataProvider);
            ContentHost.Content = view;
            ((PSParameterEditorViewModel)view.DataContext).LoadCommand(command);
        }

        public void SetCaption(string caption)
        {
            if (string.IsNullOrWhiteSpace(caption))
            {
                Caption = Resources.ExplorerWindowTitle;
            }
            else
            {
                Caption = string.Format("{0} - {1}", Resources.ExplorerWindowTitle, caption);
            }
        }

        public override bool SearchEnabled
        {
            get 
            {
                return ContentHost.IsHostingSearchTarget; 
            }
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            ShowCommandExplorer();

            ISearchTaskTarget searchTarget = ((UserControl)ContentHost.Content).DataContext as ISearchTaskTarget;
            if (searchTarget == null || pSearchQuery == null || pSearchCallback == null)
            {
                return null;
            }

            return new SearchTask(dwCookie, pSearchQuery, pSearchCallback, searchTarget);
        }

        public override void ClearSearch()
        {
            ISearchTaskTarget searchTarget = ((UserControl)ContentHost.Content).DataContext as ISearchTaskTarget;
            if (searchTarget != null)
            {
                searchTarget.ClearSearch();
            }
        }

        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings)
        {
            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.SearchStartTypeProperty.Name,
                 (uint)VSSEARCHSTARTTYPE.SST_DELAYED);
            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.SearchProgressTypeProperty.Name,
                 (uint)VSSEARCHPROGRESSTYPE.SPT_DETERMINATE);
            Utilities.SetValue(pSearchSettings,
                SearchSettingsDataSource.SearchWatermarkProperty.Name,
                 "Search PowerShell commands");
        }
    }
}
