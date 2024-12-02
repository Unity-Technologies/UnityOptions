using System;
using System.IO;
using NUnit.Framework;

namespace Unity.Options.Tests
{
    [TestFixture]
    public class ResponseFileTests
    {
        [Test]
        public void SimpleResponseFileWorks()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                File.WriteAllLines(tempFile.Path, new[]
                {
                    "--value=10"
                });

                var commandLine = new[]
                {
                    $"@{tempFile.Path}"
                };

                var types = new[] { typeof(SimpleOptions) };

                OptionsParser.Prepare(commandLine, types);

                Assert.That(SimpleOptions.Value, Is.EqualTo(10));
            }
        }

        [Test]
        public void SimpleRelativePathResponseFileWorks()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                File.WriteAllLines(tempFile.Path, new[]
                {
                    "--value=10"
                });

                var commandLine = new[]
                {
                    $"@{Path.GetFileName(tempFile.Path)}"
                };

                var types = new[] { typeof(SimpleOptions) };

                OptionsParser.Prepare(commandLine, types, currentDirectory: Path.GetDirectoryName(tempFile.Path));

                Assert.That(SimpleOptions.Value, Is.EqualTo(10));
            }
        }

        [Test]
        public void ResponseFileThatDoesNotExist()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                var commandLine = new[]
                {
                    $"@{tempFile.Path}"
                };

                var types = new[] { typeof(SimpleOptions) };

                Assert.Throws<FileNotFoundException>(() => OptionsParser.Prepare(commandLine, types));
            }
        }

        [Test]
        public void VerifyOptionsFromResponseFileMultiLine()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path))
                {
                    writer.WriteLine("--char-value=1");
                    writer.WriteLine("--byte-value=2");
                    writer.WriteLine("--s-byte-value=3");
                    writer.WriteLine("--short-value=4");
                    writer.WriteLine("--u-short-value=5");
                    writer.WriteLine("--int-value=6");
                    writer.WriteLine("--u-int-value=7");
                    writer.WriteLine("--long-value=8");
                    writer.WriteLine("--u-long-value=9");
                    writer.WriteLine("--float-value=10.42");
                    writer.WriteLine("--double-value=11.33");
                    writer.WriteLine("--bool-value");
                    writer.WriteLine("--string-value-with-spaces=\"value with spaces\"");
                    writer.WriteLine("--string-value-no-spaces=valuewithoutspaces");
                }

                var commandLine = new[]
                {
                    $"@\"{tempFile.Path}\""
                };

                BasicTypesOptions.SetupDefaults();
                var types = new[] { typeof(BasicTypesOptions) };

                OptionsParser.Prepare(commandLine, types);
                VerifyResponseFileOptions();
            }
        }

        [Test]
        public void VerifyOptionsFromResponseFileSingleLine()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path))
                {
                    writer.Write("--char-value=1");
                    writer.Write(" ");
                    writer.Write("--byte-value=2");
                    writer.Write(" ");
                    writer.Write("--s-byte-value=3");
                    writer.Write(" ");
                    writer.Write("--short-value=4");
                    writer.Write(" ");
                    writer.Write("--u-short-value=5");
                    writer.Write(" ");
                    writer.Write("--int-value=6");
                    writer.Write(" ");
                    writer.Write("--u-int-value=7");
                    writer.Write(" ");
                    writer.Write("--long-value=8");
                    writer.Write(" ");
                    writer.Write("--u-long-value=9");
                    writer.Write(" ");
                    writer.Write("--float-value=10.42");
                    writer.Write(" ");
                    writer.Write("--double-value=11.33");
                    writer.Write(" ");
                    writer.Write("--bool-value");
                    writer.Write(" ");
                    writer.Write("--string-value-with-spaces=\"value with spaces\"");
                    writer.Write(" ");
                    writer.Write("--string-value-no-spaces=valuewithoutspaces");
                }

                var commandLine = new[]
                {
                    $"@\"{tempFile.Path}\""
                };

                BasicTypesOptions.SetupDefaults();
                var types = new[] { typeof(BasicTypesOptions) };

                OptionsParser.Prepare(commandLine, types);
                VerifyResponseFileOptions();
            }
        }

        [Test]
        public void VerifyOptionsFromResponseFileMixedLines()
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                using (var writer = new StreamWriter(tempFile.Path))
                {
                    writer.Write("--char-value=1");
                    writer.Write(" ");
                    writer.Write("--byte-value=2");
                    writer.Write(" ");
                    writer.Write("--s-byte-value=3");
                    writer.Write(" ");
                    writer.Write("--short-value=4");
                    writer.WriteLine();
                    writer.Write("--u-short-value=5");
                    writer.Write(" ");
                    writer.Write("--int-value=6");
                    writer.Write(" ");
                    writer.Write("--u-int-value=7");
                    writer.WriteLine();
                    writer.Write("--long-value=8");
                    writer.Write(" ");
                    writer.Write("--u-long-value=9");
                    writer.Write(" ");
                    writer.Write("--float-value=10.42");
                    writer.Write(" ");
                    writer.Write("--double-value=11.33");
                    writer.WriteLine();
                    writer.Write("--bool-value");
                    writer.Write(" ");
                    writer.Write("--string-value-with-spaces=\"value with spaces\"");
                    writer.Write(" ");
                    writer.Write("--string-value-no-spaces=valuewithoutspaces");
                }

                var commandLine = new[]
                {
                    $"@\"{tempFile.Path}\""
                };

                BasicTypesOptions.SetupDefaults();
                var types = new[] { typeof(BasicTypesOptions) };

                OptionsParser.Prepare(commandLine, types);
                VerifyResponseFileOptions();
            }
        }

        [Test]
        [TestCase("--value=\"quotes_at_start_and_end\"", "quotes_at_start_and_end")]
        [TestCase("--value=entree_\"quotes_from_middle_to_end\"", "entree_quotes_from_middle_to_end")]
        [TestCase("--value=entree_\"quotes_from_middle_to_middle\"-postfix", "entree_quotes_from_middle_to_middle-postfix")]
        
        //you could argue this is not amazing behaviour, but at least this test documents current behaviour:
        [TestCase("--value=\\\"quoted_string_at_start_and_end\\\"", "quoted_string_at_start_and_end")]
        [TestCase("--value=entree_\\\"quoted_string_at_middle_to_end\\\"", "entree_\"quoted_string_at_middle_to_end\"")]
        public void ResponseFilesWithQuotes(string argument, string expectation)
        {
            using (var tempFile = TempFile.CreateRandom())
            {
                File.WriteAllLines(tempFile.Path, new[] {argument});
                OptionsParser.Prepare(new[] {$"@{tempFile.Path}"}, new[] { typeof(StringOptions) });
                Assert.That(StringOptions.Value, Is.EqualTo(expectation));
            }
        }
        
        static void VerifyResponseFileOptions()
        {
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
            Assert.That(BasicTypesOptions.StringValueWithSpaces, Is.EqualTo("value with spaces"));
            Assert.That(BasicTypesOptions.StringValueNoSpaces, Is.EqualTo("valuewithoutspaces"));
        }
    }
}
