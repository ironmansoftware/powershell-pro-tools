using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    [ComImport]
    [ComVisible(true)]
    [Guid("F614FCE7-8560-44F2-AAB5-B5EBA1635E7E")]
    public interface IDataProvider
    {
        void GetModules(Action<List<IPowerShellModule>> callback);
        void GetCommands(Action<List<IPowerShellCommand>> callback);
        void GetCommandHelp(IPowerShellCommand command, Action<string> callback);
        void GetCommandMetaData(IPowerShellCommand command, Action<IPowerShellCommandMetadata> callback);
    }
}
