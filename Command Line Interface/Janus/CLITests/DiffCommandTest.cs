using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class DiffCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private AddCommand _addCommand;
        private CommitCommand _commitCommand;
        private DiffCommand _diffCommand;

        private string _testDir;
        private string _initialCommitHash;
        private string _secondCommitHash;

        private List<string> _logMessages = new List<string>();


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


            // Create Commit Command instance
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
            _commitCommand = new CommitCommand(_loggerMock.Object, _paths);
            _diffCommand = new DiffCommand(_loggerMock.Object, _paths);



            // Create initial commit
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "Initial content");
            _addCommand.Execute(new[] { "file1.txt" });
            _commitCommand.Execute(new[] { "Initial commit" });
            _initialCommitHash = GetLatestCommitHash();

            // Create second commit
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "Modified content");
            _addCommand.Execute(new[] { "file1.txt" });
            _commitCommand.Execute(new[] { "Second commit" });
            _secondCommitHash = GetLatestCommitHash();

            _loggerMock.Setup(x => x.Log(It.IsAny<string>()))
                .Callback<string>(msg => _logMessages.Add(msg));
        }

        private string GetLatestCommitHash()
        {
            return Directory.GetFiles(_paths.CommitDir)
                .Select(f => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(f)))
                .OrderByDescending(c => c.Date)
                .First()
                .Commit;
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

            _logMessages.Clear();
        }



        [Test]
        public void ShouldShowUnstagedChanges()
        {
            // Modify working directory
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "New unstaged content");

            _diffCommand.Execute(Array.Empty<string>());

            Assert.Multiple(() =>
            {
                Assert.That(_logMessages, Contains.Item("- Modified content"));
                Assert.That(_logMessages, Contains.Item("+ New unstaged content"));
            });
        }

        [Test]
        public void ShouldShowStagedChanges()
        {
            // Stage new change
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "Staged change");
            _addCommand.Execute(new[] { "file1.txt" });

            _diffCommand.Execute(new[] { "--staged" });

            _loggerMock.Verify(logger => logger.Log("- Modified content"), Times.Once);
            _loggerMock.Verify(logger => logger.Log("+ Staged change"), Times.Once);
        }

        [Test]
        public void ShouldCompareTwoCommits()
        {
            _diffCommand.Execute(new[] { _initialCommitHash, _secondCommitHash });

            Assert.Multiple(() =>
            {
                Assert.That(_logMessages, Contains.Item("- Initial content"));
                Assert.That(_logMessages, Contains.Item("+ Modified content"));
            });
        }

        [Test]
        public void ShouldCompareCommitWithParent()
        {
            _diffCommand.Execute(new[] { _secondCommitHash, "--parent" });

            Assert.Multiple(() =>
            {
                Assert.That(_logMessages, Contains.Item("- Initial content"));
                Assert.That(_logMessages, Contains.Item("+ Modified content"));
            });
        }

        [Test]
        public void ShouldFilterByPath()
        {
            // Create multiple files
            File.WriteAllText(Path.Combine(_testDir, "file2.txt"), "Content");
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "New content");
            _addCommand.Execute(new[] { "file1.txt", "file2.txt" });

            _logMessages.Clear();

            _diffCommand.Execute(new[] { "--staged", "--path", "file1.txt" });

            Console.WriteLine(string.Join("\n", _logMessages));

            Assert.Multiple(() =>
            {
                Assert.That(_logMessages, Has.Some.Contains("file1.txt"));
                Assert.That(_logMessages, Has.None.Contains("file2.txt"));
            });
        }

        [Test]
        public void ShouldHandleBinaryFiles()
        {
            // Create binary file
            var binaryPath = Path.Combine(_testDir, "image.png");
            File.WriteAllBytes(binaryPath, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

            // Stage binary file
            _addCommand.Execute(new[] { "image.png" });

            _diffCommand.Execute(new[] { "--staged" });


            Assert.Multiple(() =>
            {
                Assert.That(_logMessages, Has.Some.Contains("Binary diff not supported"));
                Assert.That(_logMessages, Has.Some.Contains("image.png"));
            });
        }

        [Test]
        public void ShouldHandleDeletedFiles()
        {
            // Delete file and stage deletion
            File.Delete(Path.Combine(_testDir, "file1.txt"));
            _addCommand.Execute(new[] { "file1.txt" });

            _diffCommand.Execute(new[] { "--staged" });


            _loggerMock.Verify(logger => logger.Log("- Modified content"), Times.Once);
        }

        [Test]
        public void ShouldHandleAddedFiles()
        {
            // Create and stage new file
            File.WriteAllText(Path.Combine(_testDir, "newfile.txt"), "New content");
            _addCommand.Execute(new[] { "newfile.txt" });

            _diffCommand.Execute(new[] { "--staged" });


            _loggerMock.Verify(logger => logger.Log("+ New content"), Times.Once);

        }
    }
}