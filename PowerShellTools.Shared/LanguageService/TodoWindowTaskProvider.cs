using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.LanguageService
{
    [Guid("86D8A425-F68E-4231-9EE5-B0C15FD26C1F")]
    public class TodoWindowTaskProvider : TaskProvider
    {
        public TodoWindowTaskProvider(IServiceProvider sp)
            : base(sp)
        {
        }
    }
}
