using Janus.Plugins;

namespace Janus
{
    internal class Program
    {
        public static List<ICommand> CommandList { get; private set; } = new List<ICommand>();

        private static void LoadCommands(ILogger logger)
        {
            CommandList.AddRange(CommandHandler.GetCommands(logger));

            CommandList.AddRange(PluginLoader.LoadPlugins(logger));
        }


        static void Main(string[] args)
        {
            // Config Logger
            ILogger logger = new ConsoleLogger();


            if (args.Length == 0)
            {
                logger.Log("Please enter your command...");
                logger.Log("Use \"janus help\" to get more details");
                return;
            }

            LoadCommands(logger);

            string commandName = args[0];

            ICommand? command = CommandList.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            if (command != null)
            {
                try
                {
                    command.Execute(args.Skip(1).ToArray());
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