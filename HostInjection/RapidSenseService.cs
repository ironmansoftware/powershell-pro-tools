using HostInjection;
using HostInjection.Models;
using PowerShellToolsPro;
using PowerShellToolsPro.Cmdlets.VSCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PowerShellProTools.Host
{
    public class RapidSenseService
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IPoshToolsServer _server;

        private IEnumerable<CompletionItem> _variables;
        private IEnumerable<CompletionItem> _commands;
        private IEnumerable<CompletionItem> _types;

        private Dictionary<string, IEnumerable<CompletionItem>> _variableMembers;
        private Dictionary<string, IEnumerable<CompletionItem>> _commandParameters;

        private RapidSenseOptions _options;

        private IEnumerable<Regex> _ignoredPaths;

        public RapidSenseService(IPoshToolsServer server)
        {
            _server = server;
        }

        public void RefreshCache()
        {
            if (_options == null) return;
            UpdateCache(_options, false);
        }

        public void UpdateCache(RapidSenseOptions options, bool refreshTypes = true)
        {
            _options = options; 

            _variableMembers = new Dictionary<string, IEnumerable<CompletionItem>>();
            _commandParameters = new Dictionary<string, IEnumerable<CompletionItem>>();

            _variables = _server.ExecutePowerShellMainRunspace<PSObject>($"Get-Variable |  Out-PoshToolsVariable -PassThru | Get-CompletionItem -IgnoredVariables '{options.IgnoredVariables}'").Where(m => m != null).Select(m => new CompletionItem(m)).ToArray();
            _commands = _server.ExecutePowerShellMainRunspace<PSObject>($"Get-Command | Get-CompletionItem -IgnoredModules '{options.IgnoredModules}' -IgnoredCommands '{options.IgnoredCommands}'").Where(m => m != null).Select(m => new CompletionItem(m)).ToArray();

            if (refreshTypes)
                _types = _server.ExecutePowerShellMainRunspace<PSObject>($"Get-CompletionItem -Types -IgnoredAssemblies '{options.IgnoredAssemblies}' -IgnoredTypes '{options.IgnoredTypes}'").Where(m => m != null).Select(m => new CompletionItem(m)).ToArray();

            if (options.IgnoredPaths != null)
                _ignoredPaths = options.IgnoredPaths.Split(';').Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToArray();
        }

        public IEnumerable<CompletionItem> Complete(string trigger, string currentLine, int position)
        {
            if (!semaphoreSlim.Wait(0))
            {
                return null;
            }

            try
            {
                if (trigger == "$")
                {
                    return _variables;
                }

                if (trigger == ".")
                {
                    var path = FindTerminator(currentLine, position, '$', '[');

                    // Variables
                    if (path.StartsWith("$"))
                    {
                        if (_variableMembers.ContainsKey(path))
                        {
                            return _variableMembers[path];
                        }
                        else
                        {
                            var members = _server.ExecutePowerShellMainRunspace<PSObject>($"Get-PoshToolsVariable -Path '{path}' -ValueOnly | Get-Member | Select-Object Name,MemberType | ForEach-Object {{ [PowerShellToolsPro.Cmdlets.VSCode.CompletionItem]$_  }} ").ToArray();
                            var items = members.Where(m => m != null).Select(m => new CompletionItem(m)).ToList();
                            _variableMembers.Add(path, items);
                            return items;
                        }
                    }
                    // Types
                    else if (path.StartsWith("["))
                    {
                        return _types
                            .Where(m => m.InsertText.StartsWith(path.TrimStart('['), StringComparison.OrdinalIgnoreCase))
                            .Select(m => new CompletionItem(m.InsertText.Substring(path.TrimStart('[').Length).TrimStart('.'), CompletionKind.Class));
                    }
                }

                if (trigger == "-")
                {
                    // Search for a command
                    if (currentLine.Length > 1 && currentLine[position - 2] != ' ')
                    {
                        var commandPrefix = FindTerminator(currentLine, position, ' ', '|', '{', ';').Trim();
                        return _commands.Where(m => m.InsertText.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
                    }
                    // Search for a command parameter
                    else if (currentLine.Length > 1 && currentLine[position - 2] == ' ')
                    {
                        var ast = Parser.ParseInput(currentLine, out Token[] tokens, out ParseError[] errs);

                        var command = (CommandAst)ast.Find(m =>
                            m.Extent.StartColumnNumber < position &&
                            m.Extent.EndColumnNumber > position &&
                            m is CommandAst commandAst &&
                            commandAst.CommandElements.Any(x => x.ToString() == "-"), true);

                        var name = command.GetCommandName();

                        if (_commandParameters.ContainsKey(name))
                        {
                            return _commandParameters[name];
                        }
                        else
                        {
                            var parameters = _server.ExecutePowerShellMainRunspace<PSObject>($"(Get-Command -Name '{name}').Parameters.Values | ForEach-Object {{ [PowerShellToolsPro.Cmdlets.VSCode.CompletionItem]$_  }} ").ToArray();
                            var items = parameters.Where(m => m != null).Select(m => new CompletionItem(m)).ToList();
                            _commandParameters.Add(name, items);
                            return items;
                        }
                    }
                }

                if (trigger == "[")
                {
                    return _types;
                }

                if (trigger == "\\")
                {
                    var ast = Parser.ParseInput(currentLine, out Token[] tokens, out ParseError[] errs);
                    var stringAst = ast.Find(m =>
                            m.Extent.StartColumnNumber < position &&
                            m.Extent.EndColumnNumber > position &&
                            (m is StringConstantExpressionAst || m is ExpandableStringExpressionAst), true);

                    var terminators = new[] { ' ' };
                    if (stringAst is StringConstantExpressionAst)
                    {
                        terminators = new[] { '\'' };
                    }

                    if (stringAst is ExpandableStringExpressionAst)
                    {
                        terminators = new[] { '"' };
                    }

                    var line = FindTerminator(currentLine, position + 1, terminators);
                    var items = _server.ExecutePowerShellMainRunspace<PSObject>($"Get-ChildItem '{line.Trim('\'').Trim('"')}' | Get-CompletionItem");

                    return items.Where(m => m != null).Select(m => new CompletionItem(m)).Where(m => !_ignoredPaths.Any(x => x.IsMatch(m.InsertText))).ToList();
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return null;
        }

        private string FindTerminator(string line, int position, params char[] terminators)
        {
            var path = new StringBuilder();
            for (int i = position - 2; i >= 0; i--)
            {
                path.Append(line[i]);
                if (terminators.Any(m => m == line[i]))
                {
                    break;
                }
            }

            char[] charArray = path.ToString().ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

    }
}
