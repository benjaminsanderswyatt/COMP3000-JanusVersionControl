using Janus.Plugins;
using Moq;
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

            Directory.SetCurrentDirectory(_testDir);

            // Reset the CommandList to ensure a clean slate before each test
            Janus.Program.CommandList.Clear();

            // Create the HelpCommand instance
            _helpCommand = new HelpCommand(_loggerMock.Object, _paths);
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
            _loggerMock.Verify(logger => logger.Log("Usage: janus <command> [arguments]"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("Commands:"), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("init"))), Times.Exactly(1));
        }



        [Test]
        public void ShouldDisplayDetailedUsage_ForSpecificCommand()
        {
            // Arrange: Add commands to the CommandList
            List<ICommand> commandList = new List<ICommand>
            {
                new InitCommand(_loggerMock.Object, _paths),
                new HelpCommand(_loggerMock.Object, _paths)
            };
            Janus.Program.CommandList = commandList;

            // Act
            _helpCommand.Execute(new string[] { "init" });

            // Assert: Verify that the logger logs the usage and description for the init command
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Usage for command 'init':"))), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("janus init"))), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Initialises the janus repository"))), Times.Once);
        }

        [Test]
        public void ShouldDisplayNotFound_ForInvalidCommand()
        {
            // Arrange
            List<ICommand> commandList = new List<ICommand>
            {
                new InitCommand(_loggerMock.Object, _paths),
                new HelpCommand(_loggerMock.Object, _paths)
            };
            Janus.Program.CommandList = commandList;

            // Act
            _helpCommand.Execute(new string[] { "invalid" });

            // Assert: Verify that the logger logs that the command was not found
            _loggerMock.Verify(logger => logger.Log("Command 'invalid' not found."), Times.Once);
        }

    }
}