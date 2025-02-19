using Janus.DataTransferObjects;
using Janus.Helpers;
using Janus.Helpers.CommandHelpers;
using Janus.Models;
using Janus.Plugins;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class BranchNameTest
    {
        [Test]
        [TestCase("", ExpectedResult = false)] // Empty name
        [TestCase("   ", ExpectedResult = false)] // Whitespace-only
        [TestCase("branch~name", ExpectedResult = false)] // Contains ~
        [TestCase("invalid^branch", ExpectedResult = false)] // Contains ^
        [TestCase("name:with:colon", ExpectedResult = false)] // Contains :
        [TestCase("branch?name", ExpectedResult = false)] // Contains ?
        [TestCase("branch\\name", ExpectedResult = false)] // Contains \
        [TestCase("branch/name", ExpectedResult = false)] // Contains /
        [TestCase("name*with*asterisks", ExpectedResult = false)] // Contains *
        [TestCase("name[with]brackets", ExpectedResult = false)] // Contains []
        [TestCase("branch..name", ExpectedResult = false)] // Contains ..
        [TestCase("branch_name123", ExpectedResult = true)] // Valid
        [TestCase("random", ExpectedResult = true)] // Valid
        [TestCase("release_v1.0", ExpectedResult = true)] // Valid
        [TestCase("new_branch", ExpectedResult = true)] // Valid
        public bool IsValidBranchNameTests(string branchName)
        {
            return BranchHelper.IsValidRepoOrBranchName(branchName);
        }
    }


    [TestFixture]
    public class CreateBranchCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private CreateBranchCommand _createBranchCommand;

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
            _createBranchCommand = new CreateBranchCommand(_loggerMock.Object, _paths);
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
            _createBranchCommand.Execute(new string[] { "new_branch" });

            // Assert
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise repository."), Times.Once);

        }


        [Test]
        public void ShouldLogError_WhenBranchNameIsInvalid()
        {
            string invalidBranchName = "invalid^branch";

            // Act
            _createBranchCommand.Execute(new string[] { invalidBranchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Invalid branch name: {invalidBranchName}"), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenBranchAlreadyExists()
        {
            // Arrange
            string branchName = "existing_branch";

            // Act
            _createBranchCommand.Execute(new string[] { branchName });
            _createBranchCommand.Execute(new string[] { branchName }); // Attempt to create the same branch again

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Branch '{branchName}' already exists."), Times.Once);
        }



        [Test]
        public void ShouldCreateBranch_WhenBranchNameIsValid2()
        {
            // Arrange
            string branchName = "new_branch";

            // Act
            _createBranchCommand.Execute(new string[] { branchName });

            // Assert
            string branchPath = Path.Combine(_paths.HeadsDir, branchName);
            Assert.IsTrue(File.Exists(branchPath), "Branch head file should exist.");

            string branchFolderPath = Path.Combine(_paths.BranchesDir, branchName);
            string metadataPath = Path.Combine(branchFolderPath, "info");

            Assert.IsTrue(File.Exists(metadataPath), "Branch metadata file should exist.");

            var branchJson = File.ReadAllText(metadataPath);
            var branch = JsonSerializer.Deserialize<Branch>(branchJson);

            Assert.That(branch.Name, Is.EqualTo(branchName), "Branch name should match.");
            Assert.That(branch.CreatedBy, Is.EqualTo(MiscHelper.GetUsername()), "CreatedBy should match current user.");
            Assert.That(branch.ParentBranch, Is.EqualTo(MiscHelper.GetCurrentBranchName(_paths)), "Parent branch should match.");

            _loggerMock.Verify(logger => logger.Log($"Created new branch {branchName}"), Times.Once);
        }



    }
}