using Janus.Plugins;
using System.Text.Json;

namespace Janus.Utils
{
    public class RemoteManager
    {
        private readonly ILogger _logger;
        private readonly Paths _paths;

        public RemoteManager(ILogger logger, Paths paths)
        {
            _logger = logger;
            _paths = paths;
        }



        public class RemoteRepos
        {
            public string Name { get; set; }
            public string Link { get; set; }
        }





        public List<RemoteRepos> LoadRemotes()
        {
            string content = File.ReadAllText(_paths.Remote);
            return JsonSerializer.Deserialize<List<RemoteRepos>>(content);
        }

        public RemoteRepos LoadRemote(string remoteName)
        {
            var remotes = LoadRemotes();
            return remotes.FirstOrDefault(r => r.Name.Equals(remoteName, StringComparison.OrdinalIgnoreCase));
        }



        public void SaveRemotes(List<RemoteRepos> remotes)
        {
            string json = JsonSerializer.Serialize(remotes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_paths.Remote, json);
        }



        public async Task AddRemote(string[] args)
        {
            if (args.Length < 3)
            {
                _logger.Log("janus remote add <name> <link>");
                return;
            }

            string name = args[1];
            string link = args[2];

            List<RemoteRepos> remotes = LoadRemotes();

            // Check if name already exists
            if (remotes.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.Log($"A remote with the name '{name}' already exists");
                return;
            }

            remotes.Add(new RemoteRepos { Name = name, Link = link });

            SaveRemotes(remotes);

            _logger.Log($"Remote '{name}' added successfully with link: {link}");

            await Task.CompletedTask;
        }

        public async Task RemoveRemote(string[] args)
        {
            if (args.Length < 2)
            {
                _logger.Log("janus remote remove <name>");
                return;
            }

            string name = args[1];

            List<RemoteRepos> remotes = LoadRemotes();

            var remote = remotes.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (remote == null)
            {
                _logger.Log($"Remote '{name}' was not found");
                return;
            }

            remotes.Remove(remote);

            SaveRemotes(remotes);

            _logger.Log($"Remote '{name}' was successfully removed");

            await Task.CompletedTask;
        }

        public void ListRemotes()
        {
            List<RemoteRepos> remotes = LoadRemotes();

            if (remotes == null || remotes.Count == 0)
            {
                _logger.Log("No saved remotes");
                return;
            }

            _logger.Log("Saved remotes:");
            foreach (var remote in remotes)
            {
                _logger.Log($"   {remote.Name} : {remote.Link}");
            }

        }


    }
}
