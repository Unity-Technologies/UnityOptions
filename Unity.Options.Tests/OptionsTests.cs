using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine.Options.Tests.AnotherReference;
using UnityEngine.Options.Tests.ExternalReference;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Unity.Options.Tests
{
    [TestFixture]
    public class OptionsTests
    {
        [Test]
        public void CanParseSimpleOption()
        {
            var commandLine = new[] { "--value=10" };
            var types = new[] { typeof(SimpleOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(SimpleOptions.Value, Is.EqualTo(10));
        }

        [Test]
        public void CanParseSimpleOptionUsingOptionAttributeOnPrivateStaticField()
        {
            var commandLine = new[] {"--value-private=15"};
            var types = new[] {typeof(SimpleOptionsUsingNonPublicFields)};

            OptionsParser.Prepare(commandLine, types);

            Assert.That(SimpleOptionsUsingNonPublicFields.GetValuePrivate, Is.EqualTo(15));
        }

        [Test]
        public void CanParseSimpleOptionUsingOptionAttributeOnInternalStaticField()
        {
            var commandLine = new[] { "--value-internal=15" };
            var types = new[] { typeof(SimpleOptionsUsingNonPublicFields) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(SimpleOptionsUsingNonPublicFields.ValueInternal, Is.EqualTo(15));
        }

        [Test]
        public void CanParseMultipleSimpleOptions()
        {
            var commandLine = new[] { "--value=10", "--second-value=11" };
            var types = new[] { typeof(MultipleSimpleOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(MultipleSimpleOptions.Value, Is.EqualTo(10));
            Assert.That(MultipleSimpleOptions.SecondValue, Is.EqualTo(11));
        }

        [Test]
        public void SupportOptionGroups()
        {
            var commandLine = new[]
            {
                "--multiple-simple-options.value=10",
                "--multiple-simple-options.second-value=11"
            };
            var types = new[] { typeof(MultipleSimpleOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(MultipleSimpleOptions.Value, Is.EqualTo(10));
            Assert.That(MultipleSimpleOptions.SecondValue, Is.EqualTo(11));
        }

        [Test]
        public void SupportCustomOptionGroups()
        {
            var commandLine = new[]
            {
                "--custom.value=10",
                "--custom.second-value=11"
            };
            var types = new[] { typeof(CustomGroupOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(CustomGroupOptions.Value, Is.EqualTo(10));
            Assert.That(CustomGroupOptions.SecondValue, Is.EqualTo(11));
        }

        [Test]
        public void CanParseBasicOptionTypes()
        {
            var commandLine = new[]
            {
                "--char-value=1",
                "--byte-value=2",
                "--s-byte-value=3",
                "--short-value=4",
                "--u-short-value=5",
                "--int-value=6",
                "--u-int-value=7",
                "--long-value=8",
                "--u-long-value=9",
                "--float-value=10.42",
                "--double-value=11.33",
                "--bool-value",
                "--string-value-with-spaces=gabriele farina",
                "--string-value-no-spaces=gabrielefarina",
                "--string-value-with-path=C:\\My\\Path"
            };
            var types = new[] { typeof(BasicTypesOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(BasicTypesOptions.CharValue, Is.EqualTo('1'));
            Assert.That(BasicTypesOptions.ByteValue, Is.EqualTo(2));
            Assert.That(BasicTypesOptions.SByteValue, Is.EqualTo(3));
            Assert.That(BasicTypesOptions.ShortValue, Is.EqualTo(4));
            Assert.That(BasicTypesOptions.UShortValue, Is.EqualTo(5));
            Assert.That(BasicTypesOptions.IntValue, Is.EqualTo(6));
            Assert.That(BasicTypesOptions.UIntValue, Is.EqualTo(7));
            Assert.That(BasicTypesOptions.LongValue, Is.EqualTo(8));
            Assert.That(BasicTypesOptions.ULongValue, Is.EqualTo(9));
            Assert.That(BasicTypesOptions.FloatValue, Is.EqualTo(10.42f));
            Assert.That(BasicTypesOptions.DoubleValue, Is.EqualTo(11.33));
            Assert.That(BasicTypesOptions.BoolValue, Is.EqualTo(true));
            Assert.That(BasicTypesOptions.StringValueWithSpaces, Is.EqualTo("gabriele farina"));
            Assert.That(BasicTypesOptions.StringValueNoSpaces, Is.EqualTo("gabrielefarina"));
            Assert.That(BasicTypesOptions.StringValueWithPath, Is.EqualTo("C:\\My\\Path"));
        }
        
        [Test]
        public void CanParseBasicOptionTypesWithSlashPrefix()
        {
            var commandLine = new[]
            {
                "/char-value=1",
                "/byte-value=2",
                "/s-byte-value=3",
                "/short-value=4",
                "/u-short-value=5",
                "/int-value=6",
                "/u-int-value=7",
                "/long-value=8",
                "/u-long-value=9",
                "/float-value=10.42",
                "/double-value=11.33",
                "/bool-value",
                "/string-value-with-spaces=gabriele farina",
                "/string-value-no-spaces=gabrielefarina",
                "/string-value-with-path=C:\\My\\Path"
            };
            var types = new[] { typeof(BasicTypesOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(BasicTypesOptions.CharValue, Is.EqualTo('1'));
            Assert.That(BasicTypesOptions.ByteValue, Is.EqualTo(2));
            Assert.That(BasicTypesOptions.SByteValue, Is.EqualTo(3));
            Assert.That(BasicTypesOptions.ShortValue, Is.EqualTo(4));
            Assert.That(BasicTypesOptions.UShortValue, Is.EqualTo(5));
            Assert.That(BasicTypesOptions.IntValue, Is.EqualTo(6));
            Assert.That(BasicTypesOptions.UIntValue, Is.EqualTo(7));
            Assert.That(BasicTypesOptions.LongValue, Is.EqualTo(8));
            Assert.That(BasicTypesOptions.ULongValue, Is.EqualTo(9));
            Assert.That(BasicTypesOptions.FloatValue, Is.EqualTo(10.42f));
            Assert.That(BasicTypesOptions.DoubleValue, Is.EqualTo(11.33));
            Assert.That(BasicTypesOptions.BoolValue, Is.EqualTo(true));
            Assert.That(BasicTypesOptions.StringValueWithSpaces, Is.EqualTo("gabriele farina"));
            Assert.That(BasicTypesOptions.StringValueNoSpaces, Is.EqualTo("gabrielefarina"));
            Assert.That(BasicTypesOptions.StringValueWithPath, Is.EqualTo("C:\\My\\Path"));
        }

        [Test]
        public void CanParseFloatInCommaDecimalLocale()
        {
            var commandLine = new[]
            {
                "--float-value=10.42",
                "--double-value=11.33",
            };
            var types = new[] { typeof(BasicTypesOptions) };

            var oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("lt-lt");

            try
            {
                OptionsParser.Prepare(commandLine, types);

                Assert.That(BasicTypesOptions.FloatValue, Is.EqualTo(10.42f));
                Assert.That(BasicTypesOptions.DoubleValue, Is.EqualTo(11.33));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [Test]
        public void SupportArrayOptions()
        {
            var commandLine = new[]
            {
                "--int-array=1,2,3"
            };
            var types = new[] { typeof(ArrayOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(ArrayOptions.IntArray.Length, Is.EqualTo(3));
            Assert.That(ArrayOptions.IntArray[0], Is.EqualTo(1));
            Assert.That(ArrayOptions.IntArray[1], Is.EqualTo(2));
            Assert.That(ArrayOptions.IntArray[2], Is.EqualTo(3));
        }

        [Test]
        public void SupportArrayOptionsWithCustomSeparator()
        {
            var commandLine = new[]
            {
                "--int-array=1|2|3"
            };
            var types = new[] { typeof(CustomArrayOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(CustomArrayOptions.IntArray.Length, Is.EqualTo(3));
            Assert.That(CustomArrayOptions.IntArray[0], Is.EqualTo(1));
            Assert.That(CustomArrayOptions.IntArray[1], Is.EqualTo(2));
            Assert.That(CustomArrayOptions.IntArray[2], Is.EqualTo(3));
        }

        [Test]
        public void SupportArrayOptionsWithRepetition()
        {
            var commandLine = new[]
            {
                "--int-array=1",
                "--int-array=2",
                "--int-array=3"
            };
            var types = new[] { typeof(ArrayRepetitionOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(ArrayRepetitionOptions.IntArray.Length, Is.EqualTo(3));
            Assert.That(ArrayRepetitionOptions.IntArray[0], Is.EqualTo(1));
            Assert.That(ArrayRepetitionOptions.IntArray[1], Is.EqualTo(2));
            Assert.That(ArrayRepetitionOptions.IntArray[2], Is.EqualTo(3));
        }

        [Test]
        public void CanParseArrayBasicOptionTypes()
        {
            var commandLine = new[]
            {
                "--char-value=1,a",
                "--byte-value=2,22",
                "--s-byte-value=3,33",
                "--short-value=4,44",
                "--u-short-value=5,55",
                "--int-value=6,66",
                "--u-int-value=7,77",
                "--long-value=8,88",
                "--u-long-value=9,99",
                "--float-value=10.42,1042.1042",
                "--double-value=11.33,1133.1133",
                "--bool-value=false,true",
                "--string-value=gabriele farina,ralph hauwert"
            };
            var types = new[] { typeof(ArrayBasicTypesOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(ArrayBasicTypesOptions.CharValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.CharValue[0], Is.EqualTo('1'));
            Assert.That(ArrayBasicTypesOptions.CharValue[1], Is.EqualTo('a'));

            Assert.That(ArrayBasicTypesOptions.ByteValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.ByteValue[0], Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.ByteValue[1], Is.EqualTo(22));

            Assert.That(ArrayBasicTypesOptions.SByteValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.SByteValue[0], Is.EqualTo(3));
            Assert.That(ArrayBasicTypesOptions.SByteValue[1], Is.EqualTo(33));

            Assert.That(ArrayBasicTypesOptions.ShortValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.ShortValue[0], Is.EqualTo(4));
            Assert.That(ArrayBasicTypesOptions.ShortValue[1], Is.EqualTo(44));

            Assert.That(ArrayBasicTypesOptions.UShortValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.UShortValue[0], Is.EqualTo(5));
            Assert.That(ArrayBasicTypesOptions.UShortValue[1], Is.EqualTo(55));

            Assert.That(ArrayBasicTypesOptions.IntValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.IntValue[0], Is.EqualTo(6));
            Assert.That(ArrayBasicTypesOptions.IntValue[1], Is.EqualTo(66));

            Assert.That(ArrayBasicTypesOptions.UIntValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.UIntValue[0], Is.EqualTo(7));
            Assert.That(ArrayBasicTypesOptions.UIntValue[1], Is.EqualTo(77));

            Assert.That(ArrayBasicTypesOptions.LongValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.LongValue[0], Is.EqualTo(8));
            Assert.That(ArrayBasicTypesOptions.LongValue[1], Is.EqualTo(88));

            Assert.That(ArrayBasicTypesOptions.ULongValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.ULongValue[0], Is.EqualTo(9));
            Assert.That(ArrayBasicTypesOptions.ULongValue[1], Is.EqualTo(99));

            Assert.That(ArrayBasicTypesOptions.FloatValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.FloatValue[0], Is.EqualTo(10.42f));
            Assert.That(ArrayBasicTypesOptions.FloatValue[1], Is.EqualTo(1042.1042f));

            Assert.That(ArrayBasicTypesOptions.DoubleValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.DoubleValue[0], Is.EqualTo(11.33));
            Assert.That(ArrayBasicTypesOptions.DoubleValue[1], Is.EqualTo(1133.1133));

            Assert.That(ArrayBasicTypesOptions.BoolValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.BoolValue[0], Is.EqualTo(false));
            Assert.That(ArrayBasicTypesOptions.BoolValue[1], Is.EqualTo(true));

            Assert.That(ArrayBasicTypesOptions.StringValue.Length, Is.EqualTo(2));
            Assert.That(ArrayBasicTypesOptions.StringValue[0], Is.EqualTo("gabriele farina"));
            Assert.That(ArrayBasicTypesOptions.StringValue[1], Is.EqualTo("ralph hauwert"));
        }

        [Test]
        public void SupportListOptions()
        {
            var commandLine = new[]
            {
                "--int-list=1,2,3"
            };
            var types = new[] { typeof(ListOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(ListOptions.IntList.Count, Is.EqualTo(3));
            Assert.That(ListOptions.IntList[0], Is.EqualTo(1));
            Assert.That(ListOptions.IntList[1], Is.EqualTo(2));
            Assert.That(ListOptions.IntList[2], Is.EqualTo(3));
        }

        [Test]
        public void SupportListOptionsWithCustomSeparator()
        {
            var commandLine = new[]
            {
                "--int-list=1|2|3"
            };
            var types = new[] { typeof(CustomListOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(CustomListOptions.IntList.Count, Is.EqualTo(3));
            Assert.That(CustomListOptions.IntList[0], Is.EqualTo(1));
            Assert.That(CustomListOptions.IntList[1], Is.EqualTo(2));
            Assert.That(CustomListOptions.IntList[2], Is.EqualTo(3));
        }

        [Test]
        public void SupportListOptionsWithRepetition()
        {
            var commandLine = new[]
            {
                "--int-list=1",
                "--int-list=2",
                "--int-list=3"
            };
            var types = new[] { typeof(ListRepetitionOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(ListRepetitionOptions.IntList.Count, Is.EqualTo(3));
            Assert.That(ListRepetitionOptions.IntList[0], Is.EqualTo(1));
            Assert.That(ListRepetitionOptions.IntList[1], Is.EqualTo(2));
            Assert.That(ListRepetitionOptions.IntList[2], Is.EqualTo(3));
        }

        [Test]
        public void CanParseListBasicOptionTypes()
        {
            var commandLine = new[]
            {
                "--char-list=1,a",
                "--byte-list=2,22",
                "--s-byte-list=3,33",
                "--short-list=4,44",
                "--u-short-list=5,55",
                "--int-list=6,66",
                "--u-int-list=7,77",
                "--long-list=8,88",
                "--u-long-list=9,99",
                "--float-list=10.42,1042.1042",
                "--double-list=11.33,1133.1133",
                "--bool-list=false,true",
                "--string-list=gabriele farina,ralph hauwert"
            };
            var types = new[] { typeof(ListBasicTypesOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(ListBasicTypesOptions.CharList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.CharList[0], Is.EqualTo('1'));
            Assert.That(ListBasicTypesOptions.CharList[1], Is.EqualTo('a'));

            Assert.That(ListBasicTypesOptions.ByteList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.ByteList[0], Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.ByteList[1], Is.EqualTo(22));

            Assert.That(ListBasicTypesOptions.SByteList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.SByteList[0], Is.EqualTo(3));
            Assert.That(ListBasicTypesOptions.SByteList[1], Is.EqualTo(33));

            Assert.That(ListBasicTypesOptions.ShortList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.ShortList[0], Is.EqualTo(4));
            Assert.That(ListBasicTypesOptions.ShortList[1], Is.EqualTo(44));

            Assert.That(ListBasicTypesOptions.UShortList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.UShortList[0], Is.EqualTo(5));
            Assert.That(ListBasicTypesOptions.UShortList[1], Is.EqualTo(55));

            Assert.That(ListBasicTypesOptions.IntList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.IntList[0], Is.EqualTo(6));
            Assert.That(ListBasicTypesOptions.IntList[1], Is.EqualTo(66));

            Assert.That(ListBasicTypesOptions.UIntList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.UIntList[0], Is.EqualTo(7));
            Assert.That(ListBasicTypesOptions.UIntList[1], Is.EqualTo(77));

            Assert.That(ListBasicTypesOptions.LongList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.LongList[0], Is.EqualTo(8));
            Assert.That(ListBasicTypesOptions.LongList[1], Is.EqualTo(88));

            Assert.That(ListBasicTypesOptions.ULongList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.ULongList[0], Is.EqualTo(9));
            Assert.That(ListBasicTypesOptions.ULongList[1], Is.EqualTo(99));

            Assert.That(ListBasicTypesOptions.FloatList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.FloatList[0], Is.EqualTo(10.42f));
            Assert.That(ListBasicTypesOptions.FloatList[1], Is.EqualTo(1042.1042f));

            Assert.That(ListBasicTypesOptions.DoubleList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.DoubleList[0], Is.EqualTo(11.33));
            Assert.That(ListBasicTypesOptions.DoubleList[1], Is.EqualTo(1133.1133));

            Assert.That(ListBasicTypesOptions.BoolList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.BoolList[0], Is.EqualTo(false));
            Assert.That(ListBasicTypesOptions.BoolList[1], Is.EqualTo(true));

            Assert.That(ListBasicTypesOptions.StringList.Count, Is.EqualTo(2));
            Assert.That(ListBasicTypesOptions.StringList[0], Is.EqualTo("gabriele farina"));
            Assert.That(ListBasicTypesOptions.StringList[1], Is.EqualTo("ralph hauwert"));
        }

        [Test]
        public void CanPrepareFromAssemblyDefinition()
        {
            var commandLine = new[] { "--value=10" };

            OptionsParser.Prepare(commandLine, typeof(ExternalOptions).Assembly);

            Assert.That(ExternalOptions.Value, Is.EqualTo(10));
        }

        [Test]
        public void CanPrepareFromFile()
        {
            using (var testFile = TempFile.CreateRandom())
            {
                OptionsHelper.WriteArgumentsFile(testFile.Path.ToString(),
                    new[] { "--from-file-value-one=10", "--from-file-value-two=15" });

                OptionsParser.PrepareFromFile(testFile.Path.ToString(), new Type[] { typeof(FromFileOptions) });

                Assert.That(FromFileOptions.FromFileValueOne, Is.EqualTo(10));
                Assert.That(FromFileOptions.FromFileValueTwo, Is.EqualTo(15));
            }
        }

        [Test]
        public void CanProvideOptionsFromAssemblyReference()
        {
            var commandLine = new[]
            {
                "--value=10",
                "--name=gabriele"
            };

            OptionsParser.Prepare(commandLine, typeof(ExternalOptions).Assembly);

            Assert.That(ExternalOptions.Value, Is.EqualTo(10));
            Assert.That(OtherOptions.Name, Is.EqualTo("gabriele"));
        }

        [Test]
        public void CanParseEnumOptions()
        {
            var commandLine = new[]
            {
                "--enum-value=First"
            };
            var types = new[] { typeof(EnumOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(EnumOptions.EnumValue, Is.EqualTo(Values.First));
        }

        [Test]
        public void CanParseEnumArrayOptions()
        {
            var commandLine = new[]
            {
                "--enum-array-value=First,Second"
            };
            var types = new[] { typeof(EnumArrayOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(EnumArrayOptions.EnumArrayValue.Length, Is.EqualTo(2));
            Assert.That(EnumArrayOptions.EnumArrayValue[0], Is.EqualTo(Values.First));
            Assert.That(EnumArrayOptions.EnumArrayValue[1], Is.EqualTo(Values.Second));
        }

        [Test]
        public void CanParseFlagsEnumOptions()
        {
            var commandLine = new[]
            {
                "--flags=One,Three"
            };
            var types = new[] { typeof(FlagsEnumOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumOptions.Flags, Is.EqualTo(FlagValues.One | FlagValues.Three));
        }
        
        [Test]
        public void CanParseFlagsEnumUIntOptions()
        {
            var commandLine = new[]
            {
                "--flags=One,Three"
            };
            var types = new[] { typeof(FlagsEnumUIntOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumUIntOptions.Flags, Is.EqualTo(FlagValuesUInt.One | FlagValuesUInt.Three));
        }
        
        [Test]
        public void CanParseFlagsEnumShortOptions()
        {
            var commandLine = new[]
            {
                "--flags=One,Three"
            };
            var types = new[] { typeof(FlagsEnumShortOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumShortOptions.Flags, Is.EqualTo(FlagValuesShort.One | FlagValuesShort.Three));
        }
        
        [Test]
        public void CanParseFlagsEnumByteOptions()
        {
            var commandLine = new[]
            {
                "--flags=One,Three"
            };
            var types = new[] { typeof(FlagsEnumByteOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumByteOptions.Flags, Is.EqualTo(FlagValuesByte.One | FlagValuesByte.Three));
        }
        
        [Test]
        public void CanParseFlagsEnumSByteOptions()
        {
            var commandLine = new[]
            {
                "--flags=One,Two"
            };
            var types = new[] { typeof(FlagsEnumSByteOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumSByteOptions.Flags, Is.EqualTo(FlagValuesSByte.One | FlagValuesSByte.Two));
        }
        
        [Test]
        public void CanParseFlagsEnumOptionsLongLowValues()
        {
            var commandLine = new[]
            {
                "--flags=One,Three"
            };
            var types = new[] { typeof(FlagsEnumLongOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumLongOptions.Flags, Is.EqualTo(FlagValuesLong.One | FlagValuesLong.Three));
        }
        
        [Test]
        public void CanParseFlagsEnumOptionsLongHighValues()
        {
            var commandLine = new[]
            {
                "--flags=Nine,Ten"
            };
            var types = new[] { typeof(FlagsEnumLongOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumLongOptions.Flags, Is.EqualTo(FlagValuesLong.Nine | FlagValuesLong.Ten));
        }
        
        [Test]
        public void CanParseFlagsEnumOptionsLongSingleHighValue()
        {
            var commandLine = new[]
            {
                "--flags=Ten"
            };
            var types = new[] { typeof(FlagsEnumLongOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumLongOptions.Flags, Is.EqualTo(FlagValuesLong.Ten));
        }
        
        [Test]
        public void CanParseFlagsEnumOptionsULongLowValues()
        {
            var commandLine = new[]
            {
                "--flags=One,Three"
            };
            var types = new[] { typeof(FlagsEnumULongOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumULongOptions.Flags, Is.EqualTo(FlagValuesULong.One | FlagValuesULong.Three));
        }
        
        [Test]
        public void CanParseFlagsEnumOptionsULongHighValues()
        {
            var commandLine = new[]
            {
                "--flags=Nine,Ten"
            };
            var types = new[] { typeof(FlagsEnumULongOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumULongOptions.Flags, Is.EqualTo(FlagValuesULong.Nine | FlagValuesULong.Ten));
        }
        
        [Test]
        public void CanParseFlagsEnumOptionsULongSingleHighValue()
        {
            var commandLine = new[]
            {
                "--flags=Ten"
            };
            var types = new[] { typeof(FlagsEnumULongOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(FlagsEnumULongOptions.Flags, Is.EqualTo(FlagValuesULong.Ten));
        }

        [Test]
        public void FlagsEnumArrayAreNotSupported()
        {
            var commandLine = new[]
            {
                "--flags-array=One,Three"
            };
            var types = new[] { typeof(FlagsEnumArrayOptions) };

            Assert.Throws<NotSupportedException>(() => OptionsParser.Prepare(commandLine, types));
        }

        [Test]
        public void CanParseBoolOptions()
        {
            var commandLine = new[]
            {
                "--bool-value"
            };
            var types = new[] { typeof(BoolOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(BoolOptions.BoolValue, Is.EqualTo(true));
        }

        [Test]
        public void CanParseOptionUsingItsAlias()
        {
            var commandLine = new[]
            {
                "--alias=hello"
            };
            var types = new[] { typeof(OptionsWithAliases) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(OptionsWithAliases.OptionWithAlias, Is.EqualTo("hello"));
        }

        [Test]
        public void CanParseOptionWithMultiAliasesUsingItsAlias()
        {
            var commandLine = new[]
            {
                "--multi-alias4=hello"
            };
            var types = new[] { typeof(OptionsWithAliases) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(OptionsWithAliases.OptionWithMultipleAliases, Is.EqualTo("hello"));
        }

        [Test]
        public void CanParseBoolOptionUsingItsAlias()
        {
            var commandLine = new[]
            {
                "--enable-alias"
            };
            var types = new[] { typeof(OptionsWithAliases) };

            OptionsParser.Prepare(commandLine, types);

            Assert.That(OptionsWithAliases.BoolOptionWithAlias, Is.EqualTo(true));
        }

        [Test]
        public void CanParseTypeOptionWithCustomParser()
        {
            var commandLine = new[]
            {
                "--arg1=foo",
                "--arg2=bar"
            };
            var types = new[] { typeof(OptionsWithCustomParsers) };

            OptionsParser.Prepare(commandLine, types, OptionsWithCustomParsers.ParseCustomArgumentType);

            Assert.That(OptionsWithCustomParsers.Arg1.Value, Is.EqualTo("foo"));
            Assert.That(OptionsWithCustomParsers.Arg2.Value, Is.EqualTo("bar"));
        }

        [Test]
        public void CanParseArrayOfTypeOptionWithCustomParser()
        {
            var commandLine = new[]
            {
                "--array-arg=foo,bar"
            };
            var types = new[] { typeof(OptionsWithCustomParsers) };

            OptionsParser.Prepare(commandLine, types, OptionsWithCustomParsers.ParseCustomArgumentType);

            Assert.That(OptionsWithCustomParsers.ArrayArg.Select(t => t.Value), Is.EquivalentTo(new[] {"foo", "bar"}));
        }

        [Test]
        public void OptionNameForOnStringField()
        {
            Assert.That(OptionsParser.OptionNameFor(typeof(BasicTypesOptions), nameof(BasicTypesOptions.StringValueWithSpaces)), Is.EqualTo("--string-value-with-spaces"));
        }

        [Test]
        public void OptionNameForOnBoolField()
        {
            Assert.That(OptionsParser.OptionNameFor(typeof(BasicTypesOptions), nameof(BasicTypesOptions.BoolValue)), Is.EqualTo("--bool-value"));
        }

        [Test]
        public void OptionNameForThrowsIfNoMatchingFieldName()
        {
            Assert.Throws<ArgumentException>(() => OptionsParser.OptionNameFor(typeof(BasicTypesOptions), "SomeFieldThatDoesNotExist"));
        }

        [Test]
        public void TestParseHelpTableWhenGivenTypes()
        {
            var result = OptionsParser.ParseHelpTable(typeof(HelpOptions));
            HelpInformation resultEntry;
            Assert.That(result.TryGetValue("--option-one", out resultEntry), Is.True);
            Assert.That(resultEntry.Summary, Is.EqualTo(HelpOptions.OptionOneHelpText));
            Assert.That(resultEntry.FieldInfo.FieldType, Is.EqualTo(typeof(string)));
            Assert.That(resultEntry.HasCustomValueDescription, Is.False);

            Assert.That(result.TryGetValue("--option-two", out resultEntry), Is.True);
            Assert.That(resultEntry.Summary, Is.EqualTo(HelpOptions.OptionTwoHelpText));
            Assert.That(resultEntry.FieldInfo.FieldType, Is.EqualTo(typeof(bool)));
            Assert.That(resultEntry.HasCustomValueDescription, Is.False);

            Assert.That(result.TryGetValue("--option-missing-help-details", out resultEntry), Is.True);
            Assert.That(result["--option-missing-help-details"].HasSummary, Is.False);
            Assert.That(result["--option-missing-help-details"].FieldInfo, Is.Not.Null);
            Assert.That(resultEntry.HasCustomValueDescription, Is.False);

            Assert.That(result.TryGetValue("--custom-value-description", out resultEntry), Is.True);
            Assert.That(resultEntry.HasSummary, Is.True);
            Assert.That(resultEntry.FieldInfo.FieldType, Is.EqualTo(typeof(string)));
            Assert.That(resultEntry.CustomValueDescription, Is.EqualTo("path"));
            Assert.That(resultEntry.HasCustomValueDescription, Is.True);

            Assert.That(result.Count, Is.EqualTo(4));
        }

        [Test]
        public void TestHideFromHelp()
        {
            var result = OptionsParser.ParseHelpTable(typeof(HelpOptions));

            Assert.That(result.ContainsKey("--option-hidden"), Is.False);
        }

        [Test]
        public void TestParseHelpTableWhenGivenAnAssembly()
        {
            // Don't be strict with the asserts in this test.  We want to keep it flexible.  Just make sure
            // there are more entries in the result than HelpOptions has on it's own.
            var baseLine = OptionsParser.ParseHelpTable(typeof(OtherOptions));

            var result = OptionsParser.ParseHelpTable(typeof(OtherOptions).Assembly);

            Assert.That(result.Count > baseLine.Count, Is.True);
        }

        [Test]
        public void TestDisplayHelpWithSimpleArgumentTypes()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path.ToString()))
                {
                    OptionsParser.DisplayHelp(writer, typeof(HelpOptions));
                }

                using (var reader = new StreamReader(tempFile.Path.ToString()))
                {
                    Assert.That(reader.ReadLine(), Is.EqualTo(""));
                    Assert.That(reader.ReadLine(), Is.EqualTo("Options:"));

                    Assert.That(reader.ReadLine(), Is.EqualTo(string.Format("{0}{1}", "  --option-one=<value>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.OptionOneHelpText)));
                    Assert.That(reader.ReadLine(), Is.EqualTo(string.Format("{0}{1}", "  --option-two".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.OptionTwoHelpText)));
                    Assert.That(reader.ReadLine(), Is.EqualTo(string.Format("{0}{1}", "  --custom-value-description=<path>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.CustomValueDescriptionHelpText)));
                }
            }
        }

        [Test]
        public void TestDisplayHelpWithCollectionArgumentTypes()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path.ToString()))
                {
                    OptionsParser.DisplayHelp(writer, typeof(HelpOptionsWithCollections));
                }

                using (var reader = new StreamReader(tempFile.Path.ToString()))
                {
                    Assert.That(reader.ReadLine(), Is.EqualTo(""));
                    Assert.That(reader.ReadLine(), Is.EqualTo("Options:"));

                    Assert.That(reader.ReadLine(), Is.EqualTo(string.Format("{0}{1}", "  --string-array=<value,value,..>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptionsWithCollections.HelpText)));
                    Assert.That(reader.ReadLine(), Is.EqualTo(string.Format("{0}{1}", "  --string-list=<custom,custom,..>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptionsWithCollections.HelpText)));
                }
            }
        }

        [Test]
        public void TestHelpRequested_WhenRequested()
        {
            Assert.That(OptionsParser.HelpRequested(new[] { "--help" }), Is.True);
            Assert.That(OptionsParser.HelpRequested(new[] { "--h" }), Is.True);
            Assert.That(OptionsParser.HelpRequested(new[] { "--other", "--help" }), Is.True);
        }

        [Test]
        public void TestHelpRequested_WhenNotRequested()
        {
            Assert.That(OptionsParser.HelpRequested(new[] { "--other", "--other2"}), Is.False);
        }

        [Test]
        public void TestRecreateArgumentsFromCurrentState()
        {
            RecreateOptions.StringValue1 = "Hello";
            RecreateOptions.BoolValue1 = true;
            RecreateOptions.CollectionValue1 = new[] {"Foo", "Bar"};

            var expected = new[] {"--string-value1=Hello", "--bool-value1", "--collection-value1=Foo", "--collection-value1=Bar"};
            var result = OptionsParser.RecreateArgumentsFromCurrentState(typeof(RecreateOptions));
            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void TestRecreateArgumentsFromCurrentStateCanBeFiltered()
        {
            RecreateOptions.StringValue1 = "Hello";
            RecreateOptions.BoolValue1 = true;
            RecreateOptions.CollectionValue1 = new[] {"Foo", "Bar"};

            var expected = new[] {"--string-value1=Hello", "--collection-value1=Foo", "--collection-value1=Bar"};
            var result = OptionsParser.RecreateArgumentsFromCurrentState(typeof(RecreateOptions), (field, value) =>
            {
                if (field.Name == nameof(RecreateOptions.BoolValue1))
                    return false;

                return true;
            });
            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void TestRecreateArgumentsFromCurrentStateDoesNotIncludeConstsAreOptions()
        {
            RecreateOptions2.StringValue1 = "Hello";

            var expected = new[] {"--string-value1=Hello" };
            var result = OptionsParser.RecreateArgumentsFromCurrentState(typeof(RecreateOptions2));
            Assert.That(result, Is.EquivalentTo(expected));
        }
    }
}
