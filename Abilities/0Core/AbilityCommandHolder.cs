using Godot;
using System;

/// <summary>
/// Holds a set of ability commands. Useful for standardized actions, like attacks, which usually use the exact same set of commands.
/// </summary>
public partial class AbilityCommandHolder : Node
{
   [Export]
   public AbilityCommand[] commands = new AbilityCommand[1];
}
