using System.IO;

namespace PowerShellToolsPro.Test
{
	public class TestUtilities
	{
		public static string GetEmbeddedResource(string resourceLocation)
		{
			using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation))
			{
				if (stream == null) return null;

				using (var streamReader = new StreamReader(stream))
				{
					return streamReader.ReadToEnd();
				}
			}
		}
	}
}
