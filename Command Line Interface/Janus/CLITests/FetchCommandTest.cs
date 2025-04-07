using Janus.API;
using Janus.DataTransferObjects;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class FetchCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private FetchCommand _fetchCommand;
        private RemoteManager _remoteManager;
        private Mock<IApiHelper> _apiHelperMock;
        private Mock<ICredentialManager> _credManagerMock;

        private string _testDir;


        [SetUp]
        public void Setup()
        {
            // Create a mock logger
            _loggerMock = new Mock<ILogger>();

            // Create a temp dir for testing
            _testDir = Path.Combine(Path.GetTempPath(), "JanusTest");
            Directory.CreateDirectory(_testDir);
            _paths = new Paths(_testDir);

            // Save test credentials
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

            _apiHelperMock = new Mock<IApiHelper>();

            _remoteManager = new RemoteManager(_loggerMock.Object, _paths, _apiHelperMock.Object);

            _fetchCommand = new FetchCommand(_loggerMock.Object, _paths, _apiHelperMock.Object, _credManagerMock.Object);
        }


        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory(Path.GetTempPath());
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }

            _credManagerMock.Reset();
        }























        [Test]
        public async Task Execute_MissingCredentials_LogsError()
        {
            // Arrange
            _credManagerMock.Setup(m => m.LoadCredentials()).Returns((UserCredentials)null);

            // Act
            await _fetchCommand.Execute(Array.Empty<string>());

            // Assert
            _loggerMock.Verify(l => l.Log("Please login first. janus login"), Times.Once);
        }


        [Test]
        public async Task Execute_InvalidRemote_LogsError()
        {
            // Arrange
            _remoteManager.AddRemote(new[] { "add", "origin", "janus/owner/repo" }, _credManagerMock.Object.LoadCredentials()).Wait();

            // Act
            await _fetchCommand.Execute(new[] { "invalidRemote" });

            // Assert
            _loggerMock.Verify(l => l.Log("Remote 'invalidRemote' configuration was not found"), Times.Once);
        }


        [Test]
        public async Task Execute_SuccessfulFetch_UpdatesRepository()
        {
            // Arrange
            var testCommits = new List<CommitDto>
            {
                new CommitDto
                {
                    CommitHash = "commit123",
                    TreeHash = "tree123",
                    Tree = new TreeDto
                    {
                        Name = "root",
                        Children = new List<TreeDto>()
                    }
                }
            };

            var cloneDto = new CloneDto
            {
                Branches = new List<BranchDto>
                {
                    new BranchDto
                    {
                        BranchName = "main",
                        Commits = testCommits,
                        LatestCommitHash = "commit123"
                    }
                },
                IsPrivate = false,
                RepoDescription = "Test repo"
            };

            _apiHelperMock.Setup(a => a.SendPostAsync(
                    _paths,
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .ReturnsAsync((true, JsonSerializer.Serialize(cloneDto)));

            _apiHelperMock.Setup(a => a.DownloadBatchFilesAsync(
                    _paths,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);

            // Add valid remote
            var mockResponse = new RemoteHeadDto
            {
                Heads = new Dictionary<string, string> { { "main", "abc123" } }
            };
            _apiHelperMock
                .Setup(api => api.SendGetAsync(_paths, $"/cli/repo/janus/owner/repo/head", "testtoken"))
                .ReturnsAsync((true, JsonSerializer.Serialize(mockResponse)));

            await _remoteManager.AddRemote(new[] { "add", "origin", "janus/owner/repo" }, _credManagerMock.Object.LoadCredentials());

            // Act
            await _fetchCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(l => l.Log("Fetch completed successfully"), Times.Once);
            Assert.IsTrue(File.Exists(Path.Combine(_paths.RemoteDir, "main", "head")));
        }


        [Test]
        public async Task Execute_FailedApiCall_LogsError()
        {
            // Arrange
            _apiHelperMock.Setup(a => a.SendPostAsync(
                    _paths,
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .ReturnsAsync((false, "API error"));

            // Add remote
            var mockResponse = new RemoteHeadDto
            {
                Heads = new Dictionary<string, string> { { "main", "abc123" } }
            };
            _apiHelperMock
                .Setup(api => api.SendGetAsync(_paths, $"/cli/repo/janus/owner/repo/head", "testtoken"))
                .ReturnsAsync((true, JsonSerializer.Serialize(mockResponse)));

            await _remoteManager.AddRemote(new[] { "add", "origin", "janus/owner/repo" }, _credManagerMock.Object.LoadCredentials());

            // Act
            await _fetchCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(l => l.Log("Fetch failed: API error"), Times.Once);
        }


        [Test]
        public async Task Execute_MissingFiles_LogsDownloadError()
        {
            // Arrange
            var cloneDto = new CloneDto
            {
                Branches = new List<BranchDto>
                {
                    new BranchDto
                    {
                        BranchName = "main",
                        Commits = new List<CommitDto>
                        {
                            new CommitDto
                            {
                                CommitHash = "commit123",
                                TreeHash = "tree123",
                                Tree = new TreeDto
                                {
                                    Name = "root",
                                    Children = new List<TreeDto>
                                    {
                                        new TreeDto { Hash = "missing-file" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _apiHelperMock.Setup(a => a.SendPostAsync(
                    _paths,
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .ReturnsAsync((true, JsonSerializer.Serialize(cloneDto)));

            _apiHelperMock.Setup(a => a.DownloadBatchFilesAsync(
                    _paths,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(false);

            // Add remote
            var mockResponse = new RemoteHeadDto
            {
                Heads = new Dictionary<string, string> { { "main", "abc123" } }
            };
            _apiHelperMock
                .Setup(api => api.SendGetAsync(_paths, $"/cli/repo/janus/owner/repo/head", "testtoken"))
                .ReturnsAsync((true, JsonSerializer.Serialize(mockResponse)));

            await _remoteManager.AddRemote(new[] { "add", "origin", "janus/owner/repo" }, _credManagerMock.Object.LoadCredentials());

            // Act
            await _fetchCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(l => l.Log("Error downloading some file objects"), Times.Once);
        }
    }
}