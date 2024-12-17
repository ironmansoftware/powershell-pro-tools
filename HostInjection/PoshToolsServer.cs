using HostInjection;
using HostInjection.Models;
using Newtonsoft.Json;
using PowerShellProTools;
using PowerShellProTools.Common;
using PowerShellProTools.Common.Models;
using PowerShellProTools.Common.Refactoring;
using PowerShellProTools.Host;
using PowerShellProTools.Host.Refactoring;
using PowerShellProTools.SharedCommands;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellToolsPro.Cmdlets.Profiling;
using PowerShellToolsPro.Cmdlets.VSCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellToolsPro
{
    public class PoshToolsServer : PSHost, IPoshToolsServer
    {
        private Runspace _runspace;
        private RapidSenseService _rapidSenseService;
        private HoverService _hoverService;
        private WorkspaceAnalysisService _workspaceAnalysisService = new WorkspaceAnalysisService();
        private ManualResetEvent _connected = new ManualResetEvent(false);
        private IPackagingService _packagingService;
        private IServerLogger _serverLogger;
        private IFormGeneratorService _formGeneratorService;
        private static PoshToolsServer _staticInstance;

        public PoshToolsServer(IPackagingService packagingService, IServerLogger serverLogger, IFormGeneratorService formGeneratorService)
        {
            _packagingService = packagingService;
            _serverLogger = serverLogger;
            _formGeneratorService = formGeneratorService;
        }

        public PoshToolsServer()
        {
            _serverLogger = new ServerConsoleLogger();
            _formGeneratorService = new FormGeneratorService();
        }

        public void WriteLog(string output)
        {
            _serverLogger.WriteLog(output);
        }

        public static void StartAsync(string pipeName)
        {
            _staticInstance = new PoshToolsServer();

            Task.Run(() =>
            {
                _staticInstance.Start(pipeName);
            });
        }

        public void Start(string pipeName)
        {
            var iss = InitialSessionState.CreateDefault2();
            Runspace.DefaultRunspace = RunspaceFactory.CreateRunspace(iss);

            var Server = new NamedPipeServerStream(pipeName, PipeDirection.InOut);
            var streamReader = new StreamReader(Server);
            var streamWriter = new StreamWriter(Server);

            _serverLogger = new PipeLogger(pipeName);

            try
            {
                var input = "";
                while (input != "exit")
                {
                    try
                    {
                        if (!Server.IsConnected)
                        {
                            Server.WaitForConnection();

                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                Server.WaitForPipeDrain();
                            }
                        }

                        input = streamReader.ReadLine();

                        if (input == null) { continue; }
                        if (input == "exit") { break; }

                        var response = new Response();
                        string methodName = null;
                        try
                        {
                            var decodedRequest = Encoding.UTF8.GetString(Convert.FromBase64String(input));
                            var request = JsonConvert.DeserializeObject<Request>(decodedRequest);

                            var method = GetType().GetMethod(request.Method);
                            methodName = method?.Name;


                            if (method == null)
                            {
                                response.Error = $"Method not found: {request.Method}";
                            }
                            else
                            {
                                var args = new List<object>();

                                int index = 0;
                                foreach (var parameter in method.GetParameters())
                                {
                                    args.Add(Convert.ChangeType(request.Args.ElementAt(index), parameter.ParameterType));
                                    index++;
                                }

                                response.Data = method.Invoke(this, args.ToArray());
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog($"Exception running command: ${methodName} {ex}");
                            response.Error = ex.ToString();
                        }

                        var json = JsonConvert.SerializeObject(response);
                        var output = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                        var data = Encoding.UTF8.GetBytes(output);

                        Server.Write(data, 0, data.Length);

                        streamWriter.Flush();
                        Server.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        WriteLog("Exception in server: " + ex.ToString());
                        Server.Dispose();
                        Server = new NamedPipeServerStream("PPTPipe", PipeDirection.InOut);
                        streamReader = new StreamReader(Server);
                        streamWriter = new StreamWriter(Server);
                    }
                    finally
                    {

                    }
                }
            }
            finally
            {
                Server.Dispose();
            }
        }

        public AstNode[] GetAstChildren(int hashcode)
        {
            var nodes = _ast.FindAll((ast) =>
            {
                if (ast.Parent == null) return false;

                return ast.Parent.GetHashCode() == hashcode;
            }, true);

            return nodes.Select(x => AstToNode(x)).ToArray();
        }

        private Ast _ast;

        public override CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.CurrentUICulture;

        public override Guid InstanceId => new Guid();

        public override string Name => "PoshTools Host";

        public override PSHostUserInterface UI => null;

        public override Version Version => new Version(1, 1);

        public AstNode ParseAst(string path)
        {
            var script = File.ReadAllText(path);
            _ast = ScriptBlock.Create(script).Ast;

            return AstToNode(_ast);
        }

        private AstNode AstToNode(Ast ast)
        {
            return new AstNode
            {
                AstContent = ast.ToString(),
                AstType = ast.GetType().Name,
                EndOffset = ast.Extent.EndOffset,
                StartOffset = ast.Extent.StartOffset,
                HashCode = ast.GetHashCode()
            };
        }

        public void ShowWinFormDesigner(string designerFileName, string codeFileName)
        {
            var assemblyBasePath = Path.GetDirectoryName(GetType().Assembly.Location);
            var designer = Path.Combine(assemblyBasePath, "..", "PowerShellProTools", "FormDesigner", "WinFormDesigner.exe");

            var process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = designer;
            process.StartInfo.Arguments = $"-c \"{codeFileName}\" -d \"{designerFileName}\"";
            process.Start();
        }

        public void GenerateWinForm(string codeFilePath, string formPath, bool package)
        {
            _formGeneratorService.GenerateWinForm(codeFilePath, formPath, package);
        }

        public string Package(string packageFile)
        {
            return _packagingService.Package(packageFile);
        }

        public IEnumerable<PPTRunspace> GetRunspaces(int processId)
        {
            if (processId == Process.GetCurrentProcess().Id)
            {
                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = _runspace;
                    ps.AddScript("Get-Runspace ");
                    return ps.Invoke<PSObject>().Select(m => new PPTRunspace(m));
                }
            }

            var ci = new NamedPipeConnectionInfo(processId);
            using (var runspace = RunspaceFactory.CreateRunspace(ci))
            {
                runspace.Open();
                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    ps.AddScript("Get-Runspace ");
                    return ps.Invoke<PSObject>().Select(m => new PPTRunspace(m));
                }
            }
        }

        public IEnumerable<PSHostProcess> GetPSHostProcesses()
        {
            var processInfos = ExecutePowerShell<PSObject>("Get-PSHostProcessInfo");
            return processInfos.Select(m => new PSHostProcess(m));
        }

        public string GetModulePath(string name, string version)
        {
            return ExecutePowerShell<string>($"Get-Module -List -Name '{name}' | Where-Object Version -eq '{version}' | Select-Object -ExpandProperty Path").FirstOrDefault();
        }

        public string FindModuleVersion(string name)
        {
            return ExecutePowerShell<string>($"Find-Module -Name '{name}' | Select-Object -Expand Version").FirstOrDefault();
        }

        public void UninstallModule(string name, string version)
        {
            ExecutePowerShell($"Uninstall-Module -Name '${name}' -RequiredVersion '${version}'");
        }

        public void UpdateModule(string name)
        {
            ExecutePowerShell($"Update-Module -Name '{name}' -Force -AcceptLicense");
        }

        public IEnumerable<string> GetProviders()
        {
            return ExecutePowerShellMainRunspace<string>("Get-PSProvider | Select-Object -ExpandProperty Name");
        }

        public void GetItemProperty(string path)
        {
            ExecutePowerShellMainRunspace<PSObject>($"Get-ItemProperty -Path '{path}' | Out-VSCodeGridView");
        }

        public void ViewItems(string path)
        {
            ExecutePowerShellMainRunspace<PSObject>($"Get-ChildItem -Path '{path}' | Out-VSCodeGridView");
        }


        public IEnumerable<Item> GetItems(string path)
        {
            var items = new List<Item>();

            var objs = ExecutePowerShellMainRunspace<PSObject>($"Get-ChildItem -Path '{path}' | Where-Object {{ -not $_.PSisContainer }}");

            if (objs == null) return items;

            foreach (var obj in objs)
            {
                if (obj == null) continue;

                if (obj.Properties.Any(m => m.Name == "Name") && obj.Properties.Any(m => m.Name == "Value"))
                {
                    var value = obj.Properties["Value"].Value;
                    var name = obj.Properties["Name"].Value.ToString();
                    items.Add(new Item
                    {
                        Name = name,
                        Value = value == null ? "$null" : value.ToString(),
                        Path = Path.Combine(path, name)
                    });
                }
                else if (obj.Properties.Any(m => m.Name == "Name"))
                {
                    var name = obj.Properties["Name"].Value.ToString();
                    items.Add(new Item
                    {
                        Name = name,
                        Value = string.Empty,
                        Path = Path.Combine(path, name)
                    });
                }
                else if (obj.Properties.Any(m => m.Name == "Subject"))
                {
                    var name = obj.Properties["Subject"].Value.ToString();
                    items.Add(new Item
                    {
                        Name = name,
                        Value = obj.Properties["Thumbprint"].Value.ToString(),
                        Path = Path.Combine(path, name)
                    });
                }
            }

            return items;
        }

        public IEnumerable<Container> GetContainers(string parent)
        {
            var containers = new List<Container>();

            var objs = ExecutePowerShellMainRunspace<PSObject>($"Get-ChildItem -Path '{parent}' | Where-Object {{ $_.PSisContainer }} | Select-Object @{{n='Name'; e={{$_.PSChildName}}}}");

            if (objs == null) return containers;

            foreach (var obj in objs)
            {
                if (obj == null) continue;

                //var drive = obj.Properties["Drive"].Value.ToString();
                //var provider = obj.Properties["Provider"].Value.ToString();
                //var path = obj.Properties["Path"].Value.ToString();
                //if (path.StartsWith(provider))
                //{
                //    path = path.Replace(provider + "::", string.Empty);
                //}

                //if (!path.StartsWith(drive + ":\\"))
                //{
                //    path = drive + ":\\" + path;
                //}

                var name = obj.Properties["Name"].Value.ToString();

                containers.Add(new Container
                {
                    Name = name,
                    Path = Path.Combine(parent, name)
                });
            }

            return containers;
        }

        public IEnumerable<string> GetProviderDrives(string name)
        {
            return ExecutePowerShellMainRunspace<string>($"Get-PSDrive -PSProvider '{name}' | Select-Object -ExpandProperty Name");
        }

        public IEnumerable<PowerShellProTools.Host.Module> GetModules()
        {
            var objs = ExecutePowerShell<PSObject>("Get-Module -ListAvailable");

            var modules = new List<PowerShellProTools.Host.Module>();
            foreach (var module in objs)
            {
                var name = module.Properties["Name"].Value.ToString();
                var version = module.Properties["Version"].Value.ToString();
                var moduleBase = module.Properties["ModuleBase"].Value.ToString();

                var moduleVersion = new ModuleVersion
                {
                    Version = version,
                    ModuleBase = moduleBase
                };

                var mod = modules.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (mod == null)
                {
                    var fromRepo = module.Properties["RepositorySourceLocation"].Value != null;

                    mod = new PowerShellProTools.Host.Module
                    {
                        Name = name,
                        FromRepository = fromRepo,
                        Versions = new List<ModuleVersion> { moduleVersion }
                    };

                    modules.Add(mod);
                }
                else
                {
                    mod.Versions.Add(moduleVersion);
                }
            }

            return modules;
        }

        private int FindRunspaceId()
        {
            var script = @"
                Get-Runspace | ForEach-Object {
                    $h = $_.GetType().GetProperty('Host', [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic).GetValue($_)
                    if ($h.Name -eq 'Visual Studio Code Host') 
                    {
                        $_.Id
                    }
                }
            ";

            int retries = 0;
            while (true)
            {
                var ids = ExecutePowerShell<int>(script);
                if (ids.Any())
                {
                    return ids.First();
                }

                if (retries == 600)
                {
                    return -1;
                }

                Thread.Sleep(100);
                retries++;
            }
        }

        private void Inject()
        {
            WriteLog("Configuring runspace scheduler.");

            var id = FindRunspaceId();
            if (id == -1)
            {
                WriteLog("Failed to find Visual Studio Code runspace. Some features will not work.");
                return;
            }

            WriteLog("Found VS Code runspace: " + id);

            var script = $@"
$RS = Get-Runspace -Id {id}
$RS.Events.SubscribeEvent($null, 'PowerShell.OnIdle',  'PowerShell.OnIdle', $null, {{ 
        try {{
        $PSPCommand = [PowerShellProTools.CommandQueue]::GetCommand()
        if ($PSPCommand) 
        {{ 
            $PSPResults = Invoke-Expression $PSPCommand.Value
            [PowerShellProTools.CommandQueue]::SetResults($PSPResults, $PSPCommand)
        }} 
        }} catch {{ [PowerShellProTools.CommandQueue]::SetResults($_.ToString(), $PSPCommand) }}
    }}, $true, $false)";

            ExecutePowerShell(script);
        }

        private void Injectv3()
        {
            WriteLog("Configuring runspace scheduler.");

            var id = FindRunspaceId();
            if (id == -1)
            {
                WriteLog("Failed to find Visual Studio Code runspace. Some features will not work.");
                return;
            }

            WriteLog("Found VS Code runspace: " + id);

            ExecutePowerShell($"[PowerShellProTools.IntelliSenseManager]::InitV3({id})");
        }



        public bool Connect()
        {
            if (_connected.WaitOne(1))
            {
                return false;
            }

            try
            {
                WriteLog("Opening a runspace.");

                _runspace = RunspaceFactory.CreateRunspace();
                _runspace.Open();

                WriteLog("Loading PowerShell Pro Tools module");

                var assemblypath = new FileInfo(typeof(PoshToolsServer).Assembly.Location).DirectoryName;
                ExecutePowerShell("Import-Module '" + Path.Combine(assemblypath, "PowerShellProTools.VSCode.psd1'"));
                ExecutePowerShell($"$Env:PSModulePath += ';{Path.Combine(assemblypath, "..")}'");

                _v3 = ExecutePowerShell<bool>("[PowerShellProTools.IntelliSenseManager]::Isv3()").First();
                if (_v3)
                {
                    Injectv3();
                }
                else
                {
                    Inject();
                }

                _connected.Set();

                ExecutePowerShellMainRunspace<PSObject>("Import-Module '" + Path.Combine(assemblypath, "PowerShellProTools.VSCode.psd1") + "' -Scope Global");

                _rapidSenseService = new RapidSenseService(this);
                _hoverService = new HoverService(this);

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                return false;
            }
        }

        public void InstallPoshToolsModule()
        {
            ExecutePowerShell("Install-Module PowerShellProTools -Scope CurrentUser -Force -ErrorAction SilentlyContinue");
        }

        public bool DisablePsesIntelliSense(string options)
        {
            var rsOptions = JsonConvert.DeserializeObject<RapidSenseOptions>(options);
            ExecutePowerShell($"[PowerShellProTools.IntelliSenseManager]::DisablePsesIntellisense()");
            _rapidSenseService.UpdateCache(rsOptions);

            return true;
        }

        public void EnablePsesIntelliSense()
        {
            ExecutePowerShell($"[PowerShellProTools.IntelliSenseManager]::EnablePsesIntelliSense()");
        }

        public IEnumerable<Certificate> GetCodeSigningCerts()
        {
            var result = ExecutePowerShell<string>("Get-ChildItem cert:\\ -Recurse -CodeSigningCert | ForEach-Object { [PowerShellToolsPro.Certificate]::new($_) } | ConvertTo-Json -WarningAction SilentlyContinue").FirstOrDefault();
            if (result != null)
            {
                if (result.StartsWith("["))
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Certificate>>(result);
                }
                else
                {
                    return new[] { JsonConvert.DeserializeObject<Certificate>(result) };
                }
            }

            return new Certificate[0];
        }

        public void SignScript(string filePath, string certPath)
        {
            ExecutePowerShell($"Set-AuthenticodeSignature -FilePath '{filePath}' -Certificate (Get-Item '{certPath}') -TimestampServer 'http://timestamp.digicert.com' -Force");
        }

        public IEnumerable<CompletionItem> Complete(string trigger, string currentLine, int position)
        {
            return _rapidSenseService.Complete(trigger, currentLine, position);
        }

        public void RefreshCompletionCache()
        {
            _rapidSenseService.RefreshCache();
        }

        public ProfilingResult MeasureScript(string fileName)
        {
            var result = ExecutePowerShellMainRunspace<string>($"Measure-Script -FilePath '{fileName}' | ConvertTo-Json -Depth 100 -WarningAction SilentlyContinue").FirstOrDefault();
            return JsonConvert.DeserializeObject<ProfilingResult>(result);
        }

        public IEnumerable<Variable> GetVariables(bool excludeAutomatic)
        {
            var variables = ExecutePowerShellMainRunspace<string>($"Get-Variable | Out-PoshToolsVariable -PassThru -ExcludeAutomatic ${excludeAutomatic} | ConvertTo-Json -Depth 1 -WarningAction SilentlyContinue").FirstOrDefault();
            if (variables != null)
            {
                if (variables.StartsWith("["))
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Variable>>(variables);
                }
                else
                {
                    return new[] { JsonConvert.DeserializeObject<Variable>(variables) };
                }
            }
            return new Variable[0];
        }

        public IEnumerable<Variable> ExpandVariableDirect(string path)
        {
            return GetPoshToolsVariable.VariableDetailsCache.Where(m => path.StartsWith(m.Path));
        }

        public IEnumerable<Variable> ExpandVariable(string path)
        {
            var variables = ExecutePowerShellMainRunspace<string>($"Get-PoshToolsVariable -Path '{path}' | ConvertTo-Json -Depth 1 -WarningAction SilentlyContinue").FirstOrDefault();
            if (variables != null)
            {
                return JsonConvert.DeserializeObject<IEnumerable<Variable>>(variables);
            }
            return new Variable[0];
        }

        public IEnumerable<PSAssembly> GetAssemblies()
        {
            return ExecutePowerShell<PSAssembly>("[PowerShellToolsPro.Cmdlets.VSCode.PSAssembly]::GetAssemblies()");
        }

        public IEnumerable<PSType> GetTypes(string assembly, string parentNamespace)
        {
            var types = ExecutePowerShell<PSType>($"[PowerShellToolsPro.Cmdlets.VSCode.PSType]::GetTypes('{assembly}')");
            return types.Where(m => m.Namespace == parentNamespace);
        }

        public IEnumerable<PSNamespace> GetNamespaces(string assembly, string parentNamespace)
        {
            return ExecutePowerShell<PSNamespace>($"[PowerShellToolsPro.Cmdlets.VSCode.PSNamespace]::GetNamespaces('{assembly}', '{parentNamespace}')");
        }

        public IEnumerable<PowerShellToolsPro.Cmdlets.VSCode.PSMethod> GetMethods(string assembly, string typeName)
        {
            return ExecutePowerShell<PowerShellToolsPro.Cmdlets.VSCode.PSMethod>($"[PowerShellToolsPro.Cmdlets.VSCode.PSMethod]::GetMethods('{assembly}', '{typeName}')");
        }

        public IEnumerable<PowerShellToolsPro.Cmdlets.VSCode.PSProperty> GetProperties(string assembly, string typeName)
        {
            return ExecutePowerShell<PowerShellToolsPro.Cmdlets.VSCode.PSProperty>($"[PowerShellToolsPro.Cmdlets.VSCode.PSProperty]::GetProperties('{assembly}', '{typeName}')");
        }

        public IEnumerable<PowerShellToolsPro.Cmdlets.VSCode.PSField> GetFields(string assembly, string typeName)
        {
            return ExecutePowerShell<PowerShellToolsPro.Cmdlets.VSCode.PSField>($"[PowerShellToolsPro.Cmdlets.VSCode.PSField]::GetFields('{assembly}', '{typeName}')");
        }

        public PSType FindType(string typeName)
        {
            return ExecutePowerShell<PSType>($"[PowerShellToolsPro.Cmdlets.VSCode.PSType]::FindType('{typeName}')").FirstOrDefault();
        }

        public string DecompileType(string assemblyName, string typeName)
        {
            return ExecutePowerShell<string>($"[PowerShellProTools.Host.Decompiler]::Decompile('{assemblyName}','{typeName}')").FirstOrDefault();
        }

        public void LoadAssembly(string assemblyPath)
        {
            ExecutePowerShell($"[System.Reflection.Assembly]::LoadFrom('{assemblyPath}')");
        }

        public IEnumerable<TextEdit> Refactor(string request)
        {
            var refactorRequest = JsonConvert.DeserializeObject<RefactorRequest>(request);
            refactorRequest.EditorState.Server = this;
            return RefactorService.Refactor(refactorRequest);
        }

        public IEnumerable<RefactorInfo> GetValidRefactors(string request)
        {
            var refactorRequest = JsonConvert.DeserializeObject<RefactorRequest>(request);
            refactorRequest.EditorState.Server = this;
            return RefactorService.GetValidRefactors(refactorRequest);
        }

        public Hover GetHover(string request)
        {
            var state = JsonConvert.DeserializeObject<TextEditorState>(request);
            return _hoverService.GetHover(state);
        }

        public void AddWorkspaceFolder(string root)
        {
            Task.Run(() =>
            {
                _workspaceAnalysisService.AddWorkspace(root);
            });

        }

        public void AnalyzeWorkspaceFile(string file)
        {
            Task.Run(() =>
            {
                _workspaceAnalysisService.AnalyzeFile(file);
            });

        }


        public void AddRemoveFolder(string root)
        {
            _workspaceAnalysisService.RemoveWorkspace(root);
        }

        public IEnumerable<FunctionDefinition> GetFunctionDefinitions(string file, string root)
        {
            return _workspaceAnalysisService.GetFunctionDefinitions(file, root);
        }

        public string GetPowerShellProcessPath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }

        public IEnumerable<TreeView> GetTreeViews()
        {
            var json = ExecutePowerShellMainRunspace<string>("[PowerShellToolsPro.VSCode.TreeViewService]::Instance.GetTreeViews() | ConvertTo-Json -WarningAction SilentlyContinue").FirstOrDefault();

            if (json == null)
            {
                return Array.Empty<TreeView>();
            }

            if (json.StartsWith("["))
            {
                return JsonConvert.DeserializeObject<IEnumerable<TreeView>>(json);
            }
            else
            {
                return new[] { JsonConvert.DeserializeObject<TreeView>(json) };
            }
        }

        public IEnumerable<TreeItem> LoadChildren(string treeViewId, string path)
        {
            var json = ExecutePowerShellMainRunspace<string>($"[PowerShellToolsPro.VSCode.TreeViewService]::Instance.LoadChildren('{treeViewId}', '{path}') | ConvertTo-Json -WarningAction SilentlyContinue").First();

            if (json.StartsWith("["))
            {
                return JsonConvert.DeserializeObject<IEnumerable<TreeItem>>(json);
            }
            else
            {
                return new[] { JsonConvert.DeserializeObject<TreeItem>(json) };
            }
        }

        public IEnumerable<Session> GetSessions()
        {
            var json = ExecutePowerShellMainRunspace<string>("Get-PSSession | ForEach-Object { [PowerShellToolsPro.Cmdlets.VSCode.Session]$_ } | ConvertTo-Json -WarningAction SilentlyContinue").FirstOrDefault();

            if (json == null)
            {
                return Array.Empty<Session>();
            }

            if (json.StartsWith("["))
            {
                return JsonConvert.DeserializeObject<IEnumerable<Session>>(json);
            }
            else
            {
                return new[] { JsonConvert.DeserializeObject<Session>(json) };
            }
        }

        public void RemoveSession(int id)
        {
            ExecutePowerShellMainRunspace($"Remove-PSSession -Id {id}");
        }

        public void InvokeChild(string treeViewId, string path)
        {
            ExecutePowerShellMainRunspace($"[PowerShellToolsPro.VSCode.TreeViewService]::Instance.InvokeChild('{treeViewId}', '{path}')");
        }

        public void RefreshTreeView(string treeViewId)
        {
            ExecutePowerShell($"[PowerShellToolsPro.VSCode.TreeViewService]::Instance.RefreshTreeView('{treeViewId}')");
        }

        public IEnumerable<string> GetHistory()
        {
            var history = ExecutePowerShellMainRunspace<string>("Get-Content (Get-PSReadLineOption).HistorySavePath | Select-Object -First 25");
            var result = new List<string>();

            StringBuilder multiLine = null;
            foreach (var item in history)
            {
                if (item.EndsWith("`"))
                {
                    multiLine = new StringBuilder();
                    multiLine.AppendLine(item.TrimEnd('`'));
                }
                else if (multiLine != null)
                {
                    result.Add(multiLine.ToString());
                    multiLine = null;
                    result.Add(item);
                }
                else
                {
                    result.Add(item);
                }
            }

            result.Reverse();

            return result;
        }

        public IEnumerable<PSJob> GetJobs()
        {
            var json = ExecutePowerShellMainRunspace<string>("Get-Job | ForEach-Object { [PowerShellToolsPro.Cmdlets.VSCode.PSJob]$_ } | ConvertTo-Json -WarningAction SilentlyContinue").FirstOrDefault();

            if (json == null)
            {
                return Array.Empty<PSJob>();
            }

            if (json.StartsWith("["))
            {
                return JsonConvert.DeserializeObject<IEnumerable<PSJob>>(json);
            }
            else
            {
                return new[] { JsonConvert.DeserializeObject<PSJob>(json) };
            }
        }

        public void RemoveJob(int id)
        {
            ExecutePowerShellMainRunspace($"Remove-Job -Id {id}");
        }

        public void StopJob(int id)
        {
            ExecutePowerShell($"Stop-Job -Id {id}");
        }

        public IEnumerable<string> FindModule(string module)
        {
            return ExecutePowerShell<string>($"Find-Module '{module}' | Select-Object -ExpandProperty Name");
        }

        public void InstallModule(string module)
        {
            ExecutePowerShellMainRunspace($"Install-Module '{module}'");
        }

        public IEnumerable<T> ExecutePowerShellMainRunspace<T>(string command)
        {
            WriteLog("Scheduling command for main runspace: " + command);

            if (!_connected.WaitOne(5000))
            {
                WriteLog("Command execution cancelled. Extension is still connecting...");
                return new T[0];
            }

            if (_v3)
            {
                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = _runspace;
                    ps.AddScript($"[PowerShellProTools.IntelliSenseManager]::InvokeCommandWithResult('{command.Replace("'", "''")}')");
                    var result = ps.Invoke<T>();

                    if (ps.HadErrors)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            WriteLog("Error executing PowerShell: " + error.ToString());
                        }
                    }

                    return result;
                }
            }

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddScript($"[PowerShellProTools.CommandQueue]::InvokeCommand('{command.Replace("'", "''")}')");
                var result = ps.Invoke<T>();

                if (ps.HadErrors)
                {
                    foreach (var error in ps.Streams.Error)
                    {
                        WriteLog("Error executing PowerShell: " + error.ToString());
                    }
                }

                return result;
            }
        }

        public IEnumerable<TextEdit> RenameSymbol(string newName, string workspaceRoot, string request)
        {
            var refactorRequest = JsonConvert.DeserializeObject<RefactorRequest>(request);
            var ast = Parser.ParseInput(refactorRequest.EditorState.Content, out Token[] tokens, out ParseError[] errors);
            var renameSymbol = new RenameSymbol();

            var workspace = _workspaceAnalysisService.GetWorkspace(workspaceRoot);
            if (workspace == null)
            {
                return new TextEdit[0];
            }

            try
            {
                var array = renameSymbol.Refactor(newName, refactorRequest.EditorState, ast, workspace).ToArray();
                return array;
            }
            catch
            {
                return Array.Empty<TextEdit>();
            }
        }

        public string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        private double _previousTime = 0;
        private DateTime _previousTimeStamp = DateTime.Now;

        public Performance GetPerformance()
        {
            var process = Process.GetCurrentProcess();
            var currentTime = process.TotalProcessorTime.TotalSeconds;
            var currentTimeStamp = DateTime.Now;
            var elapsed = currentTimeStamp - _previousTimeStamp;

            var current = (((currentTime - _previousTime) / (elapsed.TotalSeconds)) / Environment.ProcessorCount) * 100;

            _previousTime = currentTime;
            _previousTimeStamp = currentTimeStamp;

            return new Performance
            {
                Memory = FormatBytes(process.PrivateMemorySize64),
                Cpu = $"{current:F}%"
            };

        }

        bool _v3;

        public void ExecutePowerShellMainRunspace(string command)
        {
            WriteLog("Scheduling command for main runspace: " + command);

            if (!_connected.WaitOne(5000))
            {
                WriteLog("Command execution cancelled. Extension is still connecting...");
                return;
            }

            if (_v3)
            {
                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = _runspace;
                    ps.AddScript($"[PowerShellProTools.IntelliSenseManager]::InvokeCommand('{command.Replace("'", "''")}')");
                    var result = ps.Invoke();

                    if (ps.HadErrors)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            WriteLog("Error executing PowerShell: " + error.ToString());
                        }
                    }

                    return;
                }
            }

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddScript($"[PowerShellProTools.CommandQueue]::InvokeCommand('{command.Replace("'", "''")}')");
                var result = ps.Invoke();

                if (ps.HadErrors)
                {
                    foreach (var error in ps.Streams.Error)
                    {
                        WriteLog("Error executing PowerShell: " + error.ToString());
                    }
                }
            }
        }

        public IEnumerable<T> ExecutePowerShell<T>(string command)
        {
            WriteLog("Executing command: " + command);

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddScript(command);
                var result = ps.Invoke<T>();

                if (ps.HadErrors)
                {
                    foreach (var error in ps.Streams.Error)
                    {
                        WriteLog("Error executing PowerShell: " + error.ToString());
                    }
                }

                return result;
            }
        }

        public object ExecutePowerShell(string command)
        {
            WriteLog("Executing command: " + command);

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddScript(command);
                var result = ps.Invoke().Select(m => m.BaseObject).FirstOrDefault()?.ToString();

                if (ps.HadErrors)
                {
                    foreach (var error in ps.Streams.Error)
                    {
                        WriteLog("Error executing PowerShell: " + error.ToString());
                    }
                }

                return result;
            }
        }

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
        }
    }

    public class Request
    {
        public string Method { get; set; }
        public object[] Args { get; set; }
        public int Id { get; set; }
    }

    public class Response
    {
        public object Data { get; set; }
        public int Id { get; set; }
        public string Error { get; set; }
    }
}
