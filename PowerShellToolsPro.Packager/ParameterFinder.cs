using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellToolsPro.Packager
{

	public class ParameterFinder
	{
		public static Dictionary<string, object> FindBoundParameters(CommandAst commandAst)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "PowerShellToolsPro.Packager.GetBoundParameters.ps1";

			string getBoundParameterScript;
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				getBoundParameterScript = reader.ReadToEnd();
			}

			try
			{
				var result = new Dictionary<string, object>();
				using (var powerShell = System.Management.Automation.PowerShell.Create())
				{
					powerShell.AddScript(getBoundParameterScript);
					powerShell.Invoke();
					powerShell.AddCommand("Get-BoundParameter");
					powerShell.AddParameter("commandAst", commandAst);
					var psobject = powerShell.Invoke();
					if (powerShell.HadErrors)
					{
						return null;
					}

					foreach (var param in psobject.Select(m => m.BaseObject).Cast<Hashtable>())
					{
						result.Add(param["Name"] as string, param["Ast"]);
					}
				}

				return result;
			}
			catch
			{
				return null;
			}
		}
	}
}
