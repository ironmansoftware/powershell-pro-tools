using System;
using System.ComponentModel.Design.Serialization;

namespace IM.WinForms
{
	internal class NameCreationService : INameCreationService
	{
		public string CreateName(System.ComponentModel.IContainer container, System.Type dataType)
		{
			int i = 0;
			string name = dataType.Name;
			while (true)
			{
				i++;

				if (container == null) break;
				if (container.Components[name + i.ToString()] == null)
					break;
			}

			return name + i.ToString();
		}

		public void ValidateName(string name)
		{
			if (!IsValidName(name))
				throw new ArgumentException("Invalid name: " + name);
		}

		public bool IsValidName(string name)
		{
			if (name == null || name.Length == 0)
				return false;
			if (!Char.IsLetter(name, 0))
				return false;
			if (name.StartsWith("_"))
				return false;
			for (int i = 0; i < name.Length; i++)
			{
				if (!Char.IsLetterOrDigit(name, i))
					return false;
			}

			return true;
		}

	}
}
