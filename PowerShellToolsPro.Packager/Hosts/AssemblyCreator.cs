using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellToolsPro.Packager.Hosts
{
    public class AssemblyCreator
    {
		private static string[] _commonParameters = new[] { "Verbose", "Debug", "ErrorAction", "WarningAction", "InformationAction", "ErrorVariable", "WarningVariable", "InformationVariable", "OutVariable", "OutBuffer", "PipelineVariable", "Confirm", "WhatIf" };

	    public class NamespaceDefinition
	    {
		    public NamespaceDefinition(string name, IEnumerable<ClassDefinition> classes)
		    {
			    Name = name;
			    Classes = classes;
		    }

		    public string Name { get; }
		    public IEnumerable<ClassDefinition> Classes { get; }
	    }

	    public class ClassDefinition
	    {
		    public ClassDefinition(string name, IEnumerable<MethodDefinition> methods)
		    {
			    Name = name;
			    Methods = methods;
		    }

		    public string Name { get; }
		    public IEnumerable<MethodDefinition> Methods { get; }
	    }

	    public class MethodDefinition
	    {
		    public MethodDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters)
		    {
			    Name = name;
			    ReturnType = returnType;
			    Parameters = parameters;
		    }

		    public string Name { get; }
			public Type ReturnType { get; }
			public IEnumerable<ParameterDefinition> Parameters { get; }
	    }

	    public class ParameterDefinition
	    {
		    public ParameterDefinition(string name, Type type, bool isMandatory)
		    {
			    Name = name;
			    Type = type;
			    IsMandatory = isMandatory;
		    }

		    public string Name { get; }
			public Type Type { get; }
			public bool IsMandatory { get; }
	    }

	    public NamespaceDefinition GenerateNamespace(string module)
	    {
		    var runspace = RunspaceFactory.CreateRunspace();
			runspace.Open();
		    
		    using (var ps = PowerShell.Create())
		    {
			    ps.Runspace = runspace;
			    ps.AddCommand("Import-Module");
			    ps.AddParameter("Name", module);
			    ps.Invoke();

				ps.Commands.Clear();
				
			    ps.AddCommand("Get-Module");
			    ps.AddParameter("Name", module);
			    var moduleDefinition = ps.Invoke().FirstOrDefault()?.ImmediateBaseObject as PSModuleInfo;
			    if (moduleDefinition == null)
			    {
				    return null;
			    }

			    var classes = moduleDefinition.ExportedCommands.Where(m => m.Key.Contains("-")).Select(m => m.Key.Split('-')[1]).Distinct();

			    var classDefinitions = new List<ClassDefinition>();
			    foreach (var @class in classes)
			    {
				    var methods =
					    moduleDefinition.ExportedCommands.Where(m => m.Key.Contains("-") && m.Key.Split('-')[1].Equals(@class));

				    var methodDefinitions = new List<MethodDefinition>();
					foreach (var method in methods)
				    {
					    foreach (var parameterSet in method.Value.ParameterSets)
					    {
						    var methodName = method.Key.Split('-')[0];
						    if (method.Value.ParameterSets.Count > 1)
						    {
							    methodName += parameterSet.Name;
						    }

						    methodName = MakeValidMethodName(methodName);

							var methodDefinition = GenerateMethodDefintion(methodName, method.Value, parameterSet);
						    if (methodDefinition == null) continue;

						    methodDefinitions.Add(methodDefinition);
						}
					}

				    classDefinitions.Add(new ClassDefinition(@class, methodDefinitions));
				}

				return new NamespaceDefinition(moduleDefinition.Name, classDefinitions);
		    }
		}

	    private string MakeValidMethodName(string methodName)
	    {
		    if (methodName.Contains(" "))
		    {
			    return methodName.Split(' ')[0];
		    }

		    return methodName;
	    }

	    public MethodDefinition GenerateMethodDefintion(string name, CommandInfo commandInfo, CommandParameterSetInfo parameterSetInfo)
	    {
		    var parameterDefinitions = new List<ParameterDefinition>();
		    foreach (var parameter in parameterSetInfo.Parameters.Where(m => !_commonParameters.Contains(m.Name)))
		    {
			    parameterDefinitions.Add(new ParameterDefinition(parameter.Name, parameter.ParameterType, parameter.IsMandatory));
		    }

			return new MethodDefinition(name, typeof(object), parameterDefinitions);
	    }
    }
}
