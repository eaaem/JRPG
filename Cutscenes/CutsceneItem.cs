using Godot;
using System;

[GlobalClass]
public partial class CutsceneItem : Resource
{
	[Export]
	public DialogueObject dialogue;
   [Export]
   public ActorCommand[] commands;
}
