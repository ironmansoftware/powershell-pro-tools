using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Common
{
    public interface IPackagingService
    {
        string Package(string packageFile);
    }
}
