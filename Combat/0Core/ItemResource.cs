using Godot;
using System;

public enum ItemType
{
   None,
   Weapon,
   Head,
   Chest,
   Legs,
   Arms,
   Accessory,
   Consumable
}

public enum ItemCategory
{
   None,
   Light,
   Medium,
   Heavy,
   Bow,
   Staff,
   Battleaxe
}

[GlobalClass]
public partial class ItemResource : Resource
{
   [Export]
   public string name;
   [Export(PropertyHint.MultilineText)]
   public string description;
   [Export]
   public string scriptPath;
   [Export]
   public float specialModifier;
   [Export]
   public bool hitsAll;
   [Export]
   public bool hitsSurrounding;
   [Export]
   public bool hitsSelf;
   [Export]
   public bool hitsTeam;
   [Export]
   public ItemType itemType;
   [Export]
   public ItemCategory itemCategory;
   [Export]
   public int price;
   [Export]
   public StatContainer affectedStats = new StatContainer();
   [Export]
   public bool usableOutsideCombat;
   [Export(PropertyHint.MultilineText)]
   public string outOfCombatUseMessage;
}

public partial class InventoryItem : Node
{
   public ItemResource item;
   public int quantity;

   public InventoryItem(ItemResource item, int quantity)
   {
      this.item = item;
      this.quantity = quantity;
   }

   public Godot.Collections.Dictionary<string, Variant> SaveItem()
   {
      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "ItemName", item.ResourcePath},
         { "Quantity", quantity }
      };
   }
}