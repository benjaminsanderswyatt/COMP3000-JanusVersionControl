namespace Janus.Plugins
{
    public interface ICommand
    {
        string Name { get; } // Command name
        string Description { get; } // Command description
        void Execute(string[] args); // Command execution logic

        void SetLogger(ILogger logger); // Set logger for the command

    }
}
