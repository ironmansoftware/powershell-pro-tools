using System;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace PowerShellTools
{
    /// <summary>
    /// Duck type for SMA.Remoting.RunspaceRef
    /// </summary>
    internal class RunspaceRef
    {
        private static readonly Type RunspaceRefType;
        private readonly object _realRunspaceRef;

        private static readonly PropertyInfo RunspaceProperty;
        private static readonly PropertyInfo OldRunspaceProperty;
        private static readonly PropertyInfo IsRunspaceOverriddenProperty;
        private static readonly MethodInfo OverrideMethod;
        private static readonly MethodInfo RevertMethod;

        static RunspaceRef()
        {
            var remoteRunspaceType = typeof(Runspace).Assembly.GetType("System.Management.Automation.RemoteRunspace");

            RunspaceRefType = typeof(Runspace).Assembly.GetType("System.Management.Automation.Remoting.RunspaceRef");
            RunspaceProperty = RunspaceRefType.GetProperty("Runspace", BindingFlags.NonPublic | BindingFlags.Instance);
            OldRunspaceProperty = RunspaceRefType.GetProperty("OldRunspace", BindingFlags.NonPublic | BindingFlags.Instance);
            IsRunspaceOverriddenProperty = RunspaceRefType.GetProperty("IsRunspaceOverridden", BindingFlags.NonPublic | BindingFlags.Instance);
            OverrideMethod = RunspaceRefType.GetMethod("Override", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { remoteRunspaceType }, null);
            RevertMethod = RunspaceRefType.GetMethod("Revert", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {}, null);
        }

        public RunspaceRef(Runspace runspace)
        {
            var constructor = RunspaceRefType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new [] { typeof(Runspace) }, null);
            _realRunspaceRef = constructor.Invoke(new object[] { runspace });
        }

        public Runspace Runspace
        {
            get { return (Runspace)RunspaceProperty.GetGetMethod(true).Invoke(_realRunspaceRef, null); }
        }

        public bool IsRunspaceOverridden
        {
            get { return (bool)IsRunspaceOverriddenProperty.GetGetMethod(true).Invoke(_realRunspaceRef, null); }
        }

        public Runspace OldRunspace
        {
            get { return (Runspace)OldRunspaceProperty.GetGetMethod(true).Invoke(_realRunspaceRef, null); }
        }

        public void Override(Runspace runspace)
        {
            OverrideMethod.Invoke(_realRunspaceRef, new object[] {runspace});
        }

        public void Revert()
        {
            RevertMethod.Invoke(_realRunspaceRef, new object[] {});
        }
    }
}
