using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Moq;
using System.Text.Json;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class TreeTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private Dictionary<string, object> _tree;

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

            // Initialize the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);

            _tree = new Dictionary<string, object>();
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
        public void ShouldAddSingleFileToTree()
        {
            // Arrange
            string[] pathParts = { "file.txt" };
            string fileHash = "12345";

            // Act
            TreeHelper.AddToTreeRecursive(_tree, pathParts, fileHash);

            // Assert
            Assert.IsTrue(_tree.ContainsKey("file.txt"));
            Assert.AreEqual(fileHash, _tree["file.txt"]);
        }


        [Test]
        public void ShouldAddSingleFilePathToTree()
        {
            // Arrange
            string[] pathParts = { "folder", "subfolder", "file.txt" };
            string fileHash = "12345";

            // Act
            TreeHelper.AddToTreeRecursive(_tree, pathParts, fileHash);

            // Assert
            Assert.IsTrue(_tree.ContainsKey("folder"));
            var subTree = _tree["folder"] as Dictionary<string, object>;

            Assert.IsNotNull(subTree);
            Assert.IsTrue(subTree.ContainsKey("subfolder"));

            var subSubTree = subTree["subfolder"] as Dictionary<string, object>;

            Assert.IsNotNull(subSubTree);
            Assert.AreEqual(fileHash, subSubTree["file.txt"]);

        }


        [Test]
        public void ShouldAddMultipleFilesToTree()
        {
            // Arrange
            string[] pathParts1 = { "folder1", "subfolder1", "file1.txt" };
            string[] pathParts2 = { "folder1", "subfolder1", "file2.txt" };
            string[] pathParts3 = { "folder2", "file3.txt" };
            string[] pathParts4 = { "folder2", "subfolder2", "subsubfolder", "file4.txt" };

            string fileHash1 = "12345";
            string fileHash2 = "67890";
            string fileHash3 = "abcde";
            string fileHash4 = "fghij";

            // Act
            TreeHelper.AddToTreeRecursive(_tree, pathParts1, fileHash1);
            TreeHelper.AddToTreeRecursive(_tree, pathParts2, fileHash2);
            TreeHelper.AddToTreeRecursive(_tree, pathParts3, fileHash3);
            TreeHelper.AddToTreeRecursive(_tree, pathParts4, fileHash4);

            // Assert
            // file 1 & file 2
            Assert.IsTrue(_tree.ContainsKey("folder1"));
            var folder1Tree = _tree["folder1"] as Dictionary<string, object>;
            Assert.IsNotNull(folder1Tree);

            Assert.IsTrue(folder1Tree.ContainsKey("subfolder1"));
            var subfolder1Tree = folder1Tree["subfolder1"] as Dictionary<string, object>;
            Assert.IsNotNull(subfolder1Tree);

            Assert.AreEqual(fileHash1, subfolder1Tree["file1.txt"]);
            Assert.AreEqual(fileHash2, subfolder1Tree["file2.txt"]);

            // file 3 & file 4
            Assert.IsTrue(_tree.ContainsKey("folder2"));
            var folder2Tree = _tree["folder2"] as Dictionary<string, object>;
            Assert.IsNotNull(folder2Tree);

            Assert.AreEqual(fileHash3, folder2Tree["file3.txt"]);

            Assert.IsTrue(folder2Tree.ContainsKey("subfolder2"));
            var subfolder2Tree = folder2Tree["subfolder2"] as Dictionary<string, object>;
            Assert.IsNotNull(subfolder2Tree);

            Assert.IsTrue(subfolder2Tree.ContainsKey("subsubfolder"));
            var subsubfolderTree = subfolder2Tree["subsubfolder"] as Dictionary<string, object>;
            Assert.IsNotNull(subsubfolderTree);

            Assert.AreEqual(fileHash4, subsubfolderTree["file4.txt"]);
        }



        [Test]
        public void ShouldGetHashFromTree_WhenGivenFilePath()
        {
            // Arrange
            TreeHelper.AddToTreeRecursive(_tree, new[] { "folder", "file.txt" }, "12345");

            // Act
            string fileHash = TreeHelper.GetHashFromTree(_tree, "folder/file.txt".Replace('/', Path.DirectorySeparatorChar));

            // Assert
            Assert.AreEqual("12345", fileHash);
        }


        [Test]
        public void ShouldReturnNull_WhenGivenNonExistentFilePath()
        {
            // Act
            string fileHash = TreeHelper.GetHashFromTree(_tree, "nonexistent/file.txt".Replace('/', Path.DirectorySeparatorChar));

            // Assert
            Assert.IsNull(fileHash);
        }


        [Test]
        public void ShouldGetTreeFromCommitMetadata_GivenCommitHash()
        {
            // Arrange: create a tree and save it in treeDir
            string commitHash = "abc123";
            string treeHash = "tree123";

            var commitMetadata = new CommitMetadata { Tree = treeHash };
            File.WriteAllText(Path.Combine(_paths.CommitDir, commitHash), JsonSerializer.Serialize(commitMetadata));

            var treeContent = new Dictionary<string, object>
            {
                { "folder", new Dictionary<string, object>
                    {
                        { "file.txt", "12345" }
                    }
                }
            };

            File.WriteAllText(Path.Combine(_paths.TreeDir, treeHash), JsonSerializer.Serialize(treeContent));


            // Act
            var tree = TreeHelper.GetTreeFromCommitHash(_paths, commitHash);

            // Assert: tree is equal to inputted
            Assert.AreEqual(treeContent, tree);
        }


        [Test]
        public void ShouldGetEmptyTree_WhenGivenInitialCommitHash()
        {
            // Arrange: Initial commit hash
            string commitHash = "4a35387be739933f7c9e6486959ec1affb2c1648";

            // Act
            var tree = TreeHelper.GetTreeFromCommitHash(_paths, commitHash);

            // Assert
            Assert.IsEmpty(tree);

        }

    }
}