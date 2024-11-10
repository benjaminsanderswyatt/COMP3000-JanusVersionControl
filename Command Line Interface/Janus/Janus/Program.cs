using Janus.Plugins;

namespace Janus
{
    internal class Program
    {
        public static List<ICommand> CommandList { get; private set; } = new List<ICommand>();

        private static void LoadCommands()
        {
            CommandList.AddRange(CommandHandler.GetCommands());
            CommandList.AddRange(PluginLoader.LoadPlugins());
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter your command...");
                Console.WriteLine("Use \"janus help\" to get more details");
                return;
            }

            LoadCommands();

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
                    Console.WriteLine($"Error executing command: {ex.Message}");
                }
            }
            else
            {
                // Handle unknown commands
                Console.WriteLine($"Unknown command: {command}");
                Console.WriteLine("Use \"janus help\" to get more details");
            }

        }


        static void ShowHelp()
        {
            Console.WriteLine("Usage: janus [command]");
            Console.WriteLine("Commands:");
            Console.WriteLine("  init       Initialize a repository");
            Console.WriteLine("  commit     Commit changes");
            Console.WriteLine("  push       Push to a remote repository");
            Console.WriteLine("  -----TODO-----");
        }
        
    }
}