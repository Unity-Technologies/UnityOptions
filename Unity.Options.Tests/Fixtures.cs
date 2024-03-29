using System;
using System.Collections.Generic;

namespace Unity.Options.Tests
{
    [ProgramOptions]
    public sealed class SimpleOptions
    {
        public static int Value;
    }

    [ProgramOptions]
    public sealed class StringOptions
    {
        public static string Value;
    }

    [ProgramOptions]
    public sealed class SimpleOptionsUsingNonPublicFields
    {
        [Option]
        internal static int ValueInternal;

        [Option]
        private static int ValuePrivate;

        public static int GetValuePrivate
        {
            get
            {
                return ValuePrivate;
            }
        }
    }

    [ProgramOptions]
    public sealed class MultipleSimpleOptions
    {
        public static int Value;
        public static int SecondValue;
    }

    [ProgramOptions(Group = "custom")]
    public sealed class CustomGroupOptions
    {
        public static int Value;
        public static int SecondValue;
    }

    [ProgramOptions]
    public sealed class BasicTypesOptions
    {
        public static char CharValue;
        public static byte ByteValue;
        public static sbyte SByteValue;
        public static short ShortValue;
        public static ushort UShortValue;
        public static int IntValue;
        public static uint UIntValue;
        public static long LongValue;
        public static ulong ULongValue;
        public static float FloatValue;
        public static double DoubleValue;
        public static bool BoolValue;
        public static string StringValueNoSpaces;
        public static string StringValueWithSpaces;
        public static string StringValueWithPath;

        public static void SetupDefaults()
        {
            CharValue = (char)0;
            ByteValue = 0;
            SByteValue = 0;
            ShortValue = 0;
            UShortValue = 0;
            IntValue = 0;
            UIntValue = 0;
            LongValue = 0;
            ULongValue = 0;
            FloatValue = 0;
            DoubleValue = 0;
            BoolValue = false;
            StringValueNoSpaces = string.Empty;
            StringValueWithSpaces = string.Empty;
            StringValueWithPath = string.Empty;
        }
    }

    [ProgramOptions]
    public sealed class ArrayOptions
    {
        public static int[] IntArray;
    }

    [ProgramOptions]
    public sealed class ArrayRepetitionOptions
    {
        public static int[] IntArray;
    }

    [ProgramOptions(CollectionSeparator = "|")]
    public sealed class CustomArrayOptions
    {
        public static int[] IntArray;
    }

    [ProgramOptions]
    public sealed class ArrayBasicTypesOptions
    {
        public static char[] CharValue;
        public static byte[] ByteValue;
        public static sbyte[] SByteValue;
        public static short[] ShortValue;
        public static ushort[] UShortValue;
        public static int[] IntValue;
        public static uint[] UIntValue;
        public static long[] LongValue;
        public static ulong[] ULongValue;
        public static float[] FloatValue;
        public static double[] DoubleValue;
        public static bool[] BoolValue;
        public static string[] StringValue;
    }

    [ProgramOptions]
    public sealed class ListOptions
    {
        public static List<int> IntList;
    }

    [ProgramOptions]
    public sealed class ListRepetitionOptions
    {
        public static List<int> IntList;
    }

    [ProgramOptions(CollectionSeparator = "|")]
    public sealed class CustomListOptions
    {
        public static List<int> IntList;
    }

    [ProgramOptions]
    public sealed class ListBasicTypesOptions
    {
        public static List<char> CharList;
        public static List<byte> ByteList;
        public static List<sbyte> SByteList;
        public static List<short> ShortList;
        public static List<ushort> UShortList;
        public static List<int> IntList;
        public static List<uint> UIntList;
        public static List<long> LongList;
        public static List<ulong> ULongList;
        public static List<float> FloatList;
        public static List<double> DoubleList;
        public static List<bool> BoolList;
        public static List<string> StringList;
    }

    public enum Values
    {
        First,
        Second
    }

    [ProgramOptions]
    public sealed class EnumOptions
    {
        public static Values EnumValue;
    }

    [ProgramOptions]
    public sealed class EnumArrayOptions
    {
        public static Values[] EnumArrayValue;
    }

    [Flags]
    public enum FlagValues
    {
        One = 1,
        Two = 2,
        Three = 4,
        Four = 8
    }

