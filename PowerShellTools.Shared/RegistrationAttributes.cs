// Copyright Andreas Kirsch 2008
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using PowerShellTools.Common.Logging;

namespace Microsoft.VisualStudio.Shell
{
    /// <summary>
    /// Abstract class that is used to build lists of GUIDs for the incompatible engines list and the autoselect incompatible one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class AProvideEngineInfo : RegistrationAttribute
    {
        private string engineGUID;
        public string EngineGUID
        {
            get { return engineGUID; }
        }

        public AProvideEngineInfo(string engineGUID)
        {
            this.engineGUID = engineGUID;
        }

        public override void Register(RegistrationContext context)
        {
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
        }
    }

    /// <summary>
    /// The specified debug engine will be added to the incompatible engines list
    /// </summary>
    public sealed class ProvideIncompatibleEngineInfo : AProvideEngineInfo
    {
        public ProvideIncompatibleEngineInfo(string incompatibleEngineGUID)
            : base(incompatibleEngineGUID)
        {
        }
    }

    /// <summary>
    /// The specified debug engine will be added to the incompatible engines list
    /// </summary>
    public sealed class ProvideAutoSelectIncompatibleEngineInfo : AProvideEngineInfo
    {
        public ProvideAutoSelectIncompatibleEngineInfo(string incompatibleEngineGUID)
            : base(incompatibleEngineGUID)
        {
        }
    }
}
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class ClsidAttribute : Attribute
{
    public string Assembly { get; set; }
    public string CodeBase
    {
        get
        {
            var assemblyLoc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(assemblyLoc, Assembly + ".dll");

        }

    }
    public string Class { get; set; }
    public string InprocServer32 { get; set; }
    public string Clsid { get; set; }
}

