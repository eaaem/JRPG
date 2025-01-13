using Godot;
using System;

public partial class DialogueRejectionBox : Area3D
{
   private DialogueInteraction dialogue;
   private Node3D moveLocation;
   private DialogueManager dialogueManager;
   private CharacterController controller;

   public override void _Ready()
   {
      dialogueManager = GetNode<DialogueManager>("/root/BaseNode/UI/DialogueScreen/Back");
      controller = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");

      moveLocation = GetNode<Node3D>("MoveLocation");
      dialogue = GetNode<DialogueInteraction>("DialogueHolder");
   }

   public void OnBodyEntered(Node3D body)
   {
      if (!dialogueManager.DialogueIsActive)
      {
         dialogueManager.InitiateDialogue(dialogue, false);
         Vector3 target = controller.GlobalPosition + (GlobalBasis.Z * 4f);
         controller.OverridenTargetLocation = target;
         controller.IsOverridingMovement = true;
         
         Basis lookAt = Basis.LookingAt(target - controller.GlobalPosition, Vector3.Up, true);
         controller.TargetOverridenRotation = lookAt.GetEuler().Y;
      }
   }
}
