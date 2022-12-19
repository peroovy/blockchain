using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WalletPeer.Commands;

public class HelpCommand : ICommand
{
    private readonly ImmutableArray<ICommand> commands;

    public HelpCommand(IEnumerable<ICommand> commands)
    {
        this.commands = commands
            .Where(command => command != this)
            .ToImmutableArray();
    }

    public string Name => "help";

    public string Description => "Shows all commands";

    public void Execute()
    {
        foreach (var command in commands)
            Console.WriteLine($"{command.Name} - {command.Description}");
    }
}