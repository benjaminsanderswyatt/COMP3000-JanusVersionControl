using Janus.API;
using Janus.DataTransferObjects;
using Janus.Plugins;
using System.Text.Json;

namespace Janus.Utils
{
    public class RemoteManager
    {
        private readonly ILogger _logger;
        private readonly Paths _paths;
        private readonly IApiHelper _apiHelper;

        public RemoteManager(ILogger logger, Paths paths, IApiHelper apiHelper)
        {
            _logger = logger;
            _paths = paths;
            _apiHelper = apiHelper;
        }



        public class RemoteRepos
        {
            public string Name { get; set; }
            public string Link { get; set; }
            public Dictionary<string, string> Heads { get; set; }
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


            // Load the store user credentials
            var credManager = new CredentialManager();
            var credentials = credManager.LoadCredentials();
            if (credentials == null)
            {
                _logger.Log("Please login first. Usage: janus login");
                return;
            }


            // Use your ApiHelper to perform a GET request.
            var (success, response) = await _apiHelper.SendGetAsync(_paths, $"/cli/repo/{link}/head", credentials.Token);
            if (!success)
            {
                _logger.Log("Failed to fetch remote head. Remote not added");
                return;
            }

            // Deserialize the remote head DTO.
            var remoteHeadDto = JsonSerializer.Deserialize<RemoteHeadDto>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (remoteHeadDto == null)
            {
                _logger.Log("Failed to parse remote head. Remote not added");
                return;
            }


            remotes.Add(new RemoteRepos { Name = name, Link = link, Heads = remoteHeadDto.Heads });

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



        public void UpdateRemoteHead(string remoteName, Dictionary<string, string> heads)
        {
            var remotes = LoadRemotes();
            
            var remote = remotes.FirstOrDefault(r => r.Name.Equals(remoteName, StringComparison.OrdinalIgnoreCase));
            
            if (remote != null)
            {
                remote.Heads = heads;
                SaveRemotes(remotes);
            }
        }


    }
}
