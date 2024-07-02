using PowerShellToolsPro.Packager.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PowerShellToolsPro.Packager
{
    [Cmdlet("Invoke", "Packager")]
    public class VSEntry : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "PackageConfig")]
        public string SerializedPackageConfig { get; set; }

		private static T Deserialize<T>(string body)
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(body);
				writer.Flush();
				stream.Position = 0;
				return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
			}
		}

		protected override void EndProcessing()
		{
			var config = Deserialize<PsPackConfig>(SerializedPackageConfig);
			ExecuteWithConfig(config);
		}

		private void ExecuteWithConfig(PsPackConfig config)
		{
			var packageProcess = new PackageProcess();
			packageProcess.Config = config;
			packageProcess.OnMessage += (sender, s) =>
			{
				WriteVerbose(s);
			};

			packageProcess.OnErrorMessage += (sender, s) =>
			{
				WriteError(new ErrorRecord(new Exception(s), string.Empty, ErrorCategory.InvalidOperation, null));
			};

			var result = packageProcess.Execute();
		}
	}
}
