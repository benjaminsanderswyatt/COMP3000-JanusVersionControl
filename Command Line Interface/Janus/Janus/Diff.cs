using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Janus
{
    public class Diff
    {
        public static void TestDiff(string before, string after)
        {
            var diff = InlineDiffBuilder.Diff(before, after);

            var savedColor = Console.ForegroundColor;
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+ ");
                        break;
                    case ChangeType.Deleted:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("- ");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("  ");
                        break;
                }

                Console.WriteLine(line.Text);
            }
            Console.ResetColor();
        }

    }
}