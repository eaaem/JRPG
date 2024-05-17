using Godot;
using System;

[GlobalClass]
public partial class ShopInventoryItem : Resource
{
   [Export]
   public ItemResource item;
   [Export]
   public int inStock;
   [Export]
   public int maxStock;

   public ShopInventoryItem()
   {
      item = null;
      inStock = 0;
      maxStock = 0;
   }

   public ShopInventoryItem(ItemResource item, int inStock, int maxStock)
   {
      this.item = item;
      this.inStock = inStock;
      this.maxStock = maxStock;
   }
}
