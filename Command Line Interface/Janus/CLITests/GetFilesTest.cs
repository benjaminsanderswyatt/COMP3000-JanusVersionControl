using Janus.Helpers;
using Janus.Plugins;

namespace CLITests
{
    [TestFixture]
    public class GetFilesTest
    {
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            // Set up a temporary directory for testing
            _testDir = Path.Combine(Path.GetTempPath(), "JanusTestFiles");
            Directory.CreateDirectory(_testDir);

            // Create sample directory structure and files
            Directory.CreateDirectory(Path.Combine(_testDir, "build"));
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "Test file 1");
            File.WriteAllText(Path.Combine(_testDir, "build/file2.log"), "Test file 2");
            File.WriteAllText(Path.Combine(_testDir, "build/file3.exe"), "Test file 3");
            File.WriteAllText(Path.Combine(_testDir, "important.log"), "Important log file");
            File.WriteAllText(Path.Combine(_testDir, "error.log"), "Error log file");

            // Create a .janusignore file
            File.WriteAllLines(Path.Combine(_testDir, ".janusignore"), new[]
            {
                "*.log",
                "build/**/*",
                "!important.log"
            });
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up temporary test directory
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void GetAllFilesInDir_ShouldReturnNonIgnoredFiles()
        {
            // Act
            var paths = new Paths(_testDir);
            var allFiles = GetFilesHelper.GetAllFilesInDir(paths, _testDir).ToList();

            foreach (var file in allFiles)
            {
                Console.WriteLine(file);
            }

            // Assert
            Assert.That(allFiles.Count, Is.EqualTo(3)); // Only non ignored files -> .janusigore, file1.txt, important.log
            CollectionAssert.Contains(allFiles, "file1.txt");
            CollectionAssert.Contains(allFiles, "important.log");
            CollectionAssert.DoesNotContain(allFiles, "error.log");
            CollectionAssert.DoesNotContain(allFiles, "build/file2.log");
        }
    }
}