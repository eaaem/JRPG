using Godot;
using System;

[GlobalClass]
public partial class InspectorDialogueInteraction : Resource
{
   [Export]
	public DialogueList dialogues;
   [Export]
   public CharacterType associatedInteractor;
   [Export]
   public CharacterType associatedReceiver;
}
