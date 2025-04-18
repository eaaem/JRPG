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
   Consumable,
   Special
}

public enum ItemCategory
{
   None,
   Light,
   Medium,
   Heavy,
   Bow,
   Staff,
   Battleaxe,
   Bell,
   Whip
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
   public string combatGraphicsPath;
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
   public bool onlyHitsTeam;
   [Export]
   public bool onlyHitsSelf;
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
   [Export]
   public string outOfCombatAudioPath;
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