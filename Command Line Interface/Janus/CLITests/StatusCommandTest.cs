using Janus.Plugins;
using Moq;
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
        public void ShouldDisplayCleanRepo_WhenWorkingDirEmpty()
        {
            // Arrange

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("All clean."), Times.Once);
        }


        [Test]
        public void ShouldDisplayCleanRepo_WhenNoChangesFromCommit()
        {
            // Arrange: Create and commit a file 
            string filePath = Path.Combine(_testDir, "testFile.txt");
            File.WriteAllText(filePath, "Test content");

            _addCommand.Execute(new[] { "testFile.txt" });
            _commitCommand.Execute(new[] { "Test commit" });

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert: that repo is up to date with the commit
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
            // Arrange: Create, stage, and modify a file
            string filePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(filePath, "Initial content");

            _addCommand.Execute(new[] { "file.txt" });
            _commitCommand.Execute(new[] { "Initial commit" });

            File.WriteAllText(filePath, "Modified content"); // Modify the file

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Changes not staged for commit:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("file.txt "))), Times.Once);
        }

        [Test]
        public void ShouldDisplayChangesNotStagedForCommit_WhenFileInsideFolder()
        {
            // Arrange: Create, stage, and modify a file
            var dir = Path.Combine(_testDir, "dir");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, "file.txt");
            File.WriteAllText(filePath, "Initial content");

            _addCommand.Execute(new[] { "dir/file.txt" });
            _commitCommand.Execute(new[] { "Initial commit" });

            File.WriteAllText(filePath, "Modified content"); // Modify the file

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Changes not staged for commit:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains(@"dir\file.txt "))), Times.Once);
        }



        [Test]
        public void ShouldDisplayUntrackedFiles()
        {
            // Arrange: Create an untracked file
            string untrackedFilePath = Path.Combine(_testDir, "untracked.txt");
            File.WriteAllText(untrackedFilePath, "Test content");

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Untracked files:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("untracked.txt"))), Times.Once);
        }

        [Test]
        public void ShouldDisplayUntrackedFiles_WhenFileInsideFolder()
        {
            // Arrange: Create an untracked file in a folder
            var dir = Path.Combine(_testDir, "dir");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, "untracked.txt");
            File.WriteAllText(filePath, "Test content");

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Untracked files:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains(@"dir\untracked.txt "))), Times.Once);
        }




        [Test]
        public void ShouldDisplayStagedFiles()
        {
            // Create and stage a file
            string filePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(filePath, "Test content");
            _addCommand.Execute(new[] { "file.txt" });

            _statusCommand.Execute(new string[0]);

            _loggerMock.Verify(logger => logger.Log("Changes to be committed:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("file.txt") && msg.Contains("(added)"))), Times.Once);
        }

        [Test]
        public void ShouldDisplayStagedFiles_WhenFileInsideFolder()
        {
            // Create and stage a file
            var dir = Path.Combine(_testDir, "dir");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, "file.txt");
            File.WriteAllText(filePath, "Test content");

            _addCommand.Execute(new[] { "dir/file.txt" });

            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Changes to be committed:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains(@"dir\file.txt") && msg.Contains("(added)"))), Times.Once);
        }



        [Test]
        public void ShouldDisplayAllStatuses()
        {
            // Arrange: Create files for all statuses
            string addedFile = Path.Combine(_testDir, "added.txt");
            string modifiedFile = Path.Combine(_testDir, "modified.txt");
            string notstagedFile = Path.Combine(_testDir, "notstaged.txt");
            string deletedFile = Path.Combine(_testDir, "deleted.txt");
            string untrackedFile = Path.Combine(_testDir, "untracked.txt");

            // Commit files
            File.WriteAllText(modifiedFile, "Initial content");
            _addCommand.Execute(new[] { "modified.txt" });
            File.WriteAllText(deletedFile, "Content to delete");
            _addCommand.Execute(new[] { "deleted.txt" });

            _commitCommand.Execute(new[] { "Test commit" }); // Commit changes

            // Change the working dir after the commit
            File.WriteAllText(addedFile, "Added content");
            _addCommand.Execute(new[] { "added.txt" });

            File.WriteAllText(modifiedFile, "Modified content"); // Modify after staging
            _addCommand.Execute(new[] { "modified.txt" });

            File.Delete(deletedFile); // Delete after staging
            _addCommand.Execute(new[] { "deleted.txt" });

            File.WriteAllText(notstagedFile, "Initial Not Staged content");
            _addCommand.Execute(new[] { "notstaged.txt" });
            File.WriteAllText(notstagedFile, "Not Staged content");

            File.WriteAllText(untrackedFile, "Untracked content");// Untracked file


            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Changes to be committed:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("added.txt (added)"))), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("modified.txt (modified)"))), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("deleted.txt (deleted)"))), Times.Once);

            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("notstaged.txt (added)"))), Times.Once);

            _loggerMock.Verify(logger => logger.Log("Changes not staged for commit:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("notstaged.txt "))), Times.Exactly(2));

            _loggerMock.Verify(logger => logger.Log("Untracked files:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("untracked.txt "))), Times.Once);


            // Assert
            //_loggerMock.Verify(logger => logger.Log("modified.txt (modified)"), Times.Once);
            //_loggerMock.Verify(logger => logger.Log("deleted.txt (deleted)"), Times.Once);

        }









        [Test]
        public void ShouldIgnoreFilesInTheIgnoreFile()
        {

        }


        [Test]
        public void ShouldDisplayCurrentBranch()
        {
            // Act
            _statusCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("On branch:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.Contains("main"))), Times.Once);


            // Arrange: Create switch to a new branch

            // Act

            // Assert: Verify that the new branch is displayed
            
        }


        [Test]
        public void ShouldDisplaySyncStatus()
        {

        }

    }
}