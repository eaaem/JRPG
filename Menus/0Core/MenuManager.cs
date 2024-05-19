using Godot;
using System;

public partial class MenuManager : Node
{
   public CanvasGroup menu;
   public CharacterController controller;
   private AnimationPlayer animationPlayer;
   private TabContainer container;
   //public bool closeDisabled;

   private PartyMenuManager partyMenuManager;
   private ItemMenuManager itemMenuManager;
   SettingsMenuManager settingsMenuManager;
   private MiscMenuManager mainMenuManager;

   public bool canTakeInput = true;

   Node3D baseNode;

   public override void _Ready()
   {
      menu = GetParent<CanvasGroup>();
      container = GetNode<TabContainer>("../TabContainer");
      partyMenuManager = container.GetNode<PartyMenuManager>("Party");
      itemMenuManager = container.GetNode<ItemMenuManager>("Items");
      settingsMenuManager = container.GetNode<SettingsMenuManager>("Settings");
      mainMenuManager = container.GetNode<MiscMenuManager>("Menu");
      baseNode = GetNode<Node3D>("/root/BaseNode");
      controller = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
   }

   void OnTabPressed(int tabID)
   {
      if (tabID == 0) // Party menu
      {
         partyMenuManager.LoadPartyMenu();
      }
      else if (tabID == 1) // Item menu
      {
         itemMenuManager.LoadItemMenu();
         partyMenuManager.isActive = false;
      }
      else if (tabID == 2) // Settings menu
      {
         settingsMenuManager.LoadSettingsMenu();
      }
      else if (tabID == 3) // Main menu
      {
         mainMenuManager.LoadMainMenu();
      }
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("menu") && canTakeInput)
      {
         if (menu.Visible)
         {
            if (partyMenuManager.isReequipping)
            {
               partyMenuManager.CancelReequip();
            }
            else if (partyMenuManager.isSwapping)
            {
               partyMenuManager.CancelSwap();
            }
            else if (itemMenuManager.isUsingItem)
            {
               itemMenuManager.CancelItemUsage();
            }
            else
            {
               CloseMenu();
            }
         }
         else
         {
            OpenMenu();
         }
      }
   }

   public void OpenMenu()
   {
      menu.Visible = true;
      controller.DisableMovement = true;
      controller.IsSprinting = false;
      controller.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Idle");
      EnableTabs();
      Input.MouseMode = Input.MouseModeEnum.Visible;
      container.CurrentTab = 0;
      partyMenuManager.LoadPartyMenu();

      if (baseNode.HasNode("WorldMap"))
      {
         baseNode.GetNode<WorldMapController>("WorldMap/Player").DisableMovement = true;
      }
   }

   public void CloseMenu()
   {
      menu.Visible = false;
      controller.DisableMovement = false;
      Input.MouseMode = Input.MouseModeEnum.Captured;

      if (baseNode.HasNode("WorldMap"))
      {
         baseNode.GetNode<WorldMapController>("WorldMap/Player").DisableMovement = false;
      }
   }

   public void DisableTabs()
   {
      for (int i = 0; i < container.GetTabCount(); i++)
      {
         container.SetTabDisabled(i, true);
      }
   }

   public void EnableTabs()
   {
      for (int i = 0; i < container.GetTabCount(); i++)
      {
         container.SetTabDisabled(i, false);
      }
   }
}
