using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class MergeCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private AddCommand _addCommand;
        private CommitCommand _commitCommand;
        private CreateBranchCommand _createBranchCommand;
        private MergeCommand _mergeCommand;
        private SwitchBranchCommand _switchBranchCommand;
        private Mock<ICredentialManager> _credManagerMock;


        private string _testDir;
        private string _mainBranchHead;
        private string _featureBranchHead;



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
            _createBranchCommand = new CreateBranchCommand(_loggerMock.Object, _paths);
            _mergeCommand = new MergeCommand(_loggerMock.Object, _paths);
            _switchBranchCommand = new SwitchBranchCommand(_loggerMock.Object, _paths);


            // Create initial commit structure
            File.WriteAllText(Path.Combine(_testDir, "base.txt"), "Base content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Initial commit" });
            _mainBranchHead = GetLatestCommitHash();

            // Create feature branch
            _createBranchCommand.Execute(new[] { "featureBranch" });
            _switchBranchCommand.Execute(new[] { "featureBranch" });

            // Commit to feature branch
            File.WriteAllText(Path.Combine(_testDir, "feature.txt"), "Feature content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature commit" });
            _featureBranchHead = GetLatestCommitHash();

            // Switch back to main
            _switchBranchCommand.Execute(new[] { "main" });
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
        public void ShouldCreateMergeCommitWithTwoParents()
        {
            // Arrange: Modify main after branch split
            File.WriteAllText(Path.Combine(_testDir, "main.txt"), "Main content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main commit" });

            var mainHeadBeforeMerge = GetLatestCommitHash();

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            var mergeCommit = GetLatestCommitMetadata();
            Assert.That(mergeCommit.Parents, Has.Count.EqualTo(2));
            Assert.That(mergeCommit.Parents, Contains.Item(mainHeadBeforeMerge));
            Assert.That(mergeCommit.Parents, Contains.Item(_featureBranchHead));

        }


        [Test]
        public void ShouldDetectMergeConflicts()
        {
            // Arrange: Create conflicting changes in main
            File.WriteAllText(Path.Combine(_testDir, "conflict.txt"), "Main version");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main conflict commit" });

            // Create conflicting changes in feature
            _switchBranchCommand.Execute(new[] { "featureBranch" });
            File.WriteAllText(Path.Combine(_testDir, "conflict.txt"), "Feature version");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature conflict commit" });
            _switchBranchCommand.Execute(new[] { "main" });


            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            _loggerMock.Verify(x => x.Log("Merge conflicts detected:"), Times.Once);
            _loggerMock.Verify(x => x.Log("Conflict: conflict.txt"), Times.Once);

            var mergeCommit = GetLatestCommitMetadata();
            Assert.That(mergeCommit.Message.StartsWith("Merge"), Is.False);

        }


        [Test]
        public void ShouldUpdateWorkingDirAfterMerge()
        {
            // Arrange: Non conflicting merge
            File.WriteAllText(Path.Combine(_testDir, "mainOnly.txt"), "Main content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main only commit" });

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            Assert.That(File.Exists(Path.Combine(_testDir, "mainOnly.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(_testDir, "feature.txt")), Is.True);

        }


        [Test]
        public void ShouldCreateCorrectMergeTree()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "main.txt"), "Main content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main commit" });

            _switchBranchCommand.Execute(new[] { "featureBranch" });
            File.WriteAllText(Path.Combine(_testDir, "feature.txt"), "Feature content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature commit" });
            _switchBranchCommand.Execute(new[] { "main" });

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            var mergeCommit = GetLatestCommitMetadata();
            var tree = new TreeBuilder(_paths).RecreateTree(_loggerMock.Object, mergeCommit.Tree);

            var files = tree.Children.Select(c => c.Name).ToList();
            Assert.That(files, Contains.Item("main.txt"));
            Assert.That(files, Contains.Item("feature.txt"));

        }






        [Test]
        public void ShouldHandleDifferentDirStructures()
        {
            // Arrange: different dir structures
            var mainDir = Path.Combine(_testDir, "common-dir");
            Directory.CreateDirectory(mainDir);
            File.WriteAllText(Path.Combine(mainDir, "main-file.txt"), "Main file");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main directory structure" });

            // Feature branch dir structure
            _switchBranchCommand.Execute(new[] { "featureBranch" });
            var featureDir = Path.Combine(_testDir, "common-dir");
            Directory.CreateDirectory(featureDir);
            File.WriteAllText(Path.Combine(featureDir, "feature-file.txt"), "Feature file");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature directory structure" });
            _switchBranchCommand.Execute(new[] { "main" });

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            var mergedDir = Path.Combine(_testDir, "common-dir");
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(mergedDir), Is.True);
                Assert.That(File.Exists(Path.Combine(mergedDir, "main-file.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(mergedDir, "feature-file.txt")), Is.True);
            });
        }



        [Test]
        public void ShouldHandleNestedConflicts()
        {
            // Arrange
            var nestedDir = Path.Combine(_testDir, "nested");
            Directory.CreateDirectory(nestedDir);

            // Base file
            File.WriteAllText(Path.Combine(nestedDir, "file.txt"), "Base content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Nested base" });

            // Modify in main
            File.WriteAllText(Path.Combine(nestedDir, "file.txt"), "Main content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main nested change" });

            // Modify in feature
            _switchBranchCommand.Execute(new[] { "featureBranch" });
            Directory.CreateDirectory(nestedDir);
            File.WriteAllText(Path.Combine(nestedDir, "file.txt"), "Created content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Nested base" });
            File.WriteAllText(Path.Combine(nestedDir, "file.txt"), "Feature content");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature nested change" });
            _switchBranchCommand.Execute(new[] { "main" });

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            _loggerMock.Verify(x => x.Log($"Conflict: nested/file.txt".Replace('/', Path.DirectorySeparatorChar)), Times.Once);
        }


        [Test]
        public void ShouldHandleMergeAfterMultipleCommits()
        {
            // Arrange: create multiple commits
            File.WriteAllText(Path.Combine(_testDir, "main1.txt"), "Commit 1");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main commit 1" });

            File.WriteAllText(Path.Combine(_testDir, "main2.txt"), "Commit 2");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main commit 2" });

            // Feature branch commits
            _switchBranchCommand.Execute(new[] { "featureBranch" });
            File.WriteAllText(Path.Combine(_testDir, "feature1.txt"), "Feature 1");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature commit 1" });

            File.WriteAllText(Path.Combine(_testDir, "feature2.txt"), "Feature 2");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature commit 2" });
            _switchBranchCommand.Execute(new[] { "main" });

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(_testDir, "main1.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(_testDir, "main2.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(_testDir, "feature1.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(_testDir, "feature2.txt")), Is.True);
            });
        }


        [Test]
        public void ShouldNotCreateCommitOnConflict()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "conflict.txt"), "Main version");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Main conflict commit" });

            _switchBranchCommand.Execute(new[] { "featureBranch" });
            File.WriteAllText(Path.Combine(_testDir, "conflict.txt"), "Feature version");
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Feature conflict commit" });
            _switchBranchCommand.Execute(new[] { "main" });

            var preMergeCommitCount = Directory.GetFiles(_paths.CommitDir).Length;

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            var postMergeCommitCount = Directory.GetFiles(_paths.CommitDir).Length;
            Assert.That(postMergeCommitCount, Is.EqualTo(preMergeCommitCount));
        }


        [Test]
        public void ShouldHandleBinaryFilesInMerge()
        {
            // Arrange: Create binary files
            var binaryFile = Path.Combine(_testDir, "image.jpg");
            File.WriteAllBytes(binaryFile, new byte[] { 0xFF, 0xD8, 0xFF }); // Invalid JPEG header

            // Commit to main
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Add binary file" });

            // Modify in feature branch
            _switchBranchCommand.Execute(new[] { "featureBranch" });
            File.WriteAllBytes(binaryFile, new byte[] { 0x89, 0x50, 0x4E }); // PNG header
            _addCommand.Execute(new[] { "--all" });
            _commitCommand.Execute(new[] { "Modify binary in feature" });
            _switchBranchCommand.Execute(new[] { "main" });

            // Act
            _mergeCommand.Execute(new[] { "featureBranch" });

            // Assert
            _loggerMock.Verify(x => x.Log("Merge conflicts detected:"), Times.Once);
            _loggerMock.Verify(x => x.Log("Conflict: image.jpg"), Times.Once);
        }



    }
}