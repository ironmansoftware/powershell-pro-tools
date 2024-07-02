using System.ComponentModel.Design;
using System.Collections;

namespace IM.WinForms
{
	internal class MenuCommandService : IMenuCommandService
	{
		ArrayList menuCommands = null;

		public MenuCommandService()
		{
			menuCommands = new ArrayList();
		}

		public void AddCommand(MenuCommand command)
		{
			menuCommands.Add(command);
		}

		public void AddVerb(DesignerVerb verb)
		{
		}

		public MenuCommand FindCommand(CommandID commandID)
		{
			return null;
		}

		public bool GlobalInvoke(CommandID commandID)
		{
			foreach (MenuCommand command in menuCommands)
			{
				if (command.CommandID == commandID)
				{
					command.Invoke();
					break;
				}
			}

			return false;
		}

		public void RemoveCommand(MenuCommand command)
		{
			menuCommands.Remove(command);
		}

		public void RemoveVerb(DesignerVerb verb)
		{
		}

		public void ShowContextMenu(CommandID menuID, int x, int y)
		{
		}

		public DesignerVerbCollection Verbs
		{
			get
			{
				return new DesignerVerbCollection();
			}
		}
	}
}
