using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using static Janus.CommandHandler;
using static Janus.Helpers.FileMetadataHelper;

namespace CLITests
{

    [TestFixture]
    public class TreeTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private TreeBuilder _treeBuilder;


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

            // Login with test user
            var credManager = new CredentialManager();
            var testCredentials = new UserCredentials
            {
                Username = "testuser",
                Email = "test@user.com",
                Token = "testtoken"
            };

            credManager.SaveCredentials(testCredentials);

            // Initialize the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);

            _treeBuilder = new TreeBuilder(_paths);
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
            var credManager = new CredentialManager();
            credManager.ClearCredentials();
        }


        [Test]
        public void ShouldBuildTreeFromDictionary()
        {
            // Arrange
            var testTime = DateTimeOffset.UtcNow;
            var index = new Dictionary<string, FileMetadata>
            {
                { "file1.txt", new FileMetadata { Hash = "hash1", MimeType = "text/plain", Size = 1024, LastModified = testTime } },
                { "dir1/file2.txt", new FileMetadata { Hash = "hash2", MimeType = "text/svg+xml", Size = 2048, LastModified = testTime } },
                { "dir1/file3.txt", new FileMetadata { Hash = "hash3", MimeType = "text/pdf", Size = 3072, LastModified = testTime } },
                { "dir2/file4.txt", new FileMetadata { Hash = "hash4", MimeType = "application/octet-stream", Size = 4096, LastModified = testTime } },
                { "dir2/subdir1/file5.txt", new FileMetadata { Hash = "hash5", MimeType = "text/markdown", Size = 512, LastModified = testTime } }
            };



            // Act
            var root = _treeBuilder.BuildTreeFromDiction(index);


            _treeBuilder.PrintTree();

            // Assert
            Assert.Multiple(() =>
            {
                // Root directory
                Assert.That(root.Name, Is.EqualTo("root"));
                Assert.That(root.MimeType, Is.EqualTo("inode/directory"));
                Assert.That(root.Size, Is.EqualTo(1024 + 2048 + 3072 + 4096 + 512));
                Assert.That(root.Children, Has.Count.EqualTo(3));
            });

            // File1 checks
            var file1 = root.Children.First(c => c.Name == "file1.txt");
            Assert.Multiple(() =>
            {
                Assert.That(file1.Hash, Is.EqualTo("hash1"));
                Assert.That(file1.MimeType, Is.EqualTo("text/plain"));
                Assert.That(file1.Size, Is.EqualTo(1024));
                Assert.That(file1.LastModified, Is.EqualTo(testTime));
            });

            // Dir1 checks
            var dir1 = root.Children.First(c => c.Name == "dir1");
            Assert.Multiple(() =>
            {
                Assert.That(dir1.Hash, Is.Null);
                Assert.That(dir1.MimeType, Is.EqualTo("inode/directory"));
                Assert.That(dir1.Size, Is.EqualTo(2048 + 3072));
                Assert.That(dir1.Children, Has.Count.EqualTo(2));
            });

            // Verify that the last modified for dir is latest of its children
            Assert.That(dir1.LastModified, Is.EqualTo(
                index["dir1/file2.txt"].LastModified > index["dir1/file3.txt"].LastModified
                    ? index["dir1/file2.txt"].LastModified
                    : index["dir1/file3.txt"].LastModified
            ));

            // Dir1 files
            var file2 = dir1.Children.First(c => c.Name == "file2.txt");
            Assert.Multiple(() =>
            {
                Assert.That(file2.Hash, Is.EqualTo("hash2"));
                Assert.That(file2.MimeType, Is.EqualTo("text/svg+xml"));
                Assert.That(file2.Size, Is.EqualTo(2048));
            });

            var file3 = dir1.Children.First(c => c.Name == "file3.txt");
            Assert.Multiple(() =>
            {
                Assert.That(file3.Hash, Is.EqualTo("hash3"));
                Assert.That(file3.MimeType, Is.EqualTo("text/pdf"));
                Assert.That(file3.Size, Is.EqualTo(3072));
            });

            // Dir2 checks
            var dir2 = root.Children.First(c => c.Name == "dir2");
            Assert.Multiple(() =>
            {
                Assert.That(dir2.Hash, Is.Null);
                Assert.That(dir2.MimeType, Is.EqualTo("inode/directory"));
                Assert.That(dir2.Size, Is.EqualTo(4096 + 512));
                Assert.That(dir2.Children, Has.Count.EqualTo(2));
            });

            // Dir2 files
            var file4 = dir2.Children.First(c => c.Name == "file4.txt");
            Assert.Multiple(() =>
            {
                Assert.That(file4.Hash, Is.EqualTo("hash4"));
                Assert.That(file4.MimeType, Is.EqualTo("application/octet-stream"));
                Assert.That(file4.Size, Is.EqualTo(4096));
            });

            // Subdir1 checks
            var subdir1 = dir2.Children.First(c => c.Name == "subdir1");
            Assert.Multiple(() =>
            {
                Assert.That(subdir1.Hash, Is.Null);
                Assert.That(subdir1.MimeType, Is.EqualTo("inode/directory"));
                Assert.That(subdir1.Size, Is.EqualTo(512));
                Assert.That(subdir1.Children, Has.Count.EqualTo(1));
            });

            // File5 checks
            var file5 = subdir1.Children.First(c => c.Name == "file5.txt");
            Assert.Multiple(() =>
            {
                Assert.That(file5.Hash, Is.EqualTo("hash5"));
                Assert.That(file5.MimeType, Is.EqualTo("text/markdown"));
                Assert.That(file5.Size, Is.EqualTo(512));
            });

        }


        [Test]
        public void ShouldSaveTree()
        {
            // Arrange
            var testTime = DateTimeOffset.UtcNow;
            var index = new Dictionary<string, FileMetadata>
            {
                { "file1.txt", new FileMetadata { Hash = "hash1", MimeType = "text/plain", Size = 1024, LastModified = testTime } },
                { "file2.txt", new FileMetadata { Hash = "hash2", MimeType = "text/plain", Size = 2048, LastModified = testTime } },
                { "file3.txt", new FileMetadata { Hash = "hash3", MimeType = "application/octet-stream", Size = 3072, LastModified = testTime } }
            };

            var root = _treeBuilder.BuildTreeFromDiction(index);


            // Act
            var rootHash = _treeBuilder.SaveTree();

            // Assert
            var treeFilePath = Path.Combine(_paths.TreeDir, rootHash);
            Assert.IsTrue(File.Exists(treeFilePath));

            var treeContent = File.ReadAllLines(treeFilePath);

            Assert.Multiple(() =>
            {
                Assert.That(treeContent, Has.Length.EqualTo(3));
                Assert.That(treeContent, Contains.Item($"blob|file1.txt|text/plain|1024|hash1|{testTime.ToString("o")}"));
                Assert.That(treeContent, Contains.Item($"blob|file2.txt|text/plain|2048|hash2|{testTime.ToString("o")}"));
                Assert.That(treeContent, Contains.Item($"blob|file3.txt|application/octet-stream|3072|hash3|{testTime.ToString("o")}"));
            });

        }

        [Test]
        public void ShouldSaveTree_WithFolders()
        {
            // Arrange
            var testTime = DateTimeOffset.UtcNow;
            var index = new Dictionary<string, FileMetadata>
            {
                { "dir1/file.txt", new FileMetadata { Hash = "hash1", MimeType = "text/plain", Size = 1024, LastModified = testTime } }
            };

            var root = _treeBuilder.BuildTreeFromDiction(index);

            // Act
            var rootHash = _treeBuilder.SaveTree();

            // Assert
            var treeContent = File.ReadAllLines(Path.Combine(_paths.TreeDir, rootHash));
            var dirLine = treeContent.First(l => l.StartsWith("tree|dir1|"));

            Assert.Multiple(() =>
            {
                Assert.That(dirLine, Does.StartWith("tree|dir1|inode/directory|1024|"));
                Assert.That(dirLine.Split('|'), Has.Length.EqualTo(6));
            });
        }





        [Test]
        public void ShouldRebuildTreeFromHash()
        {
            // Arrange
            var testTime = DateTimeOffset.UtcNow;
            var index = new Dictionary<string, FileMetadata>
            {
                { "file1.txt", new FileMetadata { Hash = "hash1", MimeType = "text/plain", Size = 1024, LastModified = testTime } },
                { "dir1/file2.txt", new FileMetadata { Hash = "hash2", MimeType = "image/png", Size = 2048, LastModified = testTime } }
            };

            var root = _treeBuilder.BuildTreeFromDiction(index);
            var rootHash = _treeBuilder.SaveTree();

            // Act
            var recreatedTree = _treeBuilder.RecreateTree(_loggerMock.Object, rootHash);

            // Assert
            Assert.Multiple(() =>
            {
                // Root
                Assert.That(recreatedTree.Name, Is.EqualTo("root"));
                Assert.That(recreatedTree.MimeType, Is.EqualTo("inode/directory"));
                Assert.That(recreatedTree.Size, Is.EqualTo(1024 + 2048));

                // File1
                var file1 = recreatedTree.Children.First(c => c.Name == "file1.txt");
                Assert.That(file1.Hash, Is.EqualTo("hash1"));
                Assert.That(file1.MimeType, Is.EqualTo("text/plain"));
                Assert.That(file1.Size, Is.EqualTo(1024));
                Assert.That(file1.LastModified, Is.EqualTo(testTime));

                // Dir1
                var dir1 = recreatedTree.Children.First(c => c.Name == "dir1");
                Assert.That(dir1.MimeType, Is.EqualTo("inode/directory"));
                Assert.That(dir1.Size, Is.EqualTo(2048));

                // File2
                var file2 = dir1.Children.First(c => c.Name == "file2.txt");
                Assert.That(file2.Hash, Is.EqualTo("hash2"));
                Assert.That(file2.MimeType, Is.EqualTo("image/png"));
                Assert.That(file2.Size, Is.EqualTo(2048));
            });
        }


        [Test]
        public void ShouldCompareTrees()
        {
            // Arrange
            var diction1 = new Dictionary<string, FileMetadata>
            {
                { "file1.txt", new FileMetadata { Hash = "hash1", MimeType = "text/plain", Size = 1024 } },
                { "dir1/file2.txt", new FileMetadata { Hash = "hash2", MimeType = "image/png", Size = 2048 } }
            };

            var diction2 = new Dictionary<string, FileMetadata>
            {
                { "file1.txt", new FileMetadata { Hash = "hash1", MimeType = "text/plain", Size = 1024 } },
                { "dir1/file2.txt", new FileMetadata { Hash = "hash2mod", MimeType = "image/jpeg", Size = 3072 } },
                { "newfile.txt", new FileMetadata { Hash = "hash3", MimeType = "text/csv", Size = 512 } }
            };

            var tree1 = _treeBuilder.BuildTreeFromDiction(diction1);

            var tree2Builder = new TreeBuilder(_paths);
            var tree2 = tree2Builder.BuildTreeFromDiction(diction2);


            // Act
            var comparisonResult = Tree.CompareTrees(tree1, tree2);


            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(comparisonResult.AddedOrUntracked, Has.Count.EqualTo(1));
                Assert.That(comparisonResult.ModifiedOrNotStaged, Has.Count.EqualTo(1));
                Assert.That(comparisonResult.Deleted, Has.Count.EqualTo(0));

                // Added file
                Assert.That(comparisonResult.AddedOrUntracked, Contains.Item("newfile.txt".Replace('/', Path.DirectorySeparatorChar)));

                // Modified file
                var modifiedPath = "dir1/file2.txt".Replace('/', Path.DirectorySeparatorChar);
                Assert.That(comparisonResult.ModifiedOrNotStaged, Contains.Item(modifiedPath));
            });
        }


    }


}