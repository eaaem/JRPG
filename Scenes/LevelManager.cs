using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class LevelManager : Node
{
   //public Godot.Collections.Dictionary<int, bool> defeatedEnemies = new Godot.Collections.Dictionary<int, bool>();
   //public Godot.Collections.Dictionary<int, bool> carriedItems = new Godot.Collections.Dictionary<int, bool>();
   //public Godot.Collections.Dictionary<int, bool> cutscenesSeen = new Godot.Collections.Dictionary<int, bool>();
   public int ActiveLocationDataID { get; set; }

   public List<LocationData> LocationDatas { get; set; }

   public string location;
   public string InternalLocation { get; set; }

   private CharacterController characterController;
   private PartyManager partyManager;

   Node3D baseNode;

   public override void _Ready()
   {
      LocationDatas = new List<LocationData>();

      baseNode = GetNode<Node3D>("/root/BaseNode");
      characterController = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
      partyManager = baseNode.GetNode<PartyManager>("PartyManagerObj");
   }

   public void OpenWorldMap(string map, Vector2 specificSpawnPoint, bool useSpecificSpawnPoint)
   {  
      DiscardExistingLevel();

      characterController.DisableMovement = true;
      // We need to move the controller away so that it doesn't reactivate an exit point immediately upon exiting the world map
      characterController.GlobalPosition = new Vector3(0f, 25f, 0f);

      PackedScene worldMapPrefab = GD.Load<PackedScene>("res://WorldMaps/world_map_prefab.tscn");
      Node2D worldMap = worldMapPrefab.Instantiate<Node2D>();

      PackedScene specialMapPrefab = GD.Load<PackedScene>("res://WorldMaps/Maps/" + map + ".tscn");
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

   public void TransitionLevels(string targetLevelInternal, string targetLevelName)
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
         CreateLevel(targetLevelName, targetLevelInternal, false);
      }
      else
      {
         CreateLevel(targetLevelName, targetLevelInternal, true, LocationDatas[locationDataID]);
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

	public void CreateLevel(string levelName, string internalLevelName, bool isLoaded, LocationData locationData = null)
   {
      DiscardExistingLevel();

      characterController.GlobalPosition = new Vector3(0f, 100f, 0f);

      PackedScene levelScene = GD.Load<PackedScene>("res://Scenes/" + internalLevelName + ".tscn");
      Node3D level = levelScene.Instantiate<Node3D>();
      baseNode.AddChild(level);
      level.Name = "Level";
      location = levelName;
      InternalLocation = internalLevelName;

      characterController.GlobalPosition = level.GetNode<Node3D>("SpawnPoint").GlobalPosition;
      characterController.GlobalRotation = level.GetNode<Node3D>("SpawnPoint").GlobalRotation;
      characterController.DisableMovement = false;
      characterController.GetNode<Camera3D>("CameraTarget/PlayerCamera").MakeCurrent();
      partyManager.MovePartyMembersBehindPlayer();

      // Only combat levels have arenas
      if (level.HasNode("Arena"))
      {
         GetNode<CombatManager>("/root/BaseNode/CombatManagerObj").ResetNodes();
      }

      LocationData currentLocationData = isLoaded ? locationData 
                                         : new LocationData(internalLevelName, new Godot.Collections.Dictionary<string, bool>(), 
                                                            new Godot.Collections.Dictionary<string, bool>(), new Godot.Collections.Dictionary<string, bool>(), 0);

      /*if (!isLoaded)
      {
         defeatedEnemies.Clear();
         carriedItems.Clear();
         cutscenesSeen.Clear();
      }
      else
      {
         defeatedEnemies = new Godot.Collections.Dictionary<int, bool>(loadedDefeatedEnemies);
         carriedItems = new Godot.Collections.Dictionary<int, bool>(loadedCarriedItems);
         cutscenesSeen = new Godot.Collections.Dictionary<int, bool>(loadedCutscenesSeen);
      }*/

      int enemyCounter = 0;
      int itemCounter = 0;
      int cutsceneCounter = 0;
      //int shopCounter = 0;

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
         else if (level.GetChild(i).IsInGroup("shopkeeper"))
         {
            level.GetChild(i).GetNode<AnimationPlayer>("AnimationPlayer").Play("ShopIdle");
         }
         /*else if (level.GetChild(i).IsInGroup("shop"))
         {
            ShopItem shopItem = level.GetChild(i).GetNode<ShopItem>("ShopHolder");
            shopItem.id = shopCounter;

            if (!isLoaded)
            {
               Godot.Collections.Dictionary<string, string> selectionNames = new Godot.Collections.Dictionary<string, string>();
               Godot.Collections.Dictionary<string, int> selectionQuantities = new Godot.Collections.Dictionary<string, int>();

               for (int j = 0; j < shopItem.selection.Length; j++)
               {
                  selectionNames.Add(j.ToString(), shopItem.selection[j].ResourcePath);
                  //selectionQuantities.Add(j.ToString(), shopItem.baseSelection[j].inStock);

                  shopItem.AddItemToSelection(shopItem.baseSelection[j].item, shopItem.baseSelection[j].inStock);
               }
               
               ShopData shopData = new ShopData(internalLevelName, shopCounter, selectionNames, selectionQuantities, 0);
               shopData.shopItem = shopItem;
               currentLocationData.shops.Add(shopData);
            }
            else
            {
               currentLocationData.shops[shopCounter].shopItem = shopItem;

               for (int j = 0; j < currentLocationData.shops[shopCounter].selectionNames.Count; j++)
               {
                  ItemResource item = GD.Load<ItemResource>(currentLocationData.shops[shopCounter].selectionNames[j.ToString()]);
                  shopItem.AddItemToSelection(item, currentLocationData.shops[shopCounter].selectionQuantities[j.ToString()]);
               }

               /*int restockAmount = (int)(currentLocationData.timeSinceLastVisit / ShopItem.RestockTickTime);

               for (int j = 0; j < restockAmount; j++)
               {
                  shopItem.Restock();
               }*/
            //}

            //shopCounter++;
         //}
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
      //}
   }
}

public partial class LocationData : Node
{
   public string locationName;
   public Godot.Collections.Dictionary<string, bool> defeatedEnemies = new Godot.Collections.Dictionary<string, bool>();
   public Godot.Collections.Dictionary<string, bool> pickedUpItems = new Godot.Collections.Dictionary<string, bool>();
   public Godot.Collections.Dictionary<string, bool> cutscenesSeen = new Godot.Collections.Dictionary<string, bool>();
   //public List<ShopData> shops = new List<ShopData>();
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

/*public partial class ShopData : Node
{
   public string shopLocation;
   public int id;
   public Godot.Collections.Dictionary<string, string> selectionNames = new Godot.Collections.Dictionary<string, string>();
   public ShopItem shopItem;

   public ShopData(string shopLocation, int id, Godot.Collections.Dictionary<string, string> selectionNames)
   {
      this.shopLocation = shopLocation;
      this.id = id;
      this.selectionNames = selectionNames;
   }

   public void SyncData()
   {
      selectionNames.Clear();
      //selectionQuantities.Clear();

      for (int i = 0; i < shopItem.selection.Length; i++)
      {
         selectionNames.Add(i.ToString(), shopItem.selection[i].ResourcePath);
         //selectionQuantities.Add(i.ToString(), shopItem.actualSelection[i].inStock);
      }
   }

   public Godot.Collections.Dictionary<string, Variant> SaveShopData()
   {
      SyncData();

      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "ShopLocation", shopLocation },
         { "ID", id },
         { "SelectionNames", selectionNames }
      };
   }
}*/