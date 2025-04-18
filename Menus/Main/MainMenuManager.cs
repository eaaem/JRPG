using Godot;
using System;

public partial class MainMenuManager : CanvasLayer
{
   [Export]
   private ManagerReferenceHolder managers;
   
	private Control mainScreen;
   private Control settingsScreen;
   private Control loadGameSlots;
   private VBoxContainer slots;
   private Control deleteConfirmationWindow;

   private Control partyMenu;
   private Panel partyContainer;

   private Button deleteButton;

   bool isDeletingSaves;
   int currentDeletingSlotIndex = -1;

   public override void _Ready()
   {
      mainScreen = GetNode<Control>("Background/Main");
      settingsScreen = GetNode<Control>("Background/Settings");
      loadGameSlots = GetNode<Control>("Background/LoadGameSlots");
      slots = GetNode<VBoxContainer>("/root/BaseNode/UI/Overlay/Slots");
      deleteButton = loadGameSlots.GetNode<Button>("DeleteButton");
      deleteConfirmationWindow = GetNode<Control>("/root/BaseNode/UI/Overlay/ConfirmationWindow");

      partyMenu = GetNode<Control>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu");
      partyContainer = partyMenu.GetNode<Panel>("MenuContainer");

      managers.MenuManager.canTakeInput = false;
      managers.Controller.DisableGravity = true;

      slots.GetNode<Button>("Slot1").ButtonDown += () => OnSlotDown(0);
      slots.GetNode<Button>("Slot2").ButtonDown += () => OnSlotDown(1);
      slots.GetNode<Button>("Slot3").ButtonDown += () => OnSlotDown(2);
      slots.GetNode<Button>("Slot4").ButtonDown += () => OnSlotDown(3);
      slots.GetNode<Button>("Slot5").ButtonDown += () => OnSlotDown(4);

      loadGameSlots.GetNode<Button>("Back").ButtonDown += () => OnBackButtonDown("/root/BaseNode/MainMenu/Background/LoadGameSlots",
                                                                                 "/root/BaseNode/MainMenu/Background/Main");
      settingsScreen.GetNode<Button>("Back").ButtonDown += () => OnBackButtonDown("/root/BaseNode/MainMenu/Background/Settings",
                                                                                 "/root/BaseNode/MainMenu/Background/Main");                                                                          

      CreateMainMenuLevel();
      CheckLoadGameButtonAvailability();
      CheckNewGameButtonAvailability();
   }

   public void CheckLoadGameButtonAvailability()
   {
      for (int i = 0; i < 5; i++)
      {
         if (FileAccess.FileExists("user://savegame" + i + ".save"))
         {
            mainScreen.GetNode<Button>("LoadGame").Disabled = false;
            mainScreen.GetNode<Button>("LoadGame").MouseFilter = Control.MouseFilterEnum.Stop;
            mainScreen.GetNode<Button>("Continue").Disabled = false;
            mainScreen.GetNode<Button>("Continue").MouseFilter = Control.MouseFilterEnum.Stop;

            return;
         }
      }

      mainScreen.GetNode<Button>("LoadGame").Disabled = true;
      mainScreen.GetNode<Button>("LoadGame").MouseFilter = Control.MouseFilterEnum.Ignore;
      mainScreen.GetNode<Button>("Continue").Disabled = true;
      mainScreen.GetNode<Button>("Continue").MouseFilter = Control.MouseFilterEnum.Ignore;
   }

   public void CheckNewGameButtonAvailability()
   {
      int fillCounter = 0;
      for (int i = 0; i < 5; i++)
      {
         if (FileAccess.FileExists("user://savegame" + i + ".save"))
         {
            fillCounter++;
         }
      }

      if (fillCounter >= 5)
      {
         mainScreen.GetNode<Button>("NewGame").Disabled = true;
         mainScreen.GetNode<Button>("NewGame").MouseFilter = Control.MouseFilterEnum.Ignore;
      }
      else
      {
         mainScreen.GetNode<Button>("NewGame").Disabled = false;
         mainScreen.GetNode<Button>("NewGame").MouseFilter = Control.MouseFilterEnum.Stop;
      }
   }

   async void OnNewGameButtonDown()
   {
      for (int i = 0; i < 5; i++)
      {
         if (!FileAccess.FileExists("user://savegame" + i + ".save"))
         {
            managers.SaveManager.elapsedTime = 0;

            managers.SaveManager.startingTime = Time.GetUnixTimeFromSystem();
            managers.SaveManager.currentSaveIndex = i;

            Tween tween = CreateTween();
            managers.MenuManager.FadeToBlack(tween);
            await ToSignal(tween, Tween.SignalName.Finished);

            managers.SaveManager.SaveGame(true, i);

            Visible = false;
            DestroyMainMenuLevel();

            managers.MenuManager.canTakeInput = true;
            managers.Controller.DisableGravity = false;

            return;
         }
      }
   }

