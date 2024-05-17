using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CutsceneObject : Resource
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
