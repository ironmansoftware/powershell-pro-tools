using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;
using PowerShellProTools.Common.Profiling;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class Instrumentation
    {
        public ScriptBlock Instrument(ScriptBlock scriptBlock, PSCmdlet cmdlet = null, string fileName = null)
        {
            var scriptBlockAst = scriptBlock.Ast as ScriptBlockAst;
            var ast = scriptBlockAst.EndBlock;

            int offset = ast.Extent.StartOffset * -1;
            var scriptText = ast.ToString();
            foreach(var originalAst in ast.FindAll(m => IsTopLevelPipeline(ast, m as PipelineAst), true))
            {
                var candidate = originalAst.Extent;

                scriptText = scriptText.Remove(candidate.StartOffset + offset, candidate.EndOffset - candidate.StartOffset);
                var replacementText = AdjustPipelineAst(fileName, originalAst as PipelineAst);
                scriptText = scriptText.Insert(candidate.StartOffset + offset, replacementText);
                offset += replacementText.Length - (candidate.EndOffset - candidate.StartOffset);
            }

            if (scriptText.StartsWith("{"))
            {
                scriptText = scriptText.Trim('{', '}');
            }

            var stringBuilder = new StringBuilder();

            if (cmdlet != null)
            {
                var proxyCommands = CreateProxyCommands(scriptBlock, cmdlet).ToArray();
                foreach (var proxyCommand in proxyCommands)
                {
                    stringBuilder.AppendLine(proxyCommand);
                }
            }

            var importModule = $"Import-Module '{Assembly.GetExecutingAssembly().Location}';";

            var start = scriptBlockAst.ToString().Substring(0, ast.Extent.StartOffset);

            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(start);
            scriptBuilder.AppendLine(importModule);
            scriptBuilder.AppendLine(stringBuilder.ToString());
            scriptBuilder.AppendLine(scriptText);

            return ScriptBlock.Create(scriptBuilder.ToString());
        }

        private string AdjustPipelineAst(string fileName, PipelineAst pipeline)
        {
            int offset = pipeline.Extent.StartOffset * -1;
            var scriptText = pipeline.ToString();
            foreach (var originalAst in pipeline.FindAll(m => IsChildPipeline(pipeline, m as PipelineAst), true).Cast<PipelineAst>())
            {
                var candidate = originalAst.Extent;
                scriptText = scriptText.Remove(candidate.StartOffset + offset, candidate.EndOffset - candidate.StartOffset);
                var replacementText = AdjustPipelineAst(fileName, originalAst);
                scriptText = scriptText.Insert(candidate.StartOffset + offset, replacementText);
                offset += replacementText.Length - (candidate.EndOffset - candidate.StartOffset);
            }

            var fileNameParameter = string.Empty;
            if (fileName != null)
            {
                fileNameParameter = $"-FileName '{fileName}' ";
            }

            return $"Measure-Block {fileNameParameter}-StartOffset {pipeline.Extent.StartOffset} -EndOffset {pipeline.Extent.EndOffset} -ScriptBlock {{ { scriptText } }}";
        }

        private bool IsTopLevelPipeline(Ast parentBlock, PipelineAst pipelineAst)
        {
            if (pipelineAst == null) return false;

            var parentAst = pipelineAst.Parent;
            while (parentAst != null && !(parentAst is PipelineAst) && parentAst != parentBlock)
            {
                parentAst = parentAst.Parent;
            }

            return !(parentAst is PipelineAst);
        }

        private bool IsChildPipeline(PipelineAst parent, PipelineAst ast)
        {
            if (ast == null) return false;

            var parentAst = ast.Parent;
            while(parentAst != null && parentAst != parent && !(parentAst is PipelineAst))
            {
                parentAst = parentAst.Parent;
            }

            return parentAst == parent;
        }

        public IEnumerable<string> CreateProxyCommands(ScriptBlock scriptBlock, PSCmdlet cmdlet)
        {
            var commands = scriptBlock.Ast.FindAll(m => m is CommandAst, true).Cast<CommandAst>().DistinctBy(m => m.GetCommandName());

            foreach (var command in commands)
            {
                var commandInfo = cmdlet.InvokeCommand.GetCommand(command.GetCommandName(), CommandTypes.All);
                if (commandInfo != null)
                {
                    var commandMetadata = new CommandMetadata(commandInfo);
                    var rawProxyCommand = ProxyCommand.Create(commandMetadata);

                    var block = $"Measure-Block -ModuleName '{commandInfo.ModuleName}' -CommandName '{commandInfo.Name}' -PipelineMethod 'begin' -ScriptBlock {{\r$steppablePipeline.Begin($PSCmdlet)}}";
                    rawProxyCommand = rawProxyCommand.Replace("$steppablePipeline.Begin($PSCmdlet)", block);

                    block = $"Measure-Block -ModuleName '{commandInfo.ModuleName}' -CommandName '{commandInfo.Name}' -PipelineMethod 'process' -ScriptBlock {{\r$steppablePipeline.Process($_)}}";
                    rawProxyCommand = rawProxyCommand.Replace("$steppablePipeline.Process($_)", block);

                    block = $"Measure-Block -ModuleName '{commandInfo.ModuleName}' -CommandName '{commandInfo.Name}' -PipelineMethod 'end' -ScriptBlock {{\r$steppablePipeline.End()}}";
                    rawProxyCommand = rawProxyCommand.Replace("$steppablePipeline.End()", block);

                    yield return $"function {command.GetCommandName()} {{ {rawProxyCommand} }}";
                }
            }
        }
    }
}
