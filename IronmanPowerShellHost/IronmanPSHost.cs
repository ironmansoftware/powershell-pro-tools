using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;

namespace IronmanPowerShellHost
{
    internal class IronmanPSHost : PSHost
    {
        public override string Name => "Ironman Host";

        public override Version Version => new Version(1, 0);

        public override Guid InstanceId => throw new NotImplementedException();

        public override PSHostUserInterface UI => throw new NotImplementedException();

        public override CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.CurrentUICulture;

        public override void EnterNestedPrompt()
        {

        }

        public override void ExitNestedPrompt()
        {

        }

        public override void NotifyBeginApplication()
        {

        }

        public override void NotifyEndApplication()
        {

        }

        public override void SetShouldExit(int exitCode)
        {

        }
    }
}
