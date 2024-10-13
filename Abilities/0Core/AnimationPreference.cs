using Godot;
using System;

[GlobalClass]
public partial class AnimationPreference : Resource
{
	[Export]
   public string animationName;
   [Export]
   public WaitTimeEvent[] events = new WaitTimeEvent[0];
}