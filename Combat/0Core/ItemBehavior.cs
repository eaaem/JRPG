using Godot;
using System;
using System.Collections.Generic;

public abstract partial class ItemBehavior : Node
{
   protected CombatManager combatManager;
   protected CombatItemManager itemManager;
   protected CombatUIManager uiManager;

   protected InventoryItem resource;
   protected Button button;
   protected Button cancelButton;

   protected ItemMenuManager itemMenuManager;
   protected MenuManager menuManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      ReadyBehavior();
	}

   public void ReadyBehavior()
   {
      // This needs to be its own function for scripts that override the ready behavior (those scripts need to call this, or else they'll miss important behaviors)
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManager");
      itemManager = combatManager.GetNode<CombatItemManager>("ItemManager");
      uiManager = combatManager.GetNode<CombatUIManager>("UIManager");

      itemMenuManager = GetNode<ItemMenuManager>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu/MenuContainer/Items");
      menuManager = itemMenuManager.GetNode<MenuManager>("../../MenuManager");
      cancelButton = GetNode<Button>("/root/BaseNode/UI/Options/CancelButton");

      button = GetParent<Button>();
      resource = button.GetNode<ItemResourceHolder>("ResourceHolder").itemResource;
      button.ButtonDown += OnButtonDown;
      itemManager.ItemUse += OnItemUse;
      itemMenuManager.ItemUse += OnOutOfCombatItemUse;
   }

   public void FriendlyItemInCombatUse()
   {
      combatManager.CurrentItem = resource;
      cancelButton.Visible = true;
      uiManager.GenerateTargets();
      uiManager.SetItemListVisible(false);
   }

   public void OutOfCombatItemSelect()
   {
      itemMenuManager.currentItem = resource;
      itemMenuManager.OpenPartyScreen();
      menuManager.DisableTabs();
      itemMenuManager.DisableMenu();
      itemMenuManager.isUsingItem = true;
      itemMenuManager.partyContainer.Visible = true;
   }

   public abstract void OnButtonDown();
   public abstract void OnItemUse();
   public abstract void OnOutOfCombatItemUse();

	public override void _ExitTree()
   {
      itemManager.ItemUse -= OnItemUse;
      itemMenuManager.ItemUse -= OnOutOfCombatItemUse;
   }
}
