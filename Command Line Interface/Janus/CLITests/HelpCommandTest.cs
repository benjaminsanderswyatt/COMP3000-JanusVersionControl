using Janus;
using Janus.Plugins;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Runtime.InteropServices;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class HelpCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private HelpCommand _helpCommand;

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

            // Reset the CommandList to ensure a clean slate before each test
            Janus.Program.CommandList.Clear();

            // Create the HelpCommand instance
            _helpCommand = new HelpCommand(_loggerMock.Object, _paths);
        }


        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }


        [Test]
        public void ShouldDisplayHelpMessage_WithCommandsList()
        {
            // Arrange: Add a few commands to the Program.CommandList for this test
            List<ICommand> commandList = new List<ICommand>
            {
                new InitCommand(_loggerMock.Object, _paths),
                new HelpCommand(_loggerMock.Object, _paths)
            };
            Janus.Program.CommandList = commandList;

            // Act
            _helpCommand.Execute(new string[0]);

            // Assert: Verify that the logger is called for each command in the list
            _loggerMock.Verify(logger => logger.Log("Usage: janus <command>"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("Commands:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("init                 : Initializes the janus repository."), Times.Once);
            _loggerMock.Verify(logger => logger.Log("help                 : Displays a list of available commands."), Times.Once);
        }


    }
}