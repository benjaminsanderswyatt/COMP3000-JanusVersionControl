using Janus.Plugins;
using System.Text.Json;

namespace Janus.Helpers.CommandHelpers
{
    public class ConfigHelper
    {

        public static void HandleGetConfig(ILogger logger, Paths paths, string[] args)
        {
            bool useGlobal = args.Contains("--global");
            string configType = useGlobal ? "global" : "local";
            string configPath = useGlobal ? paths.GlobalConfig : paths.LocalConfig;

            if (args.Length < 2 || args[1] != "ip")
            {
                logger.Log("Invalid get command. Usage: config get ip [--global]");
                return;
            }

            var config = LoadConfig(paths, configPath);
            logger.Log($"{configType} IP: {(config?.IP ?? "Not configured")}");
        }

        public static void HandleSetConfig(ILogger logger, Paths paths, string[] args)
        {
            bool useGlobal = args.Contains("--global");
            string configType = useGlobal ? "global" : "local";
            string configPath = useGlobal ? paths.GlobalConfig : paths.LocalConfig;

            if (args.Length < 3 || args[1] != "ip")
            {
                logger.Log("Invalid set command. Usage: config set ip <value> [--global]");
                return;
            }

            string ipValue = args[2];
            var config = LoadConfig(paths, configPath) ?? new JanusConfig();
            config.IP = ipValue;

            SaveConfig(configPath, config);
            logger.Log($"Successfully set {configType} IP to: {ipValue}");
        }

        public static void HandleResetConfig(ILogger logger, Paths paths, string[] args)
        {
            bool useGlobal = args.Contains("--global");
            string configType = useGlobal ? "global" : "local";
            string configPath = useGlobal ? paths.GlobalConfig : paths.LocalConfig;

            if (!File.Exists(configPath))
            {
                logger.Log($"{configType} config does not exist");
                return;
            }

            try
            {
                File.Delete(configPath);
                logger.Log($"Successfully removed {configType} config");
            }
            catch (Exception ex)
            {
                logger.Log($"Error removing {configType} config: {ex.Message}");
            }
        }

        public static JanusConfig LoadConfig(Paths paths, string configPath)
        {
            if (!File.Exists(configPath))
                return null;

            try
            {
                return JsonSerializer.Deserialize<JanusConfig>(File.ReadAllText(configPath));
            }
            catch
            {
                return null;
            }
        }

        public static void SaveConfig(Paths paths, string configPath, JanusConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        }

        public class JanusConfig
        {
            public string IP { get; set; }
        }



    }
}
