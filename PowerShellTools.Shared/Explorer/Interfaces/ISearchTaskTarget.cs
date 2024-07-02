using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    public interface ISearchTaskTarget
    {
        List<IPowerShellCommand> SearchSourceData();
        void SearchResultData(List<IPowerShellCommand> result);
        void ClearSearch();
    }
}
