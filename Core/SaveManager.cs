using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class SaveManager : Node
{
   [Export]
   private ManagerReferenceHolder managers;

   public double startingTime = 0;
   public double endingTime = 0;
   public double elapsedTime = 0;

   public int currentSaveIndex = -1;

   public ColorRect blackScreen;
   public Label loadingLabel;

   public Vector2 worldMapPosition;

   public override void _Ready()
   {
      blackScreen = GetNode<ColorRect>("/root/BaseNode/UI/Overlay/BlackScreen");
      loadingLabel = GetNode<Label>("/root/BaseNode/UI/Overlay/LoadingText");
   }

   public Godot.Collections.Dictionary<string, Variant> Save()
   {
      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "TimeSpent", elapsedTime },
         { "Location", managers.LevelManager.location },
         { "InternalLocation", managers.LevelManager.InternalLocation },
         { "Gold", managers.PartyManager.Gold },
         { "PlayerPosX", managers.Controller.Position.X },
         { "PlayerPosY", managers.Controller.Position.Y },
         { "PlayerPosZ", managers.Controller.Position.Z },
         { "PlayerPosXMap", worldMapPosition.X },
         { "PlayerPosYMap", worldMapPosition.Y }
      };
   }

   public void SaveGame(bool isNewGame, int index)
   {
      elapsedTime += Time.GetUnixTimeFromSystem() - startingTime;
      startingTime = Time.GetUnixTimeFromSystem();

      if (GetNode<Node3D>("/root/BaseNode").HasNode("WorldMap"))
      {
         Vector2 position = GetNode<CharacterBody2D>("/root/BaseNode/WorldMap/Player").GlobalPosition;
         worldMapPosition.X = position.X;
         worldMapPosition.Y = position.Y;
      }

      using var saveGame = FileAccess.Open("user://savegame" + index + ".save", FileAccess.ModeFlags.Write);
      string jsonString;

      if (isNewGame)
      {
         managers.PartyManager.InitializeNewParty();
         managers.Controller.Position = new Vector3(0.922f, 0.603f, -24.508f);
         managers.PartyManager.Items.Clear();
         managers.LevelManager.CreateLevel("Dathrel's Cabin", "dathrel_cabin", "SpawnPoint", false);
         managers.Controller.DisableMovement = false;
         managers.Controller.DisableCamera = false;
         managers.Controller.HideWeapon();
      }

      // Save location datas. You need to save these FIRST (before you do the regular save), because it's needed to create the level
      for (int i = 0; i < managers.LevelManager.LocationDatas.Count; i++)
      {
         if (managers.LevelManager.LocationDatas[i] == managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID])
         {
            managers.LevelManager.LocationDatas[i].timeSinceLastVisit = 0f;
         }
         else
         {
            managers.LevelManager.LocationDatas[i].timeSinceLastVisit += Time.GetUnixTimeFromSystem() -  managers.LevelManager.LocationDatas[i].timeOfLastVisit;
         }

         var nodeData = managers.LevelManager.LocationDatas[i].Call("SaveLocationData");

         jsonString = Json.Stringify(nodeData);

         saveGame.StoreLine(jsonString);
      }

      jsonString = Json.Stringify(Save());

      saveGame.StoreLine(jsonString);

      // Save party members
      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         managers.PartyManager.Party[i].index = i;
         var nodeData = managers.PartyManager.Party[i].Call("PartyMemberInformation");

         // Json provides a static method to serialized JSON string.
         jsonString = Json.Stringify(nodeData);

         // Store the save dictionary as a new line in the save file.
         saveGame.StoreLine(jsonString);
      }

      // Save items
      for (int i = 0; i < managers.PartyManager.Items.Count; i++)
      {
         var nodeData = managers.PartyManager.Items[i].Call("SaveItem");

         // Json provides a static method to serialized JSON string.
         jsonString = Json.Stringify(nodeData);

         // Store the save dictionary as a new line in the save file.
         saveGame.StoreLine(jsonString);
      }

      var levelData = managers.LevelManager.Call("SaveLevelData");
      jsonString = Json.Stringify(levelData);
      saveGame.StoreLine(jsonString);
   }

   public void LoadGame(int index)
   {
      if (!FileAccess.FileExists("user://savegame" + index + ".save"))
      {
         return; // Error! We don't have a save to load.
      }

      Input.MouseMode = Input.MouseModeEnum.Captured;

      GetNode<ColorRect>("/root/BaseNode/UI/Overlay/BlackScreen").Color = new Color(0, 0, 0, 1);
      GetNode<Label>("/root/BaseNode/UI/Overlay/LoadingText").Visible = true;

      using var saveGame = FileAccess.Open("user://savegame" + index + ".save", FileAccess.ModeFlags.Read);

      managers.PartyManager.Party.Clear();
      managers.PartyManager.Items.Clear();
      managers.LevelManager.LocationDatas.Clear();
      managers.Controller.HideWeapon();

      startingTime = Time.GetUnixTimeFromSystem();

      while (saveGame.GetPosition() < saveGame.GetLength())
      {
         var jsonString = saveGame.GetLine();

         // Creates the helper class to interact with JSON
         var json = new Json();
         var parseResult = json.Parse(jsonString);

         if (parseResult != Error.Ok)
         {
            GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
            continue;
         }

         // Get the data from the JSON object
         var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);

         if (nodeData.ContainsKey("PlayerPosX")) // Base data
         {
            if ((string)nodeData["Location"] == "World Map")
            {
               managers.LevelManager.OpenWorldMap((string)nodeData["InternalLocation"], 
                                                  new Vector2((float)nodeData["PlayerPosXMap"], (float)nodeData["PlayerPosYMap"]), true);
            }
            else
            {
               managers.LevelManager.CreateLevel((string)nodeData["Location"], (string)nodeData["InternalLocation"], "SpawnPoint",
                                        true, managers.LevelManager.LocationDatas[managers.LevelManager.GetLocationDataID((string)nodeData["InternalLocation"])]);
               managers.Controller.Position = new Vector3((float)nodeData["PlayerPosX"], (float)nodeData["PlayerPosY"], (float)nodeData["PlayerPosZ"]);
               managers.Controller.DisableMovement = false;
               managers.Controller.DisableCamera = false;        
            }
            
            elapsedTime = (float)nodeData["TimeSpent"];
            managers.PartyManager.Gold = (int)nodeData["Gold"];
         }
         else if (nodeData.ContainsKey("CharacterType"))
         {
            managers.PartyManager.LoadPartyMember((int)nodeData["CharacterType"], (int)nodeData["CurrentHealth"], (int)nodeData["CurrentMana"],
                                         (string[])nodeData["Equipment"], (int)nodeData["Experience"],
                                         (bool)nodeData["IsInParty"], (int)nodeData["Level"], (int)nodeData["PartyIndex"]);
         }
         else if (nodeData.ContainsKey("ItemName"))
         {
            managers.PartyManager.LoadItem((string)nodeData["ItemName"], (int)nodeData["Quantity"]);
         }
         else if (nodeData.ContainsKey("LocationName")) // Location data
         {
            managers.LevelManager.LocationDatas.Add(new LocationData((string)nodeData["LocationName"], (int)nodeData["LevelProgress"],
                                    (Godot.Collections.Dictionary<string, bool>)nodeData["DefeatedEnemies"],
                                    (Godot.Collections.Dictionary<string, bool>)nodeData["PickedUpItems"],
                                    (Godot.Collections.Dictionary<string, bool>)nodeData["CutscenesSeen"], (double)nodeData["TimeSinceLastVisit"]));
         }
      }

      GetNode<ColorRect>("/root/BaseNode/UI/Overlay/BlackScreen").Color = new Color(0, 0, 0, 0);
      GetNode<Label>("/root/BaseNode/UI/Overlay/LoadingText").Visible = false;
   }

   public void ResetGameState()
   {
      Node3D baseNode = GetNode<Node3D>("/root/BaseNode");

      for (int i = 1; i < 4; i++)
      {
         baseNode.GetNode<OverworldPartyController>("PartyMembers/Member" + (i + 1)).IsActive = false;
         foreach (Node child in baseNode.GetNode<CharacterBody3D>("PartyMembers/Member" + (i + 1)).GetChildren())
         {
            child.QueueFree();
         }
      }

      managers.PartyManager.Party.Clear();
      managers.PartyManager.Items.Clear();

      baseNode.GetNode<MainMenuManager>("MainMenu").CheckLoadGameButtonAvailability();
      baseNode.GetNode<MainMenuManager>("MainMenu").CheckNewGameButtonAvailability();
      baseNode.GetNode<MainMenuManager>("MainMenu").Visible = true;

      Panel mainMenuBack = baseNode.GetNode<Panel>("MainMenu/Background");

      baseNode.GetNode<Camera2D>("MainMenu/MenuCamera").MakeCurrent();

      if (baseNode.HasNode("WorldMap"))
      {
         Node2D worldMap = baseNode.GetNode<Node2D>("WorldMap");
         baseNode.RemoveChild(worldMap);
         worldMap.QueueFree();
      }

      for (int i = 0; i < mainMenuBack.GetChildCount(); i++)
      {
         if (mainMenuBack.GetChild(i).Name == "Main")
         {
            mainMenuBack.GetChild<CanvasGroup>(i).Visible = true;
         }
         else
         {
            mainMenuBack.GetChild<CanvasGroup>(i).Visible = false;
         }
      }
   }
   
   void OnSaveButtonDown()
   {
      SaveGame(false, currentSaveIndex);
   }

   public async void FadeFromBlack()
   {
      blackScreen.Color = new Color(0, 0, 0, 1);

      while (blackScreen.Color.A > 0)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         float alpha = blackScreen.Color.A;
         alpha -= 0.05f;
         blackScreen.Color = new Color(0, 0, 0, alpha);
      }
   }

   public async void FadeToBlack()
   {
      blackScreen.Color = new Color(0, 0, 0, 0);

      while (blackScreen.Color.A < 1)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         float alpha = blackScreen.Color.A;
         alpha += 0.05f;
         blackScreen.Color = new Color(0, 0, 0, alpha);
      }
   }
}
