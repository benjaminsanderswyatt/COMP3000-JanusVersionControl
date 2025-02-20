using Janus.Helpers;
using Janus.Models;
using System.Text.Json;

namespace Janus.Utils
{
    public class CredentialManager
    {
        private readonly string _filePath;

        public CredentialManager()
        {
            string credentialDir = PathHelper.GetCredentialPath();

            if (!Directory.Exists(credentialDir))
            {
                Directory.CreateDirectory(credentialDir);
            }

            _filePath = Path.Combine(credentialDir, "credentials");
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