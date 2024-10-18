using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus
{
    public static class CommandHandler
    {
        private static string janusDir = ".janus";

        public static void Init(string[] args)
        {
            // Initialise .janus folder
            if (!Directory.Exists(janusDir))
            {
                Directory.CreateDirectory(janusDir);
                File.SetAttributes(janusDir, File.GetAttributes(janusDir) | FileAttributes.Hidden); // Makes the janus folder hidden

                Console.WriteLine("Initialized janus repository");
            }
            else
            {
                Console.WriteLine("Repository already initialized");
            }
        }

        public static void Add(string[] args)
        {
            Console.WriteLine("Add");
        }

        public static void Commit(string[] args)
        {
            Console.WriteLine("Commit");
        }

        public static void Log(string[] args)
        {
            Console.WriteLine("Log");
        }

    }
}
