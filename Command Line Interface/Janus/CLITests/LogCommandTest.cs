using Janus;
using Janus.Helpers;
using Janus.Plugins;
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
            _logCommand = new LogCommand(_loggerMock.Object, _paths);
        }

        private void CreateManyCommits(int howMany, string branch, string author, int howManyFiles)
        {
            for (int num = 1; num < howMany + 1; num++)
            {
                Dictionary<string, string> fileHashes = new Dictionary<string, string>();


                // Generate commit metadata
                string commitHash = CommandHelper.ComputeCommitHash(fileHashes, $"commitMessage{num}");

                string? parent = $"commitHash{num - 1}";
                if (num - 1 == 0)
                {
                    parent = "4A35387BE739933F7C9E6486959EC1AFFB2C1648"; // Initial commit hash
                }

                string commitMetadata = CommandHelper.GenerateCommitMetadata(branch, $"commitHash{num}", fileHashes, $"commitMessage{num}", parent, author);

                // Save commit object
                string commitFilePath = Path.Combine(_paths.CommitDir, commitHash);
                File.WriteAllText(commitFilePath, commitMetadata);

                // Update head to point to the new commit
                HeadHelper.SetHeadCommit(_paths, commitHash);
            }
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
        public void ShouldLogError_WhenRepoNotInitialised()
        {
            // Arrange: Delete the .janus directory
            Directory.SetCurrentDirectory(Path.GetTempPath());
            Directory.Delete(_testDir, true);
            var args = new string[] { };

            // Act
            _logCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Error commit directory doesnt exist."), Times.Once);
        }



        [Test]
        public void ShouldLogError_WhenNoCommitDir()
        {
            // Arrange: Delete the commit directory
            Directory.Delete(_paths.CommitDir, true);
            var args = new string[] { };

            // Act
            _logCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Error commit directory doesnt exist."), Times.Once);
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
            _loggerMock.Verify(logger => logger.Log("Error no commits found. Repository might not have initialized correctly."), Times.Once);
        }



        [Test]
        public void ShouldDisplayCommits_WhenCommitsExist()
        {
            // Arrange: Create 5 commits
            CreateManyCommits(5, "main", "testAuthor", 0);
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

            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("testAuthor"))), Times.Exactly(5));

            _loggerMock.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("main"))), Times.Exactly(6)); // 5 commits + initial commit

        }


        [Test]
        public void ShouldFilterByBranch_WhenBranchIsProvided()
        {

        }

        [Test]
        public void ShouldFilterByAuthor_WhenAuthorIsProvided()
        {

        }

        [Test]
        public void ShouldFilterUntilDate_WhenUntilIsProvided()
        {

        }

        [Test]
        public void ShouldFilterSinceDate_WhenSinceIsProvided()
        {

        }


        [Test]
        public void ShouldLimitLog_WhenLimitIsProvided()
        {

        }


        [Test]
        public void ShouldDisplayFilesLog_WhenVerboseIsTrue()
        {

        }




    }
}