using Godot;
using System;

public partial class MenuManager : Node
{
   [Export]
   private ManagerReferenceHolder managers;
   [Export]
   private AudioStreamPlayer openSound;
   [Export]
   private AudioStreamPlayer closeSound;

   public Control menu;
   public CharacterController controller;
   private AnimationPlayer animationPlayer;
   private Panel container;

   private Button[] tabs = new Button[5];
   private Panel[] menuPanels = new Panel[3];


   private Panel quitOptions;
   private Panel quitConfirmation;

   private int activeTabID = 0;
   public int ActiveSlot { get; set; }
   private bool isSaving;
   private bool isQuitting;

   SettingsMenuManager settingsMenuManager;
   MiscMenuManager miscMenuManager;

   public bool canTakeInput = true;

   [Signal]
   public delegate void FadeToBlackCompletedEventHandler();

   Node3D baseNode;

   private ColorRect blackScreen;
   public bool BlackScreenIsVisible { get; set; } = false;

   public override void _Ready()
   {
      menu = GetParent<Control>();
      container = GetNode<Panel>("../MenuContainer");
      settingsMenuManager = container.GetNode<SettingsMenuManager>("Settings");
      baseNode = GetNode<Node3D>("/root/BaseNode");
      controller = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
      miscMenuManager = container.GetNode<MiscMenuManager>("Menu");
      quitOptions = container.GetNode<Panel>("QuitOptions");
      quitConfirmation = quitOptions.GetNode<Panel>("QuitConfirmation");

      container.GetNode<Button>("Additional/TabsContainer/PartyButton").ButtonDown += () => OnTabPressed(0);
      container.GetNode<Button>("Additional/TabsContainer/ItemButton").ButtonDown += () => OnTabPressed(1);
      container.GetNode<Button>("Additional/TabsContainer/SettingsButton").ButtonDown += () => OnTabPressed(2);
      container.GetNode<Button>("Additional/TabsContainer/SaveButton").ButtonDown += () => OnTabPressed(3);
      container.GetNode<Button>("Additional/TabsContainer/QuitButton").ButtonDown += () => OnTabPressed(4);

      tabs[0] = GetNode<Button>("Additional/TabsContainer/PartyButton");
      tabs[1] = GetNode<Button>("Additional/TabsContainer/ItemButton");
      tabs[2] = GetNode<Button>("Additional/TabsContainer/SettingsButton");
      tabs[3] = GetNode<Button>("Additional/TabsContainer/SaveButton");
      tabs[4] = GetNode<Button>("Additional/TabsContainer/QuitButton");

      // Do the menu panels

      blackScreen = GetNode<ColorRect>("/root/BaseNode/UI/Overlay/BlackScreen");
   }

   void OnTabPressed(int ID)
   {
      if (ID < 3)
      {
         menuPanels[activeTabID].Visible = false;
         menuPanels[ID].Visible = true;

         tabs[activeTabID].Disabled = false;
         tabs[ID].Disabled = true;

         tabs[activeTabID].MouseFilter = Control.MouseFilterEnum.Stop;
         tabs[ID].MouseFilter = Control.MouseFilterEnum.Ignore;

         activeTabID = ID;
      }

      if (ID == 0) // Party menu
      {
         managers.PartyMenuManager.LoadPartyMenu();
      }
      else if (ID == 1) // Item menu
      {
         managers.ItemMenuManager.LoadItemMenu();
         managers.PartyMenuManager.isActive = false;
      }
      else if (ID == 2) // Settings menu
      {
         settingsMenuManager.LoadSettingsMenu();
      }
      else if (ID == 3) // Save
      {
         isSaving = true;
         LoadSaveSlots();
      }
      else // Quit
      {
         quitOptions.Visible = true;
         DisableTabs();
         isQuitting = true;
      }
   }

   void LoadSaveSlots()
   {
      VBoxContainer slots = GetNode<VBoxContainer>("/root/BaseNode/UI/Overlay/Slots");
      for(int i = 0; i < 5; i++)
      {
         Button slot = slots.GetNode<Button>("Slot" + (i + 1));
         managers.MainMenuManager.PopulateSlotInformation(slot, i);
         slot.Disabled = false;
         slot.MouseFilter = Control.MouseFilterEnum.Stop;
      }
      slots.Visible = true;
      GetNode<Panel>("/root/BaseNode/UI/Overlay/SlotsBackground").Visible = true;

      managers.PartyMenuManager.DisableMenu();
      DisableTabs();
   }

   public void OnConfirmSave()
   {
      managers.SaveManager.SaveGame(false, ActiveSlot);

      GetNode<Panel>("/root/BaseNode/UI/Overlay/OverwriteMessage").Visible = false;

      managers.MainMenuManager.PopulateSlotInformation(GetNode<Button>("/root/BaseNode/UI/Overlay/Slots/Slot" + (ActiveSlot + 1)), ActiveSlot);

      Popup popup = GD.Load<PackedScene>("res://Core/popup.tscn").Instantiate<Popup>();
      GetNode<CanvasLayer>("/root/BaseNode/UI/Overlay").AddChild(popup);
      popup.ReceiveInfo(2.5f, "Save successful!");

      for (int i = 0; i < 5; i++)
      {
         Button slot = GetNode<Button>("/root/BaseNode/UI/Overlay/Slots/Slot" + (i + 1));
         slot.Disabled = false;
         slot.MouseFilter = Control.MouseFilterEnum.Stop;
      }
   }

