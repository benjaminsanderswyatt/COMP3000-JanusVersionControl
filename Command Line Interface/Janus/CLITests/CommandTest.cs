using Janus;
using System.Runtime.InteropServices;
using static Janus.CommandHandler;

namespace CLITests
{
    public class CommandTest
    {
        private string testDirectory;
        private string janusDir;
        private string objectDir;
        private string refsDir;
        private string headsDir;
        private string pluginsDir;
        private string index;
        private string head;

        [SetUp]
        public void Setup()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), "JanusTestRepo");
            Directory.CreateDirectory(testDirectory);
            Directory.SetCurrentDirectory(testDirectory);

            string janusDir = Path.Combine(testDirectory, ".janus");
            string objectDir = Path.Combine(janusDir, "objects");
            string refsDir = Path.Combine(janusDir, "refs");
            string headsDir = Path.Combine(refsDir, "heads");
            string pluginsDir = Path.Combine(janusDir, "plugins");
            string index = Path.Combine(janusDir, "index");
            string head = Path.Combine(janusDir, "HEAD");
        }

        [TearDown]
        public void Teardown()
        {
            Directory.SetCurrentDirectory(Path.GetTempPath());

            if (Directory.Exists(testDirectory)){
                Directory.Delete(testDirectory, true);
            }
        }


        [Test]
        public void InitCommand_RepoDoesNotExist()
        {
            // Arrange
            var command = new InitCommand();
            var args = new string[0];


            // Act
            command.Execute(args);


            // Assert
            Assert.IsTrue(Directory.Exists(janusDir), ".janus directory should be created");
            Assert.IsTrue(Directory.Exists(objectDir), "objects directory should be created");
            Assert.IsTrue(Directory.Exists(refsDir), "refs directory should be created");
            Assert.IsTrue(Directory.Exists(headsDir), "heads directory should be created");
            Assert.IsTrue(Directory.Exists(pluginsDir), "plugins directory should be created");
            Assert.IsTrue(File.Exists(index), "index file should be created");
            Assert.IsTrue(File.Exists(head), "HEAD file should be created");

            var headContent = File.ReadAllText(head);
            Assert.That(headContent, Is.EqualTo("ref: refs/heads/main"), "HEAD file should point to main branch");

            var mainBranchContent = File.ReadAllText(Path.Combine(headsDir, "main"));
            Assert.That(mainBranchContent, Is.EqualTo(string.Empty), "Main branch should be empty");
        }
    }
}