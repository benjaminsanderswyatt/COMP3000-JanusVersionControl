using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class RevertCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private AddCommand _addCommand;
        private CommitCommand _commitCommand;
        private RevertCommand _revertCommand;
        private Mock<ICredentialManager> _credManagerMock;

        private string _testDir;
        private string _initialCommitHash;


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


            // Create command instances
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
            _commitCommand = new CommitCommand(_loggerMock.Object, _paths);
            _revertCommand = new RevertCommand(_loggerMock.Object, _paths);


            // Create initial commit
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "Initial content");
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
            _commitCommand = new CommitCommand(_loggerMock.Object, _paths);
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Initial commit" });
            _initialCommitHash = GetLatestCommitHash();

            // Create next commit to revert to
            File.WriteAllText(Path.Combine(_testDir, "file2.txt"), "New content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Second commit" });

        }

        private string GetLatestCommitHash()
        {
            return Directory.GetFiles(_paths.CommitDir)
                .Select(f => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(f)))
                .OrderByDescending(c => c.Date)
                .First()
                .Commit;
        }

        private CommitMetadata GetLatestCommitMetadata()
        {
            return Directory.GetFiles(_paths.CommitDir)
                .Select(f => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(f)))
                .OrderByDescending(c => c.Date)
                .First();
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

            _credManagerMock.Reset();
        }




        [Test]
        public void ShouldCreateRevertCommitWithCorrectParent()
        {
            // Arrange
            var preRevertCommitHash = GetLatestCommitHash();

            // Get the commit metadata form initial commit
            var commitPath = Path.Combine(_paths.CommitDir, _initialCommitHash);
            var initialCommitMetadata = JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commitPath));

            var initialTreeHash = initialCommitMetadata.Tree;

            // Act
            _revertCommand.Execute(new[] { _initialCommitHash, "--force" });

            // Assert
            var revertCommit = GetLatestCommitMetadata();
            Assert.Multiple(() =>
            {
                Assert.That(revertCommit.Parents, Has.Count.EqualTo(1));
                Assert.That(revertCommit.Parents[0], Is.EqualTo(preRevertCommitHash));
                Assert.That(revertCommit.Tree, Is.EqualTo(initialTreeHash));
            });
        }

        [Test]
        public void ShouldUpdateWorkingDirectoryAndIndex()
        {
            // Arrange
            var originalIndex = IndexHelper.LoadIndex(_paths.Index);

            // Act
            _revertCommand.Execute(new[] { _initialCommitHash, "--force" });

            // Assert
            var revertedIndex = IndexHelper.LoadIndex(_paths.Index);
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(_testDir, "file1.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(_testDir, "file2.txt")), Is.False);
                Assert.That(revertedIndex.ContainsKey("file1.txt"), Is.True);
                Assert.That(revertedIndex.ContainsKey("file2.txt"), Is.False);
                Assert.That(revertedIndex["file1.txt"].Hash, Is.EqualTo(originalIndex["file1.txt"].Hash));
            });
        }




        [Test]
        public void ShouldHandleForceFlagWithUncommittedChanges()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "dirty.txt"), "Uncommitted changes");

            // Act
            _revertCommand.Execute(new[] { _initialCommitHash, "--force" });

            // Assert
            Assert.That(File.Exists(Path.Combine(_testDir, "dirty.txt")), Is.False);
        }

        

        [Test]
        public void ShouldHandleNestedFileRevert()
        {
            // Arrange
            var nestedDir = Path.Combine(_testDir, "nested");
            Directory.CreateDirectory(nestedDir);
            File.WriteAllText(Path.Combine(nestedDir, "file.txt"), "Nested content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Add nested file" });
            var commitToRevertTo = GetLatestCommitHash();

            // Modify file
            File.WriteAllText(Path.Combine(nestedDir, "file.txt"), "Modified content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Modify nested file" });

            // Act
            _revertCommand.Execute(new[] { commitToRevertTo, "--force" });

            // Assert
            var revertedContent = File.ReadAllText(Path.Combine(nestedDir, "file.txt"));
            Assert.That(revertedContent, Is.EqualTo("Nested content"));
        }






    }
}