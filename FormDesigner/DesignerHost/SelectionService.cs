using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace IM.WinForms
{
	public class SelectionService : ISelectionService
	{
		IDesignerHost host = null;
		ArrayList selectedComponents = null;

		public event EventHandler SelectionChanging;
		public event EventHandler SelectionChanged;

		public SelectionService(IDesignerHost host)
		{
			this.host = host;
			selectedComponents = new ArrayList();
			IComponentChangeService c = (IComponentChangeService)host.GetService(typeof(IComponentChangeService));
			c.ComponentRemoving += new ComponentEventHandler(OnComponentRemoved);
		}

		public ICollection GetSelectedComponents()
		{
			return selectedComponents.ToArray();
		}

		internal void OnSelectionChanging(EventArgs e)
		{
			if (SelectionChanging != null)
				SelectionChanging(this, e);
		}

		internal void OnSelectionChanged(EventArgs e)
		{
			if (SelectionChanged != null)
				SelectionChanged(this, e);
		}

		public object PrimarySelection
		{
			get
			{
				if (selectedComponents.Count > 0)
					return selectedComponents[0];

				return null;
			}
		}

		public int SelectionCount
		{
			get
			{
				return selectedComponents.Count;
			}
		}

		public bool GetComponentSelected(object component)
		{
			return selectedComponents.Contains(component);
		}

		public void SetSelectedComponents(ICollection components, SelectionTypes selectionType)
		{
			bool control = false;
			bool shift = false;
			if (SelectionChanging != null)
				SelectionChanging(this, EventArgs.Empty);
			if (components == null || components.Count == 0)
				components = new object[1] { host.RootComponent };
			if ((selectionType & SelectionTypes.Click) == SelectionTypes.Click)
			{
				control = ((Control.ModifierKeys & Keys.Control) == Keys.Control);
				shift = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
			}

			if (selectionType == SelectionTypes.Replace)
			{
				selectedComponents.Clear();
				foreach (object component in components)
				{
					if (component != null && !selectedComponents.Contains(component))
						selectedComponents.Add(component);
				}
			}
			else
			{
				if (!control && !shift && components.Count == 1)
				{
					foreach (object component in components)
					{
						if (!selectedComponents.Contains(component))
							selectedComponents.Clear();
					}
				}
				foreach (object component in components)
				{
					if (component != null)
					{
						if (control || shift)
						{
							if (selectedComponents.Contains(component))
								selectedComponents.Remove(component);
							else
								selectedComponents.Insert(0, component);
						}
						else
						{
							if (!selectedComponents.Contains(component))
								selectedComponents.Add(component);
							else
							{
								selectedComponents.Remove(component);
								selectedComponents.Insert(0, component);
							}
						}
					}
				}
			}
			if (SelectionChanged != null)
				SelectionChanged(this, EventArgs.Empty);
		}

		public void SetSelectedComponents(ICollection components)
		{
			SetSelectedComponents(components, SelectionTypes.Replace);
		}

		internal void OnComponentRemoved(object sender, ComponentEventArgs e)
		{
			if (selectedComponents.Contains(e.Component))
			{
				OnSelectionChanging(EventArgs.Empty);
				selectedComponents.Remove(e.Component);
				if (SelectionCount == 0)
					selectedComponents.Add(host.RootComponent);
				OnSelectionChanged(EventArgs.Empty);
			}
		}

	}
}
