using Janus.Helpers.CommandHelpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using System.Xml.Linq;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class RemoteCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private RemoteCommand _remoteCommand;

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

            // Login with test user
            var credManager = new CredentialManager();
            var testCredentials = new UserCredentials
            {
                Username = "testuser",
                Email = "test@user.com",
                Token = "testtoken"
            };

            credManager.SaveCredentials(testCredentials);

            // Initialize the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);

            // Create InitCommand instance
            _remoteCommand = new RemoteCommand(_loggerMock.Object, _paths);
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

            var credManager = new CredentialManager();
            credManager.ClearCredentials();
        }


        [Test]
        public async Task ShouldLogError_WhenNoArgsGiven()
        {
            // Arrange: no args
            string[] args = new string[0];

            // Act
            await _remoteCommand.Execute(args);

            // Assert
            _loggerMock.Verify(l => l.Log("Provide a command"), Times.Once);
        }

        [Test]
        [TestCase("origin", "janus/owner/repoName")]
        [TestCase("random", "janus/user/name")]
        public async Task ShouldAddRemote(string name, string link)
        {
            // Arrange
            string[] args = new string[] { "add", name, link };

            // Act
            await _remoteCommand.Execute(args);

            // Assert: Verify that the remote has been added
            string remoteJson = File.ReadAllText(_paths.Remote);
            var remotes = JsonSerializer.Deserialize<List<RemoteHelper.RemoteRepos>>(remoteJson);

            Assert.IsNotNull(remotes);

            Assert.IsTrue(remotes.Any(r =>
                r.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                r.Link.Equals(link, StringComparison.OrdinalIgnoreCase)
                ));

            // Verify result was logged
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains($"Remote '{name}' added successfully"))), Times.Once);
        }

        [Test]
        [TestCase("origin")]
        [TestCase("different")]
        public async Task ShouldRemoveRemote(string name)
        {
            // Arrange: mock the remotes file
            var initialRemotes = new List<RemoteHelper.RemoteRepos>
            {
                new RemoteHelper.RemoteRepos { Name = "origin", Link = "janus/user/reponame" },
                new RemoteHelper.RemoteRepos { Name = "different", Link = "janus/name/name" }
            };
            File.WriteAllText(_paths.Remote, JsonSerializer.Serialize(initialRemotes, new JsonSerializerOptions { WriteIndented = true }));

            // Act
            string[] args = new string[] { "remove", name };
            await _remoteCommand.Execute(args);

            // Assert: the name was removed
            string remoteJson = File.ReadAllText(_paths.Remote);
            var remotes = JsonSerializer.Deserialize<List<RemoteHelper.RemoteRepos>>(remoteJson);

            Assert.IsFalse(remotes.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));

            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains($"Remote '{name}' was successfully removed"))), Times.Once);
        }

        [Test]
        public async Task ShouldLogNotFound_WhenNameDoesntExist()
        {
            // Act
            string[] args = new string[] { "remove", "NoExist" };
            await _remoteCommand.Execute(args);

            // Arrange
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Remote 'NoExist' was not found"))), Times.Once);
        }



        [Test]
        public async Task ShouldListRemotes()
        {
            // Arrange: mock the remotes file
            var initialRemotes = new List<RemoteHelper.RemoteRepos>
            {
                new RemoteHelper.RemoteRepos { Name = "origin", Link = "janus/user/reponame" },
                new RemoteHelper.RemoteRepos { Name = "different", Link = "janus/name/name" }
            };
            File.WriteAllText(_paths.Remote, JsonSerializer.Serialize(initialRemotes, new JsonSerializerOptions { WriteIndented = true }));

            // Act
            string[] args = new string[] { "list" };
            await _remoteCommand.Execute(args);

            // Assert: verify the the remote was removed
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("origin") && s.Contains("janus/user/reponame"))), Times.Once);
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("different") && s.Contains("janus/name/name"))), Times.Once);
        }

        [Test]
        public async Task ShouldLogUsage_WhenNoCommandsGiven()
        {
            // Arrange
            string[] args = new string[] { "random" };

            // Act
            await _remoteCommand.Execute(args);

            // Assert: verify the the remote was removed
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Usage:"))), Times.Once);
        }


    }
}