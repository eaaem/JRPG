using Godot;
using System;

public partial class MainMenuFunctionality : Control
{
	private CanvasGroup mainScreen;
   private CanvasGroup settingsScreen;
   private CanvasGroup loadGameSlots;
   private CanvasGroup deleteConfirmationWindow;

   private CanvasGroup partyMenu;
   private TabContainer partyTabContainer;

   private Button deleteButton;

   private SaveMenuManager saveManager;
   private LevelManager levelManager;

   bool isDeletingSaves;
   int currentDeletingSlotIndex = -1;

   public override void _Ready()
   {
      mainScreen = GetNode<CanvasGroup>("Background/Main");
      settingsScreen = GetNode<CanvasGroup>("Background/Settings");
      loadGameSlots = GetNode<CanvasGroup>("Background/LoadGameSlots");
      deleteButton = loadGameSlots.GetNode<Button>("DeleteButton");
      deleteConfirmationWindow = loadGameSlots.GetNode<CanvasGroup>("ConfirmationWindow");
      levelManager = GetNode<LevelManager>("/root/BaseNode/LevelManager");

      partyMenu = GetNode<CanvasGroup>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu");
      partyTabContainer = partyMenu.GetNode<TabContainer>("TabContainer");

      saveManager = GetNode<SaveMenuManager>("/root/BaseNode/SaveManager");

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
            return;
         }
      }

      mainScreen.GetNode<Button>("LoadGame").Disabled = true;
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
            saveManager.blackScreen.Color = new Color(0, 0, 0, 1);
            saveManager.loadingLabel.Visible = true;

            levelManager.location = "Athili Copse";
            saveManager.elapsedTime = 0;

            saveManager.startingTime = Time.GetUnixTimeFromSystem();
            saveManager.currentSaveIndex = i;

            saveManager.SaveGame(true, i);

            saveManager.blackScreen.Color = new Color(0, 0, 0, 0);
            saveManager.loadingLabel.Visible = false;

            Visible = false;

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
                  int minutes = (int)nodeData["TimeSpent"] / 60;

                  timeLabel.Text = (minutes / 60) + ":" + (minutes % 60);

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
      GetNode<CanvasGroup>(originPath).Visible = false;

      CanvasGroup targetCanvas = GetNode<CanvasGroup>(targetPath);
      GetNode<CanvasGroup>(targetPath).Visible = true;

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
         deleteConfirmationWindow.GetNode<Label>("Back/Title").Text = "Are you sure you want to delete Slot " + (index + 1) + "?";
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
         saveManager.currentSaveIndex = index;
         saveManager.LoadGame(index);
         Visible = false;
      }
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

   void OnQuitButtonDown()
   {
      GetTree().Quit();
   }
}
