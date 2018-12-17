using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NDesk.Options;
using System.Globalization;

namespace Unity.Options
{
    public class ProgramOptionsAttribute : Attribute
    {
        public string Group { get; set; }
        public string CollectionSeparator { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HelpDetailsAttribute : Attribute
    {
        public string Summary { get; set; }

        public string CustomValueDescription { get; set; }

        public HelpDetailsAttribute(string summary, string customValueDescription = null)
        {
            Summary = summary;
            CustomValueDescription = customValueDescription;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OptionAliasAttribute : Attribute
    {
        public string Name { get; set; }

        public OptionAliasAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HideFromHelpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class OptionAttribute : Attribute
    {
    }

    public class HelpInformation
    {
        public string Summary;
        public FieldInfo FieldInfo;
        public string CustomValueDescription;

        public bool HasSummary
        {
            get { return !string.IsNullOrEmpty(Summary); }
        }

        public bool HasCustomValueDescription
        {
            get { return !string.IsNullOrEmpty(CustomValueDescription); }
        }
    }

    public sealed class OptionsParser
    {
        private static readonly Regex NameBuilder = new Regex(@"([A-Z][a-z_0-9]*)");

        public const int HelpOutputColumnPadding = 50;

        public static string[] Prepare(string[] commandLine, Type[] types, Func<Type, string, object> customValueParser = null)
        {
            var parser = new OptionsParser();
            foreach (var type in types)
                parser.AddType(type);
            return parser.Parse(commandLine, customValueParser);
        }

        public static string[] PrepareFromFile(string argFile, Type[] types, Func<Type, string, object> customValueParser = null)
        {
            if (!File.Exists(argFile))
                throw new FileNotFoundException(argFile);

            return Prepare(System.IO.File.ReadAllLines(argFile), types, customValueParser);
        }

        public static string[] PrepareFromFile(string argFile, Assembly assembly, bool includeReferencedAssemblies = true, Func<Type, string, object> customValueParser = null)
        {
            return Prepare(OptionsHelper.LoadArgumentsFromFile(argFile).ToArray(), assembly, includeReferencedAssemblies, customValueParser);
        }

        public static string[] Prepare(string[] commandLine, Assembly assembly, bool includeReferencedAssemblies = true, Func<Type, string, object> customValueParser = null)
        {
            return Prepare(commandLine, LoadOptionTypesFromAssembly(assembly, includeReferencedAssemblies), customValueParser);
        }

        public static Dictionary<string, HelpInformation> ParseHelpTable(Type type, bool includeReferencedAssemblies = true)
        {
            return ParseHelpTable(new[] { type });
        }

        public static Dictionary<string, HelpInformation> ParseHelpTable(Type[] types)
        {
            Dictionary<string, HelpInformation> helpTable = new Dictionary<string, HelpInformation>();

            foreach (var optionsTypes in types)
            {
                foreach (var fieldInfo in GetOptionFields(optionsTypes))
                {
                    var helpAttrs = fieldInfo.GetCustomAttributes(typeof(HelpDetailsAttribute), false).ToArray();

                    if (helpAttrs.Length > 1)
                        throw new InvalidOperationException(string.Format("Field, {0}, has more than one help attribute", fieldInfo.Name));

                    var argName = string.Format("--{0}", NormalizeName(fieldInfo.Name));

                    if (helpTable.ContainsKey(argName))
                        throw new InvalidOperationException(string.Format("There are multiple options defined with the name : {0}", argName));

                    if (!fieldInfo.GetCustomAttributes(typeof(HideFromHelpAttribute), false).Any())
                    {
                        if (helpAttrs.Length == 0)
                            helpTable.Add(argName, new HelpInformation {Summary = null, FieldInfo = fieldInfo });
                        else
                        {
                            var helpAttr = ((HelpDetailsAttribute)helpAttrs[0]);
                            helpTable.Add(argName, new HelpInformation
                            {
                                Summary = helpAttr.Summary,
                                FieldInfo = fieldInfo,
                                CustomValueDescription = helpAttr.CustomValueDescription
                            });
                        }
                    }
                }
            }

            return helpTable;
        }

        public static Dictionary<string, HelpInformation> ParseHelpTable(Assembly assembly, bool includeReferencedAssemblies = true)
        {
            return ParseHelpTable(LoadOptionTypesFromAssembly(assembly, includeReferencedAssemblies));
        }

        public static void DisplayHelp(TextWriter writer, Type type)
        {
            DisplayHelp(writer, new[] { type });
        }

        public static void DisplayHelp(TextWriter writer, Type[] types)
        {
            writer.WriteLine();
            writer.WriteLine("Options:");

            var helpTable = ParseHelpTable(types);

            foreach (var entry in helpTable)
            {
                if (!entry.Value.HasSummary)
                    continue;

                string left;

                if (entry.Value.FieldInfo.FieldType == typeof(bool))
                    left = string.Format("  {0}", entry.Key);
                else if (entry.Value.FieldInfo.FieldType.IsArray || IsListField(entry.Value.FieldInfo))
                    left = string.Format("  {0}=<{1},{1},..>", entry.Key, entry.Value.HasCustomValueDescription ? entry.Value.CustomValueDescription : "value");
                else
                    left = string.Format("  {0}=<{1}>", entry.Key, entry.Value.HasCustomValueDescription ? entry.Value.CustomValueDescription : "value");

                if (left.Length > HelpOutputColumnPadding)
                    throw new InvalidOperationException(string.Format("Option to long for current padding : {0}, shorten name/value or increase padding if necessary. Over by {1}", entry.Key, left.Length - HelpOutputColumnPadding));

                left = left.PadRight(HelpOutputColumnPadding);
                writer.WriteLine("{0}{1}", left, entry.Value.Summary);
            }
        }

        public static void DisplayHelp(TextWriter writer, Assembly assembly, bool includeReferencedAssemblies = true)
        {
            DisplayHelp(writer, LoadOptionTypesFromAssembly(assembly, includeReferencedAssemblies));
        }

        public static void DisplayHelp(Assembly assembly, bool includeReferencedAssemblies = true)
        {
            DisplayHelp(Console.Out, LoadOptionTypesFromAssembly(assembly, includeReferencedAssemblies));
        }

        public static void DisplayHelp(Type[] optionTypes)
        {
            DisplayHelp(Console.Out, optionTypes);
        }

        public static bool HelpRequested(string[] commandLine)
        {
            return commandLine.Count(v => v == "--h" || v == "--help" || v == "-help") > 0;
        }

        public static string[] RecreateArgumentsFromCurrentState(Type optionType)
        {
            var args = new List<string>();
            var optionFields = GetOptionFields(optionType);
            foreach (var field in optionFields)
            {
                var value = field.GetValue(null);

                if (value == null)
                    continue;

                var name = OptionNameFor(optionType, field.Name);

                if (field.FieldType == typeof(bool))
                {
                    if (value is bool vBool && vBool)
                        args.Add($"{name}");
                }
                else if (field.FieldType.IsArray || IsListField(field))
                {
                    foreach (var item in (IEnumerable)value)
                        args.Add($"{name}={item}");
                }
                else
                {
                    args.Add($"{name}={value}");
                }
            }

            return args.ToArray();
        }

        private static IEnumerable<FieldInfo> GetOptionFields(Type optionType)
        {
            var defaultOptions = optionType.GetFields(BindingFlags.Static | BindingFlags.Public);

            var explicitOptions = optionType.GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(field => field.GetCustomAttributes(typeof(OptionAttribute), false).Any());

            return defaultOptions.Concat(explicitOptions);
        }

        public static Type[] LoadOptionTypesFromAssembly(Assembly assembly, bool includeReferencedAssemblies,
            Func<AssemblyName, bool> shouldProcessReference = null,
            Func<Assembly, Type[]> customTypeCollector = null)
        {
            var types = new List<Type>();
            var queue = new Stack<Assembly>();
            var processed = new HashSet<AssemblyName>(new AssemblyNameComparer());

            queue.Push(assembly);

            while (queue.Count > 0)
            {
                var current = queue.Pop();
                if (!processed.Add(current.GetName()))
                    continue;

                Type[] typesFromAssembly;
                if (customTypeCollector == null)
                    typesFromAssembly = current.GetTypes();
                else
                    typesFromAssembly = customTypeCollector(current);

                types.AddRange(typesFromAssembly.Where(HasProgramOptionsAttribute));

                if (includeReferencedAssemblies)
                {
                    foreach (var referencedAssembly in current.GetReferencedAssemblies())
                    {
                        if (referencedAssembly.Name == "mscorlib" ||
                            referencedAssembly.Name.StartsWith("System") ||
                            referencedAssembly.Name.StartsWith("Mono.Cecil") ||
                            referencedAssembly.Name.StartsWith("Microsoft"))
                            continue;

                        if (shouldProcessReference != null && !shouldProcessReference(referencedAssembly))
                            continue;

                        if (!processed.Contains(referencedAssembly))
                        {
                            Assembly loadedAssembly;
                            try
                            {
                                loadedAssembly = Assembly.Load(referencedAssembly);
                                queue.Push(loadedAssembly);
                            }
                            catch (BadImageFormatException) // .NET refuses to load winmd references, so just skip them
                            {
                            }
                            catch (FileLoadException)
                            {
                            }
                        }
                    }
                }
            }

            return types.ToArray();
        }

        private readonly List<Type> _types = new List<Type>();

        internal OptionsParser()
        {
        }

        internal void AddType(Type type)
        {
            _types.Add(type);
        }

        internal static bool HasProgramOptionsAttribute(Type type)
        {
#if NETCORE
            return type.GetTypeInfo().GetCustomAttributes(typeof(ProgramOptionsAttribute), false).Any();
#else
            return type.GetCustomAttributes(typeof(ProgramOptionsAttribute), false).Any();
#endif
        }

        internal string[] Parse(IEnumerable<string> commandLine, Func<Type, string, object> customValueParser)
        {
            var optionSet = PrepareOptionSet(customValueParser);
            return optionSet.Parse(commandLine).ToArray();
        }

        private OptionSet PrepareOptionSet(Func<Type, string, object> customValueParser)
        {
            var optionSet = new OptionSet();

            foreach (var type in _types)
                ExtendOptionSet(optionSet, type, customValueParser);

            return optionSet;
        }

        private void ExtendOptionSet(OptionSet optionSet, Type type, Func<Type, string, object> customValueParser)
        {
            var fields = GetOptionFields(type);

            foreach (var field in fields)
            {
#if NETCORE
                var options = (ProgramOptionsAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(ProgramOptionsAttribute), false).First();
#else
                var options = (ProgramOptionsAttribute)type.GetCustomAttributes(typeof(ProgramOptionsAttribute), false).First();
#endif
                foreach (var name in OptionNamesFor(options, field))
                {
                    optionSet.Add(
                        name,
                        DescriptionFor(field),
                        ActionFor(options, field, customValueParser));
                }
            }
        }

        private static IEnumerable<string> OptionNamesFor(ProgramOptionsAttribute options, FieldInfo field)
        {
            var name = NormalizeName(field.Name);

            if (field.FieldType != typeof(bool))
                name += "=";

            if (options.Group == null)
            {
                yield return name;
                yield return NormalizeName(field.DeclaringType.Name) + "." + name;
            }
            else
                yield return options.Group + "." + name;

            foreach (var aliasAttr in field.GetCustomAttributes(typeof(OptionAliasAttribute), false))
            {
                if (field.FieldType != typeof(bool))
                    yield return $"{((OptionAliasAttribute)aliasAttr).Name}=";
                else
                    yield return ((OptionAliasAttribute)aliasAttr).Name;
            }
        }

        public static string OptionNameFor(Type type, string fieldName)
        {
            var field = GetOptionFields(type).FirstOrDefault(f => f.Name == fieldName);
            if (field == null)
                throw new ArgumentException($"No field on type {type} named {fieldName}");

#if NETCORE
            var options = (ProgramOptionsAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(ProgramOptionsAttribute), false).First();
#else
            var options = (ProgramOptionsAttribute)type.GetCustomAttributes(typeof(ProgramOptionsAttribute), false).First();
#endif
            return $"--{OptionNamesFor(options, field).First()}".TrimEnd('=');
        }

        private static string NormalizeName(string name)
        {
            return NameBuilder.Matches(name)
                .Cast<Match>()
#if NETCORE
                .Select(m => m.Value.ToLowerInvariant())
#else
                .Select(m => m.Value.ToLower(CultureInfo.InvariantCulture))
#endif
                .Aggregate((buff, s) => buff + "-" + s);
        }

        private static string DescriptionFor(FieldInfo field)
        {
            return "";
        }

        private static Action<string> ActionFor(ProgramOptionsAttribute options, FieldInfo field, Func<Type, string, object> customValueParser)
        {
            if (field.FieldType.IsArray && IsFlagsEnum(field.FieldType.GetElementType()))
                throw new NotSupportedException("The parsing of a flags array is not supported.  There would be no way to know which values to combine together into each element in the array");

            if (field.FieldType.IsArray)
                return v => SetArrayType(field, v, options, customValueParser);

            if (IsListField(field))
            {
                return v => SetListType(field, v, options, customValueParser);
            }

            if (field.FieldType == typeof(bool))
                return v => SetBoolType(field, v);

            if (IsFlagsEnum(field.FieldType))
                return v => SetFlagsEnumType(field, v, options);

            return v => SetBasicType(field, v, customValueParser);
        }

        private static bool IsListField(FieldInfo field)
        {
#if NETCORE
            if (field.FieldType.GetTypeInfo().IsGenericType)
#else
            if (field.FieldType.IsGenericType)
#endif
            {
                var genericType = field.FieldType.GetGenericTypeDefinition();
                if (genericType.IsAssignableFrom(typeof(List<>)))
                    return true;
            }

            return false;
        }

        private static void SetListType(FieldInfo field, string value, ProgramOptionsAttribute options, Func<Type, string, object> customValueParser)
        {
            var listType = field.FieldType;
            var list = (IList)field.GetValue(null) ?? (IList)Activator.CreateInstance(listType);

            foreach (var v in SplitCollectionValues(options, value))
                list.Add(ParseValue(listType.GetGenericArguments()[0], v, customValueParser));

            field.SetValue(null, list);
        }

        private static void SetArrayType(FieldInfo field, string value, ProgramOptionsAttribute options, Func<Type, string, object> customValueParser)
        {
            var index = 0;
            var values = SplitCollectionValues(options, value);
            var arrayType = field.FieldType;
            var array = (Array)field.GetValue(null);

            if (array != null)
            {
                var oldArray = array;
                array = (Array)Activator.CreateInstance(arrayType, new object[] {oldArray.Length + values.Length});
                Array.Copy(oldArray, array, oldArray.Length);
                index = oldArray.Length;
            }
            else
                array = (Array)Activator.CreateInstance(arrayType, new object[] {values.Length});

            foreach (var v in values)
                array.SetValue(ParseValue(arrayType.GetElementType(), v, customValueParser), index++);

            field.SetValue(null, array);
        }

        private static void SetBoolType(FieldInfo field, string v)
        {
            field.SetValue(null, true);
        }

        private static void SetBasicType(FieldInfo field, string v, Func<Type, string, object> customValueParser)
        {
            field.SetValue(null, ParseValue(field.FieldType, v, customValueParser));
        }

        private static void SetFlagsEnumType(FieldInfo field, string value, ProgramOptionsAttribute options)
        {
            int finalValue =
                SplitCollectionValues(options, value)
                    .Select(v => (int)ParseEnumValue(field.FieldType, v))
                    .Aggregate(0, (accum, current) => accum | current);
            field.SetValue(null, finalValue);
        }

        private static string[] SplitCollectionValues(ProgramOptionsAttribute options, string value)
        {
            return value.Split(new[] { options.CollectionSeparator ?? "," }, StringSplitOptions.None);
        }

        private static bool IsEnum(Type type)
        {
#if NETCORE
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        private static bool IsFlagsEnum(Type type)
        {
            if (!IsEnum(type))
                return false;

#if NETCORE
            var attributeTypes = type.GetTypeInfo().GetCustomAttributes(false);
#else
            var attributeTypes = type.GetCustomAttributes(false);
#endif
            return attributeTypes.Any(attr => attr is FlagsAttribute);
        }

        private static object ParseEnumValue(Type type, string value)
        {
            return Enum.GetValues(type).Cast<object>().First(v => String.Equals(Enum.GetName(type, v), value, StringComparison.OrdinalIgnoreCase));
        }

        private static object ParseValue(Type type, string value, Func<Type, string, object> customValueParser)
        {
            if (customValueParser != null)
            {
                var customParsed = customValueParser(type, value);
                if (customParsed != null)
                    return customParsed;
            }

            if (IsEnum(type))
                return ParseEnumValue(type, value);

            var converted = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

            if (converted == null)
                throw new NotSupportedException("Unsupported type " + type.FullName);

            return converted;
        }
    }

    class AssemblyNameComparer : IEqualityComparer<AssemblyName>
    {
        public bool Equals(AssemblyName x, AssemblyName y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(AssemblyName obj)
        {
            return obj.FullName.GetHashCode();
        }
    }

    public sealed class OptionsHelper
    {
        public const string DefaultArgumentFileName = "args.txt";

        public static string WriteArgumentsFile(string path, IEnumerable<string> args)
        {
#if NETCORE
            using (var writer = new StreamWriter(System.IO.File.OpenWrite(path)))
#else
            using (var writer = new StreamWriter(path))
#endif
            {
                foreach (var arg in args)
                {
                    writer.WriteLine(arg);
                }
            }

            return path;
        }

        public static string WriteArgumentFileInDirectory(string directory, IEnumerable<string> args)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(directory);

            return WriteArgumentsFile(System.IO.Path.Combine(directory, DefaultArgumentFileName), args);
        }

        public static IEnumerable<string> LoadArgumentsFromFile(string argFile)
        {
            if (!File.Exists(argFile))
                throw new FileNotFoundException(argFile);

            return System.IO.File.ReadAllLines(argFile);
        }
    }
}
