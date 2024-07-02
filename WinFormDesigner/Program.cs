using CommandLine;
using System;
using System.Windows.Forms;

namespace WinFormDesigner
{
    internal static class Program
    {
        public class Options
        {
            [Option('c', "code", Required = true, HelpText = "The code file for the form.")]
            public string CodeFile { get; set; }

            [Option('d', "designer", Required = true, HelpText = "The designer file for the form.")]
            public string DesignerFile { get; set; }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmWinFormDesigner(o.CodeFile, o.DesignerFile));
                });
        }
    }
}
