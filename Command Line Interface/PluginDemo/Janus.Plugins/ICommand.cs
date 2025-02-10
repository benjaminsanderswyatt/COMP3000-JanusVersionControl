
namespace Janus.Plugins
{
    public interface ICommand
    {
        string Name { get; } // Command name
        string Description { get; } // Command description
        Task Execute(string[] args); // Command execution logic
    }
}
