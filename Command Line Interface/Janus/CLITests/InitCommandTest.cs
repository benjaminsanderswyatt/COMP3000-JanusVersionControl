using Janus;
using Janus.Models;
using Janus.Plugins;
using Moq;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class InitCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private InitCommand _initCommand;

        private string _testDir;

        [SetUp]
        public void Setup()
        {
            // Mock the ILogger
            _loggerMock = new Mock<ILogger>();

            // Set the base directory path for testing
            _testDir = Path.Combine(Path.GetTempPath(), "JanusTest"); // Using temp directory for testing
            Directory.CreateDirectory(_testDir);
            _paths = new Paths(_testDir);

            Directory.SetCurrentDirectory(_testDir);

            // Create InitCommand instance
            _initCommand = new InitCommand(_loggerMock.Object, _paths);
        }


        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            Directory.SetCurrentDirectory(Path.GetTempPath()); // Cant delete dir if in it

            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }



        [Test]
        public void ShouldInitializeRepository_WhenNotAlreadyInitialized()
        {
            // Arrange: Ensure the directories and files do not exist initially
            if (Directory.Exists(_paths.JanusDir))
            {
                Directory.Delete(_paths.JanusDir, true);
            }

            // Act
            _initCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Initialized janus repository"), Times.Once);
            Assert.True(Directory.Exists(_paths.JanusDir));
            Assert.True(Directory.Exists(_paths.ObjectDir));
            Assert.True(Directory.Exists(_paths.RefsDir));
            Assert.True(Directory.Exists(_paths.HeadsDir));
            Assert.True(Directory.Exists(_paths.PluginsDir));
            Assert.True(File.Exists(_paths.Index));
            Assert.True(File.Exists(_paths.HEAD));

            // Check the HEAD file contents
            string headContents = File.ReadAllText(_paths.HEAD);
            Assert.That(headContents, Is.EqualTo("ref: refs/heads/main"), "HEAD file should point to refs/heads/main initially.");

            // Get the commit object and verify its contents
            string[] commitPathsInFolder = Directory.GetFiles(_paths.CommitDir)
                          .OrderBy(file => new FileInfo(file).LastWriteTime) // Sort by last modified date
                          .ToArray();

            Assert.That(commitPathsInFolder.Length, Is.EqualTo(1), "Should have initial commit object inside commit dir.");

            var initialCommit = File.ReadAllText(commitPathsInFolder[0]);
            CommitMetadata initialCommitData = JsonSerializer.Deserialize<CommitMetadata>(initialCommit);
            Console.WriteLine(initialCommit);

            Assert.That(initialCommitData.Parent, Is.Null);

            Assert.That(initialCommitData.Message, Is.EqualTo("Initial commit"), "Commit message should be 'Initial commit'.");

            Assert.That(initialCommitData.Files.Count, Is.EqualTo(0), "Initial commit has no files.");


            // Check the refs/heads/main file contents
            string mainRefContents = File.ReadAllText(Path.Combine(_paths.HeadsDir, "main"));
            Assert.That(mainRefContents, Is.EqualTo(initialCommitData.Commit), "refs/heads/main file should be the initial commit hash.");

        }



        [Test]
        public void ShouldLogRepositoryAlreadyInitialized_WhenAlreadyInitialized()
        {
            // Arrange: Initialize the repository for this test
            _initCommand.Execute(new string[0]);

            // Act
            _initCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Repository already initialized"), Times.Once);
        }


    }
}