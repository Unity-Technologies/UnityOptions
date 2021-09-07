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

        public static string[] PrepareInstances(string[] commandLine, object[] optionInstances, Func<Type, string, object> customValueParser = null,
            Func<FieldInfo, string, string[]> customCollectionSplitter = null, string currentDirectory = null)
        {
            return Prepare<OptionAliasAttribute>(commandLine, optionInstances.Select(o => o.GetType()).ToArray(), customValueParser, currentDirectory, optionInstances, attr => attr.Name, customCollectionSplitter);
        }
        
        public static string[] PrepareInstances<TAliasAttribute>(string[] commandLine, object[] optionInstances, Func<TAliasAttribute, string> getAliasName, Func<Type, string, object> customValueParser = null,
            Func<FieldInfo, string, string[]> customCollectionSplitter = null, string currentDirectory = null)
        {
            return Prepare<TAliasAttribute>(commandLine, optionInstances.Select(o => o.GetType()).ToArray(), customValueParser, currentDirectory, optionInstances, getAliasName, customCollectionSplitter);
        }

        public static string[] Prepare(string[] commandLine, Type[] types, Func<Type, string, object> customValueParser = null,
            Func<FieldInfo, string, string[]> customCollectionSplitter = null, string currentDirectory = null)
        {
            return Prepare<OptionAliasAttribute>(commandLine, types, customValueParser, currentDirectory, null, attr => attr.Name, customCollectionSplitter);
        }

        static string[] Prepare<TAliasAttribute>(string[] commandLine, Type[] types, Func<Type, string, object> customValueParser, string currentDirectory, object[] instances,
            Func<TAliasAttribute, string> getAliasName, Func<FieldInfo, string, string[]> customCollectionSplitter)
        {
            var parser = new OptionsParser();
            foreach (var type in types)
                parser.AddType(type);
            return parser.Parse(commandLine, customValueParser, getAliasName, customCollectionSplitter, currentDirectory, instances);
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
            return ParseHelpTable<HelpDetailsAttribute, HideFromHelpAttribute>(types, attr => attr.Summary, attr => attr.CustomValueDescription);
        }

        public static Dictionary<string, HelpInformation> ParseHelpTable<THelpDetails, THideFromHelp>(Type[] types,
            Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            return ParseHelpTable<THelpDetails, THideFromHelp>(types, OptionFieldSelection.Any, getSummary, getCustomValueDescription);
        }
        
        public static Dictionary<string, HelpInformation> ParseHelpTable<THelpDetails, THideFromHelp>(object[] optionInstances,
            Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            return ParseHelpTable<THelpDetails, THideFromHelp>(optionInstances.Select(o => o.GetType()).ToArray(), OptionFieldSelection.Instance, getSummary, getCustomValueDescription);
        }

        static Dictionary<string, HelpInformation> ParseHelpTable<THelpDetails, THideFromHelp>(Type[] types,
            OptionFieldSelection optionFieldSelection,
            Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            Dictionary<string, HelpInformation> helpTable = new Dictionary<string, HelpInformation>();

            foreach (var optionsTypes in types)
            {
                foreach (var fieldInfo in GetOptionFields(optionsTypes, optionFieldSelection))
                {
                    var helpAttrs = fieldInfo.GetCustomAttributes(typeof(THelpDetails), false).ToArray();

                    if (helpAttrs.Length > 1)
                        throw new InvalidOperationException(string.Format("Field, {0}, has more than one help attribute", fieldInfo.Name));

                    var argName = string.Format("--{0}", NormalizeName(fieldInfo.Name));

                    if (helpTable.ContainsKey(argName))
                        throw new InvalidOperationException(string.Format("There are multiple options defined with the name : {0}", argName));

                    if (!fieldInfo.GetCustomAttributes(typeof(THideFromHelp), false).Any())
                    {
                        if (helpAttrs.Length == 0)
                            helpTable.Add(argName, new HelpInformation {Summary = null, FieldInfo = fieldInfo });
                        else
                        {
                            var helpAttr = ((THelpDetails)helpAttrs[0]);
                            helpTable.Add(argName, new HelpInformation
                            {
                                Summary = getSummary(helpAttr),
                                FieldInfo = fieldInfo,
                                CustomValueDescription = getCustomValueDescription(helpAttr)
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
            DisplayHelp<HelpDetailsAttribute, HideFromHelpAttribute>(writer, types, attr => attr.Summary, attr => attr.CustomValueDescription);
        }

        public static void DisplayHelp<THelpDetails, THideFromHelp>(TextWriter writer, object[] types, Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            DisplayHelp(writer, ParseHelpTable<THelpDetails, THideFromHelp>(types, getSummary, getCustomValueDescription));
        }

        public static void DisplayHelp<THelpDetails, THideFromHelp>(TextWriter writer, Type[] types, Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            DisplayHelp(writer, ParseHelpTable<THelpDetails, THideFromHelp>(types, getSummary, getCustomValueDescription));
        }

        public static void DisplayHelp(TextWriter writer,
            Dictionary<string, HelpInformation> helpTable)
        {
            writer.WriteLine();
            writer.WriteLine("Options:");
            
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
        
        public static void DisplayHelp<THelpDetails, THideFromHelp>(object[] optionInstances, Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            DisplayHelp<THelpDetails, THideFromHelp>(Console.Out, optionInstances, getSummary, getCustomValueDescription);
        }

        public static void DisplayHelp<THelpDetails, THideFromHelp>(Type[] types, Func<THelpDetails, string> getSummary, Func<THelpDetails, string> getCustomValueDescription)
        {
            DisplayHelp<THelpDetails, THideFromHelp>(Console.Out, types, getSummary, getCustomValueDescription);
        }

        public static bool HelpRequested(string[] commandLine)
        {
            return commandLine.Count(v => v == "--h" || v == "--help" || v == "-help") > 0;
        }

        public static string[] RecreateArgumentsFromCurrentState(object optionInstance, Func<FieldInfo, object, bool> predicate = null)
        {
            return RecreateArgumentsFromCurrentState(optionInstance.GetType(), predicate, optionInstance);
        }

        public static string[] RecreateArgumentsFromCurrentState(Type optionType, Func<FieldInfo, object, bool> predicate = null)
        {
            return RecreateArgumentsFromCurrentState(optionType, predicate, null);
        }

        static string[] RecreateArgumentsFromCurrentState(Type optionType, Func<FieldInfo, object, bool> predicate, object instance)
        {
            var args = new List<string>();
            var optionFields = GetOptionFields(optionType, instance == null ? OptionFieldSelection.Static : OptionFieldSelection.Instance);
            foreach (var field in optionFields)
            {
                var value = field.GetValue(instance);

                if (value == null)
                    continue;

                if (predicate != null && !predicate(field, value))
                    continue;

                var name = OptionNameFor<object>(optionType, field.Name, null);

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

        private static IEnumerable<FieldInfo> GetOptionFields(Type optionType, OptionFieldSelection selection)
        {
            BindingFlags bindingFlagForMode = BindingFlagsForSelection(selection);
            var defaultOptions = optionType.GetFields(bindingFlagForMode | BindingFlags.Public)
                .Where(field => !field.IsLiteral);

            var explicitOptions = optionType.GetFields(bindingFlagForMode | BindingFlags.NonPublic)
                .Where(field => field.GetCustomAttributes(typeof(OptionAttribute), false).Any());

            return defaultOptions.Concat(explicitOptions);
        }

        static BindingFlags BindingFlagsForSelection(OptionFieldSelection selection)
        {
            switch (selection)
            {
                case OptionFieldSelection.Static:
                    return BindingFlags.Static;
                case OptionFieldSelection.Instance:
                    return BindingFlags.Instance;
                case OptionFieldSelection.Any:
                    return BindingFlags.Static | BindingFlags.Instance;
                default:
                    throw new ArgumentException(nameof(selection));
            }
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

        internal string[] Parse<TAliasAttribute>(IEnumerable<string> commandLine, Func<Type, string, object> customValueParser, Func<TAliasAttribute, string> getAliasName,
            Func<FieldInfo, string, string[]> customCollectionSplitter, string currentDirectory, object[] instances)
        {
            var optionSet = PrepareOptionSet(customValueParser, getAliasName, customCollectionSplitter, instances);
            return optionSet.Parse(commandLine, currentDirectory).ToArray();
        }

        private OptionSet PrepareOptionSet<TAliasAttribute>(Func<Type, string, object> customValueParser, Func<TAliasAttribute, string> getAliasName,
            Func<FieldInfo, string, string[]> customCollectionSplitter, object[] instances)
        {
            var optionSet = new OptionSet();

            foreach (var type in _types)
                ExtendOptionSet(optionSet, type, customValueParser, getAliasName, customCollectionSplitter, instances);

            return optionSet;
        }

        private void ExtendOptionSet<TAliasAttribute>(OptionSet optionSet, Type type, Func<Type, string, object> customValueParser, Func<TAliasAttribute, string> getAliasName,
            Func<FieldInfo, string, string[]> customCollectionSplitter,
            object[] instances)
        {
            var fields = GetOptionFields(type, instances == null ? OptionFieldSelection.Static : OptionFieldSelection.Instance);

            foreach (var field in fields)
            {
#if NETCORE
                var options = (ProgramOptionsAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(ProgramOptionsAttribute), false).FirstOrDefault();
#else
                var options = (ProgramOptionsAttribute)type.GetCustomAttributes(typeof(ProgramOptionsAttribute), false).FirstOrDefault();
#endif
                foreach (var name in OptionNamesFor(options, field, getAliasName))
                {
                    optionSet.Add(
                        name,
                        DescriptionFor(field),
                        ActionFor(options, field, customValueParser, customCollectionSplitter, instances));
                }
            }
        }

        private static IEnumerable<string> OptionNamesFor<TAliasAttribute>(ProgramOptionsAttribute options, FieldInfo field, Func<TAliasAttribute, string> getAliasName)
        {
            var name = NormalizeName(field.Name);

            if (field.FieldType != typeof(bool))
                name += "=";

            if (options?.Group == null)
            {
                yield return name;
                yield return NormalizeName(field.DeclaringType.Name) + "." + name;
            }
            else
                yield return options.Group + "." + name;

            if (getAliasName != null)
            {
                foreach (var aliasAttr in field.GetCustomAttributes(typeof(TAliasAttribute), false))
                {
                    if (field.FieldType != typeof(bool))
                        yield return $"{(getAliasName((TAliasAttribute) aliasAttr))}=";
                    else
                        yield return getAliasName((TAliasAttribute) aliasAttr);
                }
            }
        }

        internal static string OptionNameFor<TAliasAttribute>(Type type, string fieldName, out Type fieldType, Func<TAliasAttribute, string> getAliasName)
        {
            var field = GetOptionFields(type, OptionFieldSelection.Any).FirstOrDefault(f => f.Name == fieldName);
            if (field == null)
                throw new ArgumentException($"No field on type {type} named {fieldName}");
            fieldType = field.FieldType;

#if NETCORE
            var options = (ProgramOptionsAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(ProgramOptionsAttribute), false).FirstOrDefault();
#else
            var options = (ProgramOptionsAttribute)type.GetCustomAttributes(typeof(ProgramOptionsAttribute), false).FirstOrDefault();
#endif
            return $"--{OptionNamesFor(options, field, getAliasName).First()}".TrimEnd('=');
        }

        public static string OptionNameFor(Type type, string fieldName)
        {
            return OptionNameFor<object>(type, fieldName, null);
        }

        public static string OptionNameFor<TAliasAttribute>(Type type, string fieldName, Func<TAliasAttribute, string> getAliasName)
        {
            var field = GetOptionFields(type, OptionFieldSelection.Any).FirstOrDefault(f => f.Name == fieldName);
            if (field == null)
                throw new ArgumentException($"No field on type {type} named {fieldName}");

#if NETCORE
            var options = (ProgramOptionsAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(ProgramOptionsAttribute), false).FirstOrDefault();
#else
            var options = (ProgramOptionsAttribute)type.GetCustomAttributes(typeof(ProgramOptionsAttribute), false).FirstOrDefault();
#endif
            return $"--{OptionNamesFor(options, field, getAliasName).First()}".TrimEnd('=');
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

        private static Action<string> ActionFor(ProgramOptionsAttribute options, FieldInfo field, Func<Type, string, object> customValueParser,
            Func<FieldInfo, string, string[]> customCollectionSplitter,
            object[] instances)
        {
            if (field.FieldType.IsArray && IsFlagsEnum(field.FieldType.GetElementType()))
                throw new NotSupportedException("The parsing of a flags array is not supported.  There would be no way to know which values to combine together into each element in the array");

            var instance = instances == null ? null : instances.FirstOrDefault(o => o.GetType() == field.DeclaringType);
            if (instances != null && instance == null)
                throw new ArgumentException($"No option instance found for field : {field}");

            if (field.FieldType.IsArray)
                return v => SetArrayType(field, v, options, customValueParser, customCollectionSplitter, instance);

            if (IsListField(field))
            {
                return v => SetListType(field, v, options, customValueParser, customCollectionSplitter, instance);
            }

            if (field.FieldType == typeof(bool))
                return v => SetBoolType(field, v, instance);

            if (IsFlagsEnum(field.FieldType))
                return v => SetFlagsEnumType(field, customCollectionSplitter, v, options, instance);

            return v => SetBasicType(field, v, customValueParser, instance);
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

        private static void SetListType(FieldInfo field, string value, ProgramOptionsAttribute options, Func<Type, string, object> customValueParser, Func<FieldInfo, string, string[]> customCollectionSplitter, object instance)
        {
            var listType = field.FieldType;
            var list = (IList)field.GetValue(instance) ?? (IList)Activator.CreateInstance(listType);

            foreach (var v in SplitCollectionValues(field, customCollectionSplitter, options, value))
                list.Add(ParseValue(listType.GetGenericArguments()[0], v, customValueParser));

            field.SetValue(instance, list);
        }

        private static void SetArrayType(FieldInfo field, string value, ProgramOptionsAttribute options, Func<Type, string, object> customValueParser,
            Func<FieldInfo, string, string[]> customCollectionSplitter, object instance)
        {
            var index = 0;
            var values = SplitCollectionValues(field, customCollectionSplitter, options, value);
            var arrayType = field.FieldType;
            var array = (Array)field.GetValue(instance);

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

            field.SetValue(instance, array);
        }

        private static void SetBoolType(FieldInfo field, string v, object instance)
        {
            field.SetValue(instance, true);
        }

        private static void SetBasicType(FieldInfo field, string v, Func<Type, string, object> customValueParser, object instance)
        {
            field.SetValue(instance, ParseValue(field.FieldType, v, customValueParser));
        }

        private static void SetFlagsEnumType(FieldInfo field, Func<FieldInfo, string, string[]> customCollectionSplitter, string value, ProgramOptionsAttribute options, object instance)
        {
            var under = Enum.GetUnderlyingType(field.FieldType);
            if (under == typeof(Int32))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter,  value, options, 0, (accum, current) => accum | current));
            else if(under == typeof(UInt32))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (uint)0, (accum, current) => accum | current));
            else if (under == typeof(Int64))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (long)0, (accum, current) => accum | current));
            else if (under == typeof(UInt64))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (ulong)0, (accum, current) => accum | current));
            else if (under == typeof(Int16))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (short)0, (accum, current) => (short)(accum | current)));
            else if (under == typeof(UInt16))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (ushort)0, (accum, current) => (ushort)(accum | current)));
            else if (under == typeof(Byte))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (byte)0, (accum, current) => (byte)(accum | current)));
            else if (under == typeof(SByte))
                field.SetValue(instance, BuildFinalEnumValue(field, customCollectionSplitter, value, options, (sbyte)0, (accum, current) => (sbyte)(accum | current)));
            else
                throw new ArgumentException($"Unhandled underlying enum type of : {under}");
        }

        private static T BuildFinalEnumValue<T>(FieldInfo field, Func<FieldInfo, string, string[]> customCollectionSplitter, string value, ProgramOptionsAttribute options, T zero, Func<T, T, T> combine)
        {
            return SplitCollectionValues(field, customCollectionSplitter, options, value)
                .Select(v => (T)ParseEnumValue(field.FieldType, v))
                .Aggregate(zero, combine);
        }

        private static string[] SplitCollectionValues(FieldInfo field, Func<FieldInfo, string, string[]> customCollectionSplitter, ProgramOptionsAttribute options, string value)
        {
            string[] result = null;
            if (customCollectionSplitter != null)
                result = customCollectionSplitter(field, value);

            if (result != null)
                return result;

            return value.Split(new[] { options?.CollectionSeparator ?? "," }, StringSplitOptions.None);
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

        enum OptionFieldSelection
        {
            Static,
            Instance,
            Any
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
