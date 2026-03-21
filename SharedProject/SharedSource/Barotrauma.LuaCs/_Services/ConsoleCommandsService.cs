using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs;

public class ConsoleCommandsService : IConsoleCommandsService
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
    public FluentResults.Result RegisterCommand(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false)
    {
        IService.CheckDisposed(this);
        var cmd = new DebugConsole.Command(name, help, onExecute, getValidArgs, isCheat);
        if (!_registeredCommands.TryAdd(name, cmd))
        {
            return FluentResults.Result.Fail($"{nameof(RegisterCommand)}: A command with the name '{name}' is already added.");
        }
        DebugConsole.Commands.Add(cmd);
        return FluentResults.Result.Ok();
    }

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
