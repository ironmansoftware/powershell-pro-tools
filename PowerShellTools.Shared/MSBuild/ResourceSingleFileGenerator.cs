using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using IMS.FormDesigner;

namespace PowerShellToolsPro.MSBuild
{
    [ComVisible(true)]
    [Guid("f0ab8c07-0950-4d45-b87a-5ebf188eb6ea")]
    [CodeGeneratorRegistration(typeof(PowerShellResourceFileGenerator), "PowerShellResourceFileGenerator", "{f5034706-568f-408a-b7b3-4d38c6db8a32}", GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(PowerShellResourceFileGenerator))]
    public class PowerShellResourceFileGeneratorProvider : IVsSingleFileGenerator
    {
#pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        internal static string name = "PowerShellResourceFileGenerator";
#pragma warning restore 0414

        private readonly PowerShellResourceFileGenerator _powerShellResourceFileGenerator;

        public PowerShellResourceFileGeneratorProvider()
        {
            _powerShellResourceFileGenerator = new PowerShellResourceFileGenerator();
        }


        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".ps1";
            return VSConstants.S_OK;
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            if (bstrInputFileContents == null)
            {
                throw new ArgumentNullException(bstrInputFileContents);
            }

            var resx = XDocument.Parse(bstrInputFileContents);

            byte[] bytes;
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                //Generate the code
                _powerShellResourceFileGenerator.GenerateCode(resx, wszInputFilePath, writer);

                writer.Flush();

                //Get the Encoding used by the writer. We're getting the WindowsCodePage encoding, 
                //which may not work with all languages
                Encoding enc = Encoding.GetEncoding(writer.Encoding.WindowsCodePage);

                //Get the preamble (byte-order mark) for our encoding
                byte[] preamble = enc.GetPreamble();
                int preambleLength = preamble.Length;

                //Convert the writer contents to a byte array
                byte[] body = enc.GetBytes(writer.ToString());

                //Prepend the preamble to body (store result in resized preamble array)
                Array.Resize<byte>(ref preamble, preambleLength + body.Length);
                Array.Copy(body, 0, preamble, preambleLength, body.Length);

                bytes = preamble;
            }

            if (bytes == null)
            {
                // This signals that GenerateCode() has failed. Tasklist items have been put up in GenerateCode()
                rgbOutputFileContents = null;
                pcbOutput = 0;

                // Return E_FAIL to inform Visual Studio that the generator has failed (so that no file gets generated)
                return VSConstants.E_FAIL;
            }
            else
            {
                // The contract between IVsSingleFileGenerator implementors and consumers is that 
                // any output returned from IVsSingleFileGenerator.Generate() is returned through  
                // memory allocated via CoTaskMemAlloc(). Therefore, we have to convert the 
                // byte[] array returned from GenerateCode() into an unmanaged blob.  

                int outputLength = bytes.Length;
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
                Marshal.Copy(bytes, 0, rgbOutputFileContents[0], outputLength);
                pcbOutput = (uint)outputLength;
                return VSConstants.S_OK;
            }
        }
    }
}
