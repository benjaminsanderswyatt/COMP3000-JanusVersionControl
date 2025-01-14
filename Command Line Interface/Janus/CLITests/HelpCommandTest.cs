using Janus;
using Janus.Plugins;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Runtime.InteropServices;
using static Janus.CommandHandler;

namespace CLITests
{
    public class HelpCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private HelpCommand _helpCommand;

        [SetUp]
        public void SetUp()
        {
            // Mock the ILogger
            _loggerMock = new Mock<ILogger>();

            // Set the base directory path for testing
            string basePath = Path.GetTempPath(); // Using temp directory for testing
            _paths = new Paths(basePath);

            // Reset the CommandList to ensure a clean slate before each test
            Janus.Program.CommandList.Clear();

            // Create the HelpCommand instance
            _helpCommand = new HelpCommand(_loggerMock.Object, _paths);
        }


        [TearDown]
        public void TearDown()
        {
            // Nothing to tear down
        }


        [Test]
        public void Execute_ShouldDisplayHelpMessage_WithCommandsList()
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
            //_loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("init"))), Times.Once);
            //_loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("help"))), Times.Once);
            _loggerMock.Verify(logger => logger.Log("Usage: janus <command>"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("Commands:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("init                 : Initializes the janus repository."), Times.Once);
            _loggerMock.Verify(logger => logger.Log("help                 : Displays a list of available commands."), Times.Once);
        }


    }
}