namespace Microsoft.VisualStudio.Shell
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ProvidePortSupplierAttribute : Attribute
    {
        public string CLSID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Register the class this attribute is applied to as debug engine.
    /// Don't forget that you still need to provide it to Visual Studio as COM object using ProvideObject.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ProvideDebugEngineAttribute : RegistrationAttribute
    {
        #region Fields
        private Hashtable optionsTable = new Hashtable();
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProvideDebugEngineAttribute)); 
        #endregion

        #region Engine Parameters
        /// <summary>Set to nonzero to indicate support for address breakpoints.</summary>
        public bool AddressBP
        {
            get
            {
                object val = optionsTable["AddressBP"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["AddressBP"] = value; }
        }

        /// <summary>Set to nonzero in order to always load the debug engine locally.</summary>
        public bool AlwaysLoadLocal
        {
            get
            {
                object val = optionsTable["AlwaysLoadLocal"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["AlwaysLoadLocal"] = value; }
        }

        /// <summary>Set to nonzero to indicate that the debug engine will always be loaded with or by the program being debugged.</summary>
        public bool LoadedByDebuggee
        {
            get
            {
                object val = optionsTable["LoadedByDebuggee"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["LoadedByDebuggee"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for attachment to existing programs.</summary>
        public bool Attach
        {
            get
            {
                object val = optionsTable["Attach"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["Attach"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for call stack breakpoints.</summary>
        public bool CallStackBP
        {
            get
            {
                object val = optionsTable["CallStackBP"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["CallStackBP"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for the setting of conditional breakpoints.</summary>
        public bool ConditionalBP
        {
            get
            {
                object val = optionsTable["ConditionalBP"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["ConditionalBP"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for the setting of breakpoints on changes in data.</summary>
        public bool DataBP
        {
            get
            {
                object val = optionsTable["DataBP"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["DataBP"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for the production of a disassembly listing.</summary>
        public bool Disassembly
        {
            get
            {
                object val = optionsTable["Disassembly"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["Disassembly"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for dump writing (the dumping of memory to an output device).</summary>
        public bool DumpWriting
        {
            get
            {
                object val = optionsTable["DumpWriting"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["DumpWriting"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for exceptions.</summary>
        public bool Exceptions
        {
            get
            {
                object val = optionsTable["Exceptions"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["Exceptions"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for named breakpoints (breakpoints that break when a certain function name is called).</summary>
        public bool FunctionBP
        {
            get
            {
                object val = optionsTable["FunctionBP"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["FunctionBP"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for the setting of "hit point" breakpoints (breakpoints that are triggered only after being hit a certain number of times).</summary>
        public bool HitCountBP
        {
            get
            {
                object val = optionsTable["HitCountBP"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["HitCountBP"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for just-in-time debugging (the debugger is launched when an exception occurs in a running process).</summary>
        public bool JITDebug
        {
            get
            {
                object val = optionsTable["JITDebug"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["JITDebug"] = value; }
        }

        /// <summary>Set this to the CLSID of the port supplier if one is implemented.</summary>
        public string PortSupplier
        {
            get
            {
                var opt = optionsTable["PortSupplier"];
                return opt  == null ? null : opt.ToString();
            }
            set { optionsTable["PortSupplier"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for setting the next statement (which skips execution of intermediate statements).</summary>
        public bool SetNextStatement
        {
            get
            {
                object val = optionsTable["SetNextStatement"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["SetNextStatement"] = value; }
        }

        /// <summary>Set to nonzero to indicate support for suspending thread execution.</summary>
        public bool SuspendThread
        {
            get
            {
                object val = optionsTable["SuspendThread"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["SuspendThread"] = value; }
        }

        /// <summary>Set to nonzero to indicate that the user should be notified if there are no symbols.</summary>
        public bool WarnIfNoSymbols
        {
            get
            {
                object val = optionsTable["WarnIfNoSymbols"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["WarnIfNoSymbols"] = value; }
        }

        /// <summary>Set this to the CLSID of the program provider.</summary>
        public string ProgramProvider
        {
            get
            {
                object val = optionsTable["ProgramProvider"];
                return (null == val) ? null : val.ToString();
            }
            set { optionsTable["ProgramProvider"] = value; }
        }

        /// <summary>Set this to nonzero to indicate that the program provider should always be loaded locally.</summary>
        public bool AlwaysLoadProgramProviderLocal
        {
            get
            {
                object val = optionsTable["AlwaysLoadProgramProviderLocal"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["AlwaysLoadProgramProviderLocal"] = value; }
        }

        /// <summary>Set this to nonzero to indicate that the debug engine will watch for process events instead of the program provider.</summary>
        public bool EngineCanWatchProcess
        {
            get
            {
                object val = optionsTable["EngineCanWatchProcess"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["EngineCanWatchProcess"] = value; }
        }

        /// <summary>Set this to nonzero to indicate support for remote debugging.</summary>
        public bool RemoteDebugging
        {
            get
            {
                object val = optionsTable["RemoteDebugging"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["RemoteDebugging"] = value; }
        }

        /// <summary>Set this to nonzero to indicate that the debug engine should be loaded in the debuggee process under WOW when debugging a 64-bit process; otherwise, the debug engine will be loaded in the Visual Studio process (which is running under WOW64).</summary>
        public bool LoadUnderWOW64
        {
            get
            {
                object val = optionsTable["LoadUnderWOW64"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["LoadUnderWOW64"] = value; }
        }

        /// <summary>Set this to nonzero to indicate that the program provider should be loaded in the debuggee process when debugging a 64-bit process under WOW; otherwise, it will be loaded in the Visual Studio process.</summary>
        public bool LoadProgramProviderUnderWOW64
        {
            get
            {
                object val = optionsTable["LoadProgramProviderUnderWOW64"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["LoadProgramProviderUnderWOW64"] = value; }
        }

        /// <summary>Set this to nonzero to indicate that the process should stop if an unhandled exception is thrown across managed/unmanaged code boundaries.</summary>
        public bool StopOnExceptionCrossingManagedBoundary
        {
            get
            {
                object val = optionsTable["StopOnExceptionCrossingManagedBoundary"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["StopOnExceptionCrossingManagedBoundary"] = value; }
        }

        /// <summary>Set this to a priority for automatic selection of the debug engine (higher values equals higher priority).</summary>
        public int AutoSelectPriority
        {
            get
            {
                object val = optionsTable["AutoSelectPriority"];
                return (null == val) ? 0 : (int)val;
            }
            set { optionsTable["AutoSelectPriority"] = value; }
        }
        /*

                /// <summary>Registry key containing entries that specify GUIDs for debug engines to be ignored in automatic selection. These entries are a number (0, 1, 2, and so on) with a GUID expressed as a string.</summary>
                public Guid[] AutoSelectIncompatibleList
                {
                    get
                    {
                        object val = optionsTable["AutoSelectIncompatibleList"];
                        return val as Guid[];
                    }
                    set { optionsTable["AutoSelectIncompatibleList"] = value; }
                }


                /// <summary>Registry key containing entries that specify GUIDs for debug engines that are incompatible with this debug engine.</summary>
                public Guid[] IncompatibleList
                {
                    get
                    {
                        object val = optionsTable["IncompatibleList"];
                        return val as Guid[];
                    }
                    set { optionsTable["IncompatibleList"] = value; }
                }*/

        /// <summary>Set this to nonzero to indicate that just-in-time optimizations (for managed code) should be disabled during debugging.</summary>
        public bool DisableJITOptimization
        {
            get
            {
                object val = optionsTable["DisableJITOptimization"];
                return (null == val) ? false : (bool)val;
            }
            set { optionsTable["DisableJITOptimization"] = value; }
        }

        public string CLSID
        {
            get
            {
                object val = optionsTable["CLSID"];
                return (null == val) ? null : (string)val;
            }
            set { optionsTable["CLSID"] = value; }
        }

        #endregion

        #region Properties
        private string name;
        private String engineGuidString;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public String EngineGuidString
        {
            get { return engineGuidString; }
        }

        public string EngineRegKey
        {
            get { return string.Format(CultureInfo.InvariantCulture, "AD7Metrics\\Engine\\{0}", EngineGuidString); }
        }

        public ProvideDebugEngineAttribute(String engineGuidString, string name)
        {
            // make sure that it uses the right GUID format
            this.engineGuidString = new Guid(engineGuidString).ToString("B");
            this.name = name;
        } 
        #endregion

        #region Methods

        private void WriteValue(RegistrationContext context, Key targetKey, string name, object value)
        {
            if (value == null)
            {
                return;
            }
            else if (value is Type)
            {
                Type type = (Type)value;
                Guid guid = type.GUID;

                if (guid != Guid.Empty)
                {
                    targetKey.SetValue(name, guid.ToString("B"));
                }
            }
            else if (value is Array)
            {
                Array array = value as Array;

                using (Key childKey = targetKey.CreateSubkey(name))
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        Object element = array.GetValue(i);
                        WriteValue(context, childKey, i.ToString(), element);
                    }
                }
            }
            else if (value.GetType().IsPrimitive)
            {
                targetKey.SetValue(name, Convert.ToInt32(value));
            }
            else
            {
                String str = value.ToString();
                if (!String.IsNullOrEmpty(str))
                {
                    targetKey.SetValue(name, context.EscapePath(str));
                }
            }
        }

        public override void Register(RegistrationContext context)
        {
            context.Log.WriteLine(string.Format(CultureInfo.InvariantCulture, "Registering Debug Engine {0}", EngineGuidString));

            using (Key childKey = context.CreateKey(EngineRegKey))
            {
                //use a friendly description if it exists.
                DescriptionAttribute attr = TypeDescriptor.GetAttributes(context.ComponentType)[typeof(DescriptionAttribute)] as DescriptionAttribute;
                if (attr != null && !String.IsNullOrEmpty(attr.Description))
                {
                    childKey.SetValue(string.Empty, attr.Description);
                }
                else
                {
                    childKey.SetValue(string.Empty, context.ComponentType.AssemblyQualifiedName);
                }

                childKey.SetValue("Name", Name);

                foreach (object key in optionsTable.Keys)
                {
                    string keyName = key.ToString();
                    WriteValue(context, childKey, keyName, optionsTable[key]);
                }

                WriteValue(context, childKey, "IncompatibleList", GetEngineGUIDs(context, typeof(ProvideIncompatibleEngineInfo)));
                WriteValue(context, childKey, "AutoSelectIncompatibleList", GetEngineGUIDs(context, typeof(ProvideAutoSelectIncompatibleEngineInfo)));
            }

            var clsidAttributes = context.ComponentType.GetCustomAttributes(typeof(ClsidAttribute), false).Cast<ClsidAttribute>();

            var registeredAssemblies = new List<string>();
            foreach (var clsidAttribute in clsidAttributes)
            {
                using (var clsid = context.CreateKey(String.Format("CLSID\\{0}", clsidAttribute.Clsid)))
                {
                    clsid.SetValue("Assembly", clsidAttribute.Assembly);
                    //clsid.SetValue("CodeBase", destFile);
                    clsid.SetValue("InprocServer32", context.InprocServerPath);
                    clsid.SetValue("Class", clsidAttribute.Class);
                }
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.Log.WriteLine("Unregistering debug engine....");
            context.RemoveKey(EngineRegKey);
            var clsidAttributes = context.ComponentType.GetCustomAttributes(typeof(ClsidAttribute), false).Cast<ClsidAttribute>();
            foreach (var clsidAttribute in clsidAttributes)
            {
                var fileName = new FileInfo(clsidAttribute.CodeBase);
                var asm = Assembly.LoadFrom(fileName.FullName);

                var regAsm = new RegistrationServices();
                if (!regAsm.UnregisterAssembly(asm))
                {
                    context.Log.WriteLine("Failed to unregister assembly " + clsidAttribute.CodeBase);
                    throw new Exception("Failed to unregister: " + clsidAttribute.CodeBase);
                }

                File.Delete(fileName.FullName);
                context.RemoveKey(String.Format("CLSID\\{0}", clsidAttribute.Clsid));
            }

        }

        private String[] GetEngineGUIDs(RegistrationContext context, Type attributeType)
        {
            AProvideEngineInfo[] engines = (AProvideEngineInfo[])context.ComponentType.GetCustomAttributes(attributeType, true);

            if (engines.Length == 0)
            {
                return null;
            }

            return Array.ConvertAll<AProvideEngineInfo, string>(engines, delegate(AProvideEngineInfo engine) { return engine.EngineGUID; });
        }
        #endregion
    }
}

namespace Microsoft.VisualStudio.Shell
{
    /// <summary>
    /// This is just a wrapper for ProvideObject to provide the class the attribute is used on
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    class ProvideClass : RegistrationAttribute
    {
        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            ProvideObjectAttribute objectProvider = new ProvideObjectAttribute(context.ComponentType);
            objectProvider.Register(context);
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
            ProvideObjectAttribute objectProvider = new ProvideObjectAttribute(context.ComponentType);
            objectProvider.Unregister(context);
        }
    }
    /// <summary>
    /// This class is used to create a registry key for the FontAndColors category
    /// </summary>
    public class FontAndColorsRegistrationAttribute : RegistrationAttribute {

        // this GUID is used by all VSPackages that use the default font and color configurations.
        // http://msdn.microsoft.com/en-us/library/bb165737.aspx
        private const string PackageGuid = "{F5E7E71D-1401-11D1-883B-0000F87579D2}";
        private const int CategoryNameResourceID = 114;

        public string CategoryGuid { get; private set; }
        public string ToolWindowPackageGuid { get; private set; }
        public string CategoryKey { get; private set; }
        private string _packageGuid;

        public FontAndColorsRegistrationAttribute(string categoryKeyName, string categoryGuid, string toolWindowPackageGuid, string packageGuid) {
            CategoryGuid = categoryGuid;
            ToolWindowPackageGuid = toolWindowPackageGuid;
            CategoryKey = "FontAndColors\\" + categoryKeyName;
            _packageGuid = packageGuid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public override void Register(RegistrationContext context) {
            using (var key = context.CreateKey(CategoryKey)) {
                key.SetValue("Category", CategoryGuid);
                key.SetValue("Package", _packageGuid);
                key.SetValue("NameID", CategoryNameResourceID);
                //key.SetValue("ToolWindowPackage", ToolWindowPackageGuid);
                key.Close();
            }
        }

        public override void Unregister(RegistrationContext context) {
            context.RemoveKey(CategoryKey);
        }
    }
}