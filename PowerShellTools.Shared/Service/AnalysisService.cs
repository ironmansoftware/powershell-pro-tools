using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using Common.Analysis;
using Common.ServiceManagement;
using Microsoft.VisualStudio.Shell.Interop;
using MoreLinq;
using Newtonsoft.Json;
using PowerShellTools.Common.Logging;
using PowerShellTools.Options;
using PowerShellTools.ServiceManagement;

namespace PowerShellTools.HostService.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly BlockingCollection<AnalysisRequest> requests;
        private Runspace analysisRunspace;
        private static readonly object RunspaceLock = new object();
        private string scriptAnalyzerSettings;
        private Process _process;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AnalysisService));
        private readonly IVsStatusbar _statusBar;
        private ConcurrentDictionary<string, List<FunctionDefinition>> _functionDefinitions;
        private ConcurrentDictionary<string, List<ClassDefinition>> _classDefinitions;

        private Dictionary<string, AnalysisResult> _analysisCache;

        public IEnumerable<FunctionDefinition> FunctionDefinitions
        {
            get
            {
                return _functionDefinitions.Values.SelectMany(m => m);
            }
        }

        public IEnumerable<ClassDefinition> ClassDefinitions
        {
            get
            {
                return _classDefinitions.Values.SelectMany(m => m);
            }
        }

        public void LogAnalysisResults()
        {
            if (Environment.GetEnvironmentVariable("POSHTOOLS_LOG_ANALYSIS_RESULTS") != null)
            {
                _logger.Debug("Function Definitions: " + JsonConvert.SerializeObject(FunctionDefinitions, Formatting.Indented));
                _logger.Debug("Class Definitions: " + JsonConvert.SerializeObject(ClassDefinitions, Formatting.Indented));
            }
        }

        private void Start()
        {
            _analysisCache = new Dictionary<string, AnalysisResult>();
            _functionDefinitions = new ConcurrentDictionary<string, List<FunctionDefinition>>();
            _classDefinitions = new ConcurrentDictionary<string, List<ClassDefinition>>();

            var version = GeneralOptions.Instance.PowerShellVersion;
            var hostProcess = PowershellHostProcessHelper.CreatePowerShellHostProcess(version);

            scriptAnalyzerSettings = AnalysisOptions.Instance.ConfigurationFile ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Pro Tools", "PSScriptAnalyzerSettings.psd1");
            AnalysisOptions.Instance.ConfigurationFileChanged += Instance_ConfigurationFileChanged; ;

            if (hostProcess != null)
            {
                _process = hostProcess.Process;

                var ci = new NamedPipeConnectionInfo(_process.Id);

                analysisRunspace = RunspaceFactory.CreateRunspace(ci);
                analysisRunspace.Open();
            }
            else
            {
                analysisRunspace = RunspaceFactory.CreateRunspace();
                analysisRunspace.Open();
            }
        }

        private void Instance_ConfigurationFileChanged(object sender, DebugEngine.EventArgs<string> e)
        {
            scriptAnalyzerSettings = e.Value;
            if (string.IsNullOrEmpty(scriptAnalyzerSettings))
            {
                scriptAnalyzerSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Pro Tools", "PSScriptAnalyzerSettings.psd1");
            }
        }

        public AnalysisService()
        {
            this.requests = new BlockingCollection<AnalysisRequest>();
            _statusBar = (IVsStatusbar)PowerShellToolsPackage.GetGlobalService(typeof(SVsStatusbar));

            try
            {
                GeneralOptions.Instance.PowerShellVersionChanged += (sender, args) =>
                {
                    try
                    {
                        analysisRunspace?.Dispose();
                        analysisRunspace = null;
                        _process?.Kill();
                    }
                    catch { }

                    Start();
                };

                Start();

                var task = Task.Run(() =>
                {
                    while(true)
                    {
                        try
                        {
                            var request = this.requests.Take();

                            if (request == null) continue;

                            _statusBar.SetText($"Analyzing files: {requests.Count} - {Path.GetFileName(request.FileName)}");

                            var result = new AnalysisResult
                            {
                                RequestId = request.RequestId,
                                FileName = request.FileName
                            };

                            try
                            {

                                if (request.String == null)
                                {
                                    result.LastModified = File.GetLastWriteTime(request.FileName);

                                    if (_analysisCache.ContainsKey(request.FileName) && _analysisCache[request.FileName].LastModified.Equals(result.LastModified))
                                    {
                                        continue;
                                    }
                                }


                                lock (RunspaceLock)
                                {
                                    if (request.String == null)
                                    {
                                        if (_analysisCache.ContainsKey(request.FileName))
                                        {
                                            _analysisCache[request.FileName] = result;
                                        }
                                        else
                                        {
                                            _analysisCache.Add(request.FileName, result);
                                        }
                                    }

                                    var issues = StartAnalysis(request);
                                    issues.ForEach(x => result.Issues.Add(x));

                                    Ast ast;
                                    if (request.String == null)
                                    {
                                        ast = Parser.ParseFile(request.FileName, out Token[] tokens, out ParseError[] errors);
                                    } else
                                    {
                                        ast = Parser.ParseInput(request.String, out Token[] tokens, out ParseError[] errors);
                                    }

                                    result.FunctionDefinitions = FindFunctionDefinitions(request.FileName, ast).ToList();
                                    result.ClassDefinitions = FindClassDefinitions(request.FileName, ast).ToList();

                                    if (_functionDefinitions.ContainsKey(result.FileName))
                                    {
                                        _functionDefinitions[result.FileName] = result.FunctionDefinitions.ToList();
                                        _classDefinitions[result.FileName] = result.ClassDefinitions.ToList();
                                    }
                                    else
                                    {
                                        _functionDefinitions.TryAdd(result.FileName, result.FunctionDefinitions.ToList());
                                        _classDefinitions.TryAdd(result.FileName, result.ClassDefinitions.ToList());
                                    }

                                    _logger.DebugFormat("FunctionDefinitions: {0}", result.FunctionDefinitions.Count);
                                    _logger.DebugFormat("ClassDefinitions: {0}", result.ClassDefinitions.Count);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error("Failed to analyze script.", ex);
                            }

                            if (OnAnalysisResults != null)
                            {
                                OnAnalysisResults(this, result);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Failed to notify of analysis results.", ex);
                        }
                        finally
                        {
                            _statusBar.Clear();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open analysis runspace.", ex);
            }
        }

        private IEnumerable<FunctionDefinition> FindFunctionDefinitions(string fileName, Ast ast)
        {
            foreach(var defintion in ast.FindAll(m => m is FunctionDefinitionAst, true).Cast<FunctionDefinitionAst>())
            {
                var parameters = new List<ParameterDefinition>();
                if (defintion.Parameters != null)
                {
                    parameters = defintion.Parameters.Select(m => new ParameterDefinition
                    {
                        FileName = fileName,
                        Name = m.Name.VariablePath.UserPath,
                        Line = m.Extent.StartLineNumber
                    }).ToList();
                }

                if (defintion.Body.ParamBlock != null)
                {
                    defintion.Body.ParamBlock.Parameters.ForEach(m =>
                    {
                        parameters.Add(new ParameterDefinition
                        {
                            FileName = fileName,
                            Name = m.Name.VariablePath.UserPath,
                            Line = m.Extent.StartLineNumber
                        });
                    });
                }

                string className = null;
                if (defintion.Parent is FunctionMemberAst memberAst && memberAst.Parent is TypeDefinitionAst typeDefinitionAst)
                {
                    className = typeDefinitionAst.Name;
                }

                yield return new FunctionDefinition
                {
                    FileName = fileName,
                    Name = defintion.Name,
                    Line = defintion.Extent.StartLineNumber,
                    Parameters = parameters,
                    ClassName = className
                };
            }
        }

        private IEnumerable<ClassDefinition> FindClassDefinitions(string fileName, Ast ast)
        {
            foreach (var defintion in ast.FindAll(m => m is TypeDefinitionAst, true).Cast<TypeDefinitionAst>())
            {
                var members = defintion.Members.Select(m => new MemberDefinition
                {
                    Name = m.Name,
                    Line = m.Extent.StartLineNumber,
                    FileName = fileName
                }).ToList();

                yield return new ClassDefinition
                {
                    FileName = fileName,
                    Name = defintion.Name,
                    Line = defintion.Extent.StartLineNumber,
                    Members = members
                };
            }
        }


        public List<AnalysisIssue> StartAnalysis(AnalysisRequest analysisRequest)
        {
            if (
                analysisRunspace == null ||
                analysisRunspace.RunspaceStateInfo.State != RunspaceState.Opened ||
                (string.IsNullOrEmpty(analysisRequest.String) && string.IsNullOrEmpty(analysisRequest.FileName)))
            return new List<AnalysisIssue>();

            _logger.Debug("StartAnalysis");

            using (var ps = PowerShell.Create())
            {
                if (string.IsNullOrEmpty(analysisRequest.String))
                {
                    ps.AddCommand("Invoke-ScriptAnalyzer").AddParameter("Path", analysisRequest.FileName);

                    if (File.Exists(scriptAnalyzerSettings))
                    {
                        ps.AddParameter("Setting", scriptAnalyzerSettings);
                    }
                }
                else
                {
                    ps.AddCommand("Invoke-ScriptAnalyzer").AddParameter("ScriptDefinition", analysisRequest.String);

                    if (File.Exists(scriptAnalyzerSettings))
                    {
                        ps.AddParameter("Setting", scriptAnalyzerSettings);
                    }
                }

                ps.AddCommand("ConvertTo-Json").AddParameter("Depth", 5);

                if (_process == null)
                    Runspace.DefaultRunspace = analysisRunspace;

                ps.Runspace = analysisRunspace;
                var results = ps.Invoke<string>();

                if (ps.HadErrors)
                {
                    foreach(var error in ps.Streams.Error)
                    {
                        _logger.Error($"Error running script analyzer: {error}");
                    }
                }

                if (!results.Any())
                {
                    return new List<AnalysisIssue>();
                }

                var result = results.First();
                if (!result.StartsWith("["))
                {
                    result = $"[{result}]";
                }

                return JsonConvert.DeserializeObject<List<AnalysisIssue>>(result);
            }
        }

        public void RequestAnalysis(AnalysisRequest request)
        {
            requests.Add(request);
        }

        public string GetScriptAnalyzerVersion()
        {
            using (var ps = PowerShell.Create())
            {
                lock (RunspaceLock)
                {
                    try
                    {
                        ps.AddCommand("Get-Module").AddParameter("Name", "PSScriptAnalyzer").AddParameter("ListAvailable");
                        ps.Runspace = analysisRunspace;
                        var result = ps.Invoke();

                        return result[0].Properties["Version"].Value.ToString();
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

            }
        }

        public bool InstallScriptAnalyzer(out string error)
        {
            error = string.Empty;

            InstallPackageProvider();
            using (var ps = PowerShell.Create())
            {
                lock (RunspaceLock)
                {
                    try
                    {
                        ps.AddCommand("Install-PackageProvider").AddParameter("Name", "NuGet").AddParameter("Force").AddParameter("Scope", "CurrentUser").AddStatement();
                        ps.AddCommand("Install-Module").AddParameter("Name", "PSScriptAnalyzer").AddParameter("Force").AddParameter("Scope", "CurrentUser");
                        ps.Runspace = analysisRunspace;
                        ps.Invoke();
                        if (ps.HadErrors)
                        {
                            error = ps.Streams.Error.First().ToString();
                            return false;
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                        _logger.Error("Failed to install PSScriptAnalyzer", ex);
                       return false;
                    }
                }
            }
        }

        public void InstallPackageProvider()
        {
            using (var ps = PowerShell.Create())
            {
                lock (RunspaceLock)
                {
                    try
                    {
                        ps.AddCommand("Install-PackageProvider").AddParameter("Name", "NuGet").AddParameter("Force").AddParameter("MinimumVersion", "2.8.5.201").AddParameter("ErrorAction", "Continue");
                        ps.Runspace = analysisRunspace;
                        ps.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to install PackageProvider", ex);
                    }
                }
            }
        }

        public bool IsAnalyzerInstalled()
        {
            using (var ps = PowerShell.Create())
            {
                lock (RunspaceLock)
                {
                    try
                    {
                        ps.AddCommand("Get-Module").AddParameter("Name", "PSScriptAnalyzer").AddParameter("ListAvailable");
                        ps.Runspace = analysisRunspace;
                        var result = ps.Invoke().Count > 0;

                        return result;
                    }
                    catch
                    {
                        return false;
                    }
                }

            }
        }

        public AnalysisRules GetAnalysisRules()
        {
            try
            {
                using (var ps = PowerShell.Create())
                {
                    lock (RunspaceLock)
                    {
                        ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", "Unrestricted").AddParameter("Scope", "Process");
                        ps.AddStatement();
                        ps.AddCommand("Get-ScriptAnalyzerRule");
                        ps.Runspace = analysisRunspace;
                        var results = ps.Invoke();

                        var rules = new AnalysisRules();
                        foreach (var rule in results)
                        {
                            var analysisRule = new AnalysisRule
                            {
                                Name = rule.Properties["RuleName"].Value.ToString(),
                                Description = rule.Properties["Description"].Value.ToString(),
                                Severity = rule.Properties["Severity"].Value.ToString(),
                                SourceName = rule.Properties["SourceName"].Value.ToString(),
                            };

                            rules.Rules.Add(analysisRule);
                        }

                        return rules;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get script analyzer rules.", ex);
                return new AnalysisRules();
            }
        }

        public string RequestFileAnalysis(string fileName)
        {
            var requestId = Guid.NewGuid().ToString();

            _logger.DebugFormat("RequestFileAnalysis: {0}", fileName);

            RequestAnalysis(new AnalysisRequest
            {
                FileName = fileName,
                RequestId = requestId
            });

            return requestId;
        }

        public string RequestStringAnalysis(string fileName, string content)
        {
            var requestId = Guid.NewGuid().ToString();

            _logger.DebugFormat("RequestStringAnalysis: {0}", fileName);

            RequestAnalysis(new AnalysisRequest
            {
                FileName = fileName,
                String = content,
                RequestId = requestId
            });

            return requestId;
        }

        public event EventHandler<AnalysisResult> OnAnalysisResults;
    }
}
