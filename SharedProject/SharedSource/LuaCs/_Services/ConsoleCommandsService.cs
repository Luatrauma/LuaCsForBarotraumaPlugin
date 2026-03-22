using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma.LuaCs;

internal class ConsoleCommandsService : IConsoleCommandsService
{
    private readonly ConcurrentDictionary<string, DebugConsole.Command> _registeredCommands = new();
    
    public void Dispose()
    {
        if (!ModUtils.Threading.CheckIfClearAndSetBool(ref _isDisposed))
        {
            return;
        }

        foreach (var cmd in _registeredCommands.Values.ToImmutableArray())
        {
            DebugConsole.Commands.Remove(cmd);
        }

        _registeredCommands.Clear();
    }

    private int _isDisposed = 0;
    public bool IsDisposed
    {
        get => ModUtils.Threading.GetBool(ref _isDisposed);
        private set => ModUtils.Threading.SetBool(ref _isDisposed, value);
    }

    public void RegisterCommand(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false)
    {
        IService.CheckDisposed(this);
        var cmd = new DebugConsole.Command(name, help, onExecute, getValidArgs, isCheat);
        if (!_registeredCommands.TryAdd(name, cmd))
        {
            throw new ArgumentException($"A command with the name '{name}' is already registered.");
        }
        DebugConsole.Commands.Add(cmd);
    }

    public void AssignOnExecute(string names, Action<string[]> onExecute)
    {
        var matchingCommand = DebugConsole.Commands.Find(c => c.Names.Intersect(names.Split('|').ToIdentifiers()).Any());
        if (matchingCommand == null)
        {
            throw new Exception("AssignOnExecute failed. Command matching the name(s) \"" + names + "\" not found.");
        }
        else
        {
            matchingCommand.OnExecute = onExecute;
        }
    }

#if SERVER
    public void AssignOnClientRequestExecute(string names, Action<Client, Vector2, string[]> onClientRequestExecute)
    {
        var matchingCommand = DebugConsole.Commands.Find(c => c.Names.Intersect(names.Split('|').ToIdentifiers()).Any());
        if (matchingCommand == null)
        {
            throw new Exception("AssignOnClientRequestExecute failed. Command matching the name(s) \"" + names + "\" not found.");
        }
        else
        {
            matchingCommand.OnClientRequestExecute = onClientRequestExecute;
        }
    }
#endif

    public void RemoveCommand(string name)
    {
        IService.CheckDisposed(this);
        if (_registeredCommands.TryRemove(name, out DebugConsole.Command cmd))
        {
            DebugConsole.Commands.Remove(cmd);
        }
    }

    public void RemoveRegisteredCommands()
    {
        IService.CheckDisposed(this);
        foreach (var cmd in _registeredCommands.Values.ToImmutableArray())
        {
            DebugConsole.Commands.Remove(cmd);
        }
        _registeredCommands.Clear();
    }
}
