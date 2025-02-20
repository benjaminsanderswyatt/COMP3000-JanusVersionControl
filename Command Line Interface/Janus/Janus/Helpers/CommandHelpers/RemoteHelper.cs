using Janus.Plugins;
using System.Text.Json;

namespace Janus.Helpers.CommandHelpers
{
    public class RemoteHelper
    {
        public class RemoteRepos
        {
            public string Name { get; set; }
            public string Link { get; set; }
        }

        public static List<RemoteRepos> LoadRemotes(string remotePath)
        {
            string content = File.ReadAllText(remotePath);
            return JsonSerializer.Deserialize<List<RemoteRepos>>(content);
        }

        public static void SaveRemotes(string remotePath, List<RemoteRepos> remotes)
        {
            string json = JsonSerializer.Serialize(remotes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(remotePath, json);
        }



        public static async Task AddRemote(ILogger logger, string remotePath, string[] args)
        {
            if (args.Length < 3)
            {
                logger.Log("janus remote add <name> <link>");
                return;
            }

            string name = args[1];
            string link = args[2];

            List<RemoteRepos> remotes = LoadRemotes(remotePath);

            // Check if name already exists
            if (remotes.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                logger.Log($"A remote with the name '{name}' already exists");
                return;
            }

            remotes.Add(new RemoteRepos { Name = name, Link = link });

            SaveRemotes(remotePath, remotes);

            logger.Log($"Remote '{name}' added successfully with link: {link}");

            await Task.CompletedTask;
        }

        public static async Task RemoveRemote(ILogger logger, string remotePath, string[] args)
        {
            if (args.Length < 2)
            {
                logger.Log("janus remote remove <name>");
                return;
            }

            string name = args[1];

            List<RemoteRepos> remotes = LoadRemotes(remotePath);

            var remote = remotes.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (remote == null)
            {
                logger.Log($"Remote '{name}' was not found");
                return;
            }

            remotes.Remove(remote);

            SaveRemotes(remotePath, remotes);

            logger.Log($"Remote '{name}' was successfully removed");

            await Task.CompletedTask;
        }

        public static void ListRemotes(ILogger logger, string remotePath)
        {
            List<RemoteRepos> remotes = LoadRemotes(remotePath);

            if (remotes == null)
            {
                logger.Log("No saved remotes");
                return;
            }

            logger.Log("Saved remotes:");
            foreach (var remote in remotes)
            {
                logger.Log($"   {remote.Name} : {remote.Link}");
            }

        }


    }
}
