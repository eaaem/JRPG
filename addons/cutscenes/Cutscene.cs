using Godot;
using System;

public partial class Cutscene : Node3D
{
	[Export]
   public ActorBehavior[] actorBehaviors = new ActorBehavior[1];
   [Export]
   public ActorStatus[] actors = new ActorStatus[1];
   [Export]
   public InspectorDialogueInteraction dialogueInteraction;
   [Export]
   public bool hideParty;
   [Export]
   public Vector3 cameraPosition;
   [Export]
   public Vector3 cameraRotation;
}
