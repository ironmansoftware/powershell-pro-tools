using System.Management.Automation.Language;

namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Class used for tracking the script element in the navigation bar drop down.
    /// </summary>
    internal class ScriptEntryInfo : IDropDownEntryInfo
    {
        private int _start, _end;

        public ScriptEntryInfo(Ast script)
        {
            _start = script.Extent.StartOffset;
            _end = script.Extent.EndOffset;
        }

        /// <summary>
        /// Gets the text to be displayed
        /// </summary>
        public string DisplayText
        {
            get
            {
                return Resources.DropDownScriptName;
            }
        }

        /// <summary>
        /// Gets the index in our image list which should be used for the icon to be displayed
        /// </summary>
        public int ImageListIndex
        {
            get
            {
                return (int)ImageListKind.Class;
            }
        }

        /// <summary>
        /// Gets the position in the text buffer where the element begins
        /// </summary>
        public int Start
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        /// Gets the position in the text buffer where the element ends
        /// </summary>
        public int End
        {
            get
            {
                return _end;
            }
        }
    }
}