   void OnCancelSave()
   {
      for (int i = 0; i < 5; i++)
      {
         Button slot = GetNode<Button>("/root/BaseNode/UI/Overlay/Slots/Slot" + (i + 1));
         slot.Disabled = false;
         slot.MouseFilter = Control.MouseFilterEnum.Stop;
      }
      GetNode<Panel>("/root/BaseNode/UI/Overlay/OverwriteMessage").Visible = false;;
   }

   void OnSaveQuitButtonDown()
   {
      managers.SaveManager.SaveGame(false, managers.SaveManager.currentSaveIndex);
      managers.LevelManager.ResetGameState();
      menu.Visible = false;
      quitOptions.Visible = false;
   }

   void OnQuitToDesktopButtonDown()
   {
      managers.SaveManager.SaveGame(false, managers.SaveManager.currentSaveIndex);
      GetTree().Quit();
   }

   void OnQuitNoSaveButtonDown()
   {
      DisableTabs();
      quitConfirmation.Visible = true;
   }

   void OnConfirmQuitNoSaveButtonDown()
   {
      quitConfirmation.Visible = false;
      managers.LevelManager.ResetGameState();
      menu.Visible = false;
      EnableTabs();
      quitOptions.Visible = false;
   }

   void OnCancelButtonDown()
   {
      EnableTabs();
      quitConfirmation.Visible = false;
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
               closeSound.Play();
            }
            else if (managers.PartyMenuManager.isSwapping)
            {
               managers.PartyMenuManager.CancelSwap();
               closeSound.Play();
            }
            else if (managers.ItemMenuManager.isUsingItem)
            {
               managers.ItemMenuManager.CancelItemUsage();
               closeSound.Play();
            }
            else if (isSaving)
            {
               GetNode<VBoxContainer>("/root/BaseNode/UI/Overlay/Slots").Visible = false;
               GetNode<Panel>("/root/BaseNode/UI/Overlay/SlotsBackground").Visible = false;
               GetNode<Panel>("/root/BaseNode/UI/Overlay/OverwriteMessage").Visible = false;

               managers.PartyMenuManager.EnableMenu();
               EnableTabs();
               isSaving = false;
               closeSound.Play();
            }
            else if (isQuitting)
            {
               quitOptions.Visible = false;
               EnableTabs();
               quitConfirmation.Visible = false;
               isQuitting = false;
               closeSound.Play();
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
      openSound.Play();
      menu.Visible = true;
      controller.DisableMovement = true;
      controller.DisableCamera = true;
      controller.IsSprinting = false;
      controller.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Idle");
      EnableTabs();
      Input.MouseMode = Input.MouseModeEnum.Visible;
      managers.PartyMenuManager.LoadPartyMenu();
      tabs[0].Disabled = true;
      tabs[0].MouseFilter = Control.MouseFilterEnum.Ignore;

      for (int i = 1; i < menuPanels.Length; i++)
      {
         menuPanels[i].Visible = false;
      }

      if (baseNode.HasNode("WorldMap"))
      {
         baseNode.GetNode<CharacterController>("WorldMap/Player").DisableMovement = true;
      }

      GetNode<Label>("../MenuContainer/Additional/GoldHolder/Label").Text = "Gold: " + managers.PartyManager.Gold;
      GetNode<Label>("../MenuContainer/Additional/LocationHolder/Label").Text = managers.LevelManager.location;
      activeTabID = 0;
   }

   public void CloseMenu()
   {
      if (menu.Visible)
      {
         closeSound.Play();
      }

      menu.Visible = false;
      controller.DisableCamera = false;
      Input.MouseMode = Input.MouseModeEnum.Captured;

      if (baseNode.HasNode("WorldMap"))
      {
         baseNode.GetNode<CharacterController>("WorldMap/Player").DisableMovement = false;
      }
      else
      {
         controller.DisableMovement = false;
      }
   }

   public void DisableTabs()
   {
      for (int i = 0; i < tabs.Length; i++)
      {
         tabs[i].Disabled = true;
         tabs[i].MouseFilter = Control.MouseFilterEnum.Ignore;
      }
   }

   public void EnableTabs()
   {
      for (int i = 0; i < tabs.Length; i++)
      {
         tabs[i].Disabled = false;
         tabs[i].MouseFilter = Control.MouseFilterEnum.Stop;
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

   public void FadeFromBlack(Tween tween = null)
   {
      if (tween == null)
      {
         tween = CreateTween();
      }
      
      tween.TweenProperty(blackScreen, "color", new Color(0, 0, 0, 0), 0.5f);
   }

   public void FadeToBlack(Tween tween)
   {
      tween.TweenProperty(blackScreen, "color", new Color(0, 0, 0, 1f), 0.5f);
   }

   public void SetBlackScreenAlpha(float alpha)
   {
      blackScreen.Color = new Color(0, 0, 0, alpha);
   }
}
