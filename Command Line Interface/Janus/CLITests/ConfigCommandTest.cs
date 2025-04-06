using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{


    [TestFixture]
    public class ConfigCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private ConfigCommand _configCommand;
        private Mock<ICredentialManager> _credManagerMock;

        private string _testDir;


        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _testDir = Path.Combine(Path.GetTempPath(), "JanusTest");
            Directory.CreateDirectory(_testDir);
            _paths = new Paths(_testDir);

            // Login with test user
            var testCredentials = new UserCredentials
            {
                Username = "testuser",
                Email = "test@user.com",
                Token = "testtoken"
            };
            _credManagerMock = new Mock<ICredentialManager>();
            _credManagerMock.Setup(m => m.LoadCredentials())
                .Returns(testCredentials);


            // Initialize repository
            var initCommand = new InitCommand(_loggerMock.Object, _paths, _credManagerMock.Object);
            initCommand.Execute(new string[0]);



            _configCommand = new ConfigCommand(_loggerMock.Object, _paths);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory(Path.GetTempPath());
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }

            _credManagerMock.Reset();
        }

        [Test]
        public void ShouldGetLocalIp()
        {
            // Arrange
            File.WriteAllText(_paths.LocalConfig, "192.168.1.100");

            // Act
            _configCommand.Execute(new[] { "ip", "get" });

            // Assert
            _loggerMock.Verify(x => x.Log("Configured IP: 192.168.1.100"), Times.Once);
        }


        [Test]
        public void ShouldSetLocalIp()
        {
            // Act
            _configCommand.Execute(new[] { "ip", "set", "192.168.1.150" });

            // Assert
            Assert.That(File.ReadAllText(_paths.LocalConfig).Trim(), Is.EqualTo("192.168.1.150"));
            _loggerMock.Verify(x => x.Log("IP configuration set to: 192.168.1.150"), Times.Once);
        }


        [Test]
        public void ShouldResetLocalIp()
        {
            // Arrange
            File.WriteAllText(_paths.LocalConfig, "192.168.1.100");

            // Act
            _configCommand.Execute(new[] { "ip", "reset" });

            // Assert
            Assert.That(File.Exists(_paths.LocalConfig), Is.False);
            _loggerMock.Verify(x => x.Log("IP configuration has been reset"), Times.Once);
        }

        [Test]
        public void ShouldGetRepoIsPrivate()
        {
            // Arrange
            var config = new RepoConfig { IsPrivate = true, Description = "Test repo" };
            File.WriteAllText(_paths.RepoConfig, JsonSerializer.Serialize(config));

            // Act
            _configCommand.Execute(new[] { "repo", "get", "is-private" });

            // Assert
            _loggerMock.Verify(x => x.Log("Repository is private: True"), Times.Once);
        }

        [Test]
        public void ShouldGetRepoDescription()
        {
            // Arrange
            var config = new RepoConfig { IsPrivate = false, Description = "Test description" };
            File.WriteAllText(_paths.RepoConfig, JsonSerializer.Serialize(config));

            // Act
            _configCommand.Execute(new[] { "repo", "get", "description" });

            // Assert
            _loggerMock.Verify(x => x.Log("Repository description: Test description"), Times.Once);
        }

        [Test]
        public void ShouldSetRepoIsPrivate()
        {
            // Act
            _configCommand.Execute(new[] { "repo", "set", "is-private", "false" });

            // Assert
            var config = JsonSerializer.Deserialize<RepoConfig>(File.ReadAllText(_paths.RepoConfig));
            Assert.That(config.IsPrivate, Is.False);
            _loggerMock.Verify(x => x.Log("Successfully set is-private to 'false'"), Times.Once);
        }

        [Test]
        public void ShouldSetRepoDescription()
        {
            // Act
            _configCommand.Execute(new[] { "repo", "set", "description", "My Project Description" });

            // Assert
            var config = JsonSerializer.Deserialize<RepoConfig>(File.ReadAllText(_paths.RepoConfig));
            Assert.That(config.Description, Is.EqualTo("My Project Description"));
            _loggerMock.Verify(x => x.Log("Successfully set description to 'My Project Description'"), Times.Once);
        }

        [Test]
        public void ShouldHandleInvalidRepoPropertyGet()
        {
            // Act
            _configCommand.Execute(new[] { "repo", "get", "invalid-prop" });

            // Assert
            _loggerMock.Verify(x => x.Log("Unknown property 'invalid-prop'. Valid properties are 'is-private' and 'description'"), Times.Once);
        }

        [Test]
        public void ShouldHandleInvalidRepoPropertySet()
        {
            // Act
            _configCommand.Execute(new[] { "repo", "set", "invalid-prop", "value" });

            // Assert
            _loggerMock.Verify(x => x.Log("Unknown property 'invalid-prop'. Valid properties are 'is-private' and 'description'"), Times.Once);
        }

        [Test]
        public void ShouldHandleInvalidIsPrivateValue()
        {
            // Act
            _configCommand.Execute(new[] { "repo", "set", "is-private", "not-a-bool" });

            // Assert
            _loggerMock.Verify(x => x.Log("Invalid value for 'is-private'. Use 'true' or 'false'"), Times.Once);
        }

        [Test]
        public void ShouldHandleMissingRepoConfig()
        {
            // Arrange
            File.Delete(_paths.RepoConfig);

            // Act
            _configCommand.Execute(new[] { "repo", "get", "is-private" });

            // Assert
            _loggerMock.Verify(x => x.Log("Repository configuration not found. The repository may not be initialized"), Times.Once);
        }

        [Test]
        public void ShouldHandleInvalidSubcommand()
        {
            // Act
            _configCommand.Execute(new[] { "invalid", "subcommand" });

            // Assert
            _loggerMock.Verify(x => x.Log("Invalid subcommand. Use 'ip' or 'repo'."), Times.Once);
        }
    }
}