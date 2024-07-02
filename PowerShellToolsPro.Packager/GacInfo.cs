using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PowerShellToolsPro.Packager
{

    public class AssemblyNameInfo
    {
		public string Name { get; set; }
		public Version Version { get; set; }
		public string Culture { get; set; }
    }

	/// <summary>
	/// GAC file info
	/// </summary>
	public class GacFileInfo
	{
		/// <summary>
		/// Assembly
		/// </summary>
		public AssemblyNameInfo Assembly { get; }

		/// <summary>
		/// Path to file
		/// </summary>
		public string Path { get; }

		internal GacFileInfo(AssemblyNameInfo asm, string path)
		{
			Assembly = asm;
			Path = path;
		}
	}

	/// <summary>
	/// GAC version
	/// </summary>
	public enum GacVersion
	{
		/// <summary>
		/// .NET Framework 1.0-3.5
		/// </summary>
		V2,

		/// <summary>
		/// .NET Framework 4.0+
		/// </summary>
		V4,
	}

	/// <summary>
	/// GAC path info
	/// </summary>
	public readonly struct GacPathInfo
	{
		/// <summary>
		/// Path of dir containing assemblies
		/// </summary>
		public readonly string Path;

		/// <summary>
		/// GAC version
		/// </summary>
		public readonly GacVersion Version;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="path">Path</param>
		/// <param name="version">Version</param>
		public GacPathInfo(string path, GacVersion version)
		{
			Path = path ?? throw new ArgumentNullException(nameof(path));
			Version = version;
		}
	}

	/// <summary>
	/// GAC
	/// </summary>
	public static class GacInfo
	{
		/// <summary>
		/// All GAC paths
		/// </summary>
		public static GacPathInfo[] GacPaths { get; }

		/// <summary>
		/// Other GAC paths
		/// </summary>
		public static GacPathInfo[] OtherGacPaths { get; }

		/// <summary>
		/// WinMD paths
		/// </summary>
		public static string[] WinmdPaths { get; }

		/// <summary>
		/// Checks if .NET 2.0-3.5 GAC exists
		/// </summary>
		public static bool HasGAC2 { get; }

		sealed class GacDirInfo
		{
			public readonly int Version;
			public readonly string Path;
			public readonly string Prefix;
			public readonly string[] SubDirs;

			public GacDirInfo(int version, string prefix, string path, string[] subDirs)
			{
				Version = version;
				Prefix = prefix;
				Path = path;
				SubDirs = subDirs;
			}
		}
		static readonly GacDirInfo[] gacDirInfos;

		static GacInfo()
		{
			var gacDirInfosList = new List<GacDirInfo>();
			var newOtherGacPaths = new List<GacPathInfo>();
			var newWinmdPaths = new List<string>();

			bool hasGAC2;

			hasGAC2 = false;
			var windir = Environment.GetEnvironmentVariable("WINDIR");
			if (!string.IsNullOrEmpty(windir))
			{
				string path;

				// .NET Framework 1.x and 2.x
				path = Path.Combine(windir, "assembly");
				if (Directory.Exists(path))
				{
					hasGAC2 = File.Exists(Path.Combine(path, @"GAC_32\mscorlib\2.0.0.0__b77a5c561934e089\mscorlib.dll")) ||
						File.Exists(Path.Combine(path, @"GAC_64\mscorlib\2.0.0.0__b77a5c561934e089\mscorlib.dll"));
					if (hasGAC2)
					{
						gacDirInfosList.Add(new GacDirInfo(2, "", path, gacPaths4));
					}
				}

				// .NET Framework 4.x
				path = Path.Combine(Path.Combine(windir, "Microsoft.NET"), "assembly");
				if (Directory.Exists(path))
				{
					gacDirInfosList.Add(new GacDirInfo(4, "v4.0_", path, gacPaths2));
				}
			}

			AddIfExists(newWinmdPaths, Environment.SystemDirectory, "WinMetadata");

			OtherGacPaths = newOtherGacPaths.ToArray();
			WinmdPaths = newWinmdPaths.ToArray();

			gacDirInfos = gacDirInfosList.ToArray();
			GacPaths = gacDirInfos.Select(a => new GacPathInfo(a.Path, a.Version == 2 ? GacVersion.V2 : GacVersion.V4)).ToArray();
			HasGAC2 = hasGAC2;
		}
		// Prefer GAC_32 if this is a 32-bit process, and GAC_64 if this is a 64-bit process
		static readonly string[] gacPaths2 = IntPtr.Size == 4 ?
			new string[] { "GAC_32", "GAC_64", "GAC_MSIL" } :
			new string[] { "GAC_64", "GAC_32", "GAC_MSIL" };
		static readonly string[] gacPaths4 = IntPtr.Size == 4 ?
			new string[] { "GAC_32", "GAC_64", "GAC_MSIL", "GAC" } :
			new string[] { "GAC_64", "GAC_32", "GAC_MSIL", "GAC" };

		static void AddIfExists(List<string> paths, string basePath, string extraPath)
		{
			var path = Path.Combine(basePath, extraPath);
			if (Directory.Exists(path))
				paths.Add(path);
		}

		/// <summary>
		/// Gets all assemblies in the GAC
		/// </summary>
		/// <param name="majorVersion">CLR major version, eg. 2 or 4</param>
		/// <returns></returns>
		public static IEnumerable<GacFileInfo> GetAssemblies(int majorVersion)
		{
			foreach (var info in gacDirInfos)
			{
				if (info.Version == majorVersion)
					return GetAssemblies(info);
			}
			return Array.Empty<GacFileInfo>();
		}

		static IEnumerable<GacFileInfo> GetAssemblies(GacDirInfo gacInfo)
		{
			foreach (var subDir in gacInfo.SubDirs)
			{
				var baseDir = Path.Combine(gacInfo.Path, subDir);
				foreach (var dir in GetDirectories(baseDir))
				{
					foreach (var dir2 in GetDirectories(dir))
					{
						Version version;
						string culture;
						if (gacInfo.Version == 2)
						{
							var m = gac2Regex.Match(Path.GetFileName(dir2));
							if (!m.Success || m.Groups.Count != 4)
								continue;
							if (!Version.TryParse(m.Groups[1].Value, out version))
								continue;
							culture = m.Groups[2].Value;
						}
						else if (gacInfo.Version == 4)
						{
							var m = gac4Regex.Match(Path.GetFileName(dir2));
							if (!m.Success || m.Groups.Count != 4)
								continue;
							if (!Version.TryParse(m.Groups[1].Value, out version))
								continue;
							culture = m.Groups[2].Value;
						}
						else
							throw new InvalidOperationException();
						var asmName = Path.GetFileName(dir);
						var file = Path.Combine(dir2, asmName) + ".dll";
						if (!File.Exists(file))
						{
							file = Path.Combine(dir2, asmName) + ".exe";
							if (!File.Exists(file))
								continue;
						}
						var asmInfo = new AssemblyNameInfo
						{
							Name = asmName,
							Version = version,
							Culture = culture
						};
						yield return new GacFileInfo(asmInfo, file);
					}
				}
			}
		}
		static readonly Regex gac2Regex = new Regex("^([^_]+)_([^_]*)_([a-fA-F0-9]{16})$", RegexOptions.Compiled);
		static readonly Regex gac4Regex = new Regex("^v[^_]+_([^_]+)_([^_]*)_([a-fA-F0-9]{16})$", RegexOptions.Compiled);

		static string[] GetDirectories(string dir)
		{
			try
			{
				return Directory.GetDirectories(dir);
			}
			catch
			{
			}
			return Array.Empty<string>();
		}
	}
}
