using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using static Janus.CommandHandler;

namespace CLITests
{

    [TestFixture]
    public class LoginTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private LoginCommand _loginCommand;
        private StringReader _consoleInput;

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

            // Initialise the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);


            // Create Log Command instance
            _loginCommand = new LoginCommand(_loggerMock.Object, _paths);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory(Path.GetTempPath()); // Cant delete dir if in it

            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }

            // Remove the credintials file
            var credManager = new CredentialManager();
            credManager.ClearCredentials();
        }



        [Test]
        public async Task ShouldLogError_WhenUsernameIsEmpty()
        {
            // Arrange
            SetConsoleInput("\nuser@example.com\ntesttoken\n"); // Empty username

            // Act
            await _loginCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(x => x.Log("Username is required"), Times.Once);
        }

        [Test]
        public async Task ShouldLogError_WhenEmailIsEmpty()
        {
            // Arrange
            SetConsoleInput("testuser\n\nvalidtoken\n"); // Empty email

            // Act
            await _loginCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(x => x.Log("Email is required"), Times.Once);
        }

        [Test]
        public async Task ShouldLogError_WhenPATIsEmpty()
        {
            // Arrange
            SetConsoleInput("testuser\ntest@example.com\n\n"); // Empty PAT

            // Act
            await _loginCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(x => x.Log("Saved login credantials as 'testuser' (test@example.com)"), Times.Once); // success message (PAT can be null)
        }

        [Test]
        public async Task ShouldSaveCredentials_WhenInputsAreValid()
        {
            // Arrange
            SetConsoleInput("testuser\ntest@example.com\ntesttoken\n");

            // Act
            await _loginCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(x => x.Log(It.Is<string>(msg => msg.Contains("Saved login credantials as 'testuser'"))), Times.Once);
        }



        private void SetConsoleInput(string input)
        {
            _consoleInput = new StringReader(input);
            Console.SetIn(_consoleInput);
        }


    }


}