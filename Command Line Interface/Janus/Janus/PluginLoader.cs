using Janus.Plugins;
using System.Reflection;

namespace Janus
{
    public class PluginLoader
    {
        public static List<ICommand> LoadPlugins(ILogger logger, Paths paths)
        {
            string pluginsDir = paths.PluginsDir;
            List<ICommand> commands = new List<ICommand>();

            if (!Directory.Exists(pluginsDir))
            {
                return commands;
            }

            var dllFiles = Directory.GetFiles(pluginsDir, "*.dll");

            foreach (var dll in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);

                    var commandTypes = assembly.GetTypes()
                        .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsAbstract);

                    foreach (var type in commandTypes)
                    {
                        // Use reflection to find the correct constructor
                        var constructor = type.GetConstructors().FirstOrDefault(c => c.GetParameters()
                                .All(p => p.ParameterType == typeof(ILogger) || p.ParameterType == typeof(Paths)));

                        if (constructor != null)
                        {
                            // Use Activator to create the instance with parameters
                            var command = (ICommand)Activator.CreateInstance(type, logger, paths);

                            commands.Add(command);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Error loading plugin {dll}: {ex.Message}");
                }
            }

            return commands;
        }
    }

}
