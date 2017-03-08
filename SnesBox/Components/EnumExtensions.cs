using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnesBox.Components
{
    public static class EnumExtensions
    {
        public static T[] GetEnumValues<T>()
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException("Type '" + type.Name + "' is not an enum");

            return (
              from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
              where field.IsLiteral
              select (T)field.GetValue(null)
            ).ToArray();
        }
    }
}
