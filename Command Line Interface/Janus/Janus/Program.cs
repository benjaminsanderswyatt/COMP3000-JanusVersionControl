using Janus.Plugins;

namespace Janus
{
    public class Program
    {
        public static List<ICommand> CommandList { get; set; } = new List<ICommand>();

        private static void LoadCommands(ILogger logger, Paths paths)
        {
            CommandList.AddRange(CommandHandler.GetCommands(logger, paths));

            CommandList.AddRange(PluginLoader.LoadPlugins(logger, paths));
        }


        static async Task Main(string[] args)
        {
            // Config Logger
            ILogger logger = new ConsoleLogger();

            // Base Path
            string basePath = Directory.GetCurrentDirectory();
            Paths paths = new Paths(basePath);


            if (args.Length == 0)
            {
                logger.Log("Please enter your command...");
                logger.Log("Use \"janus help\" to get more details");
                return;
            }

            LoadCommands(logger, paths);

            string commandName = args[0];

            ICommand? command = CommandList.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            if (command != null)
            {
                try
                {
                    await command.Execute(args.Skip(1).ToArray());
                }
                catch (Exception ex)
                {
                    logger.Log($"Error executing command: {ex.Message}");
                }
            }
            else
            {
                // Handle unknown commands
                logger.Log($"Unknown command: {command}");
                logger.Log("Use \"janus help\" to get more details");
            }



        }

    }
}