using System.Reflection;

namespace Janus
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter your command...");
                Console.WriteLine("Use \"janus help\" to get more details");
                return;
            }

            string command = args[0];

            // Check for "help"(ignoring cases)
            if (string.Equals(command, "help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp();
                return;
            }

            Type type = typeof(CommandHandler);

            // Use reflection to get the method of the command (ignoring cases)
            MethodInfo? method = type.GetMethod(command, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);

            if (method != null)
            {
                try
                {
                    // Invoke the method of the command inputted
                    method.Invoke(null, new object[] { args.Skip(1).ToArray() });
                } 
                catch (Exception ex)
                {
                    //Handle errors
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


        /*
         * Displays the help method
         */
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