    [ProgramOptions]
    public sealed class FlagsEnumOptions
    {
        public static FlagValues Flags;
    }
    
    [Flags]
    public enum FlagValuesUInt : uint
    {
        One = 1,
        Two = 2,
        Three = 4,
        Four = 8
    }

    [ProgramOptions]
    public sealed class FlagsEnumUIntOptions
    {
        public static FlagValuesUInt Flags;
    }
    
    [Flags]
    public enum FlagValuesLong : long
    {
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
        Four = 1 << 3,
        Five = 1 << 4,
        Six = 1 << 5,
        Seven = 1 << 6,
        Eight = 1 << 7,
        Nine = 1 << 8,
        Ten = 1 << 9,
        Eleven = 1 << 10
    }
    
    [ProgramOptions]
    public sealed class FlagsEnumULongOptions
    {
        public static FlagValuesULong Flags;
    }
    
    [Flags]
    public enum FlagValuesULong : ulong
    {
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
        Four = 1 << 3,
        Five = 1 << 4,
        Six = 1 << 5,
        Seven = 1 << 6,
        Eight = 1 << 7,
        Nine = 1 << 8,
        Ten = 1 << 9,
        Eleven = 1 << 10
    }
    
    [ProgramOptions]
    public sealed class FlagsEnumLongOptions
    {
        public static FlagValuesLong Flags;
    }
    
    [Flags]
    public enum FlagValuesShort : short
    {
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
    }
    
    [ProgramOptions]
    public sealed class FlagsEnumShortOptions
    {
        public static FlagValuesShort Flags;
    }
    
    [Flags]
    public enum FlagValuesByte : byte
    {
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
    }
    
    [ProgramOptions]
    public sealed class FlagsEnumByteOptions
    {
        public static FlagValuesByte Flags;
    }
    
    [Flags]
    public enum FlagValuesSByte : sbyte
    {
        One = 1 << 0,
        Two = 1 << 1,
    }
    
    [ProgramOptions]
    public sealed class FlagsEnumSByteOptions
    {
        public static FlagValuesSByte Flags;
    }

    [ProgramOptions]
    public sealed class FlagsEnumArrayOptions
    {
        public static FlagValues[] FlagsArray;
    }

    [ProgramOptions]
    public sealed class BoolOptions
    {
        public static bool BoolValue;
    }

    [ProgramOptions]
    public sealed class HelpOptions
    {
        internal const string OptionOneHelpText = "This is help information";
        internal const string OptionTwoHelpText = "I hope it helps you";
        internal const string CustomValueDescriptionHelpText = "Dont Care..normally";

        [HelpDetails(OptionOneHelpText)]
        public static string OptionOne;

        [HelpDetails(OptionTwoHelpText)]
        public static bool OptionTwo;

        public static string OptionMissingHelpDetails;

        [HelpDetails(CustomValueDescriptionHelpText, "path")]
        public static string CustomValueDescription;

        [HideFromHelp]
        public static bool OptionHidden;
    }

    [ProgramOptions]
    public sealed class InstanceHelpOptions
    {
        internal const string OptionOneHelpText = "This is help information";
        internal const string OptionTwoHelpText = "I hope it helps you";
        internal const string CustomValueDescriptionHelpText = "Dont Care..normally";

        [HelpDetails(OptionOneHelpText)]
        public string OptionOne;

        [HelpDetails(OptionTwoHelpText)]
        public bool OptionTwo;

        public string OptionMissingHelpDetails;

        [HelpDetails(CustomValueDescriptionHelpText, "path")]
        public string CustomValueDescription;

        [HideFromHelp]
        public bool OptionHidden;
    }
    
    [ProgramOptions]
    public sealed class InstanceHelpOptionsWithCustomHelpAttributeTypes
    {
        internal const string OptionOneHelpText = "This is help information";
        internal const string OptionTwoHelpText = "I hope it helps you";
        internal const string CustomValueDescriptionHelpText = "Dont Care..normally";

        [CustomHelpDetailsAttribute("Should be ignored")]
        public static string AStaticsAreIgnored;

        [CustomHelpDetailsAttribute(OptionOneHelpText)]
        public string OptionOne;

        [CustomHelpDetailsAttribute(OptionTwoHelpText)]
        public bool OptionTwo;

