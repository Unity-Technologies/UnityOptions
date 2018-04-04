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
        public static string StringValue;
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
}
