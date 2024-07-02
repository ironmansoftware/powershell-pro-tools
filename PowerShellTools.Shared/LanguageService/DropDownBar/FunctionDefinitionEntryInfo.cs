using System;
using System.Linq;
using System.Management.Automation.Language;

namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Class used for tracking function definitions in the navigation bar drop down.
    /// </summary>
    internal class FunctionDefinitionEntryInfo : IDropDownEntryInfo
    {
        private readonly FunctionDefinitionAst _functionDefinition;

        public FunctionDefinitionEntryInfo(FunctionDefinitionAst functionDefinition)
        {
            _functionDefinition = functionDefinition;
        }

        /// <summary>
        /// Gets the text to be displayed
        /// </summary>
        public string DisplayText
        {
            get
            {
                var parameters = _functionDefinition.Parameters;
                if (parameters != null && parameters.Any())
                {
                    return _functionDefinition.Name + "(" + String.Join(", ", parameters.Select(param => param.Name)) + ")";
                }
                else
                {
                    return _functionDefinition.Name;
                }
            }
        }

        /// <summary>
        /// Gets the index in our image list which should be used for the icon to be displayed
        /// </summary>
        public int ImageListIndex
        {
            get
            {
                var overlay = ImageListOverlay.ImageListOverlayNone;
                if (DisplayText != null && DisplayText.StartsWith("_") && !(DisplayText.StartsWith("__") && DisplayText.EndsWith("__")))
                {
                    overlay = ImageListOverlay.ImageListOverlayPrivate;
                }

                return EntryInfoEnumsExtensions.GetImageListIndex(GetImageListKind(_functionDefinition), overlay);
            }
        }

        /// <summary>
        /// Gets the position in the text buffer where the element starts
        /// </summary>
        public int Start
        {
            get
            {
                return _functionDefinition.Extent.StartOffset;
            }
        }

        /// <summary>
        /// Gets the position in the text buffer where the element ends
        /// </summary>
        public int End
        {
            get
            {
                return _functionDefinition.Extent.EndOffset;
            }
        }

        /// <summary>
        /// Gets the function definition associated with this entry
        /// </summary>
        public FunctionDefinitionAst FunctionDefinition
        {
            get
            {
                return _functionDefinition;
            }
        }

        private static ImageListKind GetImageListKind(FunctionDefinitionAst funcDef)
        {
            return ImageListKind.Method;
        }
    }
}