   void OnSettingsButtonDown()
   {
      mainScreen.Visible = false;
      settingsScreen.Visible = true;

      partyMenu.Visible = true;

      partyContainer.GetNode<Control>("Additional").Visible = false;

      foreach (Control child in partyContainer.GetChildren())
      {
         child.Visible = false;
      }

      partyContainer.GetNode<Panel>("Settings").Visible = true;
   }

   void OnLoadButtonDown()
   {
      mainScreen.Visible = false;

      for (int i = 0; i < 5; i++)
      {
         Button slotButton = slots.GetNode<Button>("Slot" + (i + 1));
         PopulateSlotInformation(slotButton, i);
      }

      deleteButton.Text = "Delete";
      loadGameSlots.Visible = true;
      slots.Visible = true;
   }

   public void PopulateSlotInformation(Button slot, int index)
   {
      slot.GetNode<Panel>("RedOverlay").Visible = false;
      if (FileAccess.FileExists("user://savegame" + index + ".save"))
      {
         using var saveGame = FileAccess.Open("user://savegame" + index + ".save", FileAccess.ModeFlags.Read);
         slot.Text = "Slot " + (index + 1);

         while (saveGame.GetPosition() < saveGame.GetLength())
         {
            string jsonString = saveGame.GetLine();
            Json json = new Json();
            Error parseResult = json.Parse(jsonString);
            Godot.Collections.Dictionary<string, Variant> nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);

            // Time
            if (nodeData.ContainsKey("TimeSpent"))
            {
               Label timeLabel = slot.GetChild<Label>(0);
               timeLabel.Text = string.Empty;
               int minutes = ((int)nodeData["TimeSpent"] / 60) % 60;
               int hours = (int)nodeData["TimeSpent"] / 3600;

               if (hours < 10)
               {
                  timeLabel.Text += "0";
               }

               timeLabel.Text += hours + ":";

               if (minutes < 10)
               {
                  timeLabel.Text += "0";
               }

               timeLabel.Text += minutes;

               slot.GetChild<Label>(1).Text = (string)nodeData["Location"];
            }
         }

         slot.GetChild<Label>(0).Visible = true;
         slot.GetChild<Label>(1).Visible = true;
         slot.Disabled = false;
         slot.MouseFilter = Control.MouseFilterEnum.Stop;
      }
      else
      {
         slot.GetChild<Label>(0).Visible = false;
         slot.GetChild<Label>(1).Visible = false;
         slot.Text = "Empty";
         slot.Disabled = true;
         slot.MouseFilter = Control.MouseFilterEnum.Ignore;
      }
   }

   void OnBackButtonDown(string originPath, string targetPath)
   {
      GetNode<Control>(originPath).Visible = false;

      Control targetCanvas = GetNode<Control>(targetPath);
      targetCanvas.Visible = true;

      isDeletingSaves = false;

      if (targetCanvas == mainScreen)
      {
         CheckLoadGameButtonAvailability();
         CheckNewGameButtonAvailability();

         partyMenu.Visible = false;

         partyContainer.GetNode<Control>("Additional").Visible = true;
         slots.Visible = false;
      }
      else if (targetCanvas == loadGameSlots)
      {
         ReenableLoadSlotsMenu();
      }
   }

   void OnSlotDown(int index)
   {
      if (isDeletingSaves)
      {
         deleteConfirmationWindow.GetNode<RichTextLabel>("Back/Title").Text = "[center]Are you sure you want to delete the save in Slot " + (index + 1) + "?\n"
                                                                              + "[color=red]This action is irreversible.";
         deleteConfirmationWindow.Visible = true;
         currentDeletingSlotIndex = index;

         for (int i = 0; i < 5; i++)
         {
            slots.GetNode<Button>("Slot" + (i + 1)).Disabled = true;
         }
         loadGameSlots.GetNode<Button>("Back").Disabled = true;
         deleteButton.Disabled = true;
      }
      else
      {
         if (Visible)
         {
            Load(index);
         }
         else // Saving
         {
            Button slotButton = slots.GetNode<Button>("Slot" + (index + 1));
            managers.MenuManager.ActiveSlot = index;

            if (slotButton.Text == "Empty")
            {
               managers.MenuManager.OnConfirmSave();
            }
            else
            {        
               foreach (Button button in slots.GetChildren())
               {
                  button.Disabled = true;
               }

               Panel message = GetNode<Panel>("/root/BaseNode/UI/Overlay/OverwriteMessage");
               message.GetNode<RichTextLabel>("Text").Text = "[center]Are you sure you want to overwrite the save in Slot " + (index + 1) + "?\n"
                                                             + "[color=red]This action cannot be reversed.[/color]";
               message.Visible = true;
            }
         }
      }
   }

   void OnContinueButtonDown()
   {
      Load(GetMostRecentSave());
   }

   int GetMostRecentSave()
   {
      int index = -1;
      double latestTime = 0;

      for (int i = 0; i < 5; i++)
      {
         Button slotButton = slots.GetNode<Button>("Slot" + (i + 1));
         if (FileAccess.FileExists("user://savegame" + i + ".save"))
         {
            using var saveGame = FileAccess.Open("user://savegame" + i + ".save", FileAccess.ModeFlags.Read);

            while (saveGame.GetPosition() < saveGame.GetLength())
            {
               string jsonString = saveGame.GetLine();
               Json json = new Json();
               Error parseResult = json.Parse(jsonString);
               Godot.Collections.Dictionary<string, Variant> nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);

               // Time
               if (nodeData.ContainsKey("LastPlayed"))
               {
                  if ((double)nodeData["LastPlayed"] > latestTime)
                  {
                     latestTime = (double)nodeData["LastPlayed"];
                     index = i;
                  }
               }
            }
         }
      }

      return index;
   }

   async void Load(int index)
   {
      Tween tween = CreateTween();
      managers.MenuManager.FadeToBlack(tween);
      await ToSignal(tween, Tween.SignalName.Finished);

      managers.SaveManager.currentSaveIndex = index;
      Visible = false;
      GetNode<VBoxContainer>("/root/BaseNode/UI/Overlay/Slots").Visible = false;
      DestroyMainMenuLevel();

      managers.SaveManager.LoadGame(index);
      managers.MenuManager.canTakeInput = true;
      managers.Controller.DisableGravity = false;
   }

   void OnDeleteButtonDown()
   {
      if (!isDeletingSaves)
      {
         isDeletingSaves = true;
         deleteButton.Text = "Cancel";

         foreach (Button button in slots.GetChildren())
         {
            if (button.Text != "Empty")
            {
               button.GetNode<Panel>("RedOverlay").Visible = true;
            }
         }
      }
      else
      {
         isDeletingSaves = false;
         deleteButton.Text = "Delete";

         foreach (Button button in slots.GetChildren())
         {
            button.GetNode<Panel>("RedOverlay").Visible = false;
         }
      }
   }

   void OnConfirmDelete()
   {
      Button correspondingSlot = slots.GetNode<Button>("Slot" + (currentDeletingSlotIndex + 1));
      correspondingSlot.Disabled = true;

      DirAccess.RemoveAbsolute("user://savegame" + currentDeletingSlotIndex + ".save");
      correspondingSlot.GetChild<Label>(0).Visible = false;
      correspondingSlot.GetChild<Label>(1).Visible = false;
      correspondingSlot.Text = "Empty";
      isDeletingSaves = false;
      deleteButton.Text = "Delete";
      deleteConfirmationWindow.Visible = false;

      ReenableLoadSlotsMenu();
   }

   void ReenableLoadSlotsMenu()
   {
      for (int i = 0; i < 5; i++)
      {
         Button slot = slots.GetNode<Button>("Slot" + (i + 1));
         slot.GetNode<Panel>("RedOverlay").Visible = false;

         if (slot.Text != "Empty")
         {
            slot.Disabled = false;
            slot.MouseFilter = Control.MouseFilterEnum.Stop;
         }
         else
         {
            slot.MouseFilter = Control.MouseFilterEnum.Ignore;
         }
      }

      loadGameSlots.GetNode<Button>("Back").Disabled = false;
      deleteButton.Disabled = false;
      deleteButton.MouseFilter = Control.MouseFilterEnum.Stop;
      deleteButton.Text = "Delete";
   }

   public void CreateMainMenuLevel()
   {
      int index = GetMostRecentSave();

      PackedScene packedScene;

      if (index == -1)
      {
         // No save exists, default to Theralin
         packedScene = GD.Load<PackedScene>("res://Menus/MainMenuLevels/theralin.tscn");
      }
      else
      {
         string location = string.Empty;
         using var saveGame = FileAccess.Open("user://savegame" + index + ".save", FileAccess.ModeFlags.Read);

         while (saveGame.GetPosition() < saveGame.GetLength())
         {
            string jsonString = saveGame.GetLine();
            Json json = new Json();
            Error parseResult = json.Parse(jsonString);
            Godot.Collections.Dictionary<string, Variant> nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);

            if (nodeData.ContainsKey("MainMenuScreen"))
            {
               location = (string)nodeData["MainMenuScreen"];
               break;
            }
         }

         packedScene = GD.Load<PackedScene>("res://Menus/MainMenuLevels/" + location + ".tscn");
      }

      Node3D level = packedScene.Instantiate<Node3D>();
      level.Name = "MainMenuLevel";
      AddChild(level);
   }

   public void DestroyMainMenuLevel()
   {
      Node3D level = GetNode<Node3D>("MainMenuLevel");
      RemoveChild(level);
      level.QueueFree();
   }

   void OnQuitButtonDown()
   {
      GetTree().Quit();
   }
}
