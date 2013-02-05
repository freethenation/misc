using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JollyBit.Utilities
{
	public abstract class Enums<Temp> where Temp : class
	{
		public static TEnum GetAValue<TEnum>() where TEnum : struct, Temp
		{
			var values = GetValues<TEnum>();
			if (values.Length > 0) return values[0];
			return (TEnum)(object)0;
		}
		public static TEnum[] GetValues<TEnum>() where TEnum : struct, Temp
		{
			return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
		}
		public static TEnum Parse<TEnum>(string name) where TEnum : struct, Temp
		{
			return (TEnum)Enum.Parse(typeof(TEnum), name);
		}
		public static bool TryParse<TEnum>(string name, out TEnum enumValue) where TEnum : struct, Temp
		{
			try
			{
				enumValue = Parse<TEnum>(name);
				return true;
			}
			catch (Exception)
			{
				enumValue = GetAValue<TEnum>();
				return false;
			}
		}
	}
	public abstract class Enums : Enums<Enum> { }
}
