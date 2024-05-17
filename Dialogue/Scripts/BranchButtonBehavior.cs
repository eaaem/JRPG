using Godot;
using System;

public partial class BranchButtonBehavior : Node
{
   private DialogueManager dialogueManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      dialogueManager = GetNode<DialogueManager>("/root/BaseNode/UI/DialogueScreen/Back");
	}

   void OnBranchDown()
   {
      dialogueManager.ReceiveBranchDown(GetParent<Button>().Text);
   }
}
