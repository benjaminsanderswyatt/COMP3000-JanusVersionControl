using Janus;
using Janus.Plugins;
using Moq;
using System.Runtime.InteropServices;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class InitCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private InitCommand _initCommand;


        [SetUp]
        public void Setup()
        {
            // Mock the ILogger
            _loggerMock = new Mock<ILogger>();

            // Set the base directory path for testing
            string basePath = Path.GetTempPath(); // Using temp directory for testing
            _paths = new Paths(basePath);

            // Create InitCommand instance
            _initCommand = new InitCommand(_loggerMock.Object, _paths);
        }


        [TearDown]
        public void TearDown()
        {
            // Delete the .janus folder and all its contents if they exist
            if (Directory.Exists(_paths.JanusDir))
            {
                Directory.Delete(_paths.JanusDir, true);
            }
        }






        [Test]
        public void ShouldInitializeRepository_WhenNotAlreadyInitialized()
        {
            // Arrange: Ensure the directories and files do not exist initially
            if (Directory.Exists(_paths.JanusDir))
            {
                Directory.Delete(_paths.JanusDir, true);
            }

            // Act
            _initCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Initialized janus repository"), Times.Once);
            Assert.True(Directory.Exists(_paths.JanusDir));
            Assert.True(Directory.Exists(_paths.ObjectDir));
            Assert.True(Directory.Exists(_paths.RefsDir));
            Assert.True(Directory.Exists(_paths.HeadsDir));
            Assert.True(Directory.Exists(_paths.PluginsDir));
            Assert.True(File.Exists(_paths.Index));
            Assert.True(File.Exists(_paths.Head));
        }



        [Test]
        public void ShouldLogRepositoryAlreadyInitialized_WhenAlreadyInitialized()
        {
            // Arrange: Initialize the repository for this test
            _initCommand.Execute(new string[0]);

            // Act
            _initCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Repository already initialized"), Times.Once);
        }


    }
}