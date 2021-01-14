using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity.Options
{
    public static class OptionsFormatter
    {
        public static string NameFor<T>(string fieldName)
        {
            return OptionsParser.OptionNameFor<object>(typeof(T), fieldName, null);
        }

        public static string FormatWithValue<T>(string fieldName, object value)
        {
            var name = OptionsParser.OptionNameFor<object>(typeof(T), fieldName, out Type fieldType, null);
            if (fieldType == typeof(bool))
            {
                if (value is bool boolValue)
                {
                    if (boolValue)
                        return name;

                    return string.Empty;
                }

                throw new ArgumentException($"Invalid value.  Value must be a bool : {value}");
            }

            if (fieldType.IsArray)
            {
                if (value is Array objArray)
                {
                    if (objArray.Length == 0)
                        return string.Empty;
                    var sb = new StringBuilder();
                    sb.Append(name);
                    sb.Append("=");
                    for (int i = 0; i < objArray.Length - 1; i++)
                    {
                        sb.Append(objArray.GetValue(i));
                        sb.Append(",");
                    }

                    sb.Append(objArray.GetValue(objArray.Length - 1));
                    return sb.ToString();
                }
            }

            if (fieldType.IsEnum)
                return $"{name}={value.ToString().Replace(" ", "")}";

            return $"{name}={value}";
        }

        private static string AggregateWithComma(IEnumerable<object> elements)
        {
            if (elements.Any())
                return elements.Select(i => i.ToString()).Aggregate((buff, s) => buff + "," + s);

            return string.Empty;
        }
    }
}
