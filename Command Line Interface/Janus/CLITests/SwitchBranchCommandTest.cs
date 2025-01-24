using Janus;
using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Moq;
using System.Text.Json;
using System.Xml.Serialization;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class SwitchBranchCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private SwitchBranchCommand _switchBranchCommand;
        private CreateBranchCommand _createBranchCommand;
        private AddCommand _addCommand;

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


            // Create command instances
            _switchBranchCommand = new SwitchBranchCommand(_loggerMock.Object, _paths);
            _createBranchCommand = new CreateBranchCommand(_loggerMock.Object, _paths);
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
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
        public void ShouldLogError_WhenRepoDoesNotExist()
        {
            // Arrange
            Directory.Delete(_paths.JanusDir, true);

            // Act
            _switchBranchCommand.Execute(new string[] { "new_branch" });

            // Assert
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise repository."), Times.Once);

        }


        [Test]
        public void ShouldLogError_WhenBranchNameIsNotProvided()
        {
            // Act
            _switchBranchCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Please provide a branch name."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenBranchDoesNotExists()
        {
            // Arrange
            string branchName = "non_existing_branch";

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Branch '{branchName}' doesnt exists."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenAlreadyOnBranch()
        {
            // Arrange
            string branchName = "main";

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Already on branch '{branchName}'."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenAlreadyOnCreatedBranch()
        {
            // Arrange
            string branchName = "test_branch";
            _createBranchCommand.Execute(new string[] { branchName });
            _switchBranchCommand.Execute(new string[] { branchName });

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Already on branch '{branchName}'."), Times.Once);
        }







        [Test]
        public void ShouldSwitchBranchSuccessfully_WhenRepoIsEmpty()
        {
            // Arrange
            string branchName = "test_branch";
            _createBranchCommand.Execute(new string[] { branchName });

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Successfully switched to branch"))), Times.Once);
            Assert.That(File.ReadAllText(_paths.HEAD), Is.EqualTo($"ref: heads/{branchName}"));
            
            string branchIndex = File.ReadAllText(Path.Combine(_paths.BranchesDir, "test_branch", "index"));
            Assert.That(File.ReadAllText(_paths.Index), Is.EqualTo(branchIndex));

        }

        [Test]
        public void ShouldSwitchBranch_WhenForcedWithUncommittedFilesExist()
        {
            // Arrange: Create a branch and have something uncommitted
            string branchName = "test_branch";
            _createBranchCommand.Execute(new string[] { branchName });

            // Create a file
            string filePath = Path.Combine(_testDir, "test.txt");
            File.WriteAllText(filePath, "test content");

            // Add the file
            _addCommand.Execute(new string[] { filePath });


            // Act: switch to the new branch
            _switchBranchCommand.Execute(new[] { branchName, "--force" }); // Force to bypass user confirmation


            // Assert: 
            _loggerMock.Verify(logger => logger.Log($"Successfully switched to branch '{branchName}'."), Times.Once);
            Assert.That(File.ReadAllText(_paths.HEAD), Is.EqualTo($"ref: heads/{branchName}"));

            string branchIndex = File.ReadAllText(Path.Combine(_paths.BranchesDir, "test_branch", "index"));
            Assert.That(File.ReadAllText(_paths.Index), Is.EqualTo(branchIndex));
        }


        [Test]
        public void ShouldPreserveCorrectFiles_AfterBranchSwitch()
        {

        }



    }

    
}