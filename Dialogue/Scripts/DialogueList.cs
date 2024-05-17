using Godot;
using System;

// The reason this script exists is for branching interactions, since DialogueInteraction inherits from node and thus is difficult to set in the inspector.

[GlobalClass]
public partial class DialogueList : Resource
{
   [Export]
	public DialogueObject[] dialogues = new DialogueObject[1];
   [Export]
   public string branchName;
   [Export]
   public bool isBranching;
   [Export]
   public DialogueList[] branchingDialogues = new DialogueList[1];
}
