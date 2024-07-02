using PowerShellToolsPro;

namespace PowerShellProTools.Host
{
    class Program
    {
        static void Main(string pipeName)
        {
            var logger = new ServerConsoleLogger();
            var packagingService = new PackagingService(logger);
            var formDesignerService = new FormGeneratorService();

            var server = new PoshToolsServer(packagingService, logger, formDesignerService);
            server.Start(pipeName);
        }
    }
}