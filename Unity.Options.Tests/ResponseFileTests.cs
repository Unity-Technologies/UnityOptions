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
                File.WriteAllLines(tempFile.Path, new []
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
                File.WriteAllLines(tempFile.Path, new []
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
    }
}