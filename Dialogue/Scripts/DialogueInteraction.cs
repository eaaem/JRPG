using Godot;
using System;
using System.Collections.Generic;

// Put this inside an EMPTY node3D inside whatever needs to be interacted with.

[GlobalClass]
public partial class DialogueInteraction : Node
{
   [Export]
	public DialogueList dialogueList;
   [Export]
   public DialogueList exitShopDialogue;
   /*[Export]
   public bool isBranching;
   [Export]
   public DialogueList[] branchingInteractions = new DialogueList[1];*/

   /*private DialogueManager dialogueManager;

   private bool currentlyHovering;

   public override void _Ready()
   {
      dialogueManager = GetNode<DialogueManager>("/root/BaseNode/UI/DialogueScreen/Back");
   }

   void OnHover()
   {
      currentlyHovering = true;
      GD.Print("hovering");
   }

   void OnStopHover()
   {
      currentlyHovering = false;
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("interact") && currentlyHovering && !dialogueManager.dialogueIsActive)
      {
         dialogueManager.InitiateDialogue(this);
      }
   }*/
}
