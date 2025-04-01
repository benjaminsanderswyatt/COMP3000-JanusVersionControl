using Janus.Models;
using Janus.Plugins;
using Janus.Utils;

namespace Janus.Helpers.CommandHelpers
{

    public class ConfigHelper
    {
        public static void HandleIpConfig(ILogger logger, Paths paths, string[] args)
        {
            // Check for the --global flag
            bool isGlobal = args.Any(arg => arg.Equals("--global", StringComparison.OrdinalIgnoreCase));

            // Determine the proper config file path based on the flag
            string configPath = isGlobal ? paths.GlobalConfig : paths.LocalConfig;

            var configManager = new ConfigManager(paths.LocalConfig, paths.GlobalConfig);

            switch (args[1].ToLower())
            {
                case "get":

                    try
                    {
                        string ip = configManager.GetIp(isGlobal);

                        if (string.IsNullOrWhiteSpace(ip))
                        {
                            logger.Log("No IP configuration found");
                        }
                        else
                        {
                            logger.Log("Configured IP: " + ip);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error reading IP configuration: " + ex.Message);
                    }

                    break;

                case "set":

                    if (args.Length < 3)
                    {
                        logger.Log("Please provide an IP");
                        return;
                    }
                    string newIP = args[2];
                    try
                    {
                        configManager.SetIp(isGlobal, newIP);
                        logger.Log("IP configuration set to: " + newIP);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error setting IP configuration: " + ex.Message);
                    }

                    break;

                case "reset":

                    try
                    {
                        configManager.ResetIp(isGlobal);
                        logger.Log("IP configuration has been reset");
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error resetting IP configuration: " + ex.Message);
                    }

                    break;


                default:
                    logger.Log("Invalid subcommand");
                    break;
            }
        }



        public static void HandleRepoConfig(ILogger logger, Paths paths, string[] args)
        {
            if (args.Length < 2)
            {
                logger.Log("Please specify 'get' or 'set' for repository configuration");
                return;
            }

            string action = args[1].ToLower();

            if (action == "get")
            {
                if (args.Length < 3)
                {
                    logger.Log("Please specify a property to get ('is-private', 'description')");
                    return;
                }

                string property = args[2].ToLower();
                try
                {
                    var repoConfig = RepoConfigHelper.GetRepoConfig(paths.RepoConfig);
                    if (repoConfig == null)
                    {
                        logger.Log("Repository configuration not found. The repository may not be initialized");
                        return;
                    }

                    switch (property)
                    {
                        case "is-private":
                            logger.Log($"Repository is private: {repoConfig.IsPrivate}");
                            break;

                        case "description":
                            logger.Log($"Repository description: {repoConfig.Description}");
                            break;

                        default:
                            logger.Log($"Unknown property '{property}'. Valid properties are 'is-private' and 'description'");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Error reading repository configuration: {ex.Message}");
                }
            }
            else if (action == "set")
            {
                if (args.Length < 4)
                {
                    logger.Log("Please specify a property and value to set");
                    return;
                }

                string property = args[2].ToLower();
                string value = string.Join(" ", args.Skip(3)); // Handle values with spaces

                try
                {
                    RepoConfig repoConfig = RepoConfigHelper.GetRepoConfig(paths.RepoConfig) ?? new RepoConfig();

                    switch (property)
                    {
                        case "is-private":
                            if (bool.TryParse(value, out bool isPrivate))
                            {
                                repoConfig.IsPrivate = isPrivate;
                            }
                            else
                            {
                                logger.Log("Invalid value for 'is-private'. Use 'true' or 'false'");
                                return;
                            }
                            break;

                        case "description":
                            repoConfig.Description = value;
                            break;

                        default:
                            logger.Log($"Unknown property '{property}'. Valid properties are 'is-private' and 'description'");
                            return;
                    }

                    RepoConfigHelper.SetRepoConfig(paths.RepoConfig, repoConfig);
                    logger.Log($"Successfully set {property} to '{value}'");
                }
                catch (Exception ex)
                {
                    logger.Log($"Error setting repository configuration: {ex.Message}");
                }
            }
            else
            {
                logger.Log("Invalid action. Use 'get' or 'set'");
            }
        }




    }
}
