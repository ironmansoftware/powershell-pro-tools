using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal class PSParameterEditorOptionSet : OptionSet
    {
        public PSParameterEditorOptionSet()
        {
            AddOption("HashTable", new OptionModel("Format as Hashtable", OptionType.Bool, "Format command as hashtable", null));
        }

        public bool FormatAsHashTable
        {
            get
            {
                try
                {
                    var s = GetOption("HashTable");
                    bool v;
                    return bool.TryParse(s.Value, out v) && v;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
