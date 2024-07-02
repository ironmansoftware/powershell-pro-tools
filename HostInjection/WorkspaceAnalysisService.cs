using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace PowerShellProTools.Host
{
    public class WorkspaceAnalysisService
    {
        private readonly List<Workspace> workspaces = new List<Workspace>();

        public void AnalyzeWorkspace(string root)
        {
            var workspace = GetWorkspace(root);
            if (workspace == null) return;

            lock(workspace.AnalysisLock)
            {
                workspace.FunctionDefinitions.Clear();
                workspace.Variables.Clear();
                workspace.ClassDefinitions.Clear();

                try
                {
                    var directoryInfo = new DirectoryInfo(root);
                    foreach (var script in directoryInfo.GetFiles("*.ps1", SearchOption.AllDirectories))
                    {
                        AnalyzeFile(workspace, script);
                    }

                    foreach (var script in directoryInfo.GetFiles("*.psm1", SearchOption.AllDirectories))
                    {
                        AnalyzeFile(workspace, script);
                    }
                }
                catch
                {

                }
            }
        }

        private void AnalyzeFile(Workspace workspace, FileInfo file)
        {
            var text = string.Empty;
            var retries = 0;
            while (string.IsNullOrEmpty(text) || retries < 3)
            {
                try
                {
                    text = File.ReadAllText(file.FullName);
                    break;
                }
                catch
                {
                    retries++;
                    Thread.Sleep(100);
                }
            }

            if (string.IsNullOrEmpty(text)) return;

            var ast = Parser.ParseInput(text, out Token[] tokens, out ParseError[] errors);

            FindVariables(workspace, file, ast);
            FindFunctions(workspace, file, ast);
            FindClasses(workspace, file, ast);
        }

        private static void FindClasses(Workspace workspace, FileInfo file, ScriptBlockAst ast)
        {
            var typeDefinitions = ast.FindAll(m => m is TypeDefinitionAst, true).Cast<TypeDefinitionAst>();

            foreach (var typeDef in typeDefinitions)
            {
                var classDefinition = new ClassDefinition
                {
                    FileName = file.FullName,
                    Name = typeDef.Name,
                    Character = typeDef.Extent.StartColumnNumber - 1,
                    Line = typeDef.Extent.StartLineNumber - 1
                };

                foreach(var member in typeDef.Members)
                {
                    var memberDefinition = new ClassMemberDefinition
                    {
                        FileName = file.FullName,
                        Name = member.Name,
                        Character = member.Extent.StartColumnNumber - 1,
                        Line = member.Extent.StartLineNumber - 1
                    };

                    classDefinition.Members.Add(memberDefinition);
                }

                var typeExpressionAsts = ast.FindAll(m => m is TypeExpressionAst typeExpression && typeExpression.TypeName.Name?.Equals(typeDef.Name, StringComparison.OrdinalIgnoreCase) == true, true).Cast<TypeExpressionAst>();

                foreach (var typeExpression in typeExpressionAsts)
                {
                    var refLoc = new ReferenceLocation
                    {
                        Ast = typeExpression,
                        FileName = typeExpression.Extent.File,
                        Character = typeExpression.Extent.StartColumnNumber - 1,
                        Line = typeExpression.Extent.StartLineNumber - 1
                    };

                    classDefinition.References.Add(refLoc);
                }

                workspace.ClassDefinitions.Add(classDefinition);
            }
        }

        private static void FindFunctions(Workspace workspace, FileInfo file, ScriptBlockAst ast)
        {
            var funcDefs = ast.FindAll(m => m is FunctionDefinitionAst, true).Cast<FunctionDefinitionAst>();

            foreach (var funcDef in funcDefs)
            {
                var commands = ast.FindAll(m => m is CommandAst commandAst && commandAst.GetCommandName()?.Equals(funcDef.Name, StringComparison.OrdinalIgnoreCase) == true, true).Cast<CommandAst>();

                var functionDefinition = new FunctionDefinition
                {
                    FileName = file.FullName,
                    Name = funcDef.Name,
                    Character = funcDef.Extent.StartColumnNumber - 1,
                    Line = funcDef.Extent.StartLineNumber - 1
                };

                foreach (var command in commands)
                {
                    var commandName = command.GetCommandName().ToLower();
                    var refLoc = new ReferenceLocation
                    {
                        Ast = command,
                        FileName = command.Extent.File,
                        Character = command.Extent.StartColumnNumber - 1,
                        Line = command.Extent.StartLineNumber - 1
                    };

                    functionDefinition.References.Add(refLoc);
                }

                var functionLocalVariables = funcDef.FindAll(m => m is VariableExpressionAst varAst && !workspace.Variables.Any(x => x.Name.Equals(varAst.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase)), true).Cast<VariableExpressionAst>();

                functionDefinition.Variables.AddRange(functionLocalVariables);

                workspace.FunctionDefinitions.Add(functionDefinition);
            }
        }

        private static void FindVariables(Workspace workspace, FileInfo file, ScriptBlockAst ast)
        {
            var globalVariables = ast.FindAll(m => m is VariableExpressionAst && !m.IsNestedInFunction(), true)
                .Cast<VariableExpressionAst>()
                .Select(m => new VariableInfo
                {
                    FileName = file.FullName,
                    Ast = m,
                    Name = m.VariablePath.UserPath
                });

            workspace.Variables.AddRange(globalVariables);
        }

        public IEnumerable<FunctionDefinition> GetFunctionDefinitions(string file, string root)
        {
            var workspace = GetWorkspace(root);
            return workspace.FunctionDefinitions.Where(m => m.FileName.Equals(file, StringComparison.OrdinalIgnoreCase));
        }

        public void AddWorkspace(string root)
        {
            var workspace = new Workspace(root);
            workspaces.Add(workspace);
            AnalyzeWorkspace(root);
        }

        public void AnalyzeFile(string file)
        {
            var workspace = workspaces.FirstOrDefault(m => m.InWorkspace(file));
            if (workspace == null) return;

            AnalyzeWorkspace(workspace.Root);
        }

        public void RemoveWorkspace(string root)
        {
            var workspace = GetWorkspace(root);
            if (workspace != null)
            {
                workspaces.Remove(workspace);
            }
        }

        public Workspace GetWorkspace(string root)
        {
            return workspaces.FirstOrDefault(m => m.Root.Equals(root, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class ClassDefinition
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Character { get; set; }
        public TypeDefinitionAst Ast { get; set; }
        public List<ClassMemberDefinition> Members { get; set; } = new List<ClassMemberDefinition>();
        public List<ReferenceLocation> References { get; set; } = new List<ReferenceLocation>();
    }

    public class ClassMemberDefinition
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Character { get; set; }
    }

    public class FunctionDefinition
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Character { get; set; }
        public FunctionDefinitionAst Ast { get; set; }
        public List<ReferenceLocation> References { get; set; } = new List<ReferenceLocation>();
        public List<VariableExpressionAst> Variables { get; set; } = new List<VariableExpressionAst>();
    }

    public class VariableInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public VariableExpressionAst Ast { get; set; }
    }

    public class ReferenceLocation
    {
        public Ast Ast { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Character { get; set; }
    }

    public class Workspace
    {
        public Workspace(string root)
        {
            Root = root;
        }

        public bool InWorkspace(string path)
        {
            var parentUri = new Uri(Root);
            var childUri = new DirectoryInfo(path).Parent;
            while (childUri != null)
            {
                if (new Uri(childUri.FullName) == parentUri)
                {
                    return true;
                }
                childUri = childUri.Parent;
            }
            return false;
        }

        public object AnalysisLock { get; set; } = new object();
        public string Root { get; set; }
        public List<ClassDefinition> ClassDefinitions { get; } = new List<ClassDefinition>();
        public List<FunctionDefinition> FunctionDefinitions { get; } = new List<FunctionDefinition>();
        public List<VariableInfo> Variables { get; } = new List<VariableInfo>();
    }
}
