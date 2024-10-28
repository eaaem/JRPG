using Godot;
using System;

public partial class MainMenuManager : CanvasLayer
{
   [Export]
   private ManagerReferenceHolder managers;
   
	private Control mainScreen;
   private Control settingsScreen;
   private Control loadGameSlots;
   private Control deleteConfirmationWindow;

   private CanvasGroup partyMenu;
   private TabContainer partyTabContainer;

   private Button deleteButton;

   bool isDeletingSaves;
   int currentDeletingSlotIndex = -1;

   public override void _Ready()
   {
      mainScreen = GetNode<Control>("Background/Main");
      settingsScreen = GetNode<Control>("Background/Settings");
      loadGameSlots = GetNode<Control>("Background/LoadGameSlots");
      deleteButton = loadGameSlots.GetNode<Button>("DeleteButton");
      deleteConfirmationWindow = loadGameSlots.GetNode<Control>("ConfirmationWindow");

      partyMenu = GetNode<CanvasGroup>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu");
      partyTabContainer = partyMenu.GetNode<TabContainer>("TabContainer");

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
            mainScreen.GetNode<Button>("Continue").Disabled = false;
            return;
         }
      }

      mainScreen.GetNode<Button>("LoadGame").Disabled = true;
      mainScreen.GetNode<Button>("Continue").Disabled = true;
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
      }
      else
      {
         mainScreen.GetNode<Button>("NewGame").Disabled = false;
      }
   }

   void OnNewGameButtonDown()
   {
      for (int i = 0; i < 5; i++)
      {
         if (!FileAccess.FileExists("user://savegame" + i + ".save"))
         {
            managers.SaveManager.blackScreen.Color = new Color(0, 0, 0, 1);
            managers.SaveManager.loadingLabel.Visible = true;

            managers.LevelManager.location = "Athili Copse";
            managers.SaveManager.elapsedTime = 0;

            managers.SaveManager.startingTime = Time.GetUnixTimeFromSystem();
            managers.SaveManager.currentSaveIndex = i;

            managers.SaveManager.SaveGame(true, i);

            managers.SaveManager.blackScreen.Color = new Color(0, 0, 0, 0);
            managers.SaveManager.loadingLabel.Visible = false;

            Visible = false;
            DestroyMainMenuLevel();

            return;
         }
      }
   }

   void OnSettingsButtonDown()
   {
      mainScreen.Visible = false;
      settingsScreen.Visible = true;

      partyMenu.Visible = true;

      partyTabContainer.CurrentTab = 2;
      partyTabContainer.TabsVisible = false;
   }

   void OnLoadButtonDown()
   {
      mainScreen.Visible = false;

      for (int i = 0; i < 5; i++)
      {
         Button slotButton = loadGameSlots.GetNode<Button>("VBoxContainer/Slot" + (i + 1));
         if (FileAccess.FileExists("user://savegame" + i + ".save"))
         {
            using var saveGame = FileAccess.Open("user://savegame" + i + ".save", FileAccess.ModeFlags.Read);
            slotButton.Text = "Slot " + (i + 1);

            while (saveGame.GetPosition() < saveGame.GetLength())
            {
               string jsonString = saveGame.GetLine();
               Json json = new Json();
               Error parseResult = json.Parse(jsonString);
               Godot.Collections.Dictionary<string, Variant> nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);

               // Time
               if (nodeData.ContainsKey("TimeSpent"))
               {
                  Label timeLabel = slotButton.GetChild<Label>(0);
                  timeLabel.Text = string.Empty;
                  int minutes = ((int)nodeData["TimeSpent"] / 60) % 60;
                  int hours = (int)nodeData["TimeSpent"] / 360;

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

                  slotButton.GetChild<Label>(1).Text = (string)nodeData["Location"];
               }
            }

            slotButton.GetChild<Label>(0).Visible = true;
            slotButton.GetChild<Label>(1).Visible = true;
            slotButton.Disabled = false;
         }
         else
         {
            slotButton.GetChild<Label>(0).Visible = false;
            slotButton.GetChild<Label>(1).Visible = false;
            slotButton.Text = "Empty";
            slotButton.Disabled = true;
         }
      }

      deleteButton.Text = "Delete";
      loadGameSlots.Visible = true;
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

         partyTabContainer.CurrentTab = 0;
         partyTabContainer.TabsVisible = true;
      }
      else if (targetCanvas == loadGameSlots)
      {
         ReenableLoadSlotsMenu();
      }
   }

   void OnLoadGameButtonDown(int index)
   {
      if (isDeletingSaves)
      {
         deleteConfirmationWindow.GetNode<Label>("Back/Title").Text = "Are you sure you want to delete the save in Slot " + (index + 1) + "?\nThis cannot be reversed.";
         deleteConfirmationWindow.Visible = true;
         currentDeletingSlotIndex = index;

         for (int i = 0; i < 5; i++)
         {
            loadGameSlots.GetNode<Button>("VBoxContainer/Slot" + (i + 1)).Disabled = true;
         }
         loadGameSlots.GetNode<Button>("Back").Disabled = true;
         deleteButton.Disabled = true;
      }
      else
      {
         Load(index);
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
         Button slotButton = loadGameSlots.GetNode<Button>("VBoxContainer/Slot" + (i + 1));
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

   void Load(int index)
   {
      managers.SaveManager.currentSaveIndex = index;
      managers.SaveManager.LoadGame(index);
      Visible = false;
      DestroyMainMenuLevel();
   }

   void OnDeleteButtonDown()
   {
      if (!isDeletingSaves)
      {
         isDeletingSaves = true;
         deleteButton.Text = "Cancel";
      }
      else
      {
         isDeletingSaves = false;
         deleteButton.Text = "Delete";
      }
   }

   void OnConfirmDelete()
   {
      Button correspondingSlot = loadGameSlots.GetNode<Button>("VBoxContainer/Slot" + (currentDeletingSlotIndex + 1));
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
         Button slot = loadGameSlots.GetNode<Button>("VBoxContainer/Slot" + (i + 1));

         if (slot.Text != "Empty")
         {
            slot.Disabled = false;
         }
      }

      loadGameSlots.GetNode<Button>("Back").Disabled = false;
      deleteButton.Disabled = false;
   }

   public void CreateMainMenuLevel()
   {
      int index = GetMostRecentSave();

      PackedScene packedScene;

      if (index == -1)
      {
         // No save exists, default to Theralin
         packedScene = GD.Load<PackedScene>("res://Menus/0Core/theralin.tscn");
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

         packedScene = GD.Load<PackedScene>("res://Menus/0Core/" + location + ".tscn");
      }

      Node3D level = packedScene.Instantiate<Node3D>();
      level.Name = "MainMenuLevel";
      //level.GetNode<Camera3D>("Camera").MakeCurrent();
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
