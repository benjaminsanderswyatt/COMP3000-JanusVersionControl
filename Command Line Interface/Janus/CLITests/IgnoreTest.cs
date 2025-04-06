using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class IgnoreTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private Mock<ICredentialManager> _credManagerMock;

        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            // Mock the ILogger
            _loggerMock = new Mock<ILogger>();

            // Set the base directory path for testing
            _testDir = Path.Combine(Path.GetTempPath(), "JanusTest"); // Using temp directory for testing
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

            // Initialise the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths, _credManagerMock.Object);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);

            // Create a sample .janusignore file
            File.WriteAllLines(Path.Combine(_testDir, ".janusignore"), new[]
            {
                "# Ignore all log files",
                "*.log",
                "# Ignore build directory",
                "build/**/*",
                "# Negation rule: Don't ignore important.log",
                "!important.log"
            });


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

            // Clean up credentials
            var credManager = new CredentialManager();
            credManager.ClearCredentials();
        }



        [Test]
        public void ShouldLoadPatternsCorrectly()
        {
            // Act
            var (includePatterns, excludePatterns) = IgnoreHelper.LoadIgnorePatterns(_paths.WorkingDir);

            // Assert
            Assert.That(includePatterns.Count, Is.EqualTo(1));
            Assert.That(includePatterns[0].Pattern, Is.EqualTo("important.log"));

            Assert.That(excludePatterns.Count, Is.EqualTo(2));
            Assert.That(excludePatterns[0].Pattern, Is.EqualTo("*.log"));
            Assert.That(excludePatterns[1].Pattern, Is.EqualTo("build/**/*"));
        }


        [Test]
        public void ShouldIgnore_ShouldReturnTrueForIgnoredFiles()
        {
            // Arrange
            var (includePatterns, excludePatterns) = IgnoreHelper.LoadIgnorePatterns(_testDir);

            // Act
            bool logShouldIgnore = IgnoreHelper.ShouldIgnore("error.log", includePatterns, excludePatterns);
            bool buildShouldIgnore = IgnoreHelper.ShouldIgnore("build/main.exe", includePatterns, excludePatterns);

            // Assert
            Assert.IsTrue(logShouldIgnore, "Should ignore as it matches the '*.log'.");
            Assert.IsTrue(buildShouldIgnore, "Should ignore as it matches the 'build/'.");
        }

        [Test]
        public void ShouldIgnore_ShouldReturnFalseForIncludedFiles()
        {
            // Arrange
            var (includePatterns, excludePatterns) = IgnoreHelper.LoadIgnorePatterns(_testDir);

            // Act
            bool shouldIgnore = IgnoreHelper.ShouldIgnore("important.log", includePatterns, excludePatterns);

            // Assert: Ignore is negated by !
            Assert.IsFalse(shouldIgnore);
        }

        [Test]
        public void ShouldIgnore_ShouldReturnFalseForUnmatchedFiles()
        {
            // Arrange
            var (includePatterns, excludePatterns) = IgnoreHelper.LoadIgnorePatterns(_testDir);

            // Act
            bool shouldIgnore = IgnoreHelper.ShouldIgnore("random.txt", includePatterns, excludePatterns);

            // Assert: Does not match any pattern
            Assert.IsFalse(shouldIgnore);
        }



    }
}