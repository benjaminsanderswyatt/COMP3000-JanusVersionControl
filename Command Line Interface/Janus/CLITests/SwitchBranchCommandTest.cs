using Janus;
using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Moq;
using System.Text.Json;
using System.Xml.Serialization;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class SwitchBranchCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private SwitchBranchCommand _switchBranchCommand;
        private CreateBranchCommand _createBranchCommand;
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


            // Create command instances
            _switchBranchCommand = new SwitchBranchCommand(_loggerMock.Object, _paths);
            _createBranchCommand = new CreateBranchCommand(_loggerMock.Object, _paths);
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
        public void ShouldLogError_WhenRepoDoesNotExist()
        {
            // Arrange
            Directory.Delete(_paths.JanusDir, true);

            // Act
            _switchBranchCommand.Execute(new string[] { "new_branch" });

            // Assert
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise repository."), Times.Once);

        }


        [Test]
        public void ShouldLogError_WhenBranchNameIsNotProvided()
        {
            // Act
            _switchBranchCommand.Execute(new string[0]);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Please provide a branch name."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenBranchDoesNotExists()
        {
            // Arrange
            string branchName = "non_existing_branch";

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Branch '{branchName}' doesnt exists."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenAlreadyOnBranch()
        {
            // Arrange
            string branchName = "main";

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Already on branch '{branchName}'."), Times.Once);
        }


        [Test]
        public void ShouldLogError_WhenAlreadyOnCreatedBranch()
        {
            // Arrange
            string branchName = "test_branch";
            _createBranchCommand.Execute(new string[] { branchName });
            _switchBranchCommand.Execute(new string[] { branchName });

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(logger => logger.Log($"Already on branch '{branchName}'."), Times.Once);
        }







        [Test]
        public void ShouldSwitchBranchSuccessfully_WhenRepoIsEmpty()
        {
            // Arrange
            string branchName = "test_branch";
            _createBranchCommand.Execute(new string[] { branchName });

            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Successfully switched to branch"))), Times.Once);
            Assert.That(File.ReadAllText(_paths.HEAD), Is.EqualTo($"ref: heads/{branchName}"));
            
            string branchIndex = File.ReadAllText(Path.Combine(_paths.BranchesDir, "test_branch", "index"));
            Assert.That(File.ReadAllText(_paths.Index), Is.EqualTo(branchIndex));


            // Check that the working tree is as expected
            var workingTree = Tree.GetWorkingTree(_paths);
            Assert.That(workingTree.Children.Count(), Is.EqualTo(0));

        }

        [Test]
        public void ShouldSwitchBranch_WhenForcedWithUncommittedFilesExist()
        {
            // Arrange: Create a branch and have something uncommitted
            string branchName = "test_branch";
            _createBranchCommand.Execute(new string[] { branchName });

            // Create a file
            string filePath = Path.Combine(_testDir, "test.txt");
            File.WriteAllText(filePath, "test content");

            // Add the file
            _addCommand.Execute(new string[] { filePath });


            // Act: switch to the new branch
            _switchBranchCommand.Execute(new[] { branchName, "--force" }); // Force to bypass user confirmation


            // Assert: 
            _loggerMock.Verify(logger => logger.Log($"Successfully switched to branch '{branchName}'."), Times.Once);
            Assert.That(File.ReadAllText(_paths.HEAD), Is.EqualTo($"ref: heads/{branchName}"));

            string branchIndex = File.ReadAllText(Path.Combine(_paths.BranchesDir, "test_branch", "index"));
            Assert.That(File.ReadAllText(_paths.Index), Is.EqualTo(branchIndex));


            // Check that the working tree is as expected
            var workingTree = Tree.GetWorkingTree(_paths);
            Assert.That(workingTree.Children.Count(), Is.EqualTo(1));

            var child = workingTree.Children[0];
            Assert.That(child.Name, Is.EqualTo("test.txt"));

        }


        [Test]
        public void ShouldSwitchBranchSuccessfully()
        {
            // Arrange

            // Fill main with files and commit them (so that the new branch is split off of that commit)

            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(_testDir, "file2.txt"), "content2");
            File.WriteAllText(Path.Combine(_testDir, "file3.txt"), "content3");

            string dirPath = Path.Combine(_testDir, "dir");
            Directory.CreateDirectory(dirPath);

            File.WriteAllText(Path.Combine(dirPath, "file4.txt"), "content4");
            File.WriteAllText(Path.Combine(dirPath, "file5.txt"), "content5");
            File.WriteAllText(Path.Combine(dirPath, "file6.txt"), "content6");

            _addCommand.Execute(new string[] { "all", "--force" });
            _commitCommand.Execute(new string[] { "Commit message" });

            // Create new branch
            string branchName = "NewBranch";
            _createBranchCommand.Execute(new string[] { branchName });

            // Switch to new branch
            _switchBranchCommand.Execute(new[] { branchName });

            // Add
            File.WriteAllText(Path.Combine(_testDir, "added1.txt"), "addedContent1");
            File.WriteAllText(Path.Combine(dirPath, "added2.txt"), "addedContent2");

            // Modify
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "modifiedContent1");
            File.WriteAllText(Path.Combine(dirPath, "file4.txt"), "modifiedContent4");

            // Delete
            File.Delete(Path.Combine(_testDir, "file3.txt"));
            File.Delete(Path.Combine(dirPath, "file6.txt"));

            _addCommand.Execute(new string[] { "all", "--force" });
            _commitCommand.Execute(new string[] { "New branch commit message" });


            // Act
            _switchBranchCommand.Execute(new string[] { "main" });

            /*
            Expected main:
                file1.txt - content1
                file2.txt - content2
                file3.txt - content3
                dir
                    file4.txt - content4
                    file5.txt - content5
                    file6.txt - content6
            */

            // Assert: Check that the main working tree is as expected
            var workingTree = Tree.GetWorkingTree(_paths);

            //workingTree.Children.ForEach(child => Console.WriteLine(child.Name));

            Assert.That(workingTree.Children.Count(), Is.EqualTo(4));

            var child0 = workingTree.Children[0];
            var child1 = workingTree.Children[1];
            var child2 = workingTree.Children[2];
            var child3 = workingTree.Children[3];

            Assert.That(child0.Name, Is.EqualTo("file1.txt"));
            Assert.That(child1.Name, Is.EqualTo("file2.txt"));
            Assert.That(child2.Name, Is.EqualTo("file3.txt"));
            Assert.That(child3.Name, Is.EqualTo("dir"));

            //child3.Children.ForEach(child => Console.WriteLine(child.Name));

            Assert.That(child3.Children.Count(), Is.EqualTo(3)); // dir

            var dirChild0 = child3.Children[0];
            var dirChild1 = child3.Children[1];
            var dirChild2 = child3.Children[2];

            Assert.That(dirChild0.Name, Is.EqualTo("file4.txt"));
            Assert.That(dirChild1.Name, Is.EqualTo("file5.txt"));
            Assert.That(dirChild2.Name, Is.EqualTo("file6.txt"));




            // Act
            _switchBranchCommand.Execute(new string[] { branchName });

            /*
            Expected newBranch:
                file1.txt - modifiedContent1        modified
                file2.txt - content2
                added1.txt - addedContent1          added
                dir
                    file4.txt - modifiedContent4    modified
                    file5.txt - content5
                    added2.txt - addedContent2      added

                file3                               deleted
                dir/file6                           deleted
            */

            // Assert: Check that the NewBranch working tree is as expected
            var branchWorkingTree = Tree.GetWorkingTree(_paths);

            //branchWorkingTree.Children.ForEach(child => Console.WriteLine(child.Name));

            Assert.That(branchWorkingTree.Children.Count(), Is.EqualTo(4));

            var branchChild0 = branchWorkingTree.Children[0];
            var branchChild1 = branchWorkingTree.Children[1];
            var branchChild2 = branchWorkingTree.Children[2];
            var branchChild3 = branchWorkingTree.Children[3];

            Assert.That(branchChild0.Name, Is.EqualTo("added1.txt"));
            Assert.That(branchChild1.Name, Is.EqualTo("file1.txt"));
            Assert.That(branchChild2.Name, Is.EqualTo("file2.txt"));
            Assert.That(branchChild3.Name, Is.EqualTo("dir"));

            //branchChild3.Children.ForEach(child => Console.WriteLine(child.Name));

            Assert.That(branchChild3.Children.Count(), Is.EqualTo(3)); // dir

            var branchDirChild0 = branchChild3.Children[0];
            var branchDirChild1 = branchChild3.Children[1];
            var branchDirChild2 = branchChild3.Children[2];

            Assert.That(branchDirChild0.Name, Is.EqualTo("added2.txt"));
            Assert.That(branchDirChild1.Name, Is.EqualTo("file4.txt"));
            Assert.That(branchDirChild2.Name, Is.EqualTo("file5.txt"));





            // Act: Switch back to main
            _switchBranchCommand.Execute(new string[] { "main" });

            // Assert: Same as first
            var workingTreeA = Tree.GetWorkingTree(_paths);

            Assert.That(workingTreeA.Children.Count(), Is.EqualTo(4));

            var child0A = workingTree.Children[0];
            var child1A = workingTree.Children[1];
            var child2A = workingTree.Children[2];
            var child3A = workingTree.Children[3];

            Assert.That(child0A.Name, Is.EqualTo("file1.txt"));
            Assert.That(child1A.Name, Is.EqualTo("file2.txt"));
            Assert.That(child2A.Name, Is.EqualTo("file3.txt"));
            Assert.That(child3A.Name, Is.EqualTo("dir"));

            Assert.That(child3.Children.Count(), Is.EqualTo(3)); // dir

            var dirChild0A = child3A.Children[0];
            var dirChild1A = child3A.Children[1];
            var dirChild2A = child3A.Children[2];

            Assert.That(dirChild0A.Name, Is.EqualTo("file4.txt"));
            Assert.That(dirChild1A.Name, Is.EqualTo("file5.txt"));
            Assert.That(dirChild2A.Name, Is.EqualTo("file6.txt"));

        }



    }

    
}