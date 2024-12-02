using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Unity.Options.Tests
{
    [TestFixture]
    public class OptionsTestsInstanceStyle
    {
        [Test]
        public void CanParseBoolOption()
        {
            var commandLine = new[] { "--bool-value" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.BoolValue, Is.EqualTo(true));
        }

        [Test]
        public void CanParseStringOption()
        {
            var commandLine = new[] { "--string-value=foo" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.StringValue, Is.EqualTo("foo"));
        }

        [Test]
        public void CanParseEnumOption()
        {
            var commandLine = new[] { "--enum-value=Second" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.EnumValue, Is.EqualTo(Values.Second));
        }

        [Test]
        public void CanParseEnumFlagsOption()
        {
            var commandLine = new[] { "--flag-enum-value=One,Three" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.FlagEnumValue, Is.EqualTo(FlagValues.One | FlagValues.Three));
        }

        [Test]
        public void CanParseArrayOption()
        {
            var commandLine = new[] { "--array-value=foo,bar" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.ArrayValue, Is.Not.Null);
            Assert.That(instance.ArrayValue, Is.EquivalentTo(new[] {"foo", "bar"}));
        }
        
        [Test]
        public void CanParseArrayOptionWithCustomCollectionValueParser()
        {
            var commandLine = new[] { "--array-value=foo,bar" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine,  new[] {instance}, customCollectionSplitter: CustomCollectionSplitter);

            Assert.That(instance.ArrayValue, Is.Not.Null);
            Assert.That(instance.ArrayValue, Is.EquivalentTo(new[]
            {
                "f", "o", "o", ",", "b", "a", "r"
            }));
        }

        private string[] CustomCollectionSplitter(FieldInfo field, string value)
        {
            return value.ToCharArray().Select(v => $"{v}").ToArray();
        }

        [Test]
        public void CanParseArrayOption2()
        {
            var commandLine = new[] { "--array-value=foo", "--array-value=bar" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.ArrayValue, Is.Not.Null);
            Assert.That(instance.ArrayValue, Is.EquivalentTo(new[] {"foo", "bar"}));
        }

        [Test]
        public void CanParseListOption()
        {
            var commandLine = new[] { "--list-value=foo,bar" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(instance.ListValue, Is.Not.Null);
            Assert.That(instance.ListValue, Is.EquivalentTo(new[] {"foo", "bar"}));
        }

        [Test]
        public void StaticFieldWillNotBeSet()
        {
            var commandLine = new[] { "--static-string-value=foo" };

            var instance = new InstanceOptionsWithStaticField();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.That(InstanceOptionsWithStaticField.StaticStringValue, Is.Null);
        }

        [Test]
        public void MultipleOptionInstances()
        {
            var commandLine = new[] { "--value3=foo", "--value2=bar", "--value1=jar" };

            var instance1 = new InstanceMultiple1();
            var instance2 = new InstanceMultiple2();
            var instance3 = new InstanceMultiple3();

            OptionsParser.PrepareInstances(commandLine, new object[] {instance2, instance3, instance1});

            Assert.That(instance1.Value1, Is.EqualTo("jar"));
            Assert.That(instance2.Value2, Is.EqualTo("bar"));
            Assert.That(instance3.Value3, Is.EqualTo("foo"));
        }

        [Test]
        public void MultipleOptionInstancesOfSameTypeThrows()
        {
            var commandLine = new[] { "--value1=jar" };

            var instance1 = new InstanceMultiple1();
            var instance2 = new InstanceMultiple1();

            Assert.Throws<ArgumentException>(() => OptionsParser.PrepareInstances(commandLine, new object[] {instance2, instance1}));
        }

        [Test]
        public void TestRecreateArgumentsFromCurrentState()
        {
            var instance = new InstanceRecreateOptions();
            instance.StringValue1 = "Hello";
            instance.BoolValue1 = true;
            instance.CollectionValue1 = new[] {"Foo", "Bar"};

            var expected = new[] {"--string-value1=Hello", "--bool-value1", "--collection-value1=Foo", "--collection-value1=Bar"};
            var result = OptionsParser.RecreateArgumentsFromCurrentState(instance);
            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void TestDisplayHelpWithSimpleArgumentTypesInstanceMode()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path.ToString()))
                {
                    OptionsParser.DisplayHelp(writer, typeof(InstanceHelpOptions));
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
        public void TestDisplayHelpWithSimpleArgumentTypesInstanceModeAndCustomHelpTypes()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path.ToString()))
                {
                    OptionsParser.DisplayHelp<CustomHelpDetailsAttribute, CustomHideFromHelpAttribute>(writer, new[] {new InstanceHelpOptionsWithCustomHelpAttributeTypes()}, attr => attr.Summary, attr => attr.CustomValueDescription);
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
        public void CanParseOptionUsingItsCAliasCustom()
        {
            var commandLine = new[]
            {
                "--alias=hello"
            };
            var instance = new OptionsWithAliasesCustom();
            OptionsParser.PrepareInstances<CustomOptionAliasAttribute>(commandLine, new[] {instance}, attr => attr.Name);

            Assert.That(instance.OptionWithAlias, Is.EqualTo("hello"));
        }

        [Test]
        public void CanParseOptionWithMultiAliasesUsingItsAliasCustom()
        {
            var commandLine = new[]
            {
                "--multi-alias4=hello"
            };
            var instance = new OptionsWithAliasesCustom();
            OptionsParser.PrepareInstances<CustomOptionAliasAttribute>(commandLine, new[] {instance}, attr => attr.Name);

            Assert.That(instance.OptionWithMultipleAliases, Is.EqualTo("hello"));
        }

        [Test]
        public void CanParseBoolOptionUsingItsAliasCustom()
        {
            var commandLine = new[]
            {
                "--enable-alias"
            };
            var instance = new OptionsWithAliasesCustom();
            OptionsParser.PrepareInstances<CustomOptionAliasAttribute>(commandLine, new[] {instance}, attr => attr.Name);

            Assert.That(instance.BoolOptionWithAlias, Is.EqualTo(true));
        }
    }
}
