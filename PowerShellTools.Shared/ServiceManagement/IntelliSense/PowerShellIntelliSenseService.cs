using Common;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement
{
    /// <summary>
    /// Represents a implementation of the service contract.
    /// </summary>
    public sealed class PowerShellIntelliSenseService : IPowerShellIntelliSenseService
    {
        private long _requestTrigger;
        private static object _syncLock = new object();

        public event EventHandler<EventArgs<CompletionResultList, int>> CompletionListUpdated;

        private Runspace _runspace
        {
            get
            {
                return PowerShellDebuggingService.Runspace;
            }
            set { }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="callback">Callback context object</param>
        public PowerShellIntelliSenseService()
        {
        }

        #region IAutoCompletionService Members

        /// <summary>
        /// Calculate the completion results list based on the script we have and the caret position.
        /// </summary>
        /// <param name="script">The active script.</param>
        /// <param name="caretPosition">The caret position.</param>
        /// <param name="triggerTag">Tag(incremental long) indicating the trigger sequence in client side</param>
        /// <returns>A completion results list.</returns>
        public async Task RequestCompletionResultsAsync(string script, int caretPosition, int requestWindowId, long triggerTag)
        {
            ServiceCommon.Log("Intellisense request received, caret position: {0}", caretPosition.ToString());

            if (_requestTrigger == 0 || triggerTag > _requestTrigger)
            {
                ServiceCommon.Log("Procesing request, caret position: {0}", caretPosition.ToString());
                DismissGetCompletionResults();
                await ProcessCompletion(script, caretPosition, requestWindowId, triggerTag); // triggering new request processing
            }
        }

        /// <summary>
        /// Suspecting this is a powershell bug, the first time you call CommandCompletion.CompleteInput, it takes much longer than usual.
        /// We are using this dummy call during intializing to warm it up.
        /// </summary>
        public void GetDummyCompletionList()
        {
            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList("Write-", 6, _runspace);
        }

        private async Task ProcessCompletion(string script, int caretPosition, int requestWindowId, long triggerTag)
        {
            lock (_syncLock)
            {
                _requestTrigger = triggerTag;
            }

            // Start process the existing waiting request, should only be one
            await Task.Run(async () =>
            {
                await Task.CompletedTask;
                try
                {
                    CommandCompletion commandCompletion = null;

                    lock(ServiceCommon.RunspaceLock)
                    {
                        if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                        {
                            commandCompletion = CommandCompletionHelper.GetCommandCompletionList(script, caretPosition, _runspace);
                        }
                        else
                        {
                            // we'll handle it when we work on giving intellisense for debugging command
                            // for now we just simply return with null for this request to complete.
                        }
                    }

                    ServiceCommon.LogCallbackEvent("Callback intellisense at position {0}", caretPosition);

                    if (CompletionListUpdated != null)
                    {
                        CompletionListUpdated(this, new EventArgs<CompletionResultList, int>(CompletionResultList.FromCommandCompletion(commandCompletion), requestWindowId));
                    }

                    // Reset trigger
                    lock (_syncLock)
                    {
                        _requestTrigger = 0;
                    }
                }
                catch (Exception ex)
                {
                    ServiceCommon.Log("Failed to retrieve the completion list per request due to exception: {0}", ex.Message);
                }
            });
        }

        /// <summary>
        /// Dismiss the current running completion request
        /// </summary>
        private void DismissGetCompletionResults()
        {
            try
            {
                CommandCompletionHelper.DismissCommandCompletionListRequest();
            }
            catch
            {
                ServiceCommon.Log("Failed to stop the existing one.");
            }
        }

        public Task GetDummyCompletionListAsync()
        {
            GetDummyCompletionList();
            return Task.CompletedTask;
        }

        #endregion
    }
}
