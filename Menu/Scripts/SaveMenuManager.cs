using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class SaveMenuManager : Node
{
   private CharacterController controller;
   private PartyManager partyManager;
   private LevelManager levelManager;
   private CombatManager combatManager;

   public double startingTime = 0;
   public double endingTime = 0;
   public double elapsedTime = 0;

   public int currentSaveIndex = -1;

   public ColorRect blackScreen;
   public Label loadingLabel;

   public Vector2 worldMapPosition;

   public override void _Ready()
   {
      controller = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");
      partyManager = GetNode<PartyManager>("/root/BaseNode/PartyManagerObj");
      levelManager = GetNode<LevelManager>("/root/BaseNode/LevelManager");
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");

      blackScreen = GetNode<ColorRect>("/root/BaseNode/UI/Overlay/BlackScreen");
      loadingLabel = GetNode<Label>("/root/BaseNode/UI/Overlay/LoadingText");
   }

   public Godot.Collections.Dictionary<string, Variant> Save()
   {
      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "TimeSpent", elapsedTime },
         { "Location", levelManager.location },
         { "InternalLocation", levelManager.InternalLocation },
         { "Gold", partyManager.Gold },
         { "PlayerPosX", controller.Position.X },
         { "PlayerPosY", controller.Position.Y },
         { "PlayerPosZ", controller.Position.Z },
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
         partyManager.InitializeNewParty();
         controller.Position = new Vector3(0.922f, 0.603f, -24.508f);
         partyManager.Items.Clear();
         partyManager.Items.Add(new InventoryItem(GD.Load<ItemResource>("res://Combat/Items/Resources/small_healing_potion.tres"), 3));
         partyManager.Items.Add(new InventoryItem(GD.Load<ItemResource>("res://Combat/Items/Resources/small_mana_potion.tres"), 3));
         levelManager.CreateLevel("Dathrel's Cabin", "dathrel_cabin", false);
         controller.DisableMovement = false;
         controller.HideWeapon();
      }

      // Save location datas. You need to save these FIRST (before you do the regular save), because it's needed to create the level
      for (int i = 0; i < levelManager.LocationDatas.Count; i++)
      {
         if (levelManager.LocationDatas[i] == levelManager.LocationDatas[levelManager.ActiveLocationDataID])
         {
            levelManager.LocationDatas[i].timeSinceLastVisit = 0f;
         }
         else
         {
            levelManager.LocationDatas[i].timeSinceLastVisit += Time.GetUnixTimeFromSystem() -  levelManager.LocationDatas[i].timeOfLastVisit;
         }

         // Save each shop in the location. This must be done FIRST, so the shops can load properly
         /*for (int j = 0; j < levelManager.LocationDatas[i].shops.Count; j++)
         {
            var shopNodeData = levelManager.LocationDatas[i].shops[j].Call("SaveShopData");

            jsonString = Json.Stringify(shopNodeData);

            saveGame.StoreLine(jsonString);
         }*/

         var nodeData = levelManager.LocationDatas[i].Call("SaveLocationData");

         jsonString = Json.Stringify(nodeData);

         saveGame.StoreLine(jsonString);
      }

      jsonString = Json.Stringify(Save());

      saveGame.StoreLine(jsonString);

      // Save party members
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         partyManager.Party[i].index = i;
         var nodeData = partyManager.Party[i].Call("PartyMemberInformation");

         // Json provides a static method to serialized JSON string.
         jsonString = Json.Stringify(nodeData);

         // Store the save dictionary as a new line in the save file.
         saveGame.StoreLine(jsonString);
      }

      // Save items
      for (int i = 0; i < partyManager.Items.Count; i++)
      {
         var nodeData = partyManager.Items[i].Call("SaveItem");

         // Json provides a static method to serialized JSON string.
         jsonString = Json.Stringify(nodeData);

         // Store the save dictionary as a new line in the save file.
         saveGame.StoreLine(jsonString);
      }

      var levelData = levelManager.Call("SaveLevelData");
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

      partyManager.Party.Clear();
      partyManager.Items.Clear();
      levelManager.LocationDatas.Clear();
      controller.HideWeapon();

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
               levelManager.OpenWorldMap((string)nodeData["InternalLocation"], new Vector2((float)nodeData["PlayerPosXMap"], (float)nodeData["PlayerPosYMap"]), true);
            }
            else
            {
               levelManager.CreateLevel((string)nodeData["Location"], (string)nodeData["InternalLocation"], 
                                        true, levelManager.LocationDatas[levelManager.GetLocationDataID((string)nodeData["InternalLocation"])]);
               controller.Position = new Vector3((float)nodeData["PlayerPosX"], (float)nodeData["PlayerPosY"], (float)nodeData["PlayerPosZ"]);
               controller.DisableMovement = false;
            }
            
            elapsedTime = (float)nodeData["TimeSpent"];
            partyManager.Gold = (int)nodeData["Gold"];
         }
         else if (nodeData.ContainsKey("CharacterType"))
         {
            partyManager.LoadPartyMember((int)nodeData["CharacterType"], (int)nodeData["CurrentHealth"], (int)nodeData["CurrentMana"],
                                         (string[])nodeData["Equipment"], (int)nodeData["Experience"],
                                         (bool)nodeData["IsInParty"], (int)nodeData["Level"], (int)nodeData["PartyIndex"]);
         }
         else if (nodeData.ContainsKey("ItemName"))
         {
            partyManager.LoadItem((string)nodeData["ItemName"], (int)nodeData["Quantity"]);
         }
         else if (nodeData.ContainsKey("LocationName")) // Location data
         {
            /*int locationDataID = levelManager.GetLocationDataID((string)nodeData["LocationName"]);

            if (locationDataID == -1) // No location data, make a new one
            {*/
               levelManager.LocationDatas.Add(new LocationData((string)nodeData["LocationName"], (Godot.Collections.Dictionary<string, bool>)nodeData["DefeatedEnemies"],
                                     (Godot.Collections.Dictionary<string, bool>)nodeData["PickedUpItems"],
                                     (Godot.Collections.Dictionary<string, bool>)nodeData["CutscenesSeen"], (double)nodeData["TimeSinceLastVisit"]));
            /*}
            else // A shop has already added this location data, so we just need to fill it in
            {
               //levelManager.LocationDatas[locationDataID].locationName = (string)nodeData["LocationName"];
               levelManager.LocationDatas[locationDataID].defeatedEnemies = (Godot.Collections.Dictionary<string, bool>)nodeData["DefeatedEnemies"];
               levelManager.LocationDatas[locationDataID].pickedUpItems = (Godot.Collections.Dictionary<string, bool>)nodeData["PickedUpItems"];
               levelManager.LocationDatas[locationDataID].cutscenesSeen = (Godot.Collections.Dictionary<string, bool>)nodeData["CutscenesSeen"];
               levelManager.LocationDatas[locationDataID].timeSinceLastVisit = (double)nodeData["TimeSinceLastVisit"];
            }*/
         }
         /*else if (nodeData.ContainsKey("ShopLocation")) // Shop data
         {
            int locationDataID = levelManager.GetLocationDataID((string)nodeData["ShopLocation"]);

            if (locationDataID == -1)
            {
               levelManager.LocationDatas.Add(new LocationData());
               locationDataID = levelManager.LocationDatas.Count - 1;
               levelManager.LocationDatas[locationDataID].locationName = (string)nodeData["ShopLocation"];
            }

            levelManager.LocationDatas[locationDataID].shops.Add(new ShopData((string)nodeData["ShopLocation"], (int)nodeData["ID"], 
                                                                (Godot.Collections.Dictionary<string, string>)nodeData["SelectionNames"],
                                                                (Godot.Collections.Dictionary<string, int>)nodeData["SelectionQuantities"], (int)nodeData["RestockTick"]));
         }*/

        // Firstly, we need to create the object and add it to the tree and set its position.
        //var newObjectScene = GD.Load<PackedScene>(nodeData["Filename"].ToString());
        //var newObject = newObjectScene.Instantiate<Node>();
        //GetNode(nodeData["Parent"].ToString()).AddChild(newObject);
        //newObject.Set(Node3D.PropertyName.Position, new Vector3((float)nodeData["PosX"], (float)nodeData["PosY"], (float)nodeData["PosZ"]));

        //controller.Position = new Vector3((float)nodeData["PlayerPosX"], (float)nodeData["PlayerPosY"], (float)nodeData["PlayerPosZ"]);

        // Now we set the remaining variables.
        /*foreach (var (key, value) in nodeData)
        {
            if (key == "Filename" || key == "Parent" || key == "PosX" || key == "PosY" || key == "PosZ")
            {
                continue;
            }
            newObject.Set(key, value);
        }*/
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

      partyManager.Party.Clear();
      partyManager.Items.Clear();

      baseNode.GetNode<MainMenuFunctionality>("MainMenu").CheckLoadGameButtonAvailability();
      baseNode.GetNode<MainMenuFunctionality>("MainMenu").CheckNewGameButtonAvailability();
      baseNode.GetNode<MainMenuFunctionality>("MainMenu").Visible = true;

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


   /*public void SaveGame()
   {
      using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);

      var saveNodes = GetTree().GetNodesInGroup("Persist");
      foreach (Node saveNode in saveNodes)
      {
         // Check the node is an instanced scene so it can be instanced again during load.
         if (string.IsNullOrEmpty(saveNode.SceneFilePath))
         {
               GD.Print($"persistent node '{saveNode.Name}' is not an instanced scene, skipped");
               continue;
         }

         // Check the node has a save function.
         if (!saveNode.HasMethod("Save"))
         {
               GD.Print($"persistent node '{saveNode.Name}' is missing a Save() function, skipped");
               continue;
         }

         // Call the node's save function.
         var nodeData = saveNode.Call("Save");

         // Json provides a static method to serialized JSON string.
         var jsonString = Json.Stringify(nodeData);

         // Store the save dictionary as a new line in the save file.
         saveGame.StoreLine(jsonString);
      }
   }

   public void LoadGame()
{
    if (!FileAccess.FileExists("user://savegame.save"))
    {
        return; // Error! We don't have a save to load.
    }

    // We need to revert the game state so we're not cloning objects during loading.
    // This will vary wildly depending on the needs of a project, so take care with
    // this step.
    // For our example, we will accomplish this by deleting saveable objects.
    var saveNodes = GetTree().GetNodesInGroup("Persist");
    foreach (Node saveNode in saveNodes)
    {
        saveNode.QueueFree();
    }

    // Load the file line by line and process that dictionary to restore the object
    // it represents.
    using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);

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

        // Firstly, we need to create the object and add it to the tree and set its position.
        var newObjectScene = GD.Load<PackedScene>(nodeData["Filename"].ToString());
        var newObject = newObjectScene.Instantiate<Node>();
        GetNode(nodeData["Parent"].ToString()).AddChild(newObject);
        newObject.Set(Node3D.PropertyName.Position, new Vector3((float)nodeData["PosX"], (float)nodeData["PosY"], (float)nodeData["PosZ"]));

        // Now we set the remaining variables.
        foreach (var (key, value) in nodeData)
        {
            if (key == "Filename" || key == "Parent" || key == "PosX" || key == "PosY" || key == "PosZ")
            {
                continue;
            }
            newObject.Set(key, value);
        }
    }
}*/
}
