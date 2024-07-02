using System;

namespace PowerShellTools
{
    internal static class GuidList
    {
        public const string PowerShellLanguage = "1C4711F1-3766-4F84-9516-43FA4169CC36";

        //Package Guids; these need to be the same as the guids in PowerShellTools.vsct/Common.Build.Targets
        public const string PowerShellToolsPackageGuid = "0429083f-fdbc-47a3-84ff-b3d50343b21e";
        public const string PowerShellToolsProjectPackageGuid = "1C9F35A6-C599-4336-A6F7-A9A9AC9A2B86";

        //Property Pages
        public const string GeneralPropertiesPageGuid = "C9619BDD-D1B3-4ACA-ADF3-2323EB62315E";
        public const string DebugPropertiesPageGuid = "E6C81F79-910C-4C91-B9DF-321883DC9F44";
        public const string BuildEventsPropertiesPageGuid = "0974F0EF-8D19-423E-9637-FB853C4EA232";
        public const string ModuleManifestPropertiesPageGuid = "3B19A81A-BE20-4F0D-B577-5093853FB681";
        public const string AdvancedPropertiesPageGuid = "1B8CADD5-EC43-474F-90FD-C0BD7EB78C8E";

        //Commands
        public const string CmdSetGuid = "099073C0-B561-4BC1-A847-92657B89A00E";
        public const uint CmdidExecuteSelection = 0x0102;
        public const uint CmdidExecuteAsScript =  0x0103;
        public const uint CmdidExecuteWithParametersAsScript = 0x0104;
        public const uint CmdidGotoDefinition = 0x0105;
        public const uint CmdidSnippet = 0x0106;
        public const uint CmdidPrettyPrint = 0x0107;
        public const uint CmdidDisplayRepl = 0x0108;
        public const uint CmdidExecuteAsScriptSolution = 0x0109;
        public const uint CmdidExecuteWithParametersAsScriptFromSolutionExplorer = 0x0110;
        public const uint CmdidDisplayExplorer = 0x0111;
        public const uint CmdidProfileScript = 0x0112;
        public const uint CmdidGenerateWinForm= 0x0113;
        public const uint CmdidSelectPowerShellVersion = 0x0114;
        public const uint CmdidPowerShellVersionList = 0x0115;
        public const uint CmdidAboutPowerShellProTools = 0x0116;
        public const uint CmdidLicenseInfo = 0x0117;
        public const uint CmdidConsole = 0x0118;
        public const uint CmdidExecuteSelectionToolbar = 0x0119;
        public const uint cmdidConvertToPowerShell = 0x0120;
        public const uint cmdidCompileScriptSolution = 0x0121;
        public const uint cmdidPasteAsCommand = 0x0122;
        public const uint cmdidPSExplorer = 0x0123;
        public const uint VariablesCommandId = 0x0124;
        public const uint ModulesCommandId = 0x0125;
        public const uint SubMenu = 0x0126;
        public const uint SubMenuGroup = 0x0127;
        public const uint SettingsCommandId = 0x0128;

        public const string guidCustomEditorCmdSetString = "73d661d7-c0a7-476c-ad5e-3b36f1e91a8f";
        public const string guidCustomEditorEditorFactoryString = "0ff6321c-6ea5-400b-8342-f126da8505a2";

        public static readonly Guid guidCustomEditorCmdSet = new Guid(guidCustomEditorCmdSetString);
        public static readonly Guid guidCustomEditorEditorFactory = new Guid(guidCustomEditorEditorFactoryString);
    };
}