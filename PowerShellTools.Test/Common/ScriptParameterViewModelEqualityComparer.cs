using System;
using System.Collections.Generic;
using System.Linq;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Test
{
    internal class ScriptParameterViewModelEqualityComparer : IEqualityComparer<ScriptParameterViewModel>
    {
        public bool Equals(ScriptParameterViewModel x, ScriptParameterViewModel y)
        {
            return String.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                   Object.Equals(x.Value, y.Value) &&
                   x.Type == y.Type &&
                   String.Equals(x.ParameterSetName, y.ParameterSetName, StringComparison.Ordinal) &&
                   CompareListOfObjects(x.AllowedValues, y.AllowedValues);

        }

        public int GetHashCode(ScriptParameterViewModel viewModel)
        {
            return (viewModel.Name.GetHashCode() ^ viewModel.Type.GetHashCode()).GetHashCode();
        }

        private static bool CompareListOfObjects(IEnumerable<object> objects1, IEnumerable<object> objects2)
        {
            if (objects1 == objects2)
            {
                return true;
            }

            if ((objects1 == null && objects2 != null) ||
                (objects1 != null && objects2 == null))
            {
                return false;
            }

            return Enumerable.SequenceEqual(objects1.OrderBy(t => t), objects2.OrderBy(t => t));

        }
    }
}
