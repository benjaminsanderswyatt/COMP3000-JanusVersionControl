using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class CloneCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private CloneCommand _cloneCommand;
        private string _testDir;


        [SetUp]
        public void Setup()
        {
            // Create a mock logger
            _loggerMock = new Mock<ILogger>();

            // Create a temp dir for testing
            _testDir = Path.Combine(Path.GetTempPath(), "JanusTest");
            Directory.CreateDirectory(_testDir);
            _paths = new Paths(_testDir);

            // Save test credentials
            var credManager = new CredentialManager();
            var testCredentials = new UserCredentials
            {
                Username = "testuser",
                Email = "test@user.com",
                Token = "testtoken"
            };
            credManager.SaveCredentials(testCredentials);

            _cloneCommand = new CloneCommand(_loggerMock.Object, _paths);
        }






        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory(Path.GetTempPath());
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }

            var credManager = new CredentialManager();
            credManager.ClearCredentials();
        }





        [Test]
        public async Task Execute_ExistingDirectory_LogsFailure()
        {
            // Arrange: create a directory with the same name as the repo
            string repoName = "existingRepo";
            string repoPath = Path.Combine(_paths.WorkingDir, repoName);
            Directory.CreateDirectory(repoPath);

            // Act
            await _cloneCommand.Execute(new string[] { $"janus/owner/{repoName}" });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains($"Failed clone: Directory named '{repoName}' already exists"))), Times.Once());

        }


        [Test]
        public async Task Execute_InvalidEndpoint_LogsError()
        {
            // Act: invalid endpoint format
            await _cloneCommand.Execute(new string[] { "janus/invalidEndpoint" });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Invalid endpoint format. Usage: janus/<Owner>/<Repository Name>"))), Times.Once());
        }


        [Test]
        public async Task Execute_MissingTreeData_LogsErrorAndCleansUp()
        {
            // Arrange
            var cloneCommand = new CloneCommand(_loggerMock.Object, _paths);

            // Act
            await cloneCommand.Execute(new string[] { "janus/owner/missingTreeRepo" });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("An error occurred during cloning:"))), Times.Once());
            Assert.That(Directory.Exists("missingTreeRepo"), Is.False);
        }








    }
}