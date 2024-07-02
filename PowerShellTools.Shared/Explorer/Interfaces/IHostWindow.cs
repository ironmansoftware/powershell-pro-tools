using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    public interface IHostWindow
    {
        HostControl ContentHost { get; set; }
        void ShowCommandExplorer();
        void ShowParameterEditor(IPowerShellCommand command);
        void SetCaption(string caption);
    }
}
