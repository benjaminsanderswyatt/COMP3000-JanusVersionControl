namespace Janus.Plugins
{
    public abstract class BaseCommand : ICommand
    {
        protected ILogger Logger { get; private set; }
        public void SetLogger(ILogger logger)
        {
            Logger = logger;
        }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract void Execute(string[] args);

    }
}
