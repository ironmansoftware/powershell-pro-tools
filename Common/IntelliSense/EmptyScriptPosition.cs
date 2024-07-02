using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.IntelliSense
{
    [Serializable]
    internal sealed class EmptyScriptPosition : IScriptPosition
    {
        public string File
        {
            get
            {
                return null;
            }
        }
        public int LineNumber
        {
            get
            {
                return 0;
            }
        }
        public int ColumnNumber
        {
            get
            {
                return 0;
            }
        }
        public int Offset
        {
            get
            {
                return 0;
            }
        }
        public string Line
        {
            get
            {
                return "";
            }
        }
        public string GetFullScript()
        {
            return null;
        }
    }
}
