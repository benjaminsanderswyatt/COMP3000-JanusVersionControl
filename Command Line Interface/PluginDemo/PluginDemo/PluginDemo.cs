using Janus.Plugins;

namespace PluginDemo
{
    public class PluginDemo : BaseCommand
    {
        public PluginDemo(ILogger logger, Paths paths) : base(logger, paths) { }
        public override string Name => "plugin";
        public override string Description => "A demo command for plugins";

        public override void Execute(string[] args)
        {
            Logger.Log("Plugin Demo - hello world");
            Logger.Log($"Plugin Demo - Paths: {Paths.JanusDir}");
        }
    }
}
