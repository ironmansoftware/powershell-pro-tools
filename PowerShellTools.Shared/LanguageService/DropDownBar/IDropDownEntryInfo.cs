namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Interface used for tracking elements in the navigation bar drop down.
    /// </summary>
    internal interface IDropDownEntryInfo
    {
        /// <summary>
        /// Gets the text to be displayed
        /// </summary>
        string DisplayText { get; }

        /// <summary>
        /// Gets the index in our image list which should be used for the icon to be displayed
        /// </summary>
        int ImageListIndex { get; }

        /// <summary>
        /// Gets the position in the text buffer where the element begins
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Gets the position in the text buffer where the element ends
        /// </summary>
        int End { get; }
    }
}