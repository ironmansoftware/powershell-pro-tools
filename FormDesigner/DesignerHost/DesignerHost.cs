using IMS.FormDesigner;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace IM.WinForms
{
	public interface ITypeInfo
	{
		string TypeName { get; }
		string Namespace { get; }
	}

	[ProvideProperty("Name", typeof(IComponent))]
	internal class DesignerHost : IDesignerHost, IContainer, IComponentChangeService, IExtenderProvider, ITypeDescriptorFilterService, IExtenderListService, IExtenderProviderService, ITypeInfo, ITypeDiscoveryService
	{
		private Stack transactions = null;
		private IServiceContainer parent = null;
		private readonly List<IComponent> components = null;
		private readonly Hashtable designers = null;
		private readonly ArrayList extenderProviders = null;
		private IComponent rootComponent = null;

		public string TypeName { get; private set; }
		public string Namespace { get; private set; }

		public CodeDomProvider CodeDomProvider { get; set; }

		public string FileName { get; set; }

		public IRootDesigner RootDesigner { get; private set; }

		public DesignerHost(IServiceContainer parent, CodeDomProvider codeDomProvider, string designerFile)
		{
			this.parent = parent;
			this.FileName = designerFile;
			components = new List<IComponent>();
			designers = new Hashtable();
			transactions = new Stack();
			parent.AddService(typeof(ITypeInfo), this);
			parent.AddService(typeof(IDesignerHost), this);
			parent.AddService(typeof(IContainer), this);
			parent.AddService(typeof(IComponentChangeService), this);
			parent.AddService(typeof(ITypeDescriptorFilterService), this);
			parent.AddService(typeof(ITypeResolutionService), new TypeResolutionService());
			parent.AddService(typeof(ITypeDiscoveryService), this);
			extenderProviders = new ArrayList();
			parent.AddService(typeof(IExtenderListService), this);
			parent.AddService(typeof(IExtenderProviderService), this);
			AddExtenderProvider(this);
			parent.AddService(typeof(ISelectionService), new SelectionService(this));

			this.CodeDomProvider = codeDomProvider;

			var component = new SplitterPanel(new SplitContainer());
			Add(component);
			Remove(component);

			Load();
		}

		public void Load()
		{
			if (!File.Exists(FileName))
			{
				Namespace = "Namespace";
				TypeName = "Form1";
				return;
			}

			CodeCompileUnit codeCompileUnit;
			using (var textReader = new StreamReader(FileName))
			{
				codeCompileUnit = CodeDomProvider.Parse(textReader);
			}

			if (codeCompileUnit.Namespaces.Count == 0 || codeCompileUnit.Namespaces[0].Types.Count == 0) return;

			var type = codeCompileUnit.Namespaces[0].Types[0];

			Namespace = codeCompileUnit.Namespaces[0].Name;
			TypeName = type.Name;

			DesignerSerializationManager manager = new DesignerSerializationManager(parent);
			using (var session = manager.CreateSession())
			{
				TypeCodeDomSerializer serializer = manager.GetSerializer(typeof(Button), typeof(TypeCodeDomSerializer)) as TypeCodeDomSerializer;

				var component = (IComponent)serializer.Deserialize(manager, type);
			}
		}

		public void Save()
		{
			DesignerSerializationManager manager = new DesignerSerializationManager(parent);
			CodeTypeDeclaration declaration;

			using (var session = manager.CreateSession())
			{
				TypeCodeDomSerializer serializer = manager.GetSerializer(rootComponent.GetType(), typeof(TypeCodeDomSerializer)) as TypeCodeDomSerializer;
				List<object> list = new List<object>();
				foreach (IComponent item in Container.Components)
				{
					//PropertyInfo vName = item.GetType().GetProperty("Name");

					//if (vName != null)
					//{
					//    string realName = vName.GetValue(item) as string;

					//    if (realName == "")
					//    {
					//        vName.SetValue(item, "RENAMED");
					//    }
					//}

					list.Add(item);
				}
				declaration = serializer.Serialize(manager, rootComponent, list);
			}

			var option = new CodeGeneratorOptions();
			using (var textWriter = new StreamWriter(FileName))
			{
				var ns = new CodeNamespace(this.Namespace);
				ns.Types.Add(declaration);
				CodeDomProvider.GenerateCodeFromNamespace(ns, textWriter, option);
			}

			Saved?.Invoke(this, new EventArgs());
		}

		public object GetService(Type serviceType)
		{
			var service = serviceType.Name;
			return parent.GetService(serviceType);
		}

		public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
			parent.AddService(serviceType, callback, promote);
		}

		public void AddService(Type serviceType, ServiceCreatorCallback callback)
		{
			parent.AddService(serviceType, callback);
		}

		public void AddService(Type serviceType, object serviceInstance, bool promote)
		{
			parent.AddService(serviceType, serviceInstance, promote);
		}

		public void AddService(Type serviceType, object serviceInstance)
		{
			parent.AddService(serviceType, serviceInstance);
		}

		public void RemoveService(Type serviceType, bool promote)
		{
			parent.RemoveService(serviceType, promote);
		}

		public void RemoveService(System.Type serviceType)
		{
			parent.RemoveService(serviceType);
		}

		public void Activate()
		{
			var s = (ISelectionService)GetService(typeof(ISelectionService));

			if (s != null)
			{
				var o = new object[] { rootComponent };
				s.SetSelectedComponents(o);

				if (Activated != null)
					Activated(this, EventArgs.Empty);
			}
		}

		public IComponent CreateComponent(Type componentClass, string name)
		{
			IComponent component = null;
			component = (IComponent)Activator.CreateInstance(componentClass);
			Add(component, name);

			return component;
		}

		public IComponent CreateComponent(Type componentClass)
		{
			return CreateComponent(componentClass, null);
		}

		public DesignerTransaction CreateTransaction(string description)
		{
			DesignerTransaction transaction = null;
			if (transactions.Count == 0)
			{
				if (TransactionOpening != null)
					TransactionOpening(this, EventArgs.Empty);
			}
			if (description == null)
				transaction = new MegaDesignerTransaction(this);
			else
				transaction = new MegaDesignerTransaction(this, description);
			transactions.Push(transaction);
			TransactionOpened?.Invoke(this, EventArgs.Empty);

			return transaction;
		}

		internal void OnTransactionClosing(bool commit)
		{
			TransactionClosing?.Invoke(this, new DesignerTransactionCloseEventArgs(commit));
		}

		internal void OnTransactionClosed(bool commit)
		{
			TransactionClosed?.Invoke(this, new DesignerTransactionCloseEventArgs(commit));
			transactions.Pop();
		}

		public DesignerTransaction CreateTransaction()
		{
			return CreateTransaction(null);
		}

		public void DestroyComponent(IComponent component)
		{
			if (component is Form)
				return;

			DesignerTransaction t = null;
			t = CreateTransaction("Destroy Component");
			if (component.Site.Container == this)
			{
				OnComponentChanging(component, null);
				Remove(component);
				component.Dispose();
				OnComponentChanged(component, null, null, null);
			}
			t.Commit();
		}

		public IDesigner GetDesigner(IComponent component)
		{
			if (component == null)
				return null;

			var designer = (IDesigner)designers[component];

			return designer;
		}

		public System.Type GetType(string typeName)
		{
			var typeResolver = (ITypeResolutionService)GetService(typeof(ITypeResolutionService));

			if (typeResolver == null)
				return Type.GetType(typeName);
			else
				return typeResolver.GetType(typeName);
		}

		public IContainer Container
		{
			get
			{
				return this;
			}
		}

		public bool InTransaction
		{
			get
			{
				return (transactions.Count > 0);
			}
		}

		public bool Loading
		{
			get
			{
				return false;
			}
		}

		public IComponent RootComponent
		{
			get
			{
				return rootComponent;
			}
		}

		public string RootComponentClassName
		{
			get
			{
				return rootComponent.GetType().Name;
			}
		}

		public string TransactionDescription
		{
			get
			{
				if (InTransaction)
				{
					var t = (DesignerTransaction)transactions.Peek();
					return t.Description;
				}
				else
					return null;
			}
		}

		public event EventHandler Activated;
		public event EventHandler Deactivated;
		public event EventHandler LoadComplete;
		public event DesignerTransactionCloseEventHandler TransactionClosed;
		public event DesignerTransactionCloseEventHandler TransactionClosing;
		public event EventHandler Saved;
		public event EventHandler TransactionOpened;
		public event EventHandler TransactionOpening;

		internal bool ContainsName(string name)
		{
			return components.OfType<IComponent>().Any(m => m.Site.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		}

		public void Add(IComponent component, string name)
		{
			IDesigner designer = null;
			DesignSite site = null;
			if (component == null)
				throw new ArgumentNullException("Cannot add a null component to the container.");
			if (component.Site != null && component.Site.Container != this)
				component.Site.Container.Remove(component);
			if (name == null)
			{
				INameCreationService nameService = (INameCreationService)GetService(typeof(INameCreationService));
				name = nameService.CreateName(this, component.GetType());
			}
			if (ContainsName(name))
				throw new ArgumentException("A component with this name already exists in the container.");
			site = new DesignSite(this, name);
			site.SetComponent(component);
			component.Site = site;
			if (ComponentAdding != null)
				ComponentAdding(this, new ComponentEventArgs(component));
			if (components.Count == 0)
			{
				designer = TypeDescriptor.CreateDesigner(component, typeof(IRootDesigner));
				RootDesigner = designer as IRootDesigner;
				rootComponent = component;
			}
			else
			{
				designer = TypeDescriptor.CreateDesigner(component, typeof(IDesigner));
			}

			if (designer != null)
			{
				designer.Initialize(component);
				designers[component] = designer;
			}
			else
			{
				component.Site = null;
				throw new InvalidOperationException("Failed to get designer for this component.");
			}

			if (component is IExtenderProvider)
			{
				IExtenderProviderService e = (IExtenderProviderService)GetService(typeof(IExtenderProviderService));
				e.AddExtenderProvider((IExtenderProvider)component);
			}

			components.Add(component);
			if (ComponentAdded != null)
				ComponentAdded(this, new ComponentEventArgs(component));
		}

		public void Add(IComponent component)
		{
			Add(component, null);
		}

		public void Remove(IComponent component)
		{
			ISite site = component.Site;
			IDesigner designer = null;

			if (component == null)
				return;

			if (component.Site == null || component.Site.Container != this)
				return;

			if (component is Form)
				return;

			if (ComponentRemoving != null)
				ComponentRemoving(this, new ComponentEventArgs(component));

			if (component is IExtenderProvider)
			{
				IExtenderProviderService e = (IExtenderProviderService)GetService(typeof(IExtenderProviderService));
				e.RemoveExtenderProvider((IExtenderProvider)component);
			}

			components.Remove(component);
			designer = (IDesigner)designers[component];
			if (designer != null)
			{
				designer.Dispose();
				designers.Remove(component);
			}

			if (ComponentRemoved != null)
				ComponentRemoved(this, new ComponentEventArgs(component));

			component.Site = null;
		}

		public ComponentCollection Components
		{
			get
			{
				var c = new IComponent[] { };
				if (components.Count == 0)
					return new ComponentCollection(c);
				c = new IComponent[components.Count];
				components.CopyTo(c, 0);
				return new ComponentCollection(c);
			}
		}

		public void Dispose()
		{
			foreach (var component in components)
			{
				component.Dispose();
			}

			components.Clear();
		}

		public void OnComponentChanged(object component, System.ComponentModel.MemberDescriptor member, object oldValue, object newValue)
		{
			if (ComponentChanged != null)
				ComponentChanged(this, new ComponentChangedEventArgs(component, member, oldValue, newValue));
		}

		public void OnComponentChanging(object component, System.ComponentModel.MemberDescriptor member)
		{
			if (ComponentChanging != null)
				ComponentChanging(this, new ComponentChangingEventArgs(component, member));
		}

		internal void OnComponentRename(object component, string oldName, string newName)
		{
			if (ComponentRename != null)
				ComponentRename(this, new ComponentRenameEventArgs(component, oldName, newName));
		}

		public event System.ComponentModel.Design.ComponentEventHandler ComponentAdded;
		public event System.ComponentModel.Design.ComponentEventHandler ComponentAdding;
		public event System.ComponentModel.Design.ComponentChangedEventHandler ComponentChanged;
		public event System.ComponentModel.Design.ComponentChangingEventHandler ComponentChanging;
		public event System.ComponentModel.Design.ComponentEventHandler ComponentRemoved;
		public event System.ComponentModel.Design.ComponentEventHandler ComponentRemoving;
		public event System.ComponentModel.Design.ComponentRenameEventHandler ComponentRename;

		public bool CanExtend(object extendee)
		{
			return (extendee is IComponent);
		}

		[DesignOnly(true), Category("Design"), Browsable(true), ParenthesizePropertyName(true), Description("The variable used to refer to this component in source code.")]
		public string GetName(IComponent component)
		{
			if (component.Site == null)
				throw new InvalidOperationException("Component is not sited.");

			return component.Site.Name;
		}

		public void SetName(IComponent component, string name)
		{
			if (component.Site == null)
				throw new InvalidOperationException("Component is not sited.");

			component.Site.Name = name;
		}

		public bool FilterAttributes(IComponent component, IDictionary attributes)
		{
			IDesigner designer = GetDesigner(component);
			if (designer is IDesignerFilter)
			{
				((IDesignerFilter)designer).PreFilterAttributes(attributes);
				((IDesignerFilter)designer).PostFilterAttributes(attributes);
			}

			return designer == null == false;
		}

		public bool FilterEvents(IComponent component, IDictionary events)
		{
			IDesigner designer = GetDesigner(component);
			if (designer is IDesignerFilter)
			{
				((IDesignerFilter)designer).PreFilterEvents(events);
				((IDesignerFilter)designer).PostFilterEvents(events);
			}

			return designer == null == false;
		}

		public bool FilterProperties(IComponent component, IDictionary properties)
		{
			IDesigner designer = GetDesigner(component);
			if (designer is IDesignerFilter)
			{
				((IDesignerFilter)designer).PreFilterProperties(properties);
				((IDesignerFilter)designer).PostFilterProperties(properties);
			}

			return designer == null == false;
		}

		public System.ComponentModel.IExtenderProvider[] GetExtenderProviders()
		{
			var e = new IExtenderProvider[extenderProviders.Count];
			extenderProviders.CopyTo(e, 0);

			return e;
		}

		public void AddExtenderProvider(System.ComponentModel.IExtenderProvider provider)
		{
			if (!extenderProviders.Contains(provider))
				extenderProviders.Add(provider);
		}

		public void RemoveExtenderProvider(System.ComponentModel.IExtenderProvider provider)
		{
			if (extenderProviders.Contains(provider))
				extenderProviders.Remove(provider);
		}

		public ICollection GetTypes(Type baseType, bool excludeGlobalTypes)
		{
			return AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.DefinedTypes).Where(m => m.BaseType == baseType).ToList();
		}
	}
}
