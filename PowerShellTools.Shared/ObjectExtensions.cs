using System.Reflection;

namespace PowerShellToolsPro
{
	public static class ObjectExtensions
	{
		public static T GetPropertyValue<T>(this object obj, string propertyName) where T : class
		{
			return obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
				.GetValue(obj) as T;
		}

		public static T GetPrivatePropertyValue<T>(this object obj, string propertyName) where T : class
		{
			return obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(obj) as T;
		}
	}
}
