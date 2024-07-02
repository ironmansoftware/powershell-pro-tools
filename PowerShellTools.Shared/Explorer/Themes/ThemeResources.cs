using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Explorer
{
    public static class ThemeResources
    {
        public static object WindowBackground
        {
            get
            {
                return VsBrushes.WindowKey;
            }
        }

        public static object WindowForeground
        {
            get
            {
                return VsBrushes.WindowTextKey;
            }
        }

        public static object AccentLight
        {
            get
            {
                return VsBrushes.AccentLightKey;
            }
        }

        public static object AccentDark
        {
            get
            {
                return VsBrushes.AccentDarkKey;
            }
        }

        public static object Border
        {
            get
            {
                return VsBrushes.ControlOutlineKey;
            }
        }

        public static object Highlight
        {
            get
            {
                return VsBrushes.HighlightKey;
            }
        }

        public static object GrayText
        {
            get
            {
                return VsBrushes.GrayTextKey;
            }
        }

        public static object ComboBoxBorder
        {
            get
            {
                return VsBrushes.ComboBoxBorderKey;
            }
        }

        public static object ComboBoxBackground
        {
            get
            {
                return VsBrushes.ComboBoxBackgroundKey;
            }
        }

        public static object DropDownBackground
        {
            get
            {
                return VsBrushes.DropDownBackgroundKey;
            }
        }

        public static object DropDownBorder
        {
            get
            {
                return VsBrushes.DropDownBorderKey;
            }
        }

        public static object RequiredHighlight
        {
            get
            {
                return VsBrushes.SmartTagBorderKey;
            }
        }
    }
}
