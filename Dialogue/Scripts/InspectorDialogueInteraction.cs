using Godot;
using System;

[GlobalClass]
public partial class InspectorDialogueInteraction : Resource
{
   [Export]
	public DialogueList dialogues;
   /*[Export]
   public bool isBranching;
   [Export]
   public DialogueList[] branchingInteractions = new DialogueList[1];*/
   [Export]
   public CharacterType associatedInteractor;
   [Export]
   public CharacterType associatedReceiver;
}
