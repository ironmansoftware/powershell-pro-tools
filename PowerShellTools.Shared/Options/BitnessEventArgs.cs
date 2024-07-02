using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Options
{
    /// <summary>
    /// An event contains info about user's new choice of bitness for remote host.
    /// </summary>
    internal class BitnessEventArgs : EventArgs
    {
        private readonly BitnessOptions _newBitness;

        public BitnessEventArgs(BitnessOptions newBitness)
        {
            _newBitness = newBitness;
        }

        public BitnessOptions NewBitness
        {
            get
            {
                return _newBitness;
            }
        }
    }
}
