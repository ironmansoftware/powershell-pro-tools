using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class PSAssembly 
    {
        public PSAssembly(Assembly assembly)
        {
            Name = assembly.GetName().Name;
            Version = assembly.GetName().Version.ToString();

            try 
            {
                Path = assembly.Location;   
            } catch {}
        }

        public PSAssembly() {}

        public static IEnumerable<PSAssembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                        .OrderBy(m => m.GetName().Name)
                        .Where(m => !m.GetName().Name.StartsWith("PowerShellToolsPro"))
                        .Where(m => !m.GetName().Name.StartsWith("PowerShellProTools"))
                        .Select(m => new PSAssembly(m));
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
    }

    public class PSType
    {
        public PSType(Type type)
        {
            Name = type.Name;
            AssemblyName = type.Assembly.GetName().Name;
            FullTypeName = type.FullName;
            Namespace = type.Namespace;
        }

        public PSType() {}

        public string Name { get; set; }
        public string AssemblyName { get; set; }
        public string FullTypeName { get; set; }
        public string Namespace { get; set; }

        public static IEnumerable<PSType> GetTypes(string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(m => m.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            return assembly.GetTypes().OrderBy(m => m.Name).Select(m => new PSType(m));
        }

        public static PSType FindType(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(m => m.GetTypes()).Where(m => m.FullName.Equals(typeName)).FirstOrDefault();

            if (type == null) return null;
            return new PSType(type);
        }
    }

    public class PSNamespace 
    {
        public PSNamespace(string fullName)
        {
            FullName = fullName;
            Name = FullName.Split('.').Last();
        }

        public PSNamespace() { }

        public string Name { get; set;  }
        public string FullName { get; set;  }

        static string GetTopLevelNamespace(string ns)
        {
            int firstDot = ns.IndexOf('.');
            return firstDot == -1 ? ns : ns.Substring(0, firstDot);
        }


        public static IEnumerable<PSNamespace> GetNamespaces(string assemblyName, string parentNamespace)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(m => m.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            var namespaces = assembly.GetTypes().Where(m => m.Namespace != null).Select(m => m.Namespace).Distinct();

            if (string.IsNullOrEmpty(parentNamespace))
            {
                return namespaces.Select(m => GetTopLevelNamespace(m)).Distinct().OrderBy(m => m).Select(m => new PSNamespace(m));
            }

            var results = new List<string>();
            foreach (var ns in namespaces)
            {
                if (ns.StartsWith(parentNamespace) && ns != parentNamespace)
                {
                    var tl = GetTopLevelNamespace(ns.Replace(parentNamespace + ".", string.Empty));
                    results.Add(parentNamespace + "." + tl);
                }
            }

            return results.Distinct().OrderBy(m => m).Select(m => new PSNamespace(m));
        }
    }

    public class PSMember
    {
        public PSMember(MemberInfo memberInfo)
        {
            Name = memberInfo.Name;
            TypeName = memberInfo.DeclaringType.Name;
            AssemblyName = memberInfo.DeclaringType.Assembly.GetName().Name;
        }

        public PSMember() {}
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
    }

    public class PSProperty : PSMember
    {
        public PSProperty(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            CanSet = propertyInfo.CanWrite;
            CanGet = propertyInfo.CanRead;
            PropertyType = propertyInfo.PropertyType.Name;
        }

        public PSProperty() {}

        public static IEnumerable<PSProperty> GetProperties(string assemblyName, string typeName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(m => m.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            var type = assembly.GetTypes().FirstOrDefault(m => m.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            return type.GetProperties().OrderBy(m => m.Name).Select(m => new PSProperty(m));
        }

        public string PropertyType { get; set; }
        public bool CanSet { get; set; }
        public bool CanGet { get; set; }
    }

    public class PSField : PSMember
    {
        public PSField(FieldInfo fieldInfo) : base(fieldInfo)
        {
            FieldType = fieldInfo.FieldType.Name;
        }

        public PSField() {}

        public static IEnumerable<PSField> GetFields(string assemblyName, string typeName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(m => m.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            var type = assembly.GetTypes().FirstOrDefault(m => m.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            return type.GetFields().OrderBy(m => m.Name).Select(m => new PSField(m));
        }

        public string FieldType { get; set; }
    }

    public class PSMethod : PSMember
    {
        public PSMethod(MethodInfo methodInfo) : base(methodInfo)
        {
            Arguments = methodInfo.GetParameters().Select(m => new PSMethodArgument(m));
            var args = string.Empty;
            if (Arguments.Any())
            {
                args = Arguments.Select(m => $"{m.Type} {m.Name}").Aggregate((x,y) => x + "," + y);
            }

            Display = $"{Name}({args})";
        }

        public static IEnumerable<PSMethod> GetMethods(string assemblyName, string typeName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(m => m.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            var type = assembly.GetTypes().FirstOrDefault(m => m.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            return type.GetMethods().OrderBy(m => m.Name).Select(m => new PSMethod(m));
        }

        public PSMethod() {}
        internal IEnumerable<PSMethodArgument> Arguments { get; set; }
        public string Display { get; set; }
    }

    public class PSMethodArgument
    {
        public PSMethodArgument(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            Type = parameterInfo.ParameterType.Name;
        }

        public PSMethodArgument() {}

        public string Name { get; set; }
        public string Type { get; set; }
    }
}