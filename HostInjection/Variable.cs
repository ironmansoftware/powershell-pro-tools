using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Xml;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    public class Variable
    {
        public string VarName { get; set; }
        public string VarValue { get; set; }
        public string Type { get; set; }        
        public bool HasChildren { get; set; }
        private Variable Parent { get; set; }

        public string Path { get; set; }

        private readonly PSObject _source;
        private readonly PSPropertyInfo _propertyInfo;

        private Type[] baseTypes = new[] { typeof(string), typeof(int), typeof(bool) };

        public object Value
        {
            get
            {
                if (_source != null) return _source;
                if (_propertyInfo != null) return _propertyInfo.Value;
                return null;
            }
        }

        private static string GetFriendlyName(Type type)
        {
            if (type.IsGenericType)
                return type.Namespace + "." + type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetFriendlyName(x)).ToArray()) + ">";
            else
                return type.Name;
        }

        bool IsBaseType(object obj)
        {
            foreach(var baseType in baseTypes)
            {
                if (obj.GetType() == baseType)
                {
                    return true;
                }
            }
            return false;
        }

        public Variable() { }

        public Variable(PSVariable psVariable)
        {
            if (psVariable.Value != null)
            {
                _source = new PSObject(psVariable.Value);
            }

            VarName = psVariable.Name;

            var value = _source;
            VarValue = value?.ToString();
            GetVariableInfo(value);
        }


        public Variable(string name, PSObject psobject, Variable parent = null)
        {
            _source = psobject;

            VarName = name;
            var value = psobject;
            VarValue = value?.ToString();
            GetVariableInfo(value);
            Parent = parent;
            Path = GetPath(this);
        }


        public Variable(PSObject psobject, Variable parent = null)
        {
            _source = psobject; 

            VarName = psobject.Properties["Name"].Value.ToString();
            var value = psobject.Properties["Value"].Value;
            VarValue = value?.ToString();
            GetVariableInfo(value);
            Parent = parent;
            Path = GetPath(this);
        }

        public Variable(PSPropertyInfo propertyInfo, Variable parent = null)
        {
            _propertyInfo = propertyInfo;

            VarName = propertyInfo.Name;

            object value;
            try
            {
                value = propertyInfo.Value;
            }
            catch
            {
                value = null;
            }
            VarValue = value?.ToString();

            if (value != null)
            {
                value = new PSObject(value);
            }

            GetVariableInfo(value);
            Parent = parent;
            Path = GetPath(this);
        }

        public Variable FindChild(string path)
        {
            var children = GetChildren().ToArray();
            var child = children.FirstOrDefault(m => m.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (child != null)
            {
                return child;
            }

            foreach(var c in children)
            {
                if (path.StartsWith(c.Path, StringComparison.OrdinalIgnoreCase))
                {
                    child = c.FindChild(path);
                }
                
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private static string GetPath(Variable variable)
        {
            var rootPath = string.Empty;   
            if (variable.Parent != null)
            {
                rootPath += GetPath(variable.Parent);            
            }

            if (variable.Parent == null)
            {
                rootPath += "$" + variable.VarName;
            }
            else if (variable.VarName.StartsWith("["))
            {
                rootPath += variable.VarName;
            }
            else
            {
                rootPath += "." + variable.VarName;
            }

            

            return rootPath;
        }

        private void GetVariableInfo(object value)
        {
            Type = "";

            if (value != null)
            {
                if (value is PSObject psObject)
                {
                    Type = psObject.TypeNames.First();
                    HasChildren = !IsBaseType(psObject.BaseObject);
                }
                else if (value is Hashtable)
                {
                    Type = value.GetType().Name;
                    HasChildren = true;
                }
                else if (value is string)
                {
                    Type = value.GetType().ToString();
                }
                else if (value is IEnumerable)
                {
                    Type = VarValue = GetFriendlyName(value.GetType());
                    HasChildren = true;
                }
                else
                {
                    Type = value.GetType().ToString();
                }
            }

            Type = Type.Replace("Deserialized.", string.Empty);
        }

        public IEnumerable<Variable> GetChildren()
        {
            if (_source != null)
            {
                foreach (var item in ProcessObject(_source))
                {
                    yield return item;
                }
                yield break;
            }

            if (_propertyInfo.Value == null) yield break;

            foreach (var item in ProcessObject(_propertyInfo.Value))
            {
                yield return item;
            }
        }

        private IEnumerable<Variable> ProcessObject(object value)
        {
            if (value is IEnumerable enumerable && !(value is string))
            {
                foreach (var item in ProcessEnumerable(enumerable))
                {
                    yield return item;
                }
            }

            if (value is PSObject psObject)
            {
                foreach (var item in ProcessPSObject(psObject))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in ProcessPSObject(new PSObject(value)))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<Variable> ProcessPSObject(PSObject psObject)
        {
            if (psObject.BaseObject is IEnumerable enumerable && !(psObject.BaseObject is string))
            {
                foreach (var item in ProcessEnumerable(enumerable))
                {
                    yield return item;
                }
            }

            if (psObject.BaseObject is Hashtable hashtable)
            {
                foreach (var key in hashtable.Keys)
                {
                    yield return new Variable(key.ToString(), new PSObject(hashtable[key]), this);
                }
            }
            else
            {
                foreach (var property in psObject.Properties)
                {
                    if (!property.IsGettable) continue;
                    yield return new Variable(property, this);
                }
            }
        }

        private IEnumerable<Variable> ProcessEnumerable(IEnumerable enumerable)
        {
            Type = VarValue = GetFriendlyName(enumerable.GetType());
            int index = 0;
            foreach (var item in enumerable)
            {
                yield return new Variable($"[{index}]", new PSObject(item), this);
                index++;
            }
        }
    }
}
