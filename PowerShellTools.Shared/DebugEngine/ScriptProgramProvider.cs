using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.DebugEngine
{
    // This class implments IDebugProgramProvider2. 
    // This registered interface allows the session debug manager (SDM) to obtain information about programs 
    // that have been "published" through the IDebugProgramPublisher2 interface.
    [ComVisible(true)]
    [ProvideClass]
    [Guid("08F3B557-C153-4F6C-8745-227439E55E79")]
    public class ScriptProgramProvider : IDebugProgramProvider2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptProgramProvider));

        #region Implementation of IDebugProgramProvider2

        public int GetProviderProcessData(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA[] pProcess)
        {
            Log.Debug("ProgramProvider: GetProviderProcessData");
            
            try
            {
                if (Flags.HasFlag(enum_PROVIDER_FLAGS.PFLAG_GET_PROGRAM_NODES))
                {
                    if(PowerShellToolsPackage.DebuggingService.IsAttachable(ProcessId.dwProcessId))
                    {
                        var node = new ScriptProgramNode(new ScriptDebugProcess(pPort, ProcessId.dwProcessId));
                        var programNodes = new[] { Marshal.GetComInterfaceForObject(node, typeof(IDebugProgramNode2)) };
                        var destinationArray = Marshal.AllocCoTaskMem(IntPtr.Size * programNodes.Length);

                        Marshal.Copy(programNodes, 0, destinationArray, programNodes.Length);
                        pProcess[0].Fields = enum_PROVIDER_FIELDS.PFIELD_PROGRAM_NODES;
                        pProcess[0].ProgramNodes.Members = destinationArray;
                        pProcess[0].ProgramNodes.dwCount = (uint)programNodes.Length;

                        return VSConstants.S_OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Exception while examining local running process: " + ex.Message);
            }
            
            return VSConstants.S_FALSE;
        }

        public int GetProviderProgramNode(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode)
        {
            Log.Debug("ProgramProvider: GetProviderProgramNode");
            ppProgramNode = null;
            return VSConstants.S_OK;
        }

        public int WatchForProviderEvents(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback)
        {
            Log.Debug("ProgramProvider: WatchForProviderEvents");

            return VSConstants.S_OK;
        }

        // Establishes a locale for any language-specific resources needed by the DE. This engine only supports Enu.
        int IDebugProgramProvider2.SetLocale(ushort wLangID)
        {
            Log.Debug("ProgramProvider: SetLocale");
            return VSConstants.S_OK;
        }


        #endregion
    }
}
