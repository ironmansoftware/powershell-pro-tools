using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;

namespace IM.WinForms
{
	public class DesignSite : ISite, IServiceProvider, IDictionaryService
	{
		private IDesignerHost host = null;
		private IComponent component = null;
		private Hashtable dictionary = null;
		private string name = null;

		public DesignSite(IDesignerHost host, string name)
		{
			this.host = host;
			this.name = name;
		}

		public System.ComponentModel.IComponent Component
		{
			get
			{
				return component;
			}
		}

		public System.ComponentModel.IContainer Container
		{
			get
			{
				return host.Container;
			}
		}

		public bool DesignMode
		{
			get
			{
				return true;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				string oldName;
				if (value == null)
					throw new ArgumentException("Cannot set a component's name to a null value.");
				if (value == name)
					return;
				if (((DesignerHost)host).ContainsName(value))
					throw new ArgumentException("There is already a component with this name in the container.");
				oldName = name;
				MemberDescriptor md = TypeDescriptor.CreateProperty(component.GetType(), "Name", typeof(string), new Attribute[] { });
				((DesignerHost)host).OnComponentChanging(component, md);
				name = value;
				((DesignerHost)host).OnComponentRename(component, oldName, name);
				((DesignerHost)host).OnComponentChanged(component, md, oldName, name);
			}
		}

		public object GetService(System.Type serviceType)
		{
			if (serviceType == typeof(IDictionaryService))
				return this;
			else
				return host.GetService(serviceType);
		}

		internal void SetComponent(IComponent component)
		{
			this.component = component;
			if (name == null)
			{
				INameCreationService nameService = (INameCreationService)GetService(typeof(INameCreationService));
				name = nameService.CreateName(host.Container, component.GetType());
			}
		}

		public object GetKey(object value)
		{
			if (dictionary == null)
				return null;
			else
				return GetKeyFromValue(value);
		}

		private object GetKeyFromValue(object value)
		{
			IDictionaryEnumerator e = dictionary.GetEnumerator();

			while (e.MoveNext())
			{
				if (e.Value == value)
					return e.Key;
			}

			return null;
		}

		public object GetValue(object key)
		{
			if (dictionary == null)
				return null;
			else
				return dictionary[key];
		}

		public void SetValue(object key, object value)
		{
			if (dictionary == null)
				dictionary = new Hashtable();
			if (value == null)
			{
				dictionary.Remove(key);
				return;
			}
			dictionary[key] = value;
		}
	}
}
