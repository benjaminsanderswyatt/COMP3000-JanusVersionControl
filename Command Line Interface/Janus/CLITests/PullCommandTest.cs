using Janus.Helpers;
using Janus.Helpers.CommandHelpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{

    [TestFixture]
    public class PullCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private PullCommand _pullCommand;
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


            // Create Command instances
            _pullCommand = new PullCommand(_loggerMock.Object, _paths);
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

        // Create mock remote branch
        private void SetupRemoteBranch(string branchName, string headCommit)
        {
            var remoteBranchDir = Path.Combine(_paths.RemoteDir, branchName);
            Directory.CreateDirectory(remoteBranchDir);
            File.WriteAllText(Path.Combine(remoteBranchDir, "head"), headCommit);
        }

        // Create mock local branch
        private void SetupLocalBranch(string branchName, string headCommit)
        {
            var localBranchDir = Path.Combine(_paths.BranchesDir, branchName);
            Directory.CreateDirectory(localBranchDir);
            File.WriteAllText(Path.Combine(localBranchDir, "head"), headCommit);
        }

        private static string CreateCommit(Paths paths, string parentCommit, string message, string treeHash)
        {
            var commit = new CommitMetadata
            {
                Commit = Guid.NewGuid().ToString("N"),
                Parents = parentCommit != null ? new List<string> { parentCommit } : new List<string>(),
                Date = DateTime.UtcNow,
                Message = message,
                Tree = treeHash,
                Branch = MiscHelper.GetCurrentBranchName(paths),
                AuthorName = MiscHelper.GetUsername(),
                AuthorEmail = MiscHelper.GetEmail()
            };

            SaveCommit(paths, commit);
            return commit.Commit;
        }

        private static void SaveCommit(Paths paths, CommitMetadata commit)
        {
            string commitPath = Path.Combine(paths.CommitDir, commit.Commit);
            File.WriteAllText(commitPath, JsonSerializer.Serialize(commit));
        }




























        [Test]
        public void ShouldLogError_WhenRepoNotInitialized()
        {
            Directory.Delete(_paths.JanusDir, true);
            _pullCommand.Execute(new string[] { "--force" });
            _loggerMock.Verify(x => x.Log("Not a janus repository. Use 'init' command to initialise a repository"), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenRemoteDataMissing()
        {
            _pullCommand.Execute(new string[] { "--force" });
            _loggerMock.Verify(x => x.Log("Remote repository data not found. Use 'janus fetch'"), Times.Once);
        }


        [Test]
        public void ShouldHandleUpToDateBranches()
        {
            // Setup remote branch data
            var branchName = "main";
            SetupRemoteBranch(branchName, "commit123");
            SetupLocalBranch(branchName, "commit123");

            _pullCommand.Execute(new string[] { "--force" });

            _loggerMock.Verify(x => x.Log("  - Branch 'main': up to date"), Times.Once);
        }


        [Test]
        public void ShouldDetectBehindStatus()
        {
            // Create initial commit
            var initialCommit = CreateCommit(_paths, null, "Initial commit", "tree0");
            SetupLocalBranch("main", initialCommit);

            // Create remote commits
            var remoteCommit1 = CreateCommit(_paths, initialCommit, "Remote commit 1", "tree1");
            var remoteCommit2 = CreateCommit(_paths, remoteCommit1, "Remote commit 2", "tree2");
            SetupRemoteBranch("main", remoteCommit2);

            _pullCommand.Execute(new string[] { "--force" });

            // Verify the exact expected log message
            _loggerMock.Verify(x => x.Log("  - Branch 'main': will fast forward by 2 commit(s)"), Times.Once);
        }


        [Test]
        public void ShouldDetectDivergedBranches()
        {
            var branchName = "dev";
            SetupRemoteBranch(branchName, "remoteCommit");
            SetupLocalBranch(branchName, "localCommit");

            // Mock diverged status
            var mockStatus = new StatusHelper.SyncStatus
            {
                FoundLocalInRemote = false,
                FoundRemoteInLocal = false
            };

            _pullCommand.Execute(new string[] { "--force" });
            _loggerMock.Verify(x => x.Log("  - Branch 'dev': diverged a merge is required"), Times.Once);
        }





















    }



}
