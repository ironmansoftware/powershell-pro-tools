using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace IMS.FormDesigner
{
    public class TypeResolutionService : ITypeResolutionService
    {
        public Assembly GetAssembly(AssemblyName name)
        {
            return null;
        }

        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            return null;
        }

        public string GetPathOfAssembly(AssemblyName name)
        {
            return null;
        }

        public Type GetType(string name)
        {
            return GetType(name, false, false);
        }

        public Type GetType(string name, bool throwOnError)
        {
            return GetType(name, throwOnError, false);
        }

        public Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            if (name.Contains(","))
            {
                return Type.GetType(name);
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(name, throwOnError, ignoreCase);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        public void ReferenceAssembly(AssemblyName name)
        {
            
        }
    }
}
