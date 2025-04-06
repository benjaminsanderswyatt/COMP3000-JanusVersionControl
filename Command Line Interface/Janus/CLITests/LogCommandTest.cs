using Janus;
using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class LogCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private LogCommand _logCommand;
        private Mock<ICredentialManager> _credManagerMock;

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


            // Create Log Command instance
            _logCommand = new LogCommand(_loggerMock.Object, _paths);


        }



        private string CreateManyCommits(int howMany, string branch, string authorName, string authorEmail, string startParentCommitHash = null, int seperator = 0)
        {
            string parentCommitHash = startParentCommitHash;

            for (int num = 1; num < howMany + 1; num++)
            {
                // Generate commit metadata   $"treeHash{num}", $"commitMessage{num}{seperator}"
                string commitHash = HashHelper.ComputeCommitHash($"parentHash{num}", $"branchName{num}", $"authorName{seperator}", $"authorEmail{num}", DateTime.UtcNow, $"commitMessage{num}{seperator}", $"treeHash{num}");

                string commitMetadata = MiscHelper.GenerateCommitMetadata(branch, $"commitHash{num}", $"treeHash{num}", $"commitMessage{num}", new List<string> { parentCommitHash }, authorName, authorEmail);

                // Save commit object
                string commitFilePath = Path.Combine(_paths.CommitDir, commitHash);
                File.WriteAllText(commitFilePath, commitMetadata);

                // Update head to point to the new commit
                HeadHelper.SetHeadCommit(_paths, commitHash);

                parentCommitHash = commitHash; // Advance parent commit for the loop
            }

            return parentCommitHash;
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

            // Clean up credentials
            _credManagerMock.Reset();
        }



        [Test]
        public void ShouldLogError_WhenRepoNotInitialised()
        {
            // Arrange: Delete the .janus directory
            Directory.SetCurrentDirectory(Path.GetTempPath());
            Directory.Delete(_testDir, true);
            var args = new string[] { };

            // Act
            _logCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise a repository"), Times.Once);
        }



        [Test]
        public void ShouldLogError_WhenNoCommitDir()
        {
            // Arrange: Delete the commit directory
            Directory.Delete(_paths.CommitDir, true);
            var args = new string[] { };

            // Act
            _logCommand.Execute(args).Wait();

            // Assert
            _loggerMock.Verify(logger => logger.Log("Error: Commit directory doesn't exist."), Times.Once);
        }




        [Test]
        public void ShouldLogError_WhenNoCommitFilesExist()
        {
            // Arrange: Delete all commit files (Shouldnt happend as repo inits with initial commit)
            foreach (var file in Directory.GetFiles(_paths.CommitDir))
            {
                File.Delete(file);
            }

            var args = new string[] { };

            // Act
            _logCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Error: No commits found. Repository might not have been initialised correctly."), Times.Once);
        }



        [Test]
        public void ShouldDisplayCommits_WhenCommitsExist()
        {
            // Arrange: Create 5 commits
            var (initCommitHash, commitMetadata) = MiscHelper.CreateInitData();

            CreateManyCommits(5, "main", "testAuthorName", "testAuthorEmail", initCommitHash);
            var args = new string[] { };

            // Act
            _logCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Commit:"))), Times.Exactly(6)); // 5 commits + initial commit
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Author:"))), Times.Exactly(6));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Date:"))), Times.Exactly(6));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Branch:"))), Times.Exactly(6));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Message:"))), Times.Exactly(6));


            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Initial commit"))), Times.Exactly(1));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("commitMessage1"))), Times.Exactly(1));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("commitMessage2"))), Times.Exactly(1));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("commitMessage3"))), Times.Exactly(1));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("commitMessage4"))), Times.Exactly(1));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("commitMessage5"))), Times.Exactly(1));

            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("testAuthorName"))), Times.Exactly(5));

            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("main"))), Times.Exactly(6)); // 5 commits + initial commit

        }


        [Test]
        public void ShouldFilterByBranch_WhenBranchIsProvided()
        {
            // Arrange: Create commits on two branches
            string finalCommitHash = CreateManyCommits(3, "main", "testAuthor", "testAuthorEmail");
            CreateManyCommits(2, "feature", "testAuthor", "testAuthorEmail", finalCommitHash, 1);
            var args = new string[] { "--branch", "feature" };

            // Act
            _logCommand.Execute(args);

            // Assert: Verify only feature branch commits are displayed
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Branch:  feature"))), Times.Exactly(2));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Branch:  main"))), Times.Never);
        }

        [Test]
        public void ShouldFilterByAuthor_WhenAuthorIsProvided()
        {
            // Arrange: Create commits with two authors
            string finalCommitHash = CreateManyCommits(3, "main", "testAuthor1", "testAuthorEmail1");
            CreateManyCommits(2, "main", "testAuthor2", "testAuthorEmail2", finalCommitHash, 1);
            var args = new string[] { "--author", "testAuthor1" };

            // Act
            _logCommand.Execute(args);

            // Assert: Verify only testAuthor1 commits are displayed
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Author:  testAuthor1"))), Times.Exactly(3));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Author:  testAuthor2"))), Times.Never);

            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("testAuthorEmail1"))), Times.Exactly(3));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("testAuthorEmail2"))), Times.Never);
        }

        [Test]
        public void ShouldFilterUntilDate_WhenUntilIsProvided()
        {
            // Arrange
            string finalCommitHash = CreateManyCommits(3, "main", "testAuthor", "testAuthorEmail");

            // Wait to create some time between commits
            Thread.Sleep(1000);
            DateTime later = DateTime.UtcNow;
            Thread.Sleep(1000);

            CreateManyCommits(2, "2nd", "testAuthor", "testAuthorEmail", finalCommitHash, 1);

            var args = new string[] { "--until", $"{later}" }; // Only commits before this date should be displayed

            // Act
            _logCommand.Execute(args);

            // Assert: Verify only dates happening before are displayed
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Date:"))), Times.Exactly(4));
        }



        [Test]
        public void ShouldFilterSinceDate_WhenSinceIsProvided()
        {
            // Arrange
            string finalCommitHash = CreateManyCommits(3, "main", "testAuthor", "testAuthorEmail");

            // Wait to create some time between commits
            Thread.Sleep(1000);
            DateTime later = DateTime.UtcNow;
            Thread.Sleep(1000);

            CreateManyCommits(2, "main", "testAuthor", "testAuthorEmail", finalCommitHash, 1);

            var args = new string[] { "--since", $"{later}" }; // Only commits before this date should be displayed

            // Act
            _logCommand.Execute(args);

            // Assert: Verify only dates happening after are displayed
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Date:"))), Times.Exactly(2));
        }



        [Test]
        public void ShouldLimitLog_WhenLimitIsProvided()
        {
            // Arrange
            CreateManyCommits(10, "main", "testAuthor", "testAuthorEmail");

            var args = new string[] { "--limit", "5" }; // Only commits this many commits should be displayed

            // Act
            _logCommand.Execute(args);

            // Assert: Verify only dates happening after are displayed
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Commit:"))), Times.Exactly(5));
        }


    }
}