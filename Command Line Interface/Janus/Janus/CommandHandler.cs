using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus
{
    public static class CommandHandler
    {
        public static void Init(string[] args)
        {
            // Initialise .janus folder
            if (!Directory.Exists(Paths.janusDir))
            {
                Directory.CreateDirectory(Paths.objectDir);
                File.SetAttributes(Paths.janusDir, File.GetAttributes(Paths.janusDir) | FileAttributes.Hidden); // Makes the janus folder hidden

                Console.WriteLine("Initialized janus repository");
            }
            else
            {
                Console.WriteLine("Repository already initialized");
            }
        }

        public static void Add(string[] args)
        {
            string fileName = args[0];

            string blobHash = CommandHelper.SaveBlob(fileName);

            // Add the file and its hash to the staged area
            string stagedPath = Path.Combine(Paths.janusDir, "index");
            File.AppendAllText(stagedPath, $"{fileName} {blobHash}\n");


            Console.WriteLine($"Added {fileName} (blob {blobHash}).");
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
