using Godot;
using System;

/// <summary>
/// Stores wait time preferences for combat animations. Used by ability/attack graphics.
/// </summary>
public partial class AnimationPreferences : Node
{
	[Export]
   public AnimationPreference[] preferences = new AnimationPreference[0];
}
