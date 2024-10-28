using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class LevelManager : Node
{
   /// <summary>
   /// The index in the list of LocationDatas that matches the LocationData currently active.
   /// </summary>
   public int ActiveLocationDataID { get; set; }

   public List<LocationData> LocationDatas { get; set; }

   public string location;
   public string InternalLocation { get; set; }

   public string MainMenuScreenName { get; set;}

   [Export]
   private ManagerReferenceHolder managers;

   [Signal]
   public delegate void SaveLevelProgressionEventHandler();
   [Signal]
   public delegate void LoadLevelProgressionEventHandler();

   Node3D baseNode;

   private AudioStreamPlayer musicPlayer;

   public override void _Ready()
   {
      LocationDatas = new List<LocationData>();

      baseNode = GetNode<Node3D>("/root/BaseNode");
      musicPlayer = baseNode.GetNode<AudioStreamPlayer>("MusicPlayer");
   }

   public async void OpenWorldMap(string map, Vector3 spawnLocation, bool useSpawnLocation, string spawnPointName = "")
   {  
      EmitSignal(SignalName.SaveLevelProgression);
      DiscardExistingLevel();

      managers.Controller.DisableMovement = true;
      managers.Controller.DisableCamera = true;
      managers.Controller.IsSprinting = false;
      
      // We need to move the controller away so that it doesn't reactivate an exit point immediately upon exiting the world map
      managers.Controller.GlobalPosition = new Vector3(0f, 25f, 0f);

      PackedScene worldMapPrefab = GD.Load<PackedScene>("res://WorldMap/0Core/world_map_base.tscn");
      Node3D worldMap = worldMapPrefab.Instantiate<Node3D>();

      PackedScene specialMapPrefab = GD.Load<PackedScene>("res://WorldMap/Maps/" + map + "/" + map + ".tscn");
      Node3D specialMap = specialMapPrefab.Instantiate<Node3D>();

      baseNode.AddChild(worldMap);
      baseNode.AddChild(specialMap);

      Node3D informationHolder = specialMap.GetNode<Node3D>("InformationHolder");
      specialMap.RemoveChild(informationHolder);
      worldMap.AddChild(informationHolder);

      if (!useSpawnLocation)
      {
         worldMap.GetNode<CharacterBody3D>("Player").GlobalPosition = specialMap.GetNode<Node3D>(spawnPointName).GlobalPosition;
      }
      else
      {
         worldMap.GetNode<CharacterBody3D>("Player").GlobalPosition = spawnLocation;
      }

      location = "World Map";
      InternalLocation = map;

      EmitSignal(SignalName.LoadLevelProgression);

      worldMap.GetNode<Camera3D>("Player/CameraTarget/PlayerCamera").MakeCurrent();
      worldMap.GetNode<CharacterController>("Player").DisableMovement = true;

      baseNode.RemoveChild(specialMap);
      specialMap.QueueFree();

      TransitionMusicTracks(worldMap);

      managers.MenuManager.FadeFromBlack();

      while (managers.MenuManager.BlackScreenIsVisible)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }

      worldMap.GetNode<CharacterController>("Player").DisableMovement = false;
   }

   public void DiscardExistingLevel()
   {
      // If a level already exists, delete it
      if (baseNode.HasNode("Level"))
      {
         Node3D levelNode = baseNode.GetNode<Node3D>("Level");
         baseNode.RemoveChild(levelNode);
         levelNode.QueueFree();
      }
   }

   public void TransitionLevels(string targetLevelInternal, string targetLevelName, string entrancePointName)
   {
      DiscardExistingLevel();

      if (baseNode.HasNode("WorldMap"))
      {
         Node2D worldMap = baseNode.GetNode<Node2D>("WorldMap");
         baseNode.RemoveChild(worldMap);
         worldMap.QueueFree();
      }

      int locationDataID = GetLocationDataID(targetLevelInternal);

      if (locationDataID == -1)
      {
         CreateLevel(targetLevelName, targetLevelInternal, entrancePointName, false);
      }
      else
      {
         CreateLevel(targetLevelName, targetLevelInternal, entrancePointName, true, LocationDatas[locationDataID]);
      }
   }

   public int GetLocationDataID(string levelName)
   {
      for (int i = 0; i < LocationDatas.Count; i++)
      {
         if (LocationDatas[i].locationName == levelName)
         {
            return i;
         }
      }

      return -1;
   }

	public void CreateLevel(string levelName, string internalLevelName, string entrancePointName, bool isLoaded, LocationData locationData = null)
   {
      EmitSignal(SignalName.SaveLevelProgression);
      DiscardExistingLevel();

      managers.Controller.GlobalPosition = new Vector3(0f, 100f, 0f);

      PackedScene levelScene = GD.Load<PackedScene>("res://Levels/" + internalLevelName + "/" + internalLevelName + ".tscn");
      Node3D level = levelScene.Instantiate<Node3D>();
      baseNode.AddChild(level);
      level.Name = "Level";
      location = levelName;
      InternalLocation = internalLevelName;

      managers.Controller.GlobalPosition = level.GetNode<Node3D>(entrancePointName).GlobalPosition;
      managers.Controller.GlobalRotation = level.GetNode<Node3D>(entrancePointName).GlobalRotation;
      managers.Controller.DisableMovement = false;
      managers.Controller.DisableCamera = false;

      managers.Controller.GetNode<Camera3D>("CameraTarget/SpringArm3D/PlayerCamera").MakeCurrent();
      managers.PartyManager.MovePartyMembersBehindPlayer();

      // Only combat levels have arenas
      if (level.HasNode("Arena"))
      {
         GetNode<CombatManager>("/root/BaseNode/CombatManager").ResetNodes();
      }

      LocationData currentLocationData = isLoaded ? locationData 
                                         : new LocationData(internalLevelName, 0, new Godot.Collections.Dictionary<string, bool>(), 
                                                            new Godot.Collections.Dictionary<string, bool>(), new Godot.Collections.Dictionary<string, bool>(), 0);

      int enemyCounter = 0;
      int itemCounter = 0;
      int cutsceneCounter = 0;

      for (int i = 0; i < level.GetChildCount(); i++)
      {
         if (level.GetChild(i).IsInGroup("cutscene"))
         {
            level.GetChild<CutsceneTrigger>(i).id = cutsceneCounter;
            if (!isLoaded)
            {
               currentLocationData.cutscenesSeen.Add(cutsceneCounter.ToString(), false);
            }
            else
            {
               if (currentLocationData.cutscenesSeen[cutsceneCounter.ToString()] == true)
               {
                  level.GetChild(i).QueueFree();
               }
            }

            cutsceneCounter++;
         }
      }

      Node3D chunks = level.GetNode<Node3D>("Chunks");

      for (int i = 0; i < chunks.GetChildCount(); i++)
      {
         for (int j = 0; j < chunks.GetChild(i).GetChildCount(); j++)
         {
            if (chunks.GetChild(i).GetChild(j).IsInGroup("enemy"))
            {
               chunks.GetChild(i).GetChild<WorldEnemy>(j).id = enemyCounter;
               if (!isLoaded)
               {
                  // We're creating the defeatedEnemies dictionary for the first time
                  currentLocationData.defeatedEnemies.Add(enemyCounter.ToString(), false);
               }
               else
               {
                  // We're loading the defeatedEnemies dictionary from save data; any defeated enemy needs to be deleted
                  if (currentLocationData.defeatedEnemies[enemyCounter.ToString()] == true)
                  {
                     chunks.GetChild(i).GetChild(j).QueueFree();
                  }
               }
               
               enemyCounter++;
            }
            else if (chunks.GetChild(i).GetChild(j).IsInGroup("item"))
            {
               chunks.GetChild(i).GetChild<ItemHolder>(j).GetNode<ItemHolder>("ItemHolder").id = itemCounter;
               if (!isLoaded)
               {
                  currentLocationData.pickedUpItems.Add(itemCounter.ToString(), false);
               }
               else
               {
                  if (currentLocationData.pickedUpItems[itemCounter.ToString()] == true)
                  {
                     chunks.GetChild(i).GetChild(j).QueueFree();
                  }
               }

               itemCounter++;
            }
         }
      }

      if (!isLoaded)
      {
         LocationDatas.Add(currentLocationData);
      }

      currentLocationData.timeOfLastVisit = Time.GetUnixTimeFromSystem();
      ActiveLocationDataID = GetLocationDataID(InternalLocation);

      TransitionMusicTracks(level);

      managers.MenuManager.FadeFromBlack();

      EmitSignal(SignalName.LoadLevelProgression);
   }

   public async void TransitionMusicTracks(Node source)
   {
      while (musicPlayer.VolumeDb > -80.1f)
      {
         musicPlayer.VolumeDb -= 8f;
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }

      LoadNewMusic(source);
      musicPlayer.Play();
      
      while (musicPlayer.VolumeDb < 0f)
      {
         musicPlayer.VolumeDb += 8f;
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }
   }

   public async void MuteMusic()
   {
      while (musicPlayer.VolumeDb > -80.1f)
      {
         musicPlayer.VolumeDb -= 8f;
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }
   }

   public void LoadNewMusic(Node source)
   {
      musicPlayer.Stream = GD.Load<AudioStreamMP3>(source.GetNode<MusicHolder>("MusicHolder").musicPath);
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

      baseNode.GetNode<CharacterController>("PartyMembers/Member1").GlobalPosition = new Vector3(0.9f, 0.6f, -24.5f);

      managers.PartyManager.Party.Clear();
      managers.PartyManager.Items.Clear();

      managers.MainMenuManager.CheckLoadGameButtonAvailability();
      managers.MainMenuManager.CheckNewGameButtonAvailability();
      managers.MainMenuManager.Visible = true;
      managers.MainMenuManager.CreateMainMenuLevel();

      Panel mainMenuBack = baseNode.GetNode<Panel>("MainMenu/Background");

      baseNode.GetNode<Camera2D>("MainMenu/MenuCamera").MakeCurrent();

      managers.LevelManager.TransitionMusicTracks(managers.MainMenuManager);

      if (baseNode.HasNode("WorldMap"))
      {
         Node2D worldMap = baseNode.GetNode<Node2D>("WorldMap");
         baseNode.RemoveChild(worldMap);
         worldMap.QueueFree();
      }

      DiscardExistingLevel();

      for (int i = 0; i < mainMenuBack.GetChildCount(); i++)
      {
         if (mainMenuBack.GetChild(i).Name == "Main")
         {
            mainMenuBack.GetChild<Control>(i).Visible = true;
         }
         else
         {
            mainMenuBack.GetChild<Control>(i).Visible = false;
         }
      }
   }
}

