using System.ComponentModel;
using System.Reflection;

namespace Utils
{
	public static class EnumUtils
	{
		/// <summary>
		/// Get description of enum value
		/// </summary>
		/// <typeparam name="E">Type of enum</typeparam>
		/// <param name="value">Enum value</param>
		/// <returns>
		/// Description of enum value - if exists
		/// string.Empty - if description is missing or empty
		/// null - if value does not exists in current enum
		/// </returns>
		public static string? GetDescription<E>(this E value) where E : Enum
		{
			var fieldInfo = typeof(E).GetField(value.ToString());

			// Value does not exists in current enum
			if (fieldInfo == null)
				return null;

			var description = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();

			// Description is missing
			if (description == null)
				return string.Empty;

			return description.Description;
		}
	}
}