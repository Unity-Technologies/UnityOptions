using NUnit.Framework;

namespace Unity.Options.Tests
{
    [TestFixture]
    public class OptionsFormatterTests
    {
        [Test]
        public void NameOfBoolOption()
        {
            var result = OptionsFormatter.NameFor<BasicTypesOptions>(nameof(BasicTypesOptions.BoolValue));
            Assert.That(result, Is.EqualTo("--bool-value"));
        }

        [Test]
        public void NameOfStringOption()
        {
            var result = OptionsFormatter.NameFor<BasicTypesOptions>(nameof(BasicTypesOptions.StringValueNoSpaces));
            Assert.That(result, Is.EqualTo("--string-value-no-spaces"));
        }

        [Test]
        public void FormatWithValueOfStringValue()
        {
            var result = OptionsFormatter.FormatWithValue<BasicTypesOptions>(nameof(BasicTypesOptions.StringValueNoSpaces), "foo");
            Assert.That(result, Is.EqualTo("--string-value-no-spaces=foo"));
        }

        [Test]
        public void FormatWithValueOBoolValueAndTrue()
        {
            var result = OptionsFormatter.FormatWithValue<BasicTypesOptions>(nameof(BasicTypesOptions.BoolValue), true);
            Assert.That(result, Is.EqualTo("--bool-value"));
        }

        [Test]
        public void FormatWithValueOBoolValueAndFalse()
        {
            var result = OptionsFormatter.FormatWithValue<BasicTypesOptions>(nameof(BasicTypesOptions.BoolValue), false);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void FormatWithValueOfArrayValue()
        {
            var result = OptionsFormatter.FormatWithValue<ArrayOptions>(nameof(ArrayOptions.IntArray), new[] {1, 2, 3});
            Assert.That(result, Is.EqualTo("--int-array=1,2,3"));
        }

        [Test]
        public void FormatWithValueOfArrayValueSingleValue()
        {
            var result = OptionsFormatter.FormatWithValue<ArrayOptions>(nameof(ArrayOptions.IntArray), new[] {1});
            Assert.That(result, Is.EqualTo("--int-array=1"));
        }

        [Test]
        public void FormatWithValueOfArrayValueEmpty()
        {
            var result = OptionsFormatter.FormatWithValue<ArrayOptions>(nameof(ArrayOptions.IntArray), new object[0]);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void FormatWithValueOfFlagsEnumSingle()
        {
            var result = OptionsFormatter.FormatWithValue<FlagsEnumOptions>(nameof(FlagsEnumOptions.Flags), FlagValues.Two);
            Assert.That(result, Is.EqualTo("--flags=Two"));
        }

        [Test]
        public void FormatWithValueOfFlagsEnum()
        {
            var result = OptionsFormatter.FormatWithValue<FlagsEnumOptions>(nameof(FlagsEnumOptions.Flags), FlagValues.One | FlagValues.Three);
            Assert.That(result, Is.EqualTo("--flags=One,Three"));
        }

        [Test]
        public void FormatWithValueOfEnum()
        {
            var result = OptionsFormatter.FormatWithValue<EnumOptions>(nameof(EnumOptions.EnumValue), Values.First);
            Assert.That(result, Is.EqualTo("--enum-value=First"));
        }
    }
}
