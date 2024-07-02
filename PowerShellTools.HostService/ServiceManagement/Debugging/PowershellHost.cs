using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using PowerShellTools.Common.Debugging;
using System.Runtime.InteropServices;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    public class HostUi : PSHostUserInterface
    {
        private readonly PowerShellDebuggingService _debuggingService;
        private PSHostRawUserInterface _rawUI;

        public HostUi(PowerShellDebuggingService debugger)
        {
            _debuggingService = debugger;
            _rawUI = new PowerShellRawHost(_debuggingService);
        }

        public Action<long, ProgressRecord> OutputProgress { get; set; }

        public Action<string> OutputString { get; set; }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return _rawUI; 
            }
        }

        public override string ReadLine()
        {
            return ReadLineFromUI(DebugEngineConstants.ReadHostDialogTitle);
        }

        public override SecureString ReadLineAsSecureString()
        {
            return new SecureString();
        }

        public override void Write(string value)
        {
            TryOutputString(value);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            TryOutputString(value);
        }

        public override void WriteLine(string value)
        {
            TryOutputString(value + Environment.NewLine);
        }

        public override void WriteErrorLine(string value)
        {
            TryOutputString("[ERROR] " + value + Environment.NewLine);
        }

        public override void WriteDebugLine(string message)
        {
            TryOutputString("[DEBUG] " + message + Environment.NewLine);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            TryOutputProgress(sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            TryOutputString("[VERBOSE] " + message + Environment.NewLine);
        }

        public override void WriteWarningLine(string message)
        {
            TryOutputString("[WARNING] " + message + Environment.NewLine);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message,
            Collection<FieldDescription> descriptions)
        {
            string promptMessage = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                promptMessage = string.Format("{0}{2}{1}", caption, message, Environment.NewLine);
                this.WriteLine(promptMessage);
            }

            Dictionary<string, PSObject> results =
                     new Dictionary<string, PSObject>();
            foreach (FieldDescription fd in descriptions)
            {
                this.Write(fd.Name + ": ");

                switch (fd.ParameterTypeFullName.ToLowerInvariant())
                {
                    case Constants.SecureStringFullTypeName:
                        SecureString secString = this.ReadLineAsSecureString(promptMessage, fd.Name);
                        results[fd.Name] = PSObject.AsPSObject(secString);
                        this.WriteLine(string.Empty);
                        break;

                    case Constants.PSCredentialFullTypeName:
                        PSCredential psCred = this.ReadPSCredential(
                            Resources.CredentialDialogCaption,
                            Resources.CredentialDialogMessage,
                            string.Empty,
                            string.Empty,
                            PSCredentialTypes.Generic | PSCredentialTypes.Domain,
                            PSCredentialUIOptions.Default);
                        if (psCred != null)
                        {
                            results[fd.Name] = PSObject.AsPSObject(psCred);
                        }
                        break;

                    default:
                        string userData = this.ReadLineFromUI(promptMessage, fd.Name);
                        if (userData == null)
                        {
                            return null;
                        }
                        this.WriteLine(userData);

                        results[fd.Name] = PSObject.AsPSObject(userData);
                        break;
                }
            }

            return results;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName)
        {
            return CredUiPromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default,
                PSCredentialUIOptions.Default);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return CredUiPromptForCredential(caption, message, userName, targetName, allowedCredentialTypes,
                options);
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            var result = ReadChoiceFromUI(caption, message, choices, defaultChoice);
            
            if (result == -1)
            {
                throw new PipelineStoppedException();
            }
            return result;
        }

        // System.Management.Automation.HostUtilities
        internal PSCredential CredUiPromptForCredential(string caption, string message, string userName,
            string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return this.ReadPSCredential(caption, message, userName, targetName,
                    allowedCredentialTypes, options);
        }

        private SecureString ReadLineAsSecureString(string message, string name)
        {
            var s = new SecureString();

            if (_debuggingService.CallbackService != null)
            {
                // SecureString is not serializable, so we have to encapsulate it into an PSCredential(ISeralizable) object
                // So that is can pass over the wcf channel between VS and remote PowerShell host process.
                s = _debuggingService.CallbackService.ReadSecureStringPrompt(message, name).Password;
            }

            return s;
        }

        private PSCredential ReadPSCredential(string caption, string message, string userName,
            string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            PSCredential psCred = null;

            if (_debuggingService.CallbackService != null)
            {
                psCred = _debuggingService.CallbackService.GetPSCredentialPrompt(caption, message, userName, targetName,
                    allowedCredentialTypes, options);
            }

            return psCred;
        }

        private void TryOutputProgress(long sourceId, ProgressRecord record)
        {
            _debuggingService.NotifyOutputProgress(sourceId, record);

            if (OutputProgress != null)
                OutputProgress(sourceId, record);
        }

        private void TryOutputString(string val)
        {
            _debuggingService.NotifyOutputString(val);

            if (OutputString != null)
                OutputString(val);
        }

        private string ReadLineFromUI(string message)
        {
            return ReadLineFromUI(message, string.Empty);
        }

        private string ReadLineFromUI(string message, string parameterName)
        {
            if (_debuggingService.CallbackService != null)
            {
                return _debuggingService.CallbackService.ReadHostPrompt(message, parameterName);
            }

            return string.Empty;
        }

        private int ReadChoiceFromUI(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (_debuggingService.CallbackService != null)
            {
                IList<ChoiceItem> items = choices.Select(c => new ChoiceItem(c)).ToList();

                return _debuggingService.CallbackService.ReadHostPromptForChoices(caption, message, items, defaultChoice);
            }

            return -1;
        }
    }
}
