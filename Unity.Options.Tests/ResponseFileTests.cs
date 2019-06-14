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

                Assert.AreEqual(10, SimpleOptions.Value);
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

                Assert.AreEqual(10, SimpleOptions.Value);
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

        static void VerifyResponseFileOptions()
        {
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
            Assert.AreEqual("value with spaces", BasicTypesOptions.StringValueWithSpaces);
            Assert.AreEqual("valuewithoutspaces", BasicTypesOptions.StringValueNoSpaces);
        }
    }
}
