using Janus.Plugins;
using System.Reflection;

namespace Janus
{
    public class PluginLoader
    {
        public static List<ICommand> LoadPlugins(ILogger logger, Paths paths)
        {
            List<ICommand> commands = new List<ICommand>();
            
            // Load local plugins
            if (Directory.Exists(paths.PluginsDir))
            {
                var localDllFiles = Directory.GetFiles(paths.PluginsDir, "*.dll");
                commands.AddRange(LoadPluginsFromDirectory(logger, localDllFiles, paths));
            }

            // Load global plugins
            if (Directory.Exists(paths.GlobalPluginsDir))
            {
                var globalDllFiles = Directory.GetFiles(paths.GlobalPluginsDir, "*.dll");
                commands.AddRange(LoadPluginsFromDirectory(logger, globalDllFiles, paths));
            }

            return commands;
        }


        private static List<ICommand> LoadPluginsFromDirectory(ILogger logger, string[] dllFiles, Paths paths)
        {
            List<ICommand> commands = new List<ICommand>();

            foreach (var dll in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);

                    var commandTypes = assembly.GetTypes()
                        .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsAbstract);

                    foreach (var type in commandTypes)
                    {
                        // Look for a constructor that accepts ILogger and/or Paths
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
