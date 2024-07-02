using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.ServiceManagement;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.Project
{
    public class PowerShellProjectLauncher : IProjectLauncher
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellProjectLauncher));
        private readonly PowerShellProjectNode _project;
        private static DebugEventManager previousEventManager;

        internal PowerShellProjectLauncher(PowerShellProjectNode project)
        {
            _project = project;
        }


        public PowerShellProjectLauncher()
        {
        }

        public int LaunchProject(bool debug)
        {
            string script = String.Empty;
            string startupFile = _project.GetStartupFile();

            if (startupFile != null && startupFile != ProjectConstants.CurrentlyOpenScript)
            {
                script = startupFile;
            }
            else
            {
                var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
                if (dte2 != null)
                {
                    if (dte2.ActiveDocument != null)
                    {
                        script = dte2.ActiveDocument.FullName;
                    }
                }
            }

            Log.Debug("PowerShellProjectLauncher.LaunchProject");
            var debugger = (IVsDebugger)Package.GetGlobalService(typeof(IVsDebugger));
            var shell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));

            var info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            info.bstrExe = script;
            info.bstrCurDir = Path.GetDirectoryName(info.bstrCurDir);

            try
            {
                info.bstrArg = _project.GetUnevaluatedProperty(ProjectConstants.DebugArguments);
            }
            catch
            {
                info.bstrArg = null;
            }

            info.bstrRemoteMachine = null; // debug locally
            info.fSendStdoutToOutputWindow = 0; // Let stdout stay with the application.
            info.clsidCustom = new Guid("{43ACAB74-8226-4920-B489-BFCF05372437}");
            // Set the launching engine the sample engine guid
            info.grfLaunch = 0;
            //info.clsidPortSupplier = new Guid("FEF0E138-4F86-467D-B5FB-46888D0D1A41");

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);

            if (PowerShellToolsPackage.Debugger == null)
            {
                throw new Exception("PowerShell Tools for Visual Studio host has not started. ");
            }

            var eventManager = new DebugEventManager(PowerShellToolsPackage.Debugger.Runspace);

            if (previousEventManager != null)
            {
                if (debugger.UnadviseDebugEventCallback(previousEventManager) != VSConstants.S_OK)
                {
                    Log.Debug("Failed to unadvise the UI of debug events.");
                }
                previousEventManager = null;
            }

            previousEventManager = eventManager;

            if (debugger.AdviseDebugEventCallback(eventManager) != VSConstants.S_OK)
            {
                Log.Debug("Failed to advise the UI of debug events.");
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            try
            {
                debugger.LaunchDebugTargets(1, pInfo);
                string outstr;
                shell.GetErrorInfo(out outstr);

                if (!String.IsNullOrWhiteSpace(outstr))
                {
                    Log.Debug("Error:" + outstr);
                }
            }
            finally
            {
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            return VSConstants.S_OK;
        }

        public int LaunchFile(string file, bool debug)
        {
            return VSConstants.S_OK;
        }

        public int LaunchSelection(string selection)
        {
            if (PowerShellToolsPackage.Debugger == null)
            {
                MessageBox.Show(
                        Resources.PowerShellHostInitializingNotComplete,
                        Resources.MessageBoxErrorTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                return VSConstants.S_OK;
            }

            Log.Debug("PowerShellProjectLauncher.LaunchSelection");
            var debugger = (IVsDebugger)Package.GetGlobalService(typeof(IVsDebugger));
            var shell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));

            var info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            info.bstrExe = "Selection";
            info.bstrCurDir = Environment.CurrentDirectory;
            info.bstrOptions = selection;
            info.bstrArg = null; // no command line parameters
            info.bstrRemoteMachine = null; // debug locally
            info.fSendStdoutToOutputWindow = 0; // Let stdout stay with the application.
            info.clsidCustom = new Guid("{43ACAB74-8226-4920-B489-BFCF05372437}");
            // Set the launching engine the sample engine guid
            info.grfLaunch = 0;
            //info.clsidPortSupplier = new Guid("FEF0E138-4F86-467D-B5FB-46888D0D1A41");

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);

            var eventManager = new DebugEventManager(PowerShellToolsPackage.Debugger.Runspace);

            if (previousEventManager != null)
            {
                if (debugger.UnadviseDebugEventCallback(previousEventManager) != VSConstants.S_OK)
                {
                    Log.Debug("Failed to unadvise the UI of debug events.");
                }
                previousEventManager = null;
            }

            previousEventManager = eventManager;

            if (debugger.AdviseDebugEventCallback(eventManager) != VSConstants.S_OK)
            {
                Log.Debug("Failed to advise the UI of debug events.");
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            try
            {
                debugger.LaunchDebugTargets(1, pInfo);
                string outstr;
                shell.GetErrorInfo(out outstr);

                if (!String.IsNullOrWhiteSpace(outstr))
                {
                    Log.Debug("Error:" + outstr);
                }
            }
            finally
            {
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            return VSConstants.S_OK;
        }

        public int LaunchFile(string file, bool debug, string args = null)
        {
            if (PowerShellToolsPackage.Debugger == null)
            {
                MessageBox.Show(
                           Resources.PowerShellHostInitializingNotComplete,
                           Resources.MessageBoxErrorTitle,
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

                return VSConstants.S_OK;
            }

            Log.Debug("PowerShellProjectLauncher.LaunchFile");
            var debugger = (IVsDebugger)Package.GetGlobalService(typeof(IVsDebugger));
            var shell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));

            var info = new VsDebugTargetInfo();
            if (args != null)
            {
                info.bstrArg = args;
            }
            
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            info.bstrExe = file;
            info.bstrCurDir = Path.GetDirectoryName(info.bstrExe);

            if (_project != null)
            {
                try
                {
                    info.bstrArg = _project.GetUnevaluatedProperty(ProjectConstants.DebugArguments);
                }
                catch
                {
                    info.bstrArg = null;
                }
            }

            info.bstrRemoteMachine = null; // debug locally
            info.fSendStdoutToOutputWindow = 0; // Let stdout stay with the application.
            info.clsidCustom = new Guid("{43ACAB74-8226-4920-B489-BFCF05372437}");
            // Set the launching engine the sample engine guid
            info.grfLaunch = 0;
            //info.clsidPortSupplier = new Guid("FEF0E138-4F86-467D-B5FB-46888D0D1A41");

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);

            var eventManager = new DebugEventManager(PowerShellToolsPackage.Debugger.Runspace);

            if (previousEventManager != null)
            {
                if (debugger.UnadviseDebugEventCallback(previousEventManager) != VSConstants.S_OK)
                {
                    Log.Debug("Failed to unadvise the UI of debug events.");
                }
                previousEventManager = null;
            }

            previousEventManager = eventManager;

            if (debugger.AdviseDebugEventCallback(eventManager) != VSConstants.S_OK)
            {
                Log.Debug("Failed to advise the UI of debug events.");
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            try
            {
                debugger.LaunchDebugTargets(1, pInfo);
                string outstr;
                shell.GetErrorInfo(out outstr);

                if (!String.IsNullOrWhiteSpace(outstr))
                {
                    Log.Debug("Error:" + outstr);
                }
            }
            finally
            {
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            return VSConstants.S_OK;
        }
    }
}
