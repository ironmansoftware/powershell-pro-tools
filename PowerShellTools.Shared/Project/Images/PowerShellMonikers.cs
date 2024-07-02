using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace PowerShellTools.Project.Images
{
    public static class PowerShellMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("072A9D46-0C24-4CFA-990A-25D7E12271F5");

        private const int ProjectIcon = 1;
        private const int ScriptIcon = 2;
        private const int DataIcon = 3;
        private const int ModuleIcon = 4;
        private const int TestIcon = 5;

        public static ImageMoniker ProjectIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = ProjectIcon };

	    public static ImageMoniker ScriptIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = ScriptIcon };

	    public static ImageMoniker DataIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = DataIcon };

	    public static ImageMoniker ModuleIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = ModuleIcon };

	    public static ImageMoniker TestIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = TestIcon };
    }
}
