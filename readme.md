# Example Barotrauma Plugin Project

This repository provides a base template used by most Barotrauma plugins.

## Requirements

Before getting started, make sure you have an IDE that supports **.NET 8**:

- **Visual Studio 2022 or later** (with C# 10 support)  
  https://www.visualstudio.com/vs/community/
- **JetBrains Rider**  
  https://www.jetbrains.com/rider/

It is also recommended to have **Git** installed:

- **Windows**: Download from https://git-scm.com/install/windows  
- **Linux**: Install Git using your distribution’s package manager

## Getting the Project

There are two ways to obtain the project:

### Option 1: Fork the Repository (Recommended)

1. Create or sign in to a GitHub account.
2. Click the **Fork** button in the top-right corner of this repository.
3. Clone your fork locally:
   ```bash
   git clone https://github.com/yourname/yourforkname
   ```

### Option 2: Download Directly

You can also download the repository as a ZIP file from GitHub and extract it locally, though this option does not include version control.

## Setting up the Project

Before opening the project, a few configuration steps are required.

> `.props` files can be edited using any text editor.

### Required Configuration

- Edit the file `BuildData.props` and replace `ExampleMod` with your actual mod name.
- Locate the file `UserBuildData.props.example` and create a copy named `UserBuildData.props`.
  - Edit `UserBuildData.props`.
  - Update the `ModDeployDir` value so it points to the **LocalMods** directory of your Barotrauma installation.

## Opening the Project

You can now open the solution file `WindowsSolution.sln` (or the appropriate solution for your platform).

> If the `.sln` file does not open, ensure that you have a compatible .NET IDE installed.

Once the solution is open, start by editing `Plugin.cs`, located in the **SharedProject** in the solution explorer.

You may also rename existing `ExampleMod` namespaces in the code to whatever fits your mod.

## Project Structure

- **Server-side code**: `ServerProject/ServerSource`
- **Client-side code**: `ClientProject/ClientSource`
- **Shared code** (used by both client and server): `SharedProject/SharedSource`

Within the **SharedProject**, you can use the `CLIENT` and `SERVER` preprocessor symbols to conditionally compile code for the client or the server as needed.

```cs
#if SERVER
DebugConsole.NewMessage("Hello from server");
#endif

#if CLIENT
DebugConsole.NewMessage("Hello from client");
#endif
```

### Content

All mod content (XML files, sounds, textures, etc.) should be placed in:

- `SharedProject/Content`

If you add new XML content, make sure to update the file list located at:

- `SharedProject/filelist.xml`

## Testing Your Mod

You can test your mod by right-clicking the **ClientProject** and **ServerProject**, then selecting **Build**.

This will automatically generate the mod in the `LocalMods` folder you configured earlier. You can then enable the mod in-game.

> **Warning**  
> This method builds the mod in **Debug** configuration and only for the current platform.  
> To properly publish or update your mod, follow the steps below.

## Building for Release

1. Select **PluginToolbox** as the startup project.
2. Run the project.
3. When the terminal prompt appears, type `build`

## Hot Reload

.NET supports Hot Reload, which lets you apply certain code changes while Barotrauma is running, without rebuilding the mod or restarting the game. This is useful for quickly iterating on gameplay logic.

To use Hot Reload in Visual Studio, start Barotrauma normally with your mod enabled. Open the plugin solution in Visual Studio, then attach the debugger to the running `Barotrauma.exe` process using **Debug → Attach to Process**.

Once attached, make changes to your C# code and apply them using **Hot Reload** (toolbar button) or **Alt + F10**. If the change is supported, it will be applied immediately and you can test it in-game.

Hot Reload works best for changes inside existing methods. Structural changes such as adding new classes, changing method signatures, or modifying fields usually require reloading the entire mod.
