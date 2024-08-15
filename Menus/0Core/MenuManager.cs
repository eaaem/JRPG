using Godot;
using System;

public partial class MenuManager : Node
{
   [Export]
   private ManagerReferenceHolder managers;

   public CanvasGroup menu;
   public CharacterController controller;
   private AnimationPlayer animationPlayer;
   private TabContainer container;
   //public bool closeDisabled;

   SettingsMenuManager settingsMenuManager;
   MiscMenuManager miscMenuManager;

   public bool canTakeInput = true;

   Node3D baseNode;

   ColorRect blackScreen;
   public bool BlackScreenIsVisible { get; set; } = false;

   public override void _Ready()
   {
      menu = GetParent<CanvasGroup>();
      container = GetNode<TabContainer>("../TabContainer");
      settingsMenuManager = container.GetNode<SettingsMenuManager>("Settings");
      baseNode = GetNode<Node3D>("/root/BaseNode");
      controller = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
      miscMenuManager = container.GetNode<MiscMenuManager>("Menu");

      blackScreen = GetNode<ColorRect>("/root/BaseNode/UI/Overlay/BlackScreen");
   }

   void OnTabPressed(int tabID)
   {
      if (tabID == 0) // Party menu
      {
         managers.PartyMenuManager.LoadPartyMenu();
      }
      else if (tabID == 1) // Item menu
      {
         managers.ItemMenuManager.LoadItemMenu();
         managers.PartyMenuManager.isActive = false;
      }
      else if (tabID == 2) // Settings menu
      {
         settingsMenuManager.LoadSettingsMenu();
      }
      else if (tabID == 3) // Main menu
      {
         miscMenuManager.LoadMainMenu();
      }
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("menu") && canTakeInput)
      {
         if (menu.Visible)
         {
            if (managers.PartyMenuManager.isReequipping)
            {
               managers.PartyMenuManager.CancelReequip();
            }
            else if (managers.PartyMenuManager.isSwapping)
            {
               managers.PartyMenuManager.CancelSwap();
            }
            else if (managers.ItemMenuManager.isUsingItem)
            {
               managers.ItemMenuManager.CancelItemUsage();
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
      controller.DisableCamera = true;
      controller.IsSprinting = false;
      controller.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Idle");
      EnableTabs();
      Input.MouseMode = Input.MouseModeEnum.Visible;
      container.CurrentTab = 0;
      managers.PartyMenuManager.LoadPartyMenu();

      if (baseNode.HasNode("WorldMap"))
      {
         baseNode.GetNode<WorldMapController>("WorldMap/Player").DisableMovement = true;
      }
   }

   public void CloseMenu()
   {
      menu.Visible = false;
      controller.DisableMovement = false;
      controller.DisableCamera = false;
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

   public async void FadeBlack(float delay)
   {

      while (blackScreen.Color.A < 1)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         float alpha = blackScreen.Color.A;
         alpha += 0.05f;
         blackScreen.Color = new Color(0, 0, 0, alpha);
      }

      await ToSignal(GetTree().CreateTimer(delay), "timeout");

      while (blackScreen.Color.A > 0)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         float alpha = blackScreen.Color.A;
         alpha -= 0.05f;
         blackScreen.Color = new Color(0, 0, 0, alpha);
      }
   }

   public async void FadeFromBlack()
   {
      while (blackScreen.Color.A >= 0)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         float alpha = blackScreen.Color.A;
         alpha -= 0.05f;
         blackScreen.Color = new Color(0, 0, 0, alpha);
      }

      BlackScreenIsVisible = false;
   }

   public async void FadeToBlack()
   {
      blackScreen.Color = new Color(0, 0, 0, 0);

      while (blackScreen.Color.A <= 1)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         float alpha = blackScreen.Color.A;
         alpha += 0.05f;
         blackScreen.Color = new Color(0, 0, 0, alpha);
      }

      BlackScreenIsVisible = true;
   }

   public void SetBlackScreenAlpha(float alpha)
   {
      blackScreen.Color = new Color(0, 0, 0, alpha);
   }
}
