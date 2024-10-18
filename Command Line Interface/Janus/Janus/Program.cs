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

            // Check for "help"(ignoring cases)
            if (string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp();
                return;
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
        }

    }
}