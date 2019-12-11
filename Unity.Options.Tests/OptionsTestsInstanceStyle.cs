using System;
using System.IO;
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

            Assert.AreEqual(true, instance.BoolValue);
        }

        [Test]
        public void CanParseStringOption()
        {
            var commandLine = new[] { "--string-value=foo" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.AreEqual("foo", instance.StringValue);
        }

        [Test]
        public void CanParseEnumOption()
        {
            var commandLine = new[] { "--enum-value=Second" };

            var instance = new InstanceOptions();

            OptionsParser.PrepareInstances(commandLine, new[] {instance});

            Assert.AreEqual(Values.Second, instance.EnumValue);
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
                    Assert.AreEqual("", reader.ReadLine());
                    Assert.AreEqual("Options:", reader.ReadLine());

                    Assert.AreEqual(string.Format("{0}{1}", "  --option-one=<value>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.OptionOneHelpText), reader.ReadLine());
                    Assert.AreEqual(string.Format("{0}{1}", "  --option-two".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.OptionTwoHelpText), reader.ReadLine());
                    Assert.AreEqual(string.Format("{0}{1}", "  --custom-value-description=<path>".PadRight(OptionsParser.HelpOutputColumnPadding), HelpOptions.CustomValueDescriptionHelpText), reader.ReadLine());
                }
            }
        }
    }
}
