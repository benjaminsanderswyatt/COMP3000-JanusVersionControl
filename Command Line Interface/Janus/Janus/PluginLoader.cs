using Janus.Plugins;
using System.Reflection;

namespace Janus
{
    public class PluginLoader
    {
        public static List<ICommand> LoadPlugins(ILogger logger)
        {
            string pluginsDir = Paths.pluginsDir;
            List<ICommand> commands = new List<ICommand>();

            if (!Directory.Exists(pluginsDir))
            {
                return commands;
            }

            var dllFiles = Directory.GetFiles(pluginsDir, "*.dll");

            foreach (var dll in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dll);

                var commandTypes = assembly.GetTypes()
                    .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsAbstract);

                foreach (var type in commandTypes)
                {
                    if (Activator.CreateInstance(type) is ICommand command)
                    {
                        command.SetLogger(logger);
                        commands.Add(command);
                    }
                }
            }

            return commands;
        }
    }

}
