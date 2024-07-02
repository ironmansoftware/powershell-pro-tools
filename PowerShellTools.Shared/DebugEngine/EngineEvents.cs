using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.DebugEngine
{
    public interface IEngineEvents
    {
        /// <summary>
        /// Fires the event notifying the SDM that the debug engine has loaded.
        /// </summary>
        void EngineCreated();
        void RunspaceRequest();
        void EngineLoaded();

        /// <summary>
        /// Fires the event notifying the SDM that a program was created
        /// </summary>
        void ProgramCreated(IDebugProgram2 program);

        void ProcessCreated(IDebugProcess2 process);
        void ProcessDestroyed();
        void DebugEntryPoint();
        void ProgramDestroyed(IDebugProgram2 program);
        void OutputString(string str);
        void Break(ScriptProgramNode program);
        void Exception(ScriptProgramNode program, Exception ex);
        void Breakpoint(ScriptProgramNode program, ScriptBreakpoint breakpoint);
        void BreakpointHit(ScriptBreakpoint breakpoint, ScriptProgramNode node);
        void BreakpointEnabled(ScriptBreakpoint breakpoint, int fEnable);
        void BreakpointRemoved(ScriptBreakpoint breakpoint);
        void BreakpointAdded(ScriptBreakpoint breakpoint);
    }

    /// <summary>
    /// Fires events triggered by the debug engine.
    /// </summary>
    public class EngineEvents : IEngineEvents
    {
        #region Fields
        private readonly IDebugEngine2 _engine;
        private readonly IDebugEventCallback2 _callback;
        private static readonly ILog Log = LogManager.GetLogger(typeof (EngineEvents));
        private static Object _debuggingStateLock = new Object();
        #endregion

        #region Constructor
        public EngineEvents(IDebugEngine2 engine, IDebugEventCallback2 callback)
        {
            _engine = engine;
            _callback = callback;
        } 
        #endregion

        #region Properties
        public ScriptDebugger Debugger
        {
            get
            {
                return PowerShellToolsPackage.Debugger;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fires the event notifying the SDM that the debug engine has loaded.
        /// </summary>
        public void EngineCreated()
        {
            Log.Debug("EngineCreated");
            var iid = new Guid(EngineCreateEvent.IID);
            _callback.Event(_engine, null, null, null, new EngineCreateEvent(_engine), ref iid, EngineCreateEvent.Attributes);
        }

        public void RunspaceRequest()
        {
            Log.Debug("RunspaceRequest");
            var iid = new Guid(RunspaceRequestEvent.IID);
            _callback.Event(_engine, null, null, null, new RunspaceRequestEvent(_engine), ref iid, RunspaceRequestEvent.Attributes);
        }

        public void EngineLoaded()
        {
            Log.Debug("EngineLoaded");
            var iid = new Guid(LoadCompleteEvent.IID);
            _callback.Event(_engine, null, null, null, new LoadCompleteEvent(), ref iid, LoadCompleteEvent.Attributes);
            lock (_debuggingStateLock)
            {
                if (Debugger != null)
                {
                    Debugger.IsDebugging = true;
                }
            }
        }

        /// <summary>
        /// Fires the event notifying the SDM that a program was created
        /// </summary>
        public void ProgramCreated(IDebugProgram2 program)
        {
            Log.Debug("ProgramCreated");
            var iid = new Guid(ProgramCreateEvent.IID);
            _callback.Event(_engine, null, program, null, new ProgramCreateEvent(), ref iid, ProgramCreateEvent.Attributes);
        } 

        public void ProcessCreated(IDebugProcess2 process)
        {
            Log.Debug("ProcessCreated");
            var iid = new Guid(ProcessCreatedEvent.IID);
            _callback.Event(_engine, process, null, null, new ProcessCreatedEvent(), ref iid, ProcessCreatedEvent.Attributes);
        }

        public void ProcessDestroyed()
        {
            Log.Debug("ProcessDestroyed");
            var iid = new Guid(ProcessDestroyEvent.IID);
            _callback.Event(_engine, null, null, null, new ProcessDestroyEvent(), ref iid, ProcessDestroyEvent.Attributes);
        }

        public void DebugEntryPoint()
        {
            Log.Debug("DebugEntryPoint");
            var iid = new Guid(DebugEntryPointEvent.IID);
            _callback.Event(_engine, null, null, null, new DebugEntryPointEvent(_engine), ref iid, DebugEntryPointEvent.Attributes);
        }

        public void ProgramDestroyed(IDebugProgram2 program)
        {
            Log.Debug("ProgramDestroyed");
            var iid = new Guid(ProgramDestoryedEvent.IID);
            _callback.Event(_engine, null, program, null, new ProgramDestoryedEvent(), ref iid, ProgramDestoryedEvent.Attributes);
            lock (_debuggingStateLock)
            {
                if (Debugger != null)
                {
                    Debugger.IsDebugging = false;
                }
            }
        }

        public void OutputString(string str)
        {
            Log.Debug("OutputString");
            var iid = new Guid(OutputStringEvent.IID);
            _callback.Event(_engine, null, null, null, new OutputStringEvent(str), ref iid, OutputStringEvent.Attributes);
        }

        public void Break(ScriptProgramNode program)
        {
            Log.Debug("Break");
            var iid = new Guid(BreakEvent.IID);
            _callback.Event(_engine, null, program, program, new BreakEvent(), ref iid, BreakEvent.Attributes);
        }

        public void Exception(ScriptProgramNode program, Exception ex)
        {
            Log.Debug("Exception");
            var iid = new Guid(ExceptionEvent.IID);
            _callback.Event(_engine, null, program, program, new ExceptionEvent(program, ex), ref iid, ExceptionEvent.Attributes);
        }

        public void Breakpoint(ScriptProgramNode program, ScriptBreakpoint breakpoint)
        {
            Log.Debug("Breakpoint");
            var iid = new Guid(BreakPointEvent.IID);
            _callback.Event(_engine, null, program, program, new BreakPointEvent(breakpoint), ref iid, BreakPointEvent.Attributes);
        }

        public void BreakpointHit(ScriptBreakpoint breakpoint, ScriptProgramNode node)
        {
            Log.Debug("BreakpointHit");
            var iid = new Guid(BreakPointHitEvent.IID);
            _callback.Event(_engine, null, node, node, new BreakPointHitEvent(breakpoint), ref iid, BreakPointHitEvent.Attributes);
        }

        public void BreakpointEnabled(ScriptBreakpoint breakpoint, int fEnable)
        {
            Log.Debug("BreakpointEnabled");

            if (Debugger != null)
            {
                Debugger.BreakpointManager.EnableBreakpoint(breakpoint, fEnable);
            }
        }

        public void BreakpointRemoved(ScriptBreakpoint breakpoint)
        {
            Log.Debug("BreakpointRemoved");

            if (Debugger != null)
            {
                Debugger.BreakpointManager.RemoveBreakpoint(breakpoint);
            }
        }

        public void BreakpointAdded(ScriptBreakpoint breakpoint)
        {
            Log.Debug("BreakpointAdded");

            if (Debugger != null)
            {
                Debugger.BreakpointManager.SetBreakpoint(breakpoint);
            }
        }

        #endregion
    }

    /// <summary>
    /// The debug engine (DE) sends this interface to the session debug manager (SDM) when an instance of the DE is created. 
    /// </summary>
    public sealed class RunspaceRequestEvent : SynchronousEvent, IDebugEngineCreateEvent2, IRunspaceRequest
    {
        public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD07";
        private IDebugEngine2 m_engine;

        public RunspaceRequestEvent(IDebugEngine2 engine)
        {
            m_engine = engine;
        }

        public void SetRunspace(Runspace runspace, IEnumerable<PendingBreakpoint> breakpoints)
        {
            Engine.PendingBreakpoints = breakpoints;
        }

        public Engine Engine
        {
            get { return m_engine as Engine; }
        }

        int IDebugEngineCreateEvent2.GetEngine(out IDebugEngine2 engine)
        {
            engine = m_engine;

            return VSConstants.S_OK;
        }
    }


    public class AsynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    public class SynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    class SynchronousStoppingEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_STOPPING | (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }


    class StoppingEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// The debug engine (DE) sends this interface to the session debug manager (SDM) when an instance of the DE is created. 
    /// </summary>
    public sealed class EngineCreateEvent : AsynchronousEvent, IDebugEngineCreateEvent2
    {
        public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD06";
        private IDebugEngine2 m_engine;

        public EngineCreateEvent(IDebugEngine2 engine)
        {
            m_engine = engine;
        }

        public Engine Engine
        {
            get { return m_engine as Engine; }
        }

        int IDebugEngineCreateEvent2.GetEngine(out IDebugEngine2 engine)
        {
            engine = m_engine;

            return VSConstants.S_OK;
        }
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is attached to.
    sealed class ProcessDestroyEvent : AsynchronousEvent, IDebugProcessDestroyEvent2 
    {
        public const string IID = "3e2a0832-17e1-4886-8c0e-204da242995f";

    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is attached to.
    sealed class ProgramCreateEvent : AsynchronousEvent, IDebugProgramCreateEvent2
    {
        public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";

    }

    sealed class ProcessCreatedEvent : AsynchronousEvent, IDebugProcessCreateEvent2
    {
        public const string IID = "BAC3780F-04DA-4726-901C-BA6A4633E1CA";
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is loaded, but before any code is executed.
    sealed class LoadCompleteEvent : StoppingEvent, IDebugLoadCompleteEvent2
    {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";
    }

    sealed class DebugEntryPointEvent : AsynchronousEvent, IDebugEntryPointEvent2
    {
        public const string IID = "e8414a3e-1642-48ec-829e-5f4040e16da9";
        public object Engine { get; set; }

        public DebugEntryPointEvent(IDebugEngine2 mEngine)
        {
            Engine = mEngine;
        }
    }

    sealed class ProgramDestoryedEvent : AsynchronousEvent, IDebugProgramDestroyEvent2
    {
        public const string IID = "e147e9e3-6440-4073-a7b7-a65592c714b5";

        public int GetExitCode(out uint pdwExit)
        {
            pdwExit = 0;
            return VSConstants.S_OK;
        }
    }

    sealed class OutputStringEvent : AsynchronousEvent, IDebugOutputStringEvent2
    {
        public const string IID = "569c4bb1-7b82-46fc-ae28-4536ddad753e";

        private string _outString;
        public OutputStringEvent(string outString)
        {
            _outString = outString;
        }

        #region Implementation of IDebugOutputStringEvent2

        public int GetString(out string pbstrString)
        {
            pbstrString = _outString;
            return VSConstants.S_OK;
        }

        #endregion
    }

    sealed class BreakEvent : StoppingEvent, IDebugBreakEvent2
    {
        public const string IID = "c7405d1d-e24b-44e0-b707-d8a5a4e1641b";
    }

   

    sealed class BreakPointEvent : AsynchronousEvent, IDebugBreakpointBoundEvent2
    {
        public const string IID = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

        private ScriptBreakpoint _breakpoint;

        public BreakPointEvent(ScriptBreakpoint breakpoint)
        {
            _breakpoint = breakpoint;
        }

        #region Implementation of IDebugBreakpointBoundEvent2

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = _breakpoint;
            return VSConstants.S_OK;
        }

        public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = _breakpoint;
            return VSConstants.S_OK;
        }

        #endregion
    }

    sealed class BreakPointHitEvent : StoppingEvent, IDebugBreakpointEvent2
    {
        public const string IID = "501c1e21-c557-48b8-ba30-a1eab0bc4a74";

        private ScriptBreakpoint _breakpoint;

        public BreakPointHitEvent(ScriptBreakpoint breakpoint)
        {
            _breakpoint = breakpoint;
        }

        #region Implementation of IDebugBreakpointEvent2

        public int EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            Trace.WriteLine("BreakpointEvent: EnumBreakpoints");
            ppEnum = _breakpoint;
            return VSConstants.S_OK;
        }


        #endregion
    }

    sealed class ExceptionEvent : StoppingEvent, IDebugExceptionEvent2
    {
        public const string IID = "51a94113-8788-4a54-ae15-08b74ff922d0";

        private readonly IDebugProgram2 _program;
        private readonly Exception _exception;

        public ExceptionEvent(IDebugProgram2 program, Exception ex)
        {
            _program = program;
            _exception = ex;
        }

        #region Implementation of IDebugExceptionEvent2

        public int GetException(EXCEPTION_INFO[] pExceptionInfo)
        {
            pExceptionInfo[0].pProgram = _program;
            pExceptionInfo[0].bstrExceptionName = _exception.GetType().ToString();
            pExceptionInfo[0].dwState = enum_EXCEPTION_STATE.EXCEPTION_CANNOT_BE_CONTINUED | enum_EXCEPTION_STATE.EXCEPTION_STOP_SECOND_CHANCE;
            pExceptionInfo[0].guidType = new Guid(GuidList.PowerShellLanguage);

            string name;
            _program.GetName(out name);

            pExceptionInfo[0].bstrProgramName = name;

            return VSConstants.S_OK;
        }

        public int GetExceptionDescription(out string pbstrDescription)
        {
            pbstrDescription = _exception.Message;

            if (_exception is RuntimeException)
            {
                var runtimeException = _exception as RuntimeException;
                pbstrDescription += Environment.NewLine;
                pbstrDescription += runtimeException.ErrorRecord.ScriptStackTrace;
            }

            return VSConstants.S_OK;
        }

        public int CanPassToDebuggee()
        {
            return VSConstants.S_FALSE;
        }

        public int PassToDebuggee(int fPass)
        {
            fPass = VSConstants.S_FALSE;
            return VSConstants.S_OK;
        }

        #endregion
    }
    
}
