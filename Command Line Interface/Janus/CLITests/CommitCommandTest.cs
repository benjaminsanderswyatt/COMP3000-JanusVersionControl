using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Moq;
using NuGet.Frameworks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class CommitCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private AddCommand _addCommand;
        private CommitCommand _commitCommand;

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

            // Initialize the repository
            InitCommand _initCommand = new InitCommand(_loggerMock.Object, _paths);
            _initCommand.Execute(new string[0]);

            Directory.SetCurrentDirectory(_testDir);


            // Create Commit Command instance
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
            _commitCommand = new CommitCommand(_loggerMock.Object, _paths);
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
        public void ShouldLogError_WhenRepositoryIsNotInitialized()
        {
            // Arrange: Delete the repository
            Directory.Delete(_paths.JanusDir, true);

            // Act: Execute 'janus commit'
            var args = new string[] { "Initial commit" };
            _commitCommand.Execute(args);

            // Assert: Verify that the error about repository initialization is logged
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise repository."), Times.Once);

        }


        [Test]
        public void ShouldLogError_WhenNoCommitMessageIsProvided()
        {
            // Act: Execute 'janus commit' without a message
            var args = new string[0];
            _commitCommand.Execute(args);

            // Assert: Verify that the correct error is logged
            _loggerMock.Verify(logger => logger.Log("No commit message provided. Use 'janus commit <message>'."), Times.Once);

        }


        [TestCase("   ")]
        [TestCase("")]
        public void ShouldLogError_WhenEmptyWhiteSpaceCommitMessageIsProvided(string commitMessage)
        {
            // Act: Execute 'janus commit' with an empty or whitespace message
            var args = new string[] { commitMessage };
            _commitCommand.Execute(args);

            // Assert: Verify that the correct error is logged
            _loggerMock.Verify(logger => logger.Log("No commit message provided. Use 'janus commit <message>'."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenCommitMessageIsTooLong()
        {
            // Arrange: Create a commit message over 256 characters
            string longMessage = new string('a', 257);

            // Act: commit with long message
            var args = new string[] { longMessage };
            _commitCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Commit message is too long. Maximum length is 256 characters."), Times.Once);

        }



        [Test]
        public void ShouldLogError_WhenNoFilesAreStaged()
        {
            // Act: Execute 'janus commit "First commit"' when no files are staged
            var args = new string[] { "First commit" };
            _commitCommand.Execute(args);

            // Assert: Verify that the correct error message is logged with no files being staged
            _loggerMock.Verify(logger => logger.Log("No changes to commit."), Times.Once);

        }


        [Test]
        public void ShouldCommitStagedFiles_WhenCommitMessageIsProvided()
        {
            // Arrange: Stage a file
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");
            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);

            // Act: Execute 'janus commit "First commit"'
            var commitArgs = new string[] { "First commit" };
            _commitCommand.Execute(commitArgs);

            // Assert: Verify that the commit was successful and the commit message is logged
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.StartsWith("Committed as"))), Times.Once);

            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();

            Assert.That(commitPathsInFolder.Count, Is.EqualTo(2), "Should have initial commit object and one created.");

            CommitMetadata initialCommitData = commitPathsInFolder[0];

            CommitMetadata newCommitData = commitPathsInFolder[1];

            Assert.That(newCommitData.Parent, Is.EqualTo(initialCommitData.Commit), "Parent commit should be the initial commit hash.");

            Assert.That(newCommitData.Message, Is.EqualTo("First commit"), "Commit message should be 'First commit'.");


            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);
            Assert.That(tree.ContainsKey("file.txt"), "File should be part of the commit tree.");

        }


        



        [Test]
        public void ShouldCommitMultipleFiles_WhenMultipleFilesAreStaged()
        {
            // Arrange: Create and stage multiple files
            var file1Path = Path.Combine(_testDir, "file1.txt");
            var file2Path = Path.Combine(_testDir, "file2.txt");
            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            var args = new string[] { "file1.txt", "file2.txt" };
            _addCommand.Execute(args);

            // Act: Execute 'janus commit "Multiple files commit"'
            var commitArgs = new string[] { "Multiple files commit" };
            _commitCommand.Execute(commitArgs);

            // Assert: Verify that both files are committed successfully
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.StartsWith("Committed as"))), Times.Once);


            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();


            Assert.That(commitPathsInFolder.Count, Is.EqualTo(2), "Should have initial commit object and one created.");

            CommitMetadata initialCommitData = commitPathsInFolder[0];

            CommitMetadata newCommitData = commitPathsInFolder[1];

            Assert.That(newCommitData.Parent, Is.EqualTo(initialCommitData.Commit), "Parent commit should be the initial commit hash.");

            Assert.That(newCommitData.Message, Is.EqualTo("Multiple files commit"), "Commit message should be 'Multiple files commit'.");

            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);
            Assert.That(tree.ContainsKey("file1.txt"), "File1 should be part of the commit tree.");
            Assert.That(tree.ContainsKey("file2.txt"), "File2 should be part of the commit tree.");

        }



        [Test]
        public void ShouldUpdateStagedFile_WhenFileIsModifiedAfterCommit()
        {
            // Arrange: Create a test file and stage it
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");

            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);
            var commitArgs = new string[] { "First commit" };
            _commitCommand.Execute(commitArgs);

            // Modify the file content after committing
            File.WriteAllText(testFilePath, "updated content");

            // Act: Re execute the commit command for the modified file
            _addCommand.Execute(args);  // Stage the updated file
            var updateArgs = new string[] { "Updated file" };
            _commitCommand.Execute(updateArgs);

            // Assert: Verify that both files are committed successfully
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.StartsWith("Committed as"))), Times.Exactly(2));

            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();

            Assert.That(commitPathsInFolder.Count, Is.EqualTo(3), "Should have initial commit, origninal and the updated commit object");

            CommitMetadata initialCommitData = commitPathsInFolder[0];

            CommitMetadata originalCommitData = commitPathsInFolder[1];

            CommitMetadata newCommitData = commitPathsInFolder[2];

            Assert.That(originalCommitData.Parent, Is.EqualTo(initialCommitData.Commit), "Parent commit should be the initial commit hash.");

            Assert.That(newCommitData.Parent, Is.EqualTo(originalCommitData.Commit), "Parent commit should be the original commit hash.");

            Assert.That(newCommitData.Message, Is.EqualTo("Updated file"), "Commit message should be 'Updated file'.");

            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);
            Assert.That(tree.ContainsKey("file.txt"), "File should be part of the commit tree.");

            // Check the refs/heads/main file contents
            string mainRefContents = File.ReadAllText(Path.Combine(_paths.HeadsDir, "main"));
            Assert.That(mainRefContents, Is.EqualTo(newCommitData.Commit), "refs/heads/main file should be the newest commit hash.");

        }


        [Test]
        public void ShouldRemoveDeletedFiles_WhenFileIsDeletedBeforeCommit()
        {
            // Arrange: Stage a file and then delete it
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");

            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);

            // Delete the file before committing
            File.Delete(testFilePath);

            // Act: Execute 'janus commit "Deleting file commit"'
            var commitArgs = new string[] { "Deleting file commit" };
            _commitCommand.Execute(commitArgs);

            // Assert: Verify that delete is committed successfully
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.StartsWith("Committed as"))), Times.Once);

            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();

            Assert.That(commitPathsInFolder.Count, Is.EqualTo(2), "Should have initial commit object and one created.");

            CommitMetadata initialCommitData = commitPathsInFolder[0];
            CommitMetadata newCommitData = commitPathsInFolder[1];


            Assert.That(newCommitData.Parent, Is.EqualTo(initialCommitData.Commit), "Parent commit should be the initial commit hash.");

            Assert.That(newCommitData.Message, Is.EqualTo("Deleting file commit"), "Commit message should be 'Deleting file commit'.");

            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);
            Assert.That(tree.ContainsKey("file.txt"), "File should be part of the commit tree.");
            Assert.That(tree["file.txt"], Is.Not.EqualTo("Deleted"), "The object if from when added so it shouldnt be deleted");

        }



        [Test]
        public void ShouldOnlyCommitStagedFiles_WhenUnstagedFilesExist()
        {
            // Arrange: Create and stage one file, but leave another unstaged
            var file1Path = Path.Combine(_testDir, "file1.txt");
            var file2Path = Path.Combine(_testDir, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            var args1 = new string[] { "file1.txt" };
            _addCommand.Execute(args1); // Stage file1.txt

            // Act: Execute commit command only for file1.txt
            var commitArgs = new string[] { "Partial commit" };
            _commitCommand.Execute(commitArgs);

            // Assert: Verify that file is committed successfully
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.StartsWith("Committed as"))), Times.Once);

            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();

            Assert.That(commitPathsInFolder.Count, Is.EqualTo(2), "Should have initial commit object and one created.");
            
            CommitMetadata initialCommitData = commitPathsInFolder[0];
            CommitMetadata newCommitData = commitPathsInFolder[1];

            Assert.That(newCommitData.Parent, Is.EqualTo(initialCommitData.Commit), "Parent commit should be the initial commit hash.");

            Assert.That(newCommitData.Message, Is.EqualTo("Partial commit"), "Commit message should be 'Partial commit'.");

            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);
            Assert.That(tree.ContainsKey("file1.txt"), "File1 should be part of the commit tree.");
            Assert.That(!tree.ContainsKey("file2.txt"), "File2 shouldnt be part of the commit tree.");
        }



        [Test]
        public void ShouldReCommitDeletedFile_WhenFileWithSameNameIsAddedAfterDeletion()
        {
            // Arrange: Create a test file and stage it
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");

            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);

            // Delete the file before committing
            File.Delete(testFilePath);

            // Deleted file is commetted
            var commitArgs = new string[] { "Deleting file commit" };
            _commitCommand.Execute(commitArgs);

            // Recreate the file with the same name
            File.WriteAllText(testFilePath, "recreated content");
            _addCommand.Execute(args);

            // Act: Execute 'janus commit "Same name commit"'
            commitArgs = new string[] { "Same name commit" };
            _commitCommand.Execute(commitArgs);
            

            // Assert: Verify that both files are committed successfully
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg.StartsWith("Committed as"))), Times.Exactly(2));

            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();

            Assert.That(commitPathsInFolder.Count, Is.EqualTo(3), "Should have initial commit, origninal and the updated commit object");

            CommitMetadata initialCommitData = commitPathsInFolder[0];

            CommitMetadata deleteCommitData = commitPathsInFolder[1];

            CommitMetadata newCommitData = commitPathsInFolder[2];

            Assert.That(deleteCommitData.Parent, Is.EqualTo(initialCommitData.Commit), "Parent commit should be the initial commit hash.");

            Assert.That(newCommitData.Parent, Is.EqualTo(deleteCommitData.Commit), "Parent commit should be the original commit hash.");

            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);
            Assert.That(tree.ContainsKey("file.txt"), "File1 should be part of the commit tree.");

            // Check the refs/heads/main file contents
            string mainRefContents = File.ReadAllText(Path.Combine(_paths.HeadsDir, "main"));
            Assert.That(mainRefContents, Is.EqualTo(newCommitData.Commit), "refs/heads/main file should be the newest commit hash.");

        }



        [Test]
        public void ShouldCommitFilesInMultipleDirectories_WhenFilesAreStagedAcrossDirs()
        {
            // Arrange: Create files in different directories
            var dir1 = Path.Combine(_testDir, "dir1");
            var dir2 = Path.Combine(_testDir, "dir2");

            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir2);

            var file1Path = Path.Combine(dir1, "file1.txt");
            var file2Path = Path.Combine(dir2, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            var args = new string[] { "dir1/file1.txt".Replace('/', Path.DirectorySeparatorChar), "dir2/file2.txt".Replace('/', Path.DirectorySeparatorChar) };

            _addCommand.Execute(args); // Stage files

            // Act: Commit with a message
            var commitArgs = new string[] { "Commit files in multiple directories" };
            _commitCommand.Execute(commitArgs);

            // Get all commit files
            var commitFiles = Directory.GetFiles(_paths.CommitDir);

            // Deserialize and sort commit metadata by date
            List<CommitMetadata> commitPathsInFolder = commitFiles
                .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                .Where(metadata => metadata != null) // Exclude invalid or null metadata
                .OrderBy(metadata => metadata.Date)
                .ToList();

            Assert.That(commitPathsInFolder.Count, Is.EqualTo(2), "Two commit, initial and new one");


            CommitMetadata initialCommitData = commitPathsInFolder[0];

            CommitMetadata newCommitData = commitPathsInFolder[1];


            var tree = TreeHelper.GetTreeFromTreeHash(_paths, newCommitData.Tree);

            Assert.IsTrue(tree.ContainsKey("dir1"));
            var dir1Tree = tree["dir1"] as Dictionary<string, object>;
            Assert.IsNotNull(dir1Tree);

            Assert.That(dir1Tree.ContainsKey("file1.txt"), "File1 should be part of the commit tree.");


            Assert.IsTrue(tree.ContainsKey("dir2"));
            var dir2Tree = tree["dir2"] as Dictionary<string, object>;
            Assert.IsNotNull(dir2Tree);

            Assert.That(dir2Tree.ContainsKey("file2.txt"), "File2 should be part of the commit tree.");
        }



    }
}