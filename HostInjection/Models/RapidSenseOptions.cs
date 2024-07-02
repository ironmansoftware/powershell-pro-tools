namespace HostInjection.Models
{
    public class RapidSenseOptions
    {
        public string IgnoredAssemblies { get; set; } = string.Empty;
        public string IgnoredTypes { get; set; } = string.Empty;
        public string IgnoredModules { get; set; } = string.Empty;
        public string IgnoredCommands { get; set; } = string.Empty;
        public string IgnoredVariables { get; set; } = string.Empty;
        public string IgnoredPaths { get; set; } = string.Empty;
    }
}
