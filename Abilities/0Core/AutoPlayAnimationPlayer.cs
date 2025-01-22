using Godot;
using System;

public partial class AutoPlayAnimationPlayer : AnimationPlayer
{
	[Export]
   string animationName;
   [Export]
   float blend = -1f;

   public override void _Ready()
   {
      Play(animationName, blend);
   }
}
