using Janus;

namespace CLITests
{
    public class CommandTest
    {
        private string testRepoDir;

        [SetUp]
        public void Setup()
        {
            testRepoDir = Path.Combine(Path.GetTempPath(), "JanusTestRepo");
            Directory.CreateDirectory(testRepoDir);
            Directory.SetCurrentDirectory(testRepoDir);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(testRepoDir)){
                Directory.Delete(testRepoDir, true);
            }
        }


        [Test]
        public void InitCommand_ShouldInitializeRepo()
        {
            var initCommand = new CommandHandler.InitCommand();

            initCommand.Execute(new string[] { });

            Assert.IsTrue(Directory.Exists(Path.Combine(testRepoDir, ".janus")));
            Assert.IsTrue(Directory.Exists(Path.Combine(testRepoDir, ".janus", "objects")));
            Assert.IsTrue(Directory.Exists(Path.Combine(testRepoDir, ".janus", "refs", "heads")));
            Assert.IsTrue(File.Exists(Path.Combine(testRepoDir, ".janus", "HEAD")));
        }
    }
}