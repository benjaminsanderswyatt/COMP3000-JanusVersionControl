using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Moq;
using NuGet.Frameworks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class StatusCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private AddCommand _addCommand;
        private CommitCommand _commitCommand;
        private StatusCommand _statusCommand;

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

            // Initialize the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);


            // Create Command instances
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
            _commitCommand = new CommitCommand(_loggerMock.Object, _paths);
            _statusCommand = new StatusCommand(_loggerMock.Object, _paths);
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
        public void ShouldLogError_WhenRepositoryIsNotInitialized()
        {
            // Arrange: Delete the repository
            Directory.Delete(_paths.JanusDir, true);

            // Act: Execute 'janus status'
            _statusCommand.Execute(new string[0]);

            // Assert: Verify that the error about repository initialization is logged
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise repository."), Times.Once);

        }



        [Test]
        public void ShouldDisplayCleanRepo_WhenNoChangesExist()
        {
            // Arrange

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("All clean."), Times.Once);
        }


        [Test]
        public void ShouldDisplayChangesToBeCommit()
        {
            // Arrange: Create and stage a file
            string filePath = Path.Combine(_testDir, "testFile.txt");
            File.WriteAllText(filePath, "Test content");

            _addCommand.Execute(new[] { "testFile.txt" });

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Changes to be committed:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("    testFile.txt (added)"), Times.Once);
        }


        [Test]
        public void ShouldDisplayChangesNotStagedForCommit()
        {

        }


        [Test]
        public void ShouldDisplayUntrackedFiles_WhenFilesAreUntracked() {

        }


        [Test]
        public void ShouldIgnoreFilesInTheIgnoreFile()
        {

        }


        [Test]
        public void ShouldDisplayCurrentBranch()
        {
            
        }


        [Test]
        public void ShouldDisplaySyncStatus()
        {

        }

    }
}