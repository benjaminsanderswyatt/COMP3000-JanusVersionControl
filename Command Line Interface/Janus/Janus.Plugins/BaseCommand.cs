
namespace Janus.Plugins
{
    public abstract class BaseCommand : ICommand
    {
        protected ILogger Logger { get; private set; }
        protected Paths Paths { get; }
        protected BaseCommand(ILogger logger, Paths paths)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Usage { get; }
        public abstract Task Execute(string[] args);

    }
}
