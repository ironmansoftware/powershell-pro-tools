using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal abstract class OptionSet
    {
        private Dictionary<string, OptionModel> _options = new Dictionary<string, OptionModel>();

        public OptionSet()
        {
        }

        public List<OptionModel> Options
        {
            get
            {
                return _options.Select(x=>x.Value).ToList<OptionModel>();
            }
        }

        public void AddOption(string key, OptionModel value)
        {
            _options.Add(key, value);
        }

        public bool ContainsOption(string key)
        {
            return _options.ContainsKey(key);
        }

        public OptionModel GetOption(string key)
        {
            return _options[key];
        }
    }
}
