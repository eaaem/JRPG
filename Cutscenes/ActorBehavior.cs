using Godot;
using System;

[GlobalClass]
public partial class ActorBehavior : Resource
{
   [Export]
   public int DialogueIndex { get; set;}
   [Export]
   public ActorCommand[] Commands { get; set; }
}
