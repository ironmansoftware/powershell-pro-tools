
using System.Windows.Forms;
using System.Drawing.Design;
using System.Collections.Generic;
using System.Linq;

namespace IM.WinForms
{
	public class ToolboxService : TreeView, IToolboxService
	{
		internal Control designPanel = null;
        private List<ToolboxItem> toolboxItems;

		public ToolboxService()
		{
            toolboxItems = new List<ToolboxItem>();
		}

		public void AddCreator(ToolboxItemCreatorCallback creator, string format, System.ComponentModel.Design.IDesignerHost host)
		{
			// No implementation
		}

		public void AddCreator(ToolboxItemCreatorCallback creator, string format)
		{
			// No implementation
		}

		public void AddLinkedToolboxItem(ToolboxItem toolboxItem, string category, System.ComponentModel.Design.IDesignerHost host)
		{
			// No implementation
		}

		public void AddLinkedToolboxItem(ToolboxItem toolboxItem, System.ComponentModel.Design.IDesignerHost host)
		{
			// No implementation
		}

		public void AddToolboxItem(ToolboxItem toolboxItem, string category)
		{
            toolboxItems.Add(toolboxItem);

			if (!Nodes.ContainsKey(category))
            {
                var node = Nodes.Add(category, category);
                node.Expand();
            }
            
            Nodes[category].Nodes.Add(toolboxItem.DisplayName);
        }

		public void AddToolboxItem(ToolboxItem toolboxItem)
		{
            AddToolboxItem(toolboxItem, "All Windows Controls");
        }

		public ToolboxItem DeserializeToolboxItem(object serializedObject, System.ComponentModel.Design.IDesignerHost host)
		{
			return null;
		}

		public ToolboxItem DeserializeToolboxItem(object serializedObject)
		{
			return null;
		}

		public ToolboxItem GetSelectedToolboxItem(System.ComponentModel.Design.IDesignerHost host)
		{
			return GetSelectedToolboxItem();
		}

		public ToolboxItem GetSelectedToolboxItem()
		{
            if (SelectedNode == null) return null;
            return toolboxItems.FirstOrDefault(m => m.DisplayName == SelectedNode.Text);
		}

		public ToolboxItemCollection GetToolboxItems(string category, System.ComponentModel.Design.IDesignerHost host)
		{
			return GetToolboxItems();
		}

		public ToolboxItemCollection GetToolboxItems(string category)
		{
			return GetToolboxItems();
		}

		public ToolboxItemCollection GetToolboxItems(System.ComponentModel.Design.IDesignerHost host)
		{
			return GetToolboxItems();
		}

		public ToolboxItemCollection GetToolboxItems()
		{
			return new ToolboxItemCollection(toolboxItems.ToArray());
		}

		public bool IsSupported(object serializedObject, System.Collections.ICollection filterAttributes)
		{
			return false;
		}

		public bool IsSupported(object serializedObject, System.ComponentModel.Design.IDesignerHost host)
		{
			return false;
		}

		public bool IsToolboxItem(object serializedObject, System.ComponentModel.Design.IDesignerHost host)
		{
			return false;
		}

		public bool IsToolboxItem(object serializedObject)
		{
			return false;
		}

		public void RemoveCreator(string format, System.ComponentModel.Design.IDesignerHost host)
		{
			// No implementation
		}

		public void RemoveCreator(string format)
		{
			// No implementation
		}

		public void RemoveToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem, string category)
		{
			RemoveToolboxItem(toolboxItem);
		}

		public void RemoveToolboxItem(ToolboxItem toolboxItem)
		{
		}

		public void SelectedToolboxItemUsed()
		{
            SelectedNode = null;
		}

		public object SerializeToolboxItem(ToolboxItem toolboxItem)
		{
			return null;
		}

		public bool SetCursor()
		{
			return false;
		}

		public void SetSelectedToolboxItem(ToolboxItem toolboxItem)
		{
		}

		public CategoryNameCollection CategoryNames
		{
			get
			{
				return null;
			}
		}

		public string SelectedCategory
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		private bool ShouldSerializeItems()
		{
			return false;
		}
	}
}
