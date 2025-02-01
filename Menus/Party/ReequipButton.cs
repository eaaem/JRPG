using Godot;
using System;

public partial class ReequipButton : Node
{
   private PartyMenuManager menuManager;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      menuManager = GetNode<PartyMenuManager>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu/MenuContainer/Party");
      GetParent<Button>().ButtonDown += OnReequipDown;
	}

   void OnReequipDown()
   {
      menuManager.Reequip(GetNode<ItemResourceHolder>("../ResourceHolder").itemResource.item);
   }

   public override void _ExitTree()
   {
      GetParent<Button>().ButtonDown -= OnReequipDown;
   }
}
