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
}
