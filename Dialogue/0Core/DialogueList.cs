using Godot;
using System;

/// <summary>
/// Serves as a list of dialogue items and a way to branch. <c>DialogueList</c> should be used for essentially any dialogue.
/// </summary>
[GlobalClass]
public partial class DialogueList : Resource
{
   [Export]
	public DialogueObject[] dialogues = new DialogueObject[0];
   /// <summary>
   /// The name of this dialogue, if this dialogue list is a branch from another dialogue. Leave blank if this dialogue is not a branch.
   /// </summary>
   [Export]
   public string branchName;
   /// <summary>
   /// The number of branches from this dialogue. If this dialogue doesn't have branches, leave the array empty.
   /// </summary>
   [Export]
   public DialogueList[] branchingDialogues = new DialogueList[0];
}
