using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer.Search
{
    public class SearchTask : VsSearchTask
    {
        private ISearchTaskTarget _searchTarget;

        public SearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, ISearchTaskTarget searchTarget)
            : base(dwCookie, pSearchQuery, pSearchCallback)
        {
            _searchTarget = searchTarget;
        }

        protected override void OnStartSearch()
        {
            var sourceItems = _searchTarget.SearchSourceData();
            var resultItems = new List<IPowerShellCommand>();
            uint resultCount = 0;
            ErrorCode = VSConstants.S_OK;

            try
            {
                string searchString = this.SearchQuery.SearchString;
                uint progress = 0;

                foreach (IPowerShellCommand item in sourceItems)
                {
                    if (item.Name.ToLowerInvariant().Contains(searchString.ToLowerInvariant()) |
                        item.ModuleName.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
                    {
                        resultItems.Add(item);
                        resultCount++;
                    }

                    SearchCallback.ReportProgress(this, progress++, (uint)sourceItems.Count); 
                }
            }
            catch
            {
                ErrorCode = VSConstants.E_FAIL;
            }
            finally
            {
                ThreadHelper.Generic.Invoke(() =>
                {
                    _searchTarget.SearchResultData(resultItems); 
                });

                SearchResults = resultCount;
            }

            base.OnStartSearch();
        }

        protected override void OnStopSearch()
        {
            SearchResults = 0;
        }
    }
}