        public string OptionMissingHelpDetails;

        [CustomHelpDetailsAttribute(CustomValueDescriptionHelpText, "path")]
        public string CustomValueDescription;

        [CustomHideFromHelpAttribute]
        public bool OptionHidden;
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class CustomHelpDetailsAttribute : Attribute
    {
        public string Summary { get; set; }

        public string CustomValueDescription { get; set; }

        public CustomHelpDetailsAttribute(string summary, string customValueDescription = null)
        {
            Summary = summary;
            CustomValueDescription = customValueDescription;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class CustomHideFromHelpAttribute : Attribute
    {
    }

    [ProgramOptions]
    public sealed class HelpOptionsWithCollections
    {
        internal const string HelpText = "This is an array arg";

        [HelpDetails(HelpText)]
        public static string[] StringArray;

        [HelpDetails(HelpText, "custom")]
        public static List<string> StringList;
    }

    [ProgramOptions]
    public sealed class FromFileOptions
    {
        public static int FromFileValueOne;
        public static int FromFileValueTwo;
    }

    [ProgramOptions]
    public sealed class OptionsWithAliases
    {
        [OptionAlias("alias")]
        public static string OptionWithAlias;

        [OptionAlias("enable-alias")]
        public static bool BoolOptionWithAlias;

        [OptionAlias("multi-alias1")]
        [OptionAlias("multi-alias2")]
        [OptionAlias("multi-alias3")]
        [OptionAlias("multi-alias4")]
        public static string OptionWithMultipleAliases;
    }
    
    public sealed class OptionsWithAliasesCustom
    {
        [CustomOptionAlias("alias")]
        public string OptionWithAlias;

        [CustomOptionAlias("enable-alias")]
        public bool BoolOptionWithAlias;

        [CustomOptionAlias("multi-alias1")]
        [CustomOptionAlias("multi-alias2")]
        [CustomOptionAlias("multi-alias3")]
        [CustomOptionAlias("multi-alias4")]
        public string OptionWithMultipleAliases;
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class CustomOptionAliasAttribute : Attribute
    {
        public string Name { get; set; }

        public CustomOptionAliasAttribute(string name)
        {
            Name = name;
        }
    }

    [ProgramOptions]
    public sealed class OptionsWithCustomParsers
    {
        public static CustomType1 Arg1;
        public static CustomType2 Arg2;

        public static CustomType1[] ArrayArg;

        public static object ParseCustomArgumentType(Type fieldType, string value)
        {
            if (fieldType == typeof(CustomType1))
                return new CustomType1(value);

            if (fieldType == typeof(CustomType2))
                return new CustomType2(value);

            return null;
        }

        public class CustomType1
        {
            public CustomType1(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
        }

        public class CustomType2
        {
            public CustomType2(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
        }
    }

    [ProgramOptions]
    public sealed class RecreateOptions
    {
        public static string StringValue1;

        public static string StringValue2;

        public static bool BoolValue1;

        public static bool BoolValue2;

        public static string[] CollectionValue1;

        public static string[] CollectionValue2;
    }

    [ProgramOptions]
    public sealed class InstanceRecreateOptions
    {
        public string StringValue1;

        public string StringValue2;

        public bool BoolValue1;

        public bool BoolValue2;

        public string[] CollectionValue1;

        public string[] CollectionValue2;
    }

    [ProgramOptions]
    public sealed class RecreateOptions2
    {
        public static string StringValue1;

        public const string ShouldNotBeIncluded = "No!";
    }

    // Don't require program options
    public sealed class InstanceOptions
    {
        public string StringValue;

        public bool BoolValue;

        public string[] ArrayValue;

        public List<string> ListValue;

        public Values EnumValue;

        public FlagValues FlagEnumValue;
    }

    [ProgramOptions]
    public sealed class InstanceOptionsWithStaticField
    {
        public string StringValue;

        public static string StaticStringValue;
    }

    [ProgramOptions]
    public sealed class InstanceMultiple1
    {
        public string Value1;
    }

    [ProgramOptions]
    public sealed class InstanceMultiple2
    {
        public string Value2;
    }

    [ProgramOptions]
    public sealed class InstanceMultiple3
    {
        public string Value3;
    }
}