public partial class LocationData : Node
{
   public string locationName;
   public int levelProgress;
   public Godot.Collections.Dictionary<string, bool> defeatedEnemies = new Godot.Collections.Dictionary<string, bool>();
   public Godot.Collections.Dictionary<string, bool> pickedUpItems = new Godot.Collections.Dictionary<string, bool>();
   public Godot.Collections.Dictionary<string, bool> cutscenesSeen = new Godot.Collections.Dictionary<string, bool>();
   public double timeOfLastVisit;
   public double timeSinceLastVisit;

   public LocationData() { }

   public LocationData(string locationName, int levelProgress, Godot.Collections.Dictionary<string, bool> defeatedEnemies, 
                       Godot.Collections.Dictionary<string, bool> pickedUpItems, Godot.Collections.Dictionary<string, bool> cutscenesSeen, double timeSinceLastVisit)
   {
      this.locationName = locationName;
      this.levelProgress = levelProgress;
      this.defeatedEnemies = defeatedEnemies;
      this.pickedUpItems = pickedUpItems;
      this.cutscenesSeen = cutscenesSeen;
      this.timeSinceLastVisit = timeSinceLastVisit;
   }

   public Godot.Collections.Dictionary<string, Variant> SaveLocationData()
   {
      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "LocationName", locationName },
         { "LevelProgress", levelProgress },
         { "DefeatedEnemies", defeatedEnemies },
         { "PickedUpItems", pickedUpItems },
         { "CutscenesSeen", cutscenesSeen },
         { "TimeSinceLastVisit", timeSinceLastVisit }
      };
   }
}