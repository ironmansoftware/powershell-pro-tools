using Microsoft.VisualStudio.PlatformUI;

namespace PowerShellTools.Common
{
    /// <summary>
    /// DialogWindow is from Microsoft.VisualStudio.Shell.$(VsVer).dll .
    /// DialogWindow is referenced in many xaml files in this solution.
    /// Xaml needs to explicitly specify assembly name, so every xaml using
    /// DialogWindow must reference the dll. This is a problem after building 
    /// this solution against more than one VS version. Xaml does not allow 
    /// #if or anything similar. VsShellDialogWindow serves like type forward, 
    /// it hides the dll from Xaml. 
    /// Another solution is to use type forward, but this forces exposing
    /// DialogWindow through this dll.    
    /// </summary>
    public class VsShellDialogWindow : DialogWindow
    {
    }
}
