using Janus.Models;
using Janus.Plugins;
using System.Text.Json;

namespace Janus.Utils
{
    public interface ICredentialManager
    {
        void SaveCredentials(UserCredentials credentials);
        UserCredentials LoadCredentials();
        void ClearCredentials();
    }


    public class CredentialManager : ICredentialManager
    {
        private readonly string _filePath;


        public CredentialManager(string baseDirectory = null)
        {
            string configDir = baseDirectory != null ?
                Path.GetFullPath(baseDirectory) :
                Paths.GetGlobalConfigPath();


            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            _filePath = Path.Combine(configDir, "credentials");
        }


        public void SaveCredentials(UserCredentials credentials)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string content = JsonSerializer.Serialize(credentials, options);
            File.WriteAllText(_filePath, content);
        }


        public UserCredentials LoadCredentials()
        {
            if (File.Exists(_filePath))
            {
                string content = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<UserCredentials>(content);
            }

            return null;
        }


        public void ClearCredentials()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

    }
}