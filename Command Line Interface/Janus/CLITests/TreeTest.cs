using Janus.Plugins;
using Janus.Utils;
using Moq;
using static Janus.CommandHandler;

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
        }


        [Test]
        public void ShouldBuildTreeFromDictionary()
        {
            // Arrange
            var index = new Dictionary<string, string>
            {
                { "file1.txt", "hash1" },
                { "dir1/file2.txt", "hash2" },
                { "dir1/file3.txt", "hash3" },
                { "dir2/file4.txt", "hash4" },
                { "dir2/subdir1/file5.txt", "hash5" }
            };

            // Act
            var root = _treeBuilder.BuildTreeFromDiction(index);


            _treeBuilder.PrintTree();

            // Assert
            // Root
            Assert.That(root.Name, Is.EqualTo("root"));
            Assert.That(root.Children.Count, Is.EqualTo(3));

            // File 1
            var file1 = root.Children.FirstOrDefault(c => c.Name == "file1.txt");
            Assert.That(file1.Hash, Is.EqualTo("hash1"));

            // Dir 1
            var dir1 = root.Children.FirstOrDefault(c => c.Name == "dir1");
            Assert.That(dir1.Hash, Is.Null);
            Assert.That(dir1.Children.Count, Is.EqualTo(2));

            // File 2
            var file2 = dir1.Children.FirstOrDefault(c => c.Name == "file2.txt");
            Assert.That(file2.Hash, Is.EqualTo("hash2"));

            // File 3
            var file3 = dir1.Children.FirstOrDefault(c => c.Name == "file3.txt");
            Assert.That(file3.Hash, Is.EqualTo("hash3"));


            // Dir 2
            var dir2 = root.Children.FirstOrDefault(c => c.Name == "dir2");
            Assert.That(dir2.Hash, Is.Null);
            Assert.That(dir2.Children.Count, Is.EqualTo(2));

            // File 4
            var file4 = dir2.Children.FirstOrDefault(c => c.Name == "file4.txt");
            Assert.That(file4.Hash, Is.EqualTo("hash4"));


            // Subdir1
            var subdir1 = dir2.Children.FirstOrDefault(c => c.Name == "subdir1");
            Assert.That(subdir1.Hash, Is.Null);
            Assert.That(subdir1.Children.Count, Is.EqualTo(1));

            // File 5
            var file5 = subdir1.Children.FirstOrDefault(c => c.Name == "file5.txt");
            Assert.That(file5.Hash, Is.EqualTo("hash5"));

        }


        [Test]
        public void ShouldSaveTree()
        {
            // Arrange
            var index = new Dictionary<string, string>
            {
                { "file1.txt", "hash1" },
                { "file2.txt", "hash2" },
                { "file3.txt", "hash3" }
            };

            var root = _treeBuilder.BuildTreeFromDiction(index);


            // Act
            var rootHash = _treeBuilder.SaveTree();

            // Assert
            var treeFilePath = Path.Combine(_paths.TreeDir, rootHash);
            Assert.IsTrue(File.Exists(treeFilePath));

            var treeContent = File.ReadAllLines(treeFilePath);

            Assert.That(treeContent.Length, Is.EqualTo(3));
            Assert.IsTrue(treeContent.Any(line => line.Contains("blob file1.txt hash1")));
            Assert.IsTrue(treeContent.Any(line => line.Contains("blob file2.txt hash2")));
            Assert.IsTrue(treeContent.Any(line => line.Contains("blob file3.txt hash3")));


        }


        [Test]
        public void ShouldSaveTree_WithFolders()
        {
            // Arrange
            var index = new Dictionary<string, string>
            {
                { "file1.txt", "hash1" },
                { "dir1/file2.txt", "hash2" },
                { "dir1/file3.txt", "hash3" },
                { "dir2/file4.txt", "hash4" },
                { "dir2/subdir1/file5.txt", "hash5" }
            };

            var root = _treeBuilder.BuildTreeFromDiction(index);

            // Act
            var rootHash = _treeBuilder.SaveTree();

            // Assert
            var treeFilePath = Path.Combine(_paths.TreeDir, rootHash);
            Assert.IsTrue(File.Exists(treeFilePath));

            var treeContent = File.ReadAllLines(treeFilePath);

            Assert.That(treeContent.Length, Is.EqualTo(3));
            Assert.IsTrue(treeContent.Any(line => line.Contains("blob file1.txt hash1")));
            Assert.IsTrue(treeContent.Any(line => line.Contains("tree dir1 ")));
            Assert.IsTrue(treeContent.Any(line => line.Contains("tree dir2 ")));

            // Dir 1
            var dir1Hash = GetHashFromTreeLine(treeContent.First(line => line.StartsWith("tree dir1 ")));
            var dir1Path = Path.Combine(_paths.TreeDir, dir1Hash);
            Assert.IsTrue(File.Exists(dir1Path));

            var dir1Content = File.ReadAllLines(dir1Path);
            Assert.That(dir1Content.Length, Is.EqualTo(2));
            Assert.IsTrue(dir1Content.Any(line => line.Contains("blob file2.txt hash2")));
            Assert.IsTrue(dir1Content.Any(line => line.Contains("blob file3.txt hash3")));

            // Dir 2
            var dir2Hash = GetHashFromTreeLine(treeContent.First(line => line.StartsWith("tree dir2 ")));
            var dir2Path = Path.Combine(_paths.TreeDir, dir2Hash);
            Assert.IsTrue(File.Exists(dir2Path));

            var dir2Content = File.ReadAllLines(dir2Path);
            Assert.That(dir2Content.Length, Is.EqualTo(2));
            Assert.IsTrue(dir2Content.Any(line => line.Contains("blob file4.txt hash4")));
            Assert.IsTrue(dir2Content.Any(line => line.StartsWith("tree subdir1 ")));

            // Subdir 1
            var subdir1Hash = GetHashFromTreeLine(dir2Content.First(line => line.StartsWith("tree subdir1 ")));
            var subdir1Path = Path.Combine(_paths.TreeDir, subdir1Hash);
            Assert.IsTrue(File.Exists(subdir1Path));

            var subdir1Content = File.ReadAllLines(subdir1Path);
            Assert.That(subdir1Content.Length, Is.EqualTo(1));
            Assert.IsTrue(subdir1Content.Any(line => line.Contains("blob file5.txt hash5")));
        }

        private string GetHashFromTreeLine(string treeLine)
        {
            var parts = treeLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 2 ? parts[2] : string.Empty;
        }





        [Test]
        public void ShouldRebuildTreeFromHash()
        {
            // Arrange
            var index = new Dictionary<string, string>
            {
                { "file1.txt", "hash1" },
                { "dir1/file2.txt", "hash2" },
                { "dir1/file3.txt", "hash3" },
                { "dir2/file4.txt", "hash4" },
                { "dir2/subdir1/file5.txt", "hash5" }
            };

            var root = _treeBuilder.BuildTreeFromDiction(index);
            var rootHash = _treeBuilder.SaveTree();

            // Act
            var recreatedTree = _treeBuilder.RecreateTree(_loggerMock.Object, rootHash);

            // Assert

            // Root
            Assert.That(recreatedTree.Name, Is.EqualTo("root"));
            Assert.That(recreatedTree.Children.Count, Is.EqualTo(3));

            // File 1
            var file1 = recreatedTree.Children.FirstOrDefault(c => c.Name == "file1.txt");
            Assert.That(file1.Hash, Is.EqualTo("hash1"));

            // Dir 1
            var dir1 = recreatedTree.Children.FirstOrDefault(c => c.Name == "dir1");
            Assert.That(dir1.Hash, Is.Null);
            Assert.That(dir1.Children.Count, Is.EqualTo(2));

            // File 2
            var file2 = dir1.Children.FirstOrDefault(c => c.Name == "file2.txt");
            Assert.That(file2.Hash, Is.EqualTo("hash2"));

            // File 3
            var file3 = dir1.Children.FirstOrDefault(c => c.Name == "file3.txt");
            Assert.That(file3.Hash, Is.EqualTo("hash3"));

            // Dir 2
            var dir2 = recreatedTree.Children.FirstOrDefault(c => c.Name == "dir2");
            Assert.That(dir2.Hash, Is.Null);
            Assert.That(dir2.Children.Count, Is.EqualTo(2));

            // File 4
            var file4 = dir2.Children.FirstOrDefault(c => c.Name == "file4.txt");
            Assert.That(file4.Hash, Is.EqualTo("hash4"));

            // Subdir1
            var subdir1 = dir2.Children.FirstOrDefault(c => c.Name == "subdir1");
            Assert.That(subdir1.Hash, Is.Null);
            Assert.That(subdir1.Children.Count, Is.EqualTo(1));

            // File 5
            var file5 = subdir1.Children.FirstOrDefault(c => c.Name == "file5.txt");
            Assert.That(file5.Hash, Is.EqualTo("hash5"));
        }


        [Test]
        public void ShouldCompareTrees()
        {
            // Arrange
            var diction1 = new Dictionary<string, string>
            {
                { "file1.txt", "hash1" },
                { "dir1/file2.txt", "hash2" },
                { "dir1/file3.txt", "hash3" },
                { "dir2/file4.txt", "hash4" },
                { "dir2/subdir1/file5.txt", "hash5" }
            };

            var diction2 = new Dictionary<string, string>
            {
                { "file1.txt", "hash1" },
                { "dir1/file2.txt", "hash2_modified" }, // Modified file
                { "dir1/file3.txt", "hash3" },
                { "dir2/file6.txt", "hash6" }, // New file
                { "dir3/file7.txt", "hash7" } // New file
            };

            var tree1 = _treeBuilder.BuildTreeFromDiction(diction1);

            var tree2Builder = new TreeBuilder(_paths);
            var tree2 = tree2Builder.BuildTreeFromDiction(diction2);


            // Act
            var comparisonResult = Tree.CompareTrees(tree1, tree2);


            // Assert
            comparisonResult.AddedOrUntracked.ForEach(item => { Console.WriteLine($"Added: {item}"); });
            comparisonResult.ModifiedOrNotStaged.ForEach(item => { Console.WriteLine($"Modified: {item}"); });
            comparisonResult.Deleted.ForEach(item => { Console.WriteLine($"Deleted: {item}"); });

            Assert.That(comparisonResult.AddedOrUntracked.Count, Is.EqualTo(2));
            Assert.That(comparisonResult.ModifiedOrNotStaged.Count, Is.EqualTo(1));
            Assert.That(comparisonResult.Deleted.Count, Is.EqualTo(2));

            Assert.Contains("dir2/file6.txt".Replace('/', Path.DirectorySeparatorChar), comparisonResult.AddedOrUntracked);
            Assert.Contains("dir3/file7.txt".Replace('/', Path.DirectorySeparatorChar), comparisonResult.AddedOrUntracked);

            Assert.Contains("dir1/file2.txt".Replace('/', Path.DirectorySeparatorChar), comparisonResult.ModifiedOrNotStaged);

            Assert.Contains("dir2/file4.txt".Replace('/', Path.DirectorySeparatorChar), comparisonResult.Deleted);
            Assert.Contains("dir2/subdir1/file5.txt".Replace('/', Path.DirectorySeparatorChar), comparisonResult.Deleted);
        }


    }


}