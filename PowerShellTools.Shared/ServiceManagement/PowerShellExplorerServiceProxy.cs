//using PowerShellTools.Common;
//using PowerShellTools.Common.ServiceManagement.ExplorerContract;
//using System.Collections.Generic;
//using System.Linq;
//using System.Management.Automation;
//using System.Threading.Tasks;

//namespace PowerShellTools.ServiceManagement
//{
//    public class PowerShellExplorerServiceProxy : IPowerShellExplorerService
//    {
//        private HostProtocol.ExplorerService.ExplorerServiceClient client;

//        public PowerShellExplorerServiceProxy(HostProtocol.ExplorerService.ExplorerServiceClient client)
//        {
//            this.client = client;
//        }

//        public async Task<string> GetCommandHelp(IPowerShellCommand command)
//        {
//            return (await client.GetCommandHelpAsync(new HostProtocol.PowerShellComand
//            {
//                Definition = command.Definition,
//                ModuleName = command.ModuleName,
//                Name = command.Name,
//                SupportsCommonParameters = command.SupportsCommonParameters,
//                Type = (int)command.Type
//            })).Value;
//        }

//        public async Task<IPowerShellCommandMetadata> GetCommandMetadata(IPowerShellCommand command)
//        {
//            var result = await client.GetCommandMetadataAsync(new HostProtocol.PowerShellComand
//            {
//                Definition = command.Definition,
//                ModuleName = command.ModuleName,
//                Name = command.Name,
//                SupportsCommonParameters = command.SupportsCommonParameters,
//                Type = (int)command.Type
//            });

//            var metadata = new PowerShellCommandMetadata();
//            metadata.Name = result.Name;

//            metadata.Parameters = result.Parameters.Select(m =>
//            {
//                var parameterMetadata = new PowerShellParameterMetadata
//                {
//                    Name = m.Name,
//                    IsDynamic = m.IsDynamic,
//                    SwitchParameter = m.SwitchParameter,
//                    Type = (ParameterType)m.Type
//                };

//                parameterMetadata.ParameterSets = m.ParameterSets.Select(x =>
//                {
//                    return new PowerShellParameterSetMetadata
//                    {
//                        HelpMessage = x.HelpMessage,
//                        IsMandatory = x.IsMandatory,
//                        Name = x.Name,
//                        Position = x.Position,
//                        ValueFromPipeline = x.ValueFromPipeline,
//                        ValueFromPipelineByPropertyName = x.ValueFromPipelineByPropertyName,
//                        ValueFromRemainingArguments = x.ValueFromRemainingArguments
//                    };
//                }).Cast<IPowerShellParameterSetMetadata>().ToList();

//                return parameterMetadata;
//            }).Cast<IPowerShellParameterMetadata>().ToList();

//            return metadata;
//        }

//        public async Task<List<IPowerShellCommand>> GetCommands()
//        {
//            var results = await client.GetCommandsAsync(new HostProtocol.nullMessage());

//            return results.Commands.Select(command =>
//            {
//                var x = new PowerShellCommand
//                {
//                    Definition = command.Definition,
//                    ModuleName = command.ModuleName,
//                    Name = command.Name,
//                    SupportsCommonParameters = command.SupportsCommonParameters,
//                    Type = (CommandTypes)command.Type
//                };
//                return x;
//            }).Cast<IPowerShellCommand>().ToList();

//        }

//        public async Task<List<IPowerShellModule>> GetModules()
//        {
//            var result = await client.GetModulesAsync(new HostProtocol.nullMessage());

//            return result.Modules.Select(m => new PowerShellModule
//            {
//                Name = m.Name
//            }).Cast<IPowerShellModule>().ToList();
//        }
//    }
//}
