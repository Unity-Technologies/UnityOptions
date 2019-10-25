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

            Assert.AreEqual(10, SimpleOptions.Value);
        }

        [Test]
        public void CanParseSimpleOptionUsingOptionAttributeOnPrivateStaticField()
        {
            var commandLine = new[] {"--value-private=15"};
            var types = new[] {typeof(SimpleOptionsUsingNonPublicFields)};

            OptionsParser.Prepare(commandLine, types);

            Assert.AreEqual(15, SimpleOptionsUsingNonPublicFields.GetValuePrivate);
        }

        [Test]
        public void CanParseSimpleOptionUsingOptionAttributeOnInternalStaticField()
        {
            var commandLine = new[] { "--value-internal=15" };
            var types = new[] { typeof(SimpleOptionsUsingNonPublicFields) };

            OptionsParser.Prepare(commandLine, types);

            Assert.AreEqual(15, SimpleOptionsUsingNonPublicFields.ValueInternal);
        }

        [Test]
        public void CanParseMultipleSimpleOptions()
        {
            var commandLine = new[] { "--value=10", "--second-value=11" };
            var types = new[] { typeof(MultipleSimpleOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.AreEqual(10, MultipleSimpleOptions.Value);
            Assert.AreEqual(11, MultipleSimpleOptions.SecondValue);
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

            Assert.AreEqual(10, MultipleSimpleOptions.Value);
            Assert.AreEqual(11, MultipleSimpleOptions.SecondValue);
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

            Assert.AreEqual(10, CustomGroupOptions.Value);
            Assert.AreEqual(11, CustomGroupOptions.SecondValue);
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
                "--string-value-no-spaces=gabrielefarina"
            };
            var types = new[] { typeof(BasicTypesOptions) };

            OptionsParser.Prepare(commandLine, types);

            Assert.AreEqual('1', BasicTypesOptions.CharValue);
            Assert.AreEqual(2, BasicTypesOptions.ByteValue);
            Assert.AreEqual(3, BasicTypesOptions.SByteValue);
            Assert.AreEqual(4, BasicTypesOptions.ShortValue);
            Assert.AreEqual(5, BasicTypesOptions.UShortValue);
            Assert.AreEqual(6, BasicTypesOptions.IntValue);
            Assert.AreEqual(7, BasicTypesOptions.UIntValue);
            Assert.AreEqual(8, BasicTypesOptions.LongValue);
            Assert.AreEqual(9, BasicTypesOptions.ULongValue);
            Assert.AreEqual(10.42f, BasicTypesOptions.FloatValue);
            Assert.AreEqual(11.33, BasicTypesOptions.DoubleValue);
            Assert.AreEqual(true, BasicTypesOptions.BoolValue);
            Assert.AreEqual("gabriele farina", BasicTypesOptions.StringValueWithSpaces);
            Assert.AreEqual("gabrielefarina", BasicTypesOptions.StringValueNoSpaces);
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

                Assert.AreEqual(10.42f, BasicTypesOptions.FloatValue);
                Assert.AreEqual(11.33, BasicTypesOptions.DoubleValue);
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

            Assert.AreEqual(3, ArrayOptions.IntArray.Length);
            Assert.AreEqual(1, ArrayOptions.IntArray[0]);
            Assert.AreEqual(2, ArrayOptions.IntArray[1]);
            Assert.AreEqual(3, ArrayOptions.IntArray[2]);
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

            Assert.AreEqual(3, CustomArrayOptions.IntArray.Length);
            Assert.AreEqual(1, CustomArrayOptions.IntArray[0]);
            Assert.AreEqual(2, CustomArrayOptions.IntArray[1]);
            Assert.AreEqual(3, CustomArrayOptions.IntArray[2]);
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

            Assert.AreEqual(3, ArrayRepetitionOptions.IntArray.Length);
            Assert.AreEqual(1, ArrayRepetitionOptions.IntArray[0]);
            Assert.AreEqual(2, ArrayRepetitionOptions.IntArray[1]);
            Assert.AreEqual(3, ArrayRepetitionOptions.IntArray[2]);
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

            Assert.AreEqual(2, ArrayBasicTypesOptions.CharValue.Length);
            Assert.AreEqual('1', ArrayBasicTypesOptions.CharValue[0]);
            Assert.AreEqual('a', ArrayBasicTypesOptions.CharValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.ByteValue.Length);
            Assert.AreEqual(2, ArrayBasicTypesOptions.ByteValue[0]);
            Assert.AreEqual(22, ArrayBasicTypesOptions.ByteValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.SByteValue.Length);
            Assert.AreEqual(3, ArrayBasicTypesOptions.SByteValue[0]);
            Assert.AreEqual(33, ArrayBasicTypesOptions.SByteValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.ShortValue.Length);
            Assert.AreEqual(4, ArrayBasicTypesOptions.ShortValue[0]);
            Assert.AreEqual(44, ArrayBasicTypesOptions.ShortValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.UShortValue.Length);
            Assert.AreEqual(5, ArrayBasicTypesOptions.UShortValue[0]);
            Assert.AreEqual(55, ArrayBasicTypesOptions.UShortValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.IntValue.Length);
            Assert.AreEqual(6, ArrayBasicTypesOptions.IntValue[0]);
            Assert.AreEqual(66, ArrayBasicTypesOptions.IntValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.UIntValue.Length);
            Assert.AreEqual(7, ArrayBasicTypesOptions.UIntValue[0]);
            Assert.AreEqual(77, ArrayBasicTypesOptions.UIntValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.LongValue.Length);
            Assert.AreEqual(8, ArrayBasicTypesOptions.LongValue[0]);
            Assert.AreEqual(88, ArrayBasicTypesOptions.LongValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.ULongValue.Length);
            Assert.AreEqual(9, ArrayBasicTypesOptions.ULongValue[0]);
            Assert.AreEqual(99, ArrayBasicTypesOptions.ULongValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.FloatValue.Length);
            Assert.AreEqual(10.42f, ArrayBasicTypesOptions.FloatValue[0]);
            Assert.AreEqual(1042.1042f, ArrayBasicTypesOptions.FloatValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.DoubleValue.Length);
            Assert.AreEqual(11.33, ArrayBasicTypesOptions.DoubleValue[0]);
            Assert.AreEqual(1133.1133, ArrayBasicTypesOptions.DoubleValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.BoolValue.Length);
            Assert.AreEqual(false, ArrayBasicTypesOptions.BoolValue[0]);
            Assert.AreEqual(true, ArrayBasicTypesOptions.BoolValue[1]);

            Assert.AreEqual(2, ArrayBasicTypesOptions.StringValue.Length);
            Assert.AreEqual("gabriele farina", ArrayBasicTypesOptions.StringValue[0]);
            Assert.AreEqual("ralph hauwert", ArrayBasicTypesOptions.StringValue[1]);
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

            Assert.AreEqual(3, ListOptions.IntList.Count);
            Assert.AreEqual(1, ListOptions.IntList[0]);
            Assert.AreEqual(2, ListOptions.IntList[1]);
            Assert.AreEqual(3, ListOptions.IntList[2]);
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

            Assert.AreEqual(3, CustomListOptions.IntList.Count);
            Assert.AreEqual(1, CustomListOptions.IntList[0]);
            Assert.AreEqual(2, CustomListOptions.IntList[1]);
            Assert.AreEqual(3, CustomListOptions.IntList[2]);
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

            Assert.AreEqual(3, ListRepetitionOptions.IntList.Count);
            Assert.AreEqual(1, ListRepetitionOptions.IntList[0]);
            Assert.AreEqual(2, ListRepetitionOptions.IntList[1]);
            Assert.AreEqual(3, ListRepetitionOptions.IntList[2]);
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

            Assert.AreEqual(2, ListBasicTypesOptions.CharList.Count);
            Assert.AreEqual('1', ListBasicTypesOptions.CharList[0]);
            Assert.AreEqual('a', ListBasicTypesOptions.CharList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.ByteList.Count);
            Assert.AreEqual(2, ListBasicTypesOptions.ByteList[0]);
            Assert.AreEqual(22, ListBasicTypesOptions.ByteList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.SByteList.Count);
            Assert.AreEqual(3, ListBasicTypesOptions.SByteList[0]);
            Assert.AreEqual(33, ListBasicTypesOptions.SByteList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.ShortList.Count);
            Assert.AreEqual(4, ListBasicTypesOptions.ShortList[0]);
            Assert.AreEqual(44, ListBasicTypesOptions.ShortList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.UShortList.Count);
            Assert.AreEqual(5, ListBasicTypesOptions.UShortList[0]);
            Assert.AreEqual(55, ListBasicTypesOptions.UShortList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.IntList.Count);
            Assert.AreEqual(6, ListBasicTypesOptions.IntList[0]);
            Assert.AreEqual(66, ListBasicTypesOptions.IntList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.UIntList.Count);
            Assert.AreEqual(7, ListBasicTypesOptions.UIntList[0]);
            Assert.AreEqual(77, ListBasicTypesOptions.UIntList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.LongList.Count);
            Assert.AreEqual(8, ListBasicTypesOptions.LongList[0]);
            Assert.AreEqual(88, ListBasicTypesOptions.LongList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.ULongList.Count);
            Assert.AreEqual(9, ListBasicTypesOptions.ULongList[0]);
            Assert.AreEqual(99, ListBasicTypesOptions.ULongList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.FloatList.Count);
            Assert.AreEqual(10.42f, ListBasicTypesOptions.FloatList[0]);
            Assert.AreEqual(1042.1042f, ListBasicTypesOptions.FloatList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.DoubleList.Count);
            Assert.AreEqual(11.33, ListBasicTypesOptions.DoubleList[0]);
            Assert.AreEqual(1133.1133, ListBasicTypesOptions.DoubleList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.BoolList.Count);
            Assert.AreEqual(false, ListBasicTypesOptions.BoolList[0]);
            Assert.AreEqual(true, ListBasicTypesOptions.BoolList[1]);

            Assert.AreEqual(2, ListBasicTypesOptions.StringList.Count);
            Assert.AreEqual("gabriele farina", ListBasicTypesOptions.StringList[0]);
            Assert.AreEqual("ralph hauwert", ListBasicTypesOptions.StringList[1]);
        }

        [Test]
        public void CanPrepareFromAssemblyDefinition()
        {
            var commandLine = new[] { "--value=10" };

            OptionsParser.Prepare(commandLine, typeof(ExternalOptions).Assembly);

            Assert.AreEqual(10, ExternalOptions.Value);
        }

        [Test]
        public void CanPrepareFromFile()
        {
            using (var testFile = TempFile.CreateRandom())
            {
                OptionsHelper.WriteArgumentsFile(testFile.Path.ToString(),
                    new[] { "--from-file-value-one=10", "--from-file-value-two=15" });

                OptionsParser.PrepareFromFile(testFile.Path.ToString(), new Type[] { typeof(FromFileOptions) });

                Assert.AreEqual(10, FromFileOptions.FromFileValueOne);
                Assert.AreEqual(15, FromFileOptions.FromFileValueTwo);
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

            Assert.AreEqual(10, ExternalOptions.Value);
            Assert.AreEqual("gabriele", OtherOptions.Name);
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

            Assert.AreEqual(Values.First, EnumOptions.EnumValue);
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

            Assert.AreEqual(2, EnumArrayOptions.EnumArrayValue.Length);
            Assert.AreEqual(Values.First, EnumArrayOptions.EnumArrayValue[0]);
            Assert.AreEqual(Values.Second, EnumArrayOptions.EnumArrayValue[1]);
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

            Assert.AreEqual(FlagValues.One | FlagValues.Three, FlagsEnumOptions.Flags);
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

            Assert.AreEqual(true, BoolOptions.BoolValue);
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

            Assert.AreEqual("hello", OptionsWithAliases.OptionWithAlias);
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

            Assert.AreEqual("hello", OptionsWithAliases.OptionWithMultipleAliases);
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

            Assert.AreEqual(true, OptionsWithAliases.BoolOptionWithAlias);
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

            Assert.AreEqual("foo", OptionsWithCustomParsers.Arg1.Value);
            Assert.AreEqual("bar", OptionsWithCustomParsers.Arg2.Value);
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
            Assert.AreEqual("--string-value-with-spaces", OptionsParser.OptionNameFor(typeof(BasicTypesOptions), nameof(BasicTypesOptions.StringValueWithSpaces)));
        }

        [Test]
        public void OptionNameForOnBoolField()
        {
            Assert.AreEqual("--bool-value", OptionsParser.OptionNameFor(typeof(BasicTypesOptions), nameof(BasicTypesOptions.BoolValue)));
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
            Assert.IsTrue(result.TryGetValue("--option-one", out resultEntry));
            Assert.AreEqual(HelpOptions.OptionOneHelpText, resultEntry.Summary);
            Assert.AreEqual(typeof(string), resultEntry.FieldInfo.FieldType);
            Assert.IsFalse(resultEntry.HasCustomValueDescription);

            Assert.IsTrue(result.TryGetValue("--option-two", out resultEntry));
            Assert.AreEqual(HelpOptions.OptionTwoHelpText, resultEntry.Summary);
            Assert.AreEqual(typeof(bool), resultEntry.FieldInfo.FieldType);
            Assert.IsFalse(resultEntry.HasCustomValueDescription);

            Assert.IsTrue(result.TryGetValue("--option-missing-help-details", out resultEntry));
            Assert.IsFalse(result["--option-missing-help-details"].HasSummary);
            Assert.IsNotNull(result["--option-missing-help-details"].FieldInfo);
            Assert.IsFalse(resultEntry.HasCustomValueDescription);

            Assert.IsTrue(result.TryGetValue("--custom-value-description", out resultEntry));
            Assert.IsTrue(resultEntry.HasSummary);
            Assert.AreEqual(typeof(string), resultEntry.FieldInfo.FieldType);
            Assert.AreEqual("path", resultEntry.CustomValueDescription);
            Assert.IsTrue(resultEntry.HasCustomValueDescription);

            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void TestHideFromHelp()
        {
            var result = OptionsParser.ParseHelpTable(typeof(HelpOptions));

            Assert.IsFalse(result.ContainsKey("--option-hidden"));
        }

        [Test]
        public void TestParseHelpTableWhenGivenAnAssembly()
        {
            // Don't be strict with the asserts in this test.  We want to keep it flexible.  Just make sure
            // there are more entries in the result than HelpOptions has on it's own.
            var baseLine = OptionsParser.ParseHelpTable(typeof(OtherOptions));

            var result = OptionsParser.ParseHelpTable(typeof(OtherOptions).Assembly);

            Assert.IsTrue(result.Count > baseLine.Count);
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
                    Assert.AreEqual("", reader.ReadLine());
                    Assert.AreEqual("Options:", reader.ReadLine());

                    Assert.AreEqual(string.Format("{0}{1}", "  --option-one=<value>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.OptionOneHelpText), reader.ReadLine());
                    Assert.AreEqual(string.Format("{0}{1}", "  --option-two".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.OptionTwoHelpText), reader.ReadLine());
                    Assert.AreEqual(string.Format("{0}{1}", "  --custom-value-description=<path>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.CustomValueDescriptionHelpText), reader.ReadLine());
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
                    Assert.AreEqual("", reader.ReadLine());
                    Assert.AreEqual("Options:", reader.ReadLine());

                    Assert.AreEqual(string.Format("{0}{1}", "  --string-array=<value,value,..>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptionsWithCollections.HelpText), reader.ReadLine());
                    Assert.AreEqual(string.Format("{0}{1}", "  --string-list=<custom,custom,..>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptionsWithCollections.HelpText), reader.ReadLine());
                }
            }
        }

        [Test]
        public void TestHelpRequested_WhenRequested()
        {
            Assert.IsTrue(OptionsParser.HelpRequested(new[] { "--help" }));
            Assert.IsTrue(OptionsParser.HelpRequested(new[] { "--h" }));
            Assert.IsTrue(OptionsParser.HelpRequested(new[] { "--other", "--help" }));
        }

        [Test]
        public void TestHelpRequested_WhenNotRequested()
        {
            Assert.IsFalse(OptionsParser.HelpRequested(new[] { "--other", "--other2"}));
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
