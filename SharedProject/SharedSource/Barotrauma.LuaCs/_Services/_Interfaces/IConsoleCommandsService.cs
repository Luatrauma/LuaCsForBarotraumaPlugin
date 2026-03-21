using System;

namespace Barotrauma.LuaCs;

public interface IConsoleCommandsService : IService
{
    FluentResults.Result RegisterCommand(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null,
        bool isCheat = false);
    void RemoveCommand(string name);
    void RemoveRegisteredCommands();
}
