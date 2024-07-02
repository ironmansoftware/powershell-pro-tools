using PowerShellToolsPro.Packager.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSPackager
{
    public class HostDownloadService
    {
        public async Task<Version> GetLatestVersion()
        {
            var client = new HttpClient();
            var version = await client.GetStringAsync("https://imsreleases.blob.core.windows.net/packager/hosts/version.txt");

            return new Version(version);
        }

        public IEnumerable<Host> GetLocalHosts()
        {
            var hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IronmanPowerShellHost");
            if (!Directory.Exists(hostsPath)) yield break;

            var hosts = Directory.GetFiles(hostsPath, "host.manifest.json", SearchOption.AllDirectories);
            foreach(var hostFile in hosts)
            {
                var hostDirectory = Path.GetDirectoryName(hostFile);
                var content = File.ReadAllText(hostFile);
                var hostManifest = JsonSerializer.Deserialize<HostManifest>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                foreach(var host in hostManifest.Hosts)
                {
                    host.Version = hostManifest.Version.ToString();
                    host.Downloaded = true;
                    host.FileName = Path.Combine(hostDirectory, host.FileName);

                    yield return host;
                }
            }
        }

        public async Task<IEnumerable<Host>> GetHosts()
        {
            var version = await GetLatestVersion();

            var client = new HttpClient();
            var manifest = await client.GetStringAsync($"https://imsreleases.blob.core.windows.net/packager/hosts/{version}/host.manifest.json");
            var hostManifest = JsonSerializer.Deserialize<HostManifest>(manifest);

            var hostPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IronmanPowerShellHost", hostManifest.Version.ToString());
            if (Directory.Exists(hostPath))
            {
                foreach (var host in hostManifest.Hosts)
                {
                    host.Downloaded = File.Exists(Path.Combine(hostPath, host.FileName));
                }
            }

            return hostManifest.Hosts;
        }

        public async Task DownloadHost(Host host)
        {
            var hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IronmanPowerShellHost", host.Version.ToString());
            if (!Directory.Exists(hostsPath))
            {
                Directory.CreateDirectory(hostsPath);
            }

            var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync($"https://imsreleases.blob.core.windows.net/packager/hosts/{host.FileName}");

            var hostPath = Path.Combine(hostsPath, host.FileName);
            File.WriteAllBytes(hostPath, bytes);

        }
    }

    public class Host
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public PowerShellHosts GetId()
        {
            return (PowerShellHosts)Enum.Parse(typeof(PowerShellHosts), Id);
        }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string Version { get; set; }
        public bool Downloaded { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class HostManifest
    {
        public string Version { get; set; }
        public List<Host> Hosts { get; set; }
    }
}
