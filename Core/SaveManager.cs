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

   public Label loadingLabel;

   public Vector3 WorldMapPosition { get; set; }

   [Signal]
   public delegate void SaveFinishedEventHandler();

   public override void _Ready()
   {
      loadingLabel = GetNode<Label>("/root/BaseNode/UI/Overlay/LoadingText");
   }

   public Godot.Collections.Dictionary<string, Variant> Save()
   {
      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "TimeSpent", elapsedTime },
         { "LastPlayed", startingTime },
         { "Location", managers.LevelManager.location },
         { "InternalLocation", managers.LevelManager.InternalLocation },
         { "MainMenuScreen", managers.LevelManager.MainMenuScreenName },
         { "Gold", managers.PartyManager.Gold },
         { "PlayerPosX", managers.Controller.Position.X },
         { "PlayerPosY", managers.Controller.Position.Y },
         { "PlayerPosZ", managers.Controller.Position.Z },
         { "PlayerControllerRotation", managers.Controller.Rotation.Y },
         { "PlayerModelRotation", managers.Controller.GetNode<Node3D>("Model").Rotation.Y },
         { "CameraRotationX", managers.Controller.GetNode<Node3D>("CameraTarget").Rotation.X },
         { "PlayerPosXMap", WorldMapPosition.X },
         { "PlayerPosYMap", WorldMapPosition.Y },
         { "PlayerPosZMap", WorldMapPosition.Z }
      };
   }

   public void SaveGame(bool isNewGame, int index)
   {
      elapsedTime += Time.GetUnixTimeFromSystem() - startingTime;
      startingTime = Time.GetUnixTimeFromSystem();

      if (GetNode<Node3D>("/root/BaseNode").HasNode("WorldMap"))
      {
         Vector3 position = GetNode<CharacterBody3D>("/root/BaseNode/WorldMap/Player").GlobalPosition;
         WorldMapPosition = position;
      }

      using var saveGame = FileAccess.Open("user://savegame" + index + ".save", FileAccess.ModeFlags.Write);
      string jsonString;

      if (isNewGame)
      {
         managers.PartyManager.InitializeNewParty();
         managers.PartyManager.Items.Clear();
         managers.LevelManager.CreateLevel("Dathrel's Cabin", "dathrel_cabin", "SpawnPoint", false);
         managers.LevelManager.MainMenuScreenName = "theralin";
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

         managers.LevelManager.EmitSignal(LevelManager.SignalName.SaveLevelProgression);

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

      EmitSignal(SignalName.SaveFinished);
   }

   public void LoadGame(int index)
   {
      if (!FileAccess.FileExists("user://savegame" + index + ".save"))
      {
         return; // Error! We don't have a save to load.
      }

      Input.MouseMode = Input.MouseModeEnum.Captured;

      using var saveGame = FileAccess.Open("user://savegame" + index + ".save", FileAccess.ModeFlags.Read);

      managers.PartyManager.Party.Clear();
      managers.PartyManager.Items.Clear();
      managers.LevelManager.LocationDatas.Clear();

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
                                                  new Vector3((float)nodeData["PlayerPosXMap"], (float)nodeData["PlayerPosYMap"], (float)nodeData["PlayerPosZMap"]), 
                                                  true);
            }
            else
            {
               managers.LevelManager.CreateLevel((string)nodeData["Location"], (string)nodeData["InternalLocation"], "SpawnPoint",
                                        true, managers.LevelManager.LocationDatas[managers.LevelManager.GetLocationDataID((string)nodeData["InternalLocation"])]);
               managers.Controller.Position = new Vector3((float)nodeData["PlayerPosX"], (float)nodeData["PlayerPosY"], (float)nodeData["PlayerPosZ"]);
               managers.Controller.DisableMovement = false;
               managers.Controller.DisableCamera = false;
               managers.Controller.Rotation = new Vector3(0f, (float)nodeData["PlayerControllerRotation"], 0f);
               managers.Controller.GetNode<Node3D>("Model").Rotation = new Vector3(0f, (float)nodeData["PlayerModelRotation"], 0f);
               managers.Controller.GetNode<Node3D>("CameraTarget").Rotation = new Vector3((float)nodeData["CameraRotationX"], 0f, 0f);

               managers.LevelManager.MainMenuScreenName = (string)nodeData["MainMenuScreen"];        
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

      EmitSignal(SignalName.SaveFinished);
   }
   
   void OnSaveButtonDown()
   {
      SaveGame(false, currentSaveIndex);
   }
}
