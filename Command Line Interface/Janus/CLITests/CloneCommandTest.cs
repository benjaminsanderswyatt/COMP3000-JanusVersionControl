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
    public class CloneCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private CloneCommand _cloneCommand;
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

            _apiHelperMock = new Mock<IApiHelper>();

            _cloneCommand = new CloneCommand(_loggerMock.Object, _paths, _apiHelperMock.Object, _credManagerMock.Object);
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
        public async Task Execute_ExistingDirectory_LogsFailure()
        {
            // Arrange: create a directory with the same name as the repo
            string repoName = "existingRepo";
            string repoPath = Path.Combine(_paths.WorkingDir, repoName);
            Directory.CreateDirectory(repoPath);

            // Act
            await _cloneCommand.Execute(new string[] { $"janus/owner/{repoName}" });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains($"Failed clone: Directory named '{repoName}' already exists"))), Times.Once());

        }


        [Test]
        public async Task Execute_InvalidEndpoint_LogsError()
        {
            // Act: invalid endpoint format
            await _cloneCommand.Execute(new string[] { "janus/invalidEndpoint" });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Invalid endpoint format. Usage: janus/<Owner>/<Repository Name>"))), Times.Once());
        }


        [Test]
        public async Task Execute_MissingTreeData_LogsErrorAndCleansUp()
        {
            // Act
            await _cloneCommand.Execute(new string[] { "janus/owner/missingTreeRepo" });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Error cloning repository:"))), Times.Once());
            Assert.That(Directory.Exists("missingTreeRepo"), Is.False);
        }




























        [Test]
        public async Task Execute_SuccessfulClone_WithValidBranch()
        {
            // Arrange: valid CloneDto with a branch
            var cloneDto = new CloneDto
            {
                RepoName = "testRepo",
                RepoDescription = "Test repo description",
                IsPrivate = false,
                Branches = new List<BranchDto>
                {
                    new BranchDto
                    {
                        BranchName = "main",
                        ParentBranch = null,
                        SplitFromCommitHash = "0",
                        LatestCommitHash = "commit123",
                        CreatedBy = "testuser",
                        Created = DateTimeOffset.Now,
                        Commits = new List<CommitDto>
                        {
                            new CommitDto
                            {
                                CommitHash = "commit123",
                                ParentsCommitHash = new List<string>(),
                                AuthorName = "testuser",
                                AuthorEmail = "test@user.com",
                                Message = "Initial commit",
                                CommittedAt = DateTimeOffset.Now,
                                TreeHash = "tree123",
                                Tree = new TreeDto
                                {
                                    Name = "root",
                                    MimeType = "folder",
                                    Size = 0,
                                    LastModified = DateTimeOffset.Now,
                                    Children = new List<TreeDto>
                                    {
                                        new TreeDto
                                        {
                                            Name = "file.txt",
                                            Hash = "filehash123",
                                            MimeType = "text/plain",
                                            Size = 123,
                                            LastModified = DateTimeOffset.Now,
                                            Children = new List<TreeDto>()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            string jsonData = JsonSerializer.Serialize(cloneDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            string endpoint = "janus/owner/testRepo";

            // Api helper to return a successful response
            _apiHelperMock.Setup(a => a.SendGetAsync(_paths, endpoint, "testtoken"))
                          .ReturnsAsync((true, jsonData));

            // Setup file download
            _apiHelperMock.Setup(a => a.DownloadBatchFilesAsync(
                _paths,
                "owner",
                "testRepo",
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                "testtoken"))
                          .ReturnsAsync(true);

            // Act
            await _cloneCommand.Execute(new string[] { endpoint, "main" });

            // Assert: the repository folder was created
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains($"Repository 'testRepo' successfully cloned"))), Times.Once());

            string repoPath = Path.Combine(_paths.WorkingDir, "testRepo");
            Assert.IsTrue(Directory.Exists(repoPath));
        }


        [Test]
        public async Task Execute_SuccessfulClone_DefaultsToMain_WhenChosenBranchDoesNotExist()
        {
            // Arrange: create a CloneDto with only main branch
            var cloneDto = new CloneDto
            {
                RepoName = "testRepo",
                RepoDescription = "Test repo description",
                IsPrivate = false,
                Branches = new List<BranchDto>
                {
                    new BranchDto
                    {
                        BranchName = "main",
                        ParentBranch = null,
                        SplitFromCommitHash = "0",
                        LatestCommitHash = "commit123",
                        CreatedBy = "testuser",
                        Created = DateTimeOffset.Now,
                        Commits = new List<CommitDto>
                        {
                            new CommitDto
                            {
                                CommitHash = "commit123",
                                ParentsCommitHash = new List<string>(),
                                AuthorName = "testuser",
                                AuthorEmail = "test@user.com",
                                Message = "Initial commit",
                                CommittedAt = DateTimeOffset.Now,
                                TreeHash = "tree123",
                                Tree = new TreeDto
                                {
                                    Name = "root",
                                    MimeType = "folder",
                                    Size = 0,
                                    LastModified = DateTimeOffset.Now,
                                    Children = new List<TreeDto>()
                                }
                            }
                        }
                    }
                }
            };

            string jsonData = JsonSerializer.Serialize(cloneDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            string endpoint = "janus/owner/testRepo";

            // Api helper to return a successful response
            _apiHelperMock.Setup(a => a.SendGetAsync(_paths, endpoint, "testtoken"))
                          .ReturnsAsync((true, jsonData));

            // Setup file download
            _apiHelperMock.Setup(a => a.DownloadBatchFilesAsync(
                _paths,
                "owner",
                "testRepo",
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                "testtoken"))
                          .ReturnsAsync(true);

            // Act: Give a branch that does not exist (nonexistent)
            await _cloneCommand.Execute(new string[] { endpoint, "nonexistent" });

            // Assert: default to main branch
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Your chosen branch doesnt exist. Defaulting to main..."))), Times.Once());

            string repoPath = Path.Combine(_paths.WorkingDir, "testRepo");
            Assert.IsTrue(Directory.Exists(repoPath));
        }


        [Test]
        public async Task Execute_MissingCredentials_LogsLoginMessage()
        {
            // Arrange: clear credentials before execution
            _credManagerMock.Reset();

            // Act
            await _cloneCommand.Execute(new string[] { "janus/owner/testRepo" });

            // Assert: logger should notify the user to login
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Please login first. Usage: janus login"))), Times.Once());
        }

        [Test]
        public async Task Execute_ExceptionDuringClone_CleansUpAndLogsError()
        {
            // Arrange: simulate an error in Get
            string endpoint = "janus/owner/testRepo";
            _apiHelperMock.Setup(a => a.SendGetAsync(_paths, endpoint, "testtoken"))
                          .ThrowsAsync(new Exception("API failure"));

            // Act
            await _cloneCommand.Execute(new string[] { endpoint, "main" });

            // Assert: should log no repo directory exists
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("An error occurred during cloning: API failure"))), Times.Once());
            string repoPath = Path.Combine(_paths.WorkingDir, "testRepo");
            Assert.IsFalse(Directory.Exists(repoPath));
        }
    }
}
