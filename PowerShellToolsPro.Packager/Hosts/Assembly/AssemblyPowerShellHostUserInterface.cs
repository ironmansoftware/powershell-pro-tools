using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace PowerShellToolsPro.Packager
{
	internal class AssemblyPowerShellHostUserInterface : PSHostUserInterface
	{
		public override PSHostRawUserInterface RawUI
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			throw new NotImplementedException();
		}

		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			throw new NotImplementedException();
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			throw new NotImplementedException();
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			throw new NotImplementedException();
		}

		public override string ReadLine()
		{
			throw new NotImplementedException();
		}

		public override SecureString ReadLineAsSecureString()
		{
			throw new NotImplementedException();
		}

		public override void Write(string value)
		{
			//TODO: 
		}

		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			//TODO: 
		}

		public override void WriteDebugLine(string message)
		{
			//TODO: 
		}

		public override void WriteErrorLine(string value)
		{
			//TODO: 
		}

		public override void WriteLine()
		{
			//TODO: 
		}

		public override void WriteLine(string value)
		{
			//TODO: 
		}

		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			//TODO:
		}

		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			//TODO: Maybe? 
		}

		public override void WriteVerboseLine(string message)
		{
			//TODO: 
		}

		public override void WriteWarningLine(string message)
		{
			//TODO: 
		}
	}
}
