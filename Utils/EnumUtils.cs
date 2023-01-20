using System.ComponentModel;
using System.Reflection;

namespace Utils
{
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
		/// null - if value does not exists in current enum
		/// </returns>
		public static string? GetConfigurationName<E>(this E value) where E : Enum => value.GetAttribute<E, ConfigurationNameAttribute>()?.ConfigurationName ?? string.Empty;

		/// <summary>
		/// Get menu display name of enum value
		/// </summary>
		/// <typeparam name="E">Type of enum</typeparam>
		/// <param name="value">Enum value</param>
		/// <returns>
		/// Menu display name of enum value - if exists
		/// string.Empty - if menu display name is missing or empty
		/// null - if value does not exists in current enum
		/// </returns>
		public static string? GetMenuDisplayName<E>(this E value) where E : Enum => value.GetAttribute<E, MenuDisplayNameAttribute>()?.MenuDisplayName ?? string.Empty;

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
		public static A? GetAttribute<E, A>(this E enumValue)
			where E : Enum
			where A : Attribute
		{
			var fieldInfo = typeof(E).GetField(enumValue.ToString());

			// Value does not exists in current enum
			if (fieldInfo == null)
				throw new ArgumentException($"Enum value: '{enumValue}' does not exists in current enum type: '{typeof(E)}'");

			var customAttribute = fieldInfo.GetCustomAttribute<A>();

			return customAttribute;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ConfigurationNameAttribute : Attribute
	{
		public string ConfigurationName { get; protected set; }

		public ConfigurationNameAttribute(string configurationName)
		{
			ConfigurationName = configurationName;
		}
	}

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