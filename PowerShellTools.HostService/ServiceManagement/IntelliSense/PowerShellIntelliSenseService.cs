using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement
{
    /// <summary>
    /// Represents a implementation of the service contract.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [PowerShellServiceHostBehavior]
    public sealed class PowerShellIntelliSenseService : IPowerShellIntelliSenseService
    {
        private readonly Runspace _testRunspace;
        private long _requestTrigger;
        private IIntelliSenseServiceCallback _callback;
        private static object _syncLock = new object();

        private Runspace _runspace
        {
            get
            {
                if (_testRunspace != null)
                {
                    return _testRunspace;
                }
                return PowerShellDebuggingService.Runspace;
            }
            set { }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public PowerShellIntelliSenseService() { }

        /// <summary>
        /// Ctor (unit test hook)
        /// </summary>
        /// <param name="callback">Callback context object (unit test hook)</param>
        public PowerShellIntelliSenseService(IIntelliSenseServiceCallback callback)
            : this()
        {
            _callback = callback;
            _testRunspace = RunspaceFactory.CreateRunspace();
            _testRunspace.Open();
        }

        #region IAutoCompletionService Members

        /// <summary>
        /// Calculate the completion results list based on the script we have and the caret position.
        /// </summary>
        /// <param name="script">The active script.</param>
        /// <param name="caretPosition">The caret position.</param>
        /// <param name="triggerTag">Tag(incremental long) indicating the trigger sequence in client side</param>
        /// <returns>A completion results list.</returns>
        public void RequestCompletionResults(string script, int caretPosition, int requestWindowId, long triggerTag)
        {
            ServiceCommon.Log("Intellisense request received, caret position: {0}", caretPosition.ToString());

            if (_requestTrigger == 0 || triggerTag > _requestTrigger)
            {
                ServiceCommon.Log("Procesing request, caret position: {0}", caretPosition.ToString());
                DismissGetCompletionResults();
                ProcessCompletion(script, caretPosition, requestWindowId, triggerTag); // triggering new request processing
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

        /// <summary>
        /// Get error from parsing
        /// </summary>
        /// <param name="spanText">Script text</param>
        /// <returns></returns>
        public ParseErrorItem[] GetParseErrors(string spanText)
        {
            ParseError[] errors;
            Token[] tokens;
            Parser.ParseInput(spanText, out tokens, out errors);
            return (from item in errors
                    select new ParseErrorItem(item.Message,
                                              item.Extent.StartOffset,
                                              item.Extent.EndOffset)).ToArray();
        }

        private void ProcessCompletion(string script, int caretPosition, int requestWindowId, long triggerTag)
        {
            lock (_syncLock)
            {
                _requestTrigger = triggerTag;
            }

            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IIntelliSenseServiceCallback>();
            }

            // Start process the existing waiting request, should only be one
            Task.Run(() =>
            {
                try
                {
                    CommandCompletion commandCompletion = null;

                    lock (ServiceCommon.RunspaceLock)
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
                    _callback.PushCompletionResult(CompletionResultList.FromCommandCompletion(commandCompletion), requestWindowId);

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

        #endregion
    }
}
