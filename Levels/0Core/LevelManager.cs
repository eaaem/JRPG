using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class LevelManager : Node
{
   public int ActiveLocationDataID { get; set; }

   public List<LocationData> LocationDatas { get; set; }

   public string location;
   public string InternalLocation { get; set; }

   [Export]
   private ManagerReferenceHolder managers;

   Node3D baseNode;

   public override void _Ready()
   {
      LocationDatas = new List<LocationData>();

      baseNode = GetNode<Node3D>("/root/BaseNode");
   }

   public void OpenWorldMap(string map, Vector2 specificSpawnPoint, bool useSpecificSpawnPoint)
   {  
      DiscardExistingLevel();

      managers.Controller.DisableMovement = true;
      managers.Controller.DisableCamera = true;
      
      // We need to move the controller away so that it doesn't reactivate an exit point immediately upon exiting the world map
      managers.Controller.GlobalPosition = new Vector3(0f, 25f, 0f);

      PackedScene worldMapPrefab = GD.Load<PackedScene>("res://WorldMap/0Core/world_map_prefab.tscn");
      Node2D worldMap = worldMapPrefab.Instantiate<Node2D>();

      PackedScene specialMapPrefab = GD.Load<PackedScene>("res://WorldMap/Maps/" + map + "/" + map + "_map.tscn");
      Node2D specialMap = specialMapPrefab.Instantiate<Node2D>();

      baseNode.AddChild(worldMap);
      baseNode.AddChild(specialMap);

      Node2D colliderHolder = specialMap.GetNode<Node2D>("Colliders");
      specialMap.RemoveChild(colliderHolder);
      worldMap.AddChild(colliderHolder);
      worldMap.GetNode<Sprite2D>("Map").Texture = specialMap.GetNode<Sprite2D>("Map").Texture;

      if (!useSpecificSpawnPoint)
      {
         worldMap.GetNode<CharacterBody2D>("Player").GlobalPosition = specialMap.GetNode<Node2D>("SpawnPoint").GlobalPosition;
      }
      else
      {
         worldMap.GetNode<CharacterBody2D>("Player").GlobalPosition = specificSpawnPoint;
      }

      location = "World Map";
      InternalLocation = map;

      worldMap.GetNode<Camera2D>("Player/2DPlayerCamera").MakeCurrent();

      baseNode.RemoveChild(specialMap);
      specialMap.QueueFree();
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
         GetNode<CombatManager>("/root/BaseNode/CombatManagerObj").ResetNodes();
      }

      LocationData currentLocationData = isLoaded ? locationData 
                                         : new LocationData(internalLevelName, new Godot.Collections.Dictionary<string, bool>(), 
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

      NavigationRegion3D region = level.GetNode<NavigationRegion3D>("NavigationRegion3D");

      for (int i = 0; i < region.GetChildCount(); i++)
      {
         for (int j = 0; j < region.GetChild(i).GetChildCount(); j++)
         {
            if (region.GetChild(i).GetChild(j).Name == "WorldEnemy")
            {
               region.GetChild(i).GetChild<WorldEnemy>(j).id = enemyCounter;
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
                     region.GetChild(i).GetChild(j).QueueFree();
                  }
               }
               
               enemyCounter++;
            }
            else if (region.GetChild(i).GetChild(j).IsInGroup("item"))
            {
               region.GetChild(i).GetChild<ItemHolder>(j).GetNode<ItemHolder>("ItemHolder").id = itemCounter;
               if (!isLoaded)
               {
                  currentLocationData.pickedUpItems.Add(itemCounter.ToString(), false);
               }
               else
               {
                  if (currentLocationData.pickedUpItems[itemCounter.ToString()] == true)
                  {
                     region.GetChild(i).GetChild(j).QueueFree();
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
   }
}

public partial class LocationData : Node
{
   public string locationName;
   public Godot.Collections.Dictionary<string, bool> defeatedEnemies = new Godot.Collections.Dictionary<string, bool>();
   public Godot.Collections.Dictionary<string, bool> pickedUpItems = new Godot.Collections.Dictionary<string, bool>();
   public Godot.Collections.Dictionary<string, bool> cutscenesSeen = new Godot.Collections.Dictionary<string, bool>();
   public double timeOfLastVisit;
   public double timeSinceLastVisit;

   public LocationData() { }

   public LocationData(string locationName, Godot.Collections.Dictionary<string, bool> defeatedEnemies, Godot.Collections.Dictionary<string, bool> pickedUpItems, 
                       Godot.Collections.Dictionary<string, bool> cutscenesSeen, double timeSinceLastVisit)
   {
      this.locationName = locationName;
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
         { "DefeatedEnemies", defeatedEnemies },
         { "PickedUpItems", pickedUpItems },
         { "CutscenesSeen", cutscenesSeen },
         { "TimeSinceLastVisit", timeSinceLastVisit }
      };
   }
}