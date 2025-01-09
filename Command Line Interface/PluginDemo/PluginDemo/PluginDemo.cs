using Janus.Plugins;

namespace PluginDemo
{
    public class PluginDemo : ICommand
    {
        public string Name => "plugin";
        public string Description => "A demo command for plugins";

        public void Execute(string[] args)
        {
            Console.WriteLine("Plugin Demo - hello world");
        }
    }
}
