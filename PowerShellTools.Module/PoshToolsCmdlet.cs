using System;
using System.Management.Automation;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Module
{
    public abstract class PoshToolsCmdlet : Cmdlet
    {
        private static Package _package;

        public static void Initialize(Package package)
        {
            _package = package;
        }

        protected Package GetPackage()
        {
            if (_package == null)
            {
                throw new Exception("Package was not set!");
            }

            return _package;
        }
    }
}
