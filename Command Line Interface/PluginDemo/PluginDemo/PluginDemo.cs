using Janus.Plugins;

namespace PluginDemo
{
    public class PluginDemo : ICommand
    {
        public string Name => "plugin";
        public string Description => "A demo command for plugins";

        public async Task Execute(string[] args)
        {
            Console.WriteLine("Plugin Demo - hello world");
        }
    }
}
