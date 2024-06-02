using Godot;
using System;

/// <summary>
/// Controls beings with animations (people, animals, etc) that need to initialize an animation when the level loads.
/// </summary>
public partial class AnimatedBeing : Node
{
	[Export]
   private string animationName;

   public override void _Ready()
   {
      GetNode<AnimationPlayer>("AnimationPlayer").Play(animationName);
   }
}
