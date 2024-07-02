using System;
using System.ComponentModel.Design;

namespace PowerShellTools.Commands
{
    public interface ICommand
    {
        CommandID CommandId { get; }
        void Execute(object sender, EventArgs args);
        void QueryStatus(object sender, EventArgs args);
    }
}
