using System.ComponentModel;
using System.Reflection;

namespace Utils
{
	/// <summary>
	/// Class to store useful functions for enums
	/// </summary>
	public static class EnumUtils
	{
		/// <summary>
		/// Get configuration name of enum value
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <param name="value"></param>
		/// <returns>
		/// Configuration name of enum value - if exists
		/// string.Empty - if configuration name is missing or empty
		/// </returns>
		public static string GetConfigurationName<E>(this E value) where E : Enum => value.GetAttribute<E, ConfigurationNameAttribute>()?.ConfigurationName ?? string.Empty;

		/// <summary>
		/// Get menu display name of enum value
		/// </summary>
		/// <typeparam name="E">Type of enum</typeparam>
		/// <param name="value">Enum value</param>
		/// <returns>
		/// Menu display name of enum value - if exists
		/// string.Empty - if menu display name is missing or empty
		/// </returns>
		public static string GetMenuDisplayName<E>(this E value) where E : Enum => value.GetAttribute<E, MenuDisplayNameAttribute>()?.MenuDisplayName ?? string.Empty;

		/// <summary>
		/// Get description of enum value
		/// </summary>
		/// <typeparam name="E">Type of enum</typeparam>
		/// <param name="value">Enum value</param>
		/// <returns>
		/// Description of enum value - if exists
		/// string.Empty - if description is missing or empty</returns>
		public static string GetDescription<E>(this E value) where E : Enum => value.GetAttribute<E, DescriptionAttribute>()?.Description ?? string.Empty;

		/// <summary>
		/// Get specified attribute of enum value
		/// </summary>
		/// <typeparam name="E">Enum type</typeparam>
		/// <typeparam name="A">Attribute type</typeparam>
		/// <param name="enumValue">Enum value</param>
		/// <returns>
		/// Attribute - if exists
		/// null - if attribute is missing
		/// </returns>
		/// <exception cref="ArgumentException"></exception>
		private static A? GetAttribute<E, A>(this E enumValue)
			where E : Enum
			where A : Attribute
		{
			var fieldInfo = typeof(E).GetField(enumValue.ToString());

			// Value does not exists in current enum
			if (fieldInfo == null)
				throw new ArgumentException($"Enum value: '{enumValue}' does not exists in current enum type: '{typeof(E)}'");

			return fieldInfo.GetCustomAttribute<A>();
		}

		/// <summary>
		/// Get enum value based on menu display name
		/// </summary>
		/// <typeparam name="E">Enum type</typeparam>
		/// <param name="menuDisplayName">Menu display name to find</param>
		/// <returns>
		/// Enum value - if provided menu display name exists
		/// null - otherwise
		/// </returns>
		public static E? GetValueFromMenuDisplayName<E>(string menuDisplayName)
			where E : struct, Enum
		{
			foreach (var fieldInfo in typeof(E).GetFields())
				if (Attribute.GetCustomAttribute(fieldInfo, typeof(MenuDisplayNameAttribute)) is MenuDisplayNameAttribute attribute)
					if (attribute.MenuDisplayName.Equals(menuDisplayName))
						return (E?)fieldInfo.GetValue(null);

			return null;
		}
	}

	/// <summary>
	/// Attribute to represent the name of configuration file
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ConfigurationNameAttribute : Attribute
	{
		public string ConfigurationName { get; protected set; }

		public ConfigurationNameAttribute(string configurationName)
		{
			ConfigurationName = configurationName;
		}
	}

	/// <summary>
	/// Attribute to represent the name of menu item that is displayed in console
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class MenuDisplayNameAttribute : Attribute
	{
		public string MenuDisplayName { get; protected set; }

		public MenuDisplayNameAttribute(string menuDisplayName)
		{
			MenuDisplayName = menuDisplayName;
		}
	}
}