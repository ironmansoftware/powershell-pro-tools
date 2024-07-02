using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Represents an error or warning
    /// </summary>
    internal class ValidationResult
    {
        /// <summary>
        /// The error or warning message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// True if a warning, false if an error
        /// </summary>
        public bool IsWarning { get; set; }
    }
}
