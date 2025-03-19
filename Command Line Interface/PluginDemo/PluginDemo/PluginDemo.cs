using Janus.Plugins;

namespace PluginDemo
{
    public class PluginDemo : BaseCommand
    {
        public PluginDemo(ILogger logger, Paths paths) : base(logger, paths) { }
        public override string Name => "plugin";
        public override string Description => "A demo command for plugins";
        public override string Usage =>
@"janus plugin
This command shows:
    - Says hello world
Example:
    janus plugin";
        public override async Task Execute(string[] args)
        {
            Logger.Log("Plugin Demo - hello world");
        }
    }
}
