using Newtonsoft.Json;
using PowerShellProTools.Common;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using System;

namespace PowerShellProTools.Host
{
    public class PackagingService : IPathResolver, IPackagingService
    {
        private readonly IServerLogger _serverLogger;

        public PackagingService(IServerLogger serverLogger)
        {
            _serverLogger = serverLogger;
        }

        public string Package(string packageFile)
        {
            var path = packageFile;
            PsPackConfig config;
            try
            {
                var configDeserializer = new ConfigDeserializer(this);
                config = configDeserializer.Deserialize(path);
            }
            catch (Exception ex)
            {
                _serverLogger.WriteLog("ERROR: " + ex.Message);
                return JsonConvert.SerializeObject(new StageResult(false));
            }

            var packageProcess = new PackageProcess();
            packageProcess.Config = config;
            packageProcess.OnMessage += (sender, s) =>
            {
                _serverLogger.WriteLog("INFO: " + s);
            };

            packageProcess.OnErrorMessage += (sender, s) =>
            {
                _serverLogger.WriteLog("ERROR: " + s);
            };

            return JsonConvert.SerializeObject(packageProcess.Execute());
        }

        public string Resolve(string path)
        {
            return path;
        }
    }
}
