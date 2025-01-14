using Janus.Plugins;

namespace PluginDemo
{
    public class PluginDemo : BaseCommand
    {
        public override string Name => "plugin";
        public override string Description => "A demo command for plugins";

        public override void Execute(string[] args)
        {
            Logger.Log("Plugin Demo - hello world");
        }
    }
}
