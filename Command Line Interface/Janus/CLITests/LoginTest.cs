using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Runtime.InteropServices;
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
            string basePath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: use %APPDATA%
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // MacOS: use /Library/Application Support
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                basePath = Path.Combine(home, "Library", "Application Support");
            }
            else
            {
                // Misc case
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }

            
            string credentialPath = Path.Combine(basePath, "Janus");
            
            if (Directory.Exists(credentialPath))
            {
                Directory.Delete(credentialPath, true);
            }

        }


        [Test]
        public async Task ShouldLogError_WhenEmailIsEmpty()
        {
            // Arrange
            SetConsoleInput("\ninvalid_pat\n"); // Empty email

            // Act
            await _loginCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(x => x.Log("Email is required"), Times.Once);
        }

        [Test]
        public async Task ShouldLogError_WhenPATIsEmpty()
        {
            // Arrange
            SetConsoleInput("test@example.com\n\n"); // Empty PAT

            // Act
            await _loginCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(x => x.Log("The personal access token is required"), Times.Once);
        }


        private void SetConsoleInput(string input)
        {
            _consoleInput = new StringReader(input);
            Console.SetIn(_consoleInput);
        }


    }


}