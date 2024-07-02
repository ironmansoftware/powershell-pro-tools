using System;
using System.Linq;
using System.Text;
using PowerShellToolsPro.Packager.Hosts;

namespace PowerShellToolsPro.Packager
{
    public interface ICodeGenerator
    {
    }

	public class CSharpTextCodeGenerator : ICodeGenerator
	{
		public string GenerateNamespace(AssemblyCreator.NamespaceDefinition namespaceDefinition)
		{
			var namespaceName = namespaceDefinition.Name;
			if (namespaceDefinition.Name.Contains("-"))
			{
				namespaceName = namespaceDefinition.Name.Replace("-", string.Empty);
			}

			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("using System;");
			stringBuilder.AppendLine("using System.Linq;");
			stringBuilder.AppendLine($"namespace {namespaceName}");
			stringBuilder.AppendLine("{");
			foreach (var @class in namespaceDefinition.Classes)
			{
				stringBuilder.Append(GenerateClass(@class));
			}
			stringBuilder.AppendLine("}");
			return stringBuilder.ToString();
		}

		public string GenerateClass(AssemblyCreator.ClassDefinition classDefinition)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"public static class {classDefinition.Name}");
			stringBuilder.AppendLine("{");
			foreach (var method in classDefinition.Methods)
			{
				stringBuilder.Append(GenerateMethod(classDefinition.Name, method));
			}
			stringBuilder.AppendLine("}");
			return stringBuilder.ToString();
		}
		
		public string GenerateMethod(string className, AssemblyCreator.MethodDefinition methodDefinition)
		{
			var stringBuilder = new StringBuilder();

			var parameterStringBuilder = new StringBuilder();
			foreach (var parameter in methodDefinition.Parameters.Where(m => m.IsMandatory))
			{
				parameterStringBuilder.Append($"{GetTypeName(parameter.Type)} {parameter.Name}, ");
			}

			foreach (var parameter in methodDefinition.Parameters.Where(m => !m.IsMandatory))
			{
				parameterStringBuilder.Append($"{GetTypeName(parameter.Type)} {parameter.Name} = default({GetTypeName(parameter.Type)}), ");
			}

			if (parameterStringBuilder.Length > 0)
				parameterStringBuilder.Remove(parameterStringBuilder.Length - 2, 2);

			stringBuilder.AppendLine($"public static object {methodDefinition.Name} ({parameterStringBuilder})");
			stringBuilder.AppendLine("{");
			stringBuilder.AppendLine("	using(var powerShell = System.Management.Automation.PowerShell.Create())");
			stringBuilder.AppendLine("	{");

			stringBuilder.AppendLine($"powerShell.AddCommand(\"{className}-{methodDefinition.Name}\");");
			foreach (var parameter in methodDefinition.Parameters)
			{
				if (!parameter.IsMandatory)
				{
					stringBuilder.AppendLine($"if ({parameter.Name} != default({GetTypeName(parameter.Type)}))");
					stringBuilder.AppendLine("{");
				}

				stringBuilder.AppendLine($"powerShell.AddParameter(\"{parameter.Name}\", {parameter.Name});");

				if (!parameter.IsMandatory)
				{
					stringBuilder.AppendLine("}");
				}
			}

			stringBuilder.AppendLine("return powerShell.Invoke().Select(m => m.BaseObject);");

			stringBuilder.AppendLine("	}");
			stringBuilder.AppendLine("}");

			return stringBuilder.ToString();
		}

		private string GetTypeName(Type type)
		{
			var typeNameBuilder = new StringBuilder();
			if (type.IsGenericType)
			{
				var typeName = type.Name.Split('`')[0];
				typeNameBuilder.Append(typeName + "<");

				foreach (var genericParametr in type.GenericTypeArguments)
				{
					typeNameBuilder.Append($"{genericParametr.FullName}, ");
				}

				typeNameBuilder.Remove(typeNameBuilder.Length - 2, 2);
				typeNameBuilder.Append(">");
			}
			else
			{
				typeNameBuilder.Append(type.FullName);
			}

			return typeNameBuilder.ToString();
		}
	}
}
