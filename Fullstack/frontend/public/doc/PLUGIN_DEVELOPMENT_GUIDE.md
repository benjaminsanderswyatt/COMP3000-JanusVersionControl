# Plugin Developer Guide

Janus supports an extensible plugin architecture that allows you to add custom commands by creating a plugin assembly (a `.dll` file). This guide explains the plugin system and how you develop plugins for Janus CLI.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
  - [Plugin Loading](#plugin-loading)
- [Developing a Plugin](#developing-a-plugin)
  - [Implementing the ICommand Interface](#implementing-the-icommand-interface)
  - [Using BaseCommand for Convenience](#using-basecommand-for-convenience)
- [Example Plugin](#example-plugin)
- [Deployment](#deployment)
- [Additional Considerations](#additional-considerations)

---

## Overview

Janus can automatically load custom commands from external plugin assemblies. By following this guide, you will create a plugin that integrates with Janus and extends its functionality.

Plugins are compiled as .dll files and must implement the ICommand interface defined in the Janus.Plugins namespace. Janus searches designated plugin directories and loads any compatible commands at startup.

---

## Architecture

### Plugin Loading

When Janus starts, it scans two key directories for plugin assemblies:

- **Local Plugins:** Located in `<project-root>/.janus/plugins`
- **Global Plugins:** Located in `<user-home>/.janus/plugins`

For each plugin assembly (.dll file), Janus uses reflection to discover classes implementing the `ICommand` interface. It instantiates these classes, supplying dependencies such as `ILogger` and `Paths` objects via the class constructor.

---

## Developing a Plugin

### Implementing the ICommand Interface

A base plugin must implement the ICommand interface:

```csharp
namespace Janus.Plugins
{
    public interface ICommand
    {
        string Name { get; }         // The command name used to invoke the plugin
        string Description { get; }  // A short description of the command
        string Usage { get; }        // Detailed usage instructions for the command
        Task Execute(string[] args); // The logic that executes when the command is run
    }
}
```

### Using BaseCommand for Convenience

Janus provides a BaseCommand abstract class that implements ICommand and helps by providing:

- A logging mechanism via an `ILogger` instance.
- Access to paths and other utility functions via a `Paths` object.

To create your plugin, simply extend the `BaseCommand` class:

```csharp
using Janus.Plugins;

public abstract class BaseCommand : ICommand
{
    protected ILogger Logger { get; }
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
```

This simplifies development, by providing access to commonly used services.

---

## Example Plugin

Below is a simple example "Hello World" plugin:

```csharp
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
This command displays:
    - A greeting message
Example:
    janus plugin";

        public override async Task Execute(string[] args)
        {
            // Write your custom logic here.
            Logger.Log("Plugin Demo - hello world");
        }
    }
}
```

Compile this plugin into a .dll and ensure it references the proper Janus assemblies so that the ICommand, ILogger, Paths, and BaseCommand types are available.

---

## Deployment

1. Compile Your Plugin:
Ensure your project targets a compatible .NET runtime version with Janus.

2. Place the DLL:

    For local plugins: Copy the compiled DLL to <project-root>/.janus/plugins.

    For global plugins: Copy the DLL to <user-home>/.janus/plugins.

3. Run Janus:
When you start Janus, it will scan the plugin directories and automatically include your plugin command.

4. Test Your Plugin:

    List available commands with:
```bash
janus help
```

---

## Additional Considerations

- Constructor Dependencies:
Your plugin classes should have a constructor that accepts an ILogger and Paths. Janus uses reflection to instantiate your plugin, so ensure your constructor parameters match those expected types.

- Error Handling:
Use try catch blocks within your Execute method to manage any runtime errors gracefully.

- Documentation:
Document your plugin’s usage string and commands clearly for users who will execute your command.

- Multiple Commands:
An assembly can contain multiple classes implementing ICommand. Each will be loaded and listed as separate commands in Janus.

- Versioning and Compatibility:
Keep track of changes in the Janus plugin interfaces. When updating Janus, verify that your plugins are still compatible.