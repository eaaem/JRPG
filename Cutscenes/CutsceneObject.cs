using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CutsceneObject : Resource
{
   [Export]
   public CutsceneItem[] items = new CutsceneItem[0];
   [Export]
   public ActorStatus[] actors = new ActorStatus[1];
   //[Export]
   //public InspectorDialogueInteraction dialogueInteraction;
   [Export]
   public ActorCommand[] PreCutsceneCommands { get; set; } = new ActorCommand[0];
   [Export]
   public bool hideParty;
   [Export]
   public Vector3 cameraPosition;
   [Export]
   public Vector3 cameraRotation;
}
