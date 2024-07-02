using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.DebugEngine
{
    public class ScriptDocumentContext : IDebugDocumentContext2, IDebugCodeContext2, IEnumDebugCodeContexts2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptDocumentContext));
        private readonly string _fileName;
        private readonly string _description;
        private readonly int _startLine;
        private readonly int _endLine;
        private readonly int _startColumn;
        private readonly int _endColumn;

        public ScriptDocumentContext(string fileName, string description, int startLine, int startColumn)
            : this(fileName, description, startLine, startLine, startColumn, startColumn)
        {
        }

        public ScriptDocumentContext(string fileName, string description, int startLine, int endLine, int startColumn, int endColumn)
        {
            _fileName = fileName;
            _startLine = startLine;
            _endLine = endLine;
            _startColumn = startColumn;
            _endColumn = endColumn;
            _description = description;
        }

        #region Implementation of IDebugDocumentContext2

        public int GetDocument(out IDebugDocument2 ppDocument)
        {
            Log.Debug("ScriptDocumentContext: GetDocument");
            ppDocument = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            Log.Debug("ScriptDocumentContext: GetName");
            pbstrFileName = _fileName;
            return VSConstants.S_OK;
        }

        public int EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            Log.Debug("ScriptDocumentContext: EnumCodeContexts");
            ppEnumCodeCxts = this;
            return VSConstants.S_OK;
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            Log.Debug("ScriptDocumentContext: GetLanguageInfo");
            pguidLanguage = Guid.Empty;
            pbstrLanguage = "PowerShell";
            return VSConstants.S_OK;
        }

        public int GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            Log.Debug("ScriptDocumentContext: GetStatementRange");

            pBegPosition[0].dwLine = (uint)_startLine;
            pBegPosition[0].dwColumn = (uint)_startColumn;
            pEndPosition[0].dwLine = (uint)_endLine;
            pEndPosition[0].dwColumn = (uint)_endColumn;

            return VSConstants.S_OK;
        }

        public int GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            Log.Debug("ScriptDocumentContext: GetSourceRange");

            pBegPosition[0].dwLine = (uint)_startLine;
            pBegPosition[0].dwColumn = (uint)_startColumn;
            pEndPosition[0].dwLine = (uint)_endLine;
            pEndPosition[0].dwColumn = (uint)_endColumn;

            return VSConstants.S_OK;
        }

        public int Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            Log.Debug("ScriptDocumentContext: Compare");
            pdwDocContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            Log.Debug("ScriptDocumentContext: Seek");
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion


        #region Implementation of IDebugCodeContext2

        public int GetName(out string pbstrName)
        {
            Log.Debug("ScriptDocumentContext: IDebugCodeContext2.GetName");
            pbstrName = "TestName";
            return VSConstants.S_OK;
        }

        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            pinfo[0].dwFields = 0;

            Log.Debug("ScriptDocumentContext: IDebugCodeCotnext2.GetInfo");

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0)
            {
                pinfo[0].bstrFunction = "TestFunc";
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION;    
            }

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0)
            {
                pinfo[0].bstrModuleUrl = _fileName;
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL;
            }
            
            return VSConstants.S_OK;
        }

        public int Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            Log.Debug("ScriptDocumentContext: IDebugCodeContext2.Add");
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            Log.Debug("ScriptDocumentContext: IDebugCodeContext2.Subtract");
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            Log.Debug("ScriptDocumentContext: IDebugCodeContext2.Compare");
            pdwMemoryContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            Log.Debug("ScriptDocumentContext: IDebugCodeContext2.GetDocumentContext");
            ppSrcCxt = this;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IEnumDebugCodeContexts2

        List<IDebugCodeContext2> _context = new List<IDebugCodeContext2>();

        public int Next(uint celt, IDebugCodeContext2[] rgelt, ref uint pceltFetched)
        {
            if (celt == 1)
            {
                rgelt[0] = this;
                pceltFetched = 1;
            }
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugCodeContexts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = 1;
            return VSConstants.S_OK;

        }

        #endregion

        public override string ToString()
        {
            return _description;
        }
    }
}
