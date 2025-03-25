namespace Janus.Helpers.CommandHelpers
{
    public class ConfigManager
    {
        private readonly string _localConfigPath;
        private readonly string _globalConfigPath;

        public ConfigManager(string localConfigPath, string globalConfigPath)
        {
            _localConfigPath = localConfigPath;
            _globalConfigPath = globalConfigPath;
        }

        public string GetEffectiveIp()
        {
            // Check local config
            if (File.Exists(_localConfigPath))
            {
                try
                {
                    string localIp = File.ReadAllText(_localConfigPath).Trim();
                    if (!string.IsNullOrEmpty(localIp))
                    {
                        return localIp;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading local IP configuration: " + ex.Message);
                }
            }

            // Check global config
            if (File.Exists(_globalConfigPath))
            {
                try
                {
                    string globalIp = File.ReadAllText(_globalConfigPath).Trim();
                    if (!string.IsNullOrEmpty(globalIp))
                    {
                        return globalIp;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading global IP configuration: " + ex.Message);
                }
            }

            // Return default if no config found
            return "localhost:82";
        }










        public string GetIp(bool isGlobal)
        {
            string configPath = isGlobal ? _globalConfigPath : _localConfigPath;

            if (File.Exists(configPath))
            {
                try
                {
                    return File.ReadAllText(configPath).Trim();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error reading IP configuration: " + ex.Message, ex);
                }
            }

            return null;
        }

        public void SetIp(bool isGlobal, string newIp)
        {
            string configPath = isGlobal ? _globalConfigPath : _localConfigPath;

            try
            {
                File.WriteAllText(configPath, newIp);
            }
            catch (Exception ex)
            {
                throw new Exception("Error setting IP configuration: " + ex.Message, ex);
            }

        }


        public void ResetIp(bool isGlobal)
        {
            string configPath = isGlobal ? _globalConfigPath : _localConfigPath;

            if (File.Exists(configPath))
            {
                try
                {
                    File.Delete(configPath);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error resetting IP configuration: " + ex.Message, ex);
                }
            }

        }

    }
}
