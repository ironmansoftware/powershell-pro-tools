using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common.Logging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.DebugEngine
{
    /// <summary>
    /// An implementation of IDebugProperty2 used to display variables in the local and watch windows.
    /// </summary>
    public class ScriptProperty : IDebugProperty2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptProperty));

        public string Name { get; set; }

        public string Value { get; set; }

        public bool IsEnum { get; set; }

        public string TypeName { get; set; }

        public ScriptDebugger _debugger;

        private readonly Variable _variable;

        public ScriptProperty(ScriptDebugger debugger, Variable var)
        {
            Log.DebugFormat("{0} {1}", var.VarName, var.VarValue);
            Name = var.VarName;
            Value = var.VarValue;
            IsEnum = var.HasChildren;
            TypeName = (var.Type == null ? string.Empty : var.Type);
            _variable = var;
            _debugger = debugger;
        }

        #region Implementation of IDebugProperty2

        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            Log.DebugFormat("GetPropertyInfo [{0}]", dwFields);

            pPropertyInfo[0].dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NONE;

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
            {
                pPropertyInfo[0].bstrName = Name;
                pPropertyInfo[0].bstrFullName = Name;
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
            {
                pPropertyInfo[0].bstrValue = Value == null ? String.Empty : Value.ToString();
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE) != 0)
            {
                pPropertyInfo[0].bstrType = TypeName;
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
            {
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                pPropertyInfo[0].dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;
                if (!ScriptPropertyCollection.PsBaseTypes.Contains(TypeName))
                {
                    pPropertyInfo[0].dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }
            }

            pPropertyInfo[0].pProperty = this;
            pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;

            return VSConstants.S_OK;
        }

        public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            //TODO:
            _debugger.SetVariable(Name, pszValue);
            return VSConstants.S_OK;
        }

        public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
        {
            if (Value != null)
            {
                ppEnum = new ScriptPropertyCollection(GetChildren, (uint)_debugger.GetVariableDetailsCount(_variable.Path));
                return VSConstants.S_OK;
            }

            ppEnum = null;
            return VSConstants.S_FALSE;
        }

        private IEnumerable<ScriptProperty> GetChildren(int skip, int take)
        {
            var details = _debugger.GetVariableDetails(_variable.Path, skip, take);

            foreach (var item in details)
            {
                yield return new ScriptProperty(_debugger, item);
            }
        }

        public int GetParent(out IDebugProperty2 ppParent)
        {
            throw new NotImplementedException();
        }

        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            throw new NotImplementedException();
        }

        public int GetSize(out uint pdwSize)
        {
            throw new NotImplementedException();
        }

        public int GetReference(out IDebugReference2 ppReference)
        {
            throw new NotImplementedException();
        }

        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// A list of variables for a particular stack frame.
    /// </summary>
    public class ScriptPropertyCollection : IEnumDebugPropertyInfo2
    {
        private uint _count;
        private int _skip;
        private Func<int, int, IEnumerable<ScriptProperty>> _loadProperties;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptPropertyCollection));
        public static List<string> PsBaseTypes = new List<string>() { "System.String", "System.Char", "System.Byte", "System.Int32", "System.Int64", "System.Boolean", "System.Decimal", "System.Single", "System.Double", "System.IntPtr" };

        public ScriptPropertyCollection(Func<int, int, IEnumerable<ScriptProperty>> loadProperties, uint count)
        {
            Log.Debug("children");
            _count = count;
            _loadProperties = loadProperties;
        }

        #region Implementation of IEnumDebugPropertyInfo2

        public int Next(uint celt, DEBUG_PROPERTY_INFO[] rgelt, out uint pceltFetched)
        {
            Log.Debug("Next, base at" + _count.ToString());

            int i = 0;
            foreach(var variable in _loadProperties(_skip, (int)celt))
            {
                rgelt[i].bstrName = variable.Name;
                rgelt[i].bstrFullName = variable.Name;
                rgelt[i].bstrValue = variable.Value != null ? variable.Value.ToString() : "$null";
                rgelt[i].bstrType = variable.TypeName != null ? variable.TypeName : String.Empty;
                rgelt[i].pProperty = variable;
                rgelt[i].dwAttrib = GetExpandableAttributesOverride(variable)
                    & GetExpandableAttributes(variable.TypeName);
                rgelt[i].dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                i++;
            }

            pceltFetched = celt;
            //_count += celt;
            return VSConstants.S_OK;

        }

        private enum_DBG_ATTRIB_FLAGS GetExpandableAttributesOverride(ScriptProperty prop)
        {
            return prop.IsEnum ? enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE : 0;
        }

        private enum_DBG_ATTRIB_FLAGS GetExpandableAttributes(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return 0;
            }

            if (PsBaseTypes.Contains(typeName))
            {
                return 0;
            }

            return enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
        }

        public int Skip(uint celt)
        {
            Log.Debug("Skip");
            _skip += (int)celt;
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            Log.Debug("Reset");
            _skip = 0;
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugPropertyInfo2 ppEnum)
        {
            Log.Debug("Clone");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            Log.Debug("GetCount");
            pcelt = _count;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
