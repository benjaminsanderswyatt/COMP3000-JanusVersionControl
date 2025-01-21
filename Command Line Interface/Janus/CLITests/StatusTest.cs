using Janus.Plugins;
using Moq;

namespace CLITests
{
    [TestFixture]
    public class StatusTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;

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





    }
}