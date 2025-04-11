# Janus CLI Documentation

Janus is a version control command-line tool for managing codebases. It provides commands for repository configuration, managing branches, making commits, and synchronizing changes with remote servers. This documentation describes each command’s purpose, usage, and examples.

---

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Commands Reference](#commands-reference)
  - [help](#help)
  - [config](#config)
  - [login](#login)
  - [remote](#remote)
  - [clone](#clone)
  - [fetch](#fetch)
  - [pull](#pull)
  - [push](#push)
  - [init](#init)
  - [add](#add)
  - [commit](#commit)
  - [revert](#revert)
  - [log](#log)
  - [diff](#diff)
  - [merge](#merge)
  - [create_branch](#create_branch)
  - [list_branch](#list_branch)
  - [switch_branch](#switch_branch)
  - [status](#status)
- [Workflow and Usage Tips](#workflow-and-usage-tips)
- [Plugin System](#plugin-system)
- [Additional Notes](#additional-notes)

---

## Overview

Janus is a version control CLI that:

- **Manages repository configuration:** Adjust repository properties with `config`.
- **Supports local version control:** Stage changes using `add`, commit with `commit`, and view commit logs and differences.
- **Manages branches:** Create, list, switch, merge branches, and revert to previous commits.
- **Handles authentication:** Use `login` to store your credentials for remote repositories.
- **Remote repository interactions:** Clone, fetch, pull, and push changes with remote repositories.

---

## Getting Started

Before using most commands, ensure you have done the following:

- Installed Janus on your system.
- Log in with your credentials using the `janus login` command.
- Initialised a repository with `janus init` if not already done.

---

## Commands Reference

Each command supports a **usage string** that explains arguments, options, and examples. 
The following sections detail each command.

### help

**Description:**  
Displays a list of all available commands (with short descriptions) or detailed usage of a specific command if provided.

**Usage:**  
```
janus help [command]
```

**Examples:**  
- Show a list of all commands:
  ```
  janus help
  ```
- Get detailed help about the clone command:
  ```
  janus help clone
  ```

---

### config

**Description:**  
Manages local and global configuration settings. Includes subcommands for IP configuration and repository properties.

**Usage:**  
```
janus config <subcommand> [arguments]
```

**Subcommands:**
- `ip get [--global]`: Gets the configured IP (local or global)
- `ip set <value> [--global]`: Sets the IP configuration
- `ip reset [--global]`: Removes the IP configuration file
- `repo get <property>`: Gets a repository property (e.g., is-private, description)
- `repo set <property> <value>`: Sets a repository property

**Examples:**  
```
janus config ip get
janus config ip set 192.168.1.100 --global
janus config repo get is-private
janus config repo set description "My project"
```

---

### login

**Description:**  
Prompts you for your user credentials username, email, and personal access token (which is only required for commands interacting with a remote repository) and saves them for later repository interactions. 

**Usage:**  
```
janus login
```

**Example:**  
```
janus login
```
*Follow on-screen prompts to enter your credentials.*

---

### remote

**Description:**  
Manages remote repository settings. This command supports adding, removing and listing remote repositories.

**Usage:**  
```
janus remote <subcommand> [arguments]
```

**Subcommands:**
- `add <name> <endpoint>`: Adds a new remote repository.
- `remove <name>`: Removes an existing remote repository.
- `list`: Lists all configured remote repositories.

**Example:**  
```
janus remote add origin janus/repo/user
```

---

### clone

**Description:**  
Clones a repository from a remote server to your local machine, initialising repository folders and setting branch configurations.

**Usage:**  
```
janus clone <endpoint> [branch]
```

**Parameters:**  
- `<endpoint>`: Repository endpoint in the format `janus/{owner}/{repoName}`.
- `[branch]`: (Optional) Branch to check out. Defaults to `main` if not specified.

**Example:**  
```
janus clone janus/owner/repo main
```

---

### fetch

**Description:**  
Fetches the latest commits and repository information from a remote repository. Updates local commit history and repository configuration.

**Usage:**  
```
janus fetch [remote]
```

**Examples:**  
```
janus fetch
janus fetch upstream
```

---

### pull

**Description:**  
Synchronises local branches with the fetched remote commits. It analyzes each remote branch and fast-forwards if possible.

**Usage:**  
```
janus pull
```

**Example:**  
```
janus pull
```

---

### push

**Description:**  
Pushes local commits to a remote repository, uploading new commits along with file objects if changes are found.

**Usage:**  
```
janus push [remote] [branch]
```

**Examples:**  
```
janus push
janus push origin main
```

---

### init

**Description:**  
Initialises the Janus repository by creating the necessary directory structure (`.janus` folder) and configuration files.

**Usage:**  
```
janus init
```

**Example:**  
```
janus init
```

---

### add

**Description:**  
Stages the specified files or directories for the next commit.

**Usage:**  
```
janus add <file(s) or directory>
```

**Examples:**  
```
janus add file.txt
janus add folder
janus add --all
```

---

### commit

**Description:**  
Commits the staged changes to the repository with a mandatory commit message.

**Usage:**  
```
janus commit <commit message>
```

**Example:**  
```
janus commit "Fixed bug in file upload"
```

---

### revert

**Description:**  
Reverts your repository state to the state of a previous commit.

**Usage:**  
```
janus revert <commit-hash> [--force]
```

**Examples:**  
```
janus revert abcdef
janus revert 123456 --force
```

---

### log

**Description:**  
Displays the commit history. You can filter the results by branch, author, and date times, or limit the number of results displayed.

**Usage:**  
```
janus log [options]
```

**Example:**  
```
janus log --branch main --author Alice --since 2023-01-01 --limit 10
```

---

### diff

**Description:**  
Displays differences between commits.

**Usage:**  
```
janus diff [options] [<commit> [<commit>]] [--path <file>]
```

**Examples:**  
```
janus diff
janus diff --staged
janus diff abc123 def456
janus diff abc123 def456 --path file.txt
janus diff abc123 --parent
```

---

### merge

**Description:**  
Merges changes from another branch into the current branch.

**Usage:**  
```
janus merge <branch>
```

**Example:**  
```
janus merge featureBranch
```

---

### create_branch

**Description:**  
Creates a new branch from the current HEAD commit.

**Usage:**  
```
janus create_branch <branch name>
```

**Example:**  
```
janus create_branch featureBranch
```

---

### list_branch

**Description:**  
Lists all branches in the repository.

**Usage:**  
```
janus list_branch
```

**Example:**  
```
janus list_branch
```

---

### switch_branch

**Description:**  
Switches the working directory to a different branch.

**Usage:**  
```
janus switch_branch <branch name> [--force]
```

**Examples:**  
```
janus switch_branch develop
janus switch_branch featureBranch --force
```

---

### status

**Description:**  
Displays the current repository status.

**Usage:**  
```
janus status
```

**Example:**  
```
janus status
```

---

## Workflow and Usage Tips

- **Initialization:**  
  Always start with `janus init` in a new project directory.

- **Making Changes:**  
  Use `janus add` to stage new or modified files, then commit using `janus commit` with a descriptive message.

- **Branch Management:**  
  Use `janus create_branch` to start new features and `janus switch_branch` to safely change to the new branch. Display existing branches with `janus list_branch`.

- **Remote Operations:**  
  Clone repositories from a remote source with `janus clone`. Keep your repository up to date by using `janus fetch`, `janus pull`, and `janus push`.

- **Version History:**  
  Review commit history with `janus log` and compare commit differences using `janus diff`.

- **Handling Conflicts and Reverts:**  
  Merge with `janus merge` or revert using `janus revert`.

---

## Plugin System

Janus supports an extensible plugin architecture that allows developers to add custom commands by placing compiled `.dll` files into the designated plugin folders.

### How It Works

At startup, Janus loads all built in commands and scans for external plugin assemblies located in:

- **Local Plugins:**  
  `<project-root>/.janus/plugins`

- **Global Plugins:**  
  `<user-home>/Janus/plugins`

Each plugin must implement the `ICommand` interface defined in `Janus.Plugins`.

### ICommand Interface

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    Task Execute(string[] args);
}
```

### BaseCommand (Convenience Class)

For easier plugin creation, extend the BaseCommand class which provides access to logging and paths:

```csharp
public abstract class BaseCommand : ICommand
{
    protected ILogger Logger { get; }
    protected Paths Paths { get; }

    public BaseCommand(ILogger logger, Paths paths)
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
### Example Plugin

Below is an example of a simple plugin that outputs "hello world":
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
```

### Using Plugin Commands

Once the plugin is compiled into a `.dll` and placed in the plugin directory, the command is picked up by Janus to be used.

- Run janus help to see the added plugin command listed.
- Execute the plugin command as you would with any built in command:


---

## Additional Notes

- **User Prompts:**  
  Some commands require confirmation (e.g., `switch_branch`, `pull`, `revert`).

- **Configuration:**  
  Use `config` for managing IP settings and repo metadata.

- **Error Logs:**  
  Errors and conflicts are logged to assist in troubleshooting.
  
- **Plugin Developer Guide:**  
  For more detailed information on developing plugins, refer to the Plugin Developer Guide.

---