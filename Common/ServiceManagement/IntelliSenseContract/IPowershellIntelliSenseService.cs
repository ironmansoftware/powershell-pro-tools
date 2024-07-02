using Common;
using System;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
{
    /// <summary>
    /// Powershell service.
    /// </summary>
    public interface IPowerShellIntelliSenseService
    {
        Task RequestCompletionResultsAsync(string scriptUpToCaret, int carePosition, int requestWindowId, long triggerTimeTicks);

        Task GetDummyCompletionListAsync();
        void GetDummyCompletionList();

        event EventHandler<EventArgs<CompletionResultList, int>> CompletionListUpdated;
    }
}
