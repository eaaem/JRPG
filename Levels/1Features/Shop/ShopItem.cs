using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ShopItem : Node
{
   // Godot only lets us export arrays, so a list is created to copy the base selection (working with arrays is just too tedious)
   [Export]
   public ItemResource[] selection = new ItemResource[0];

   public int id;
   
   /*public List<ShopInventoryItem> actualSelection = new List<ShopInventoryItem>();

   

   public const int RestockTickTime = 300;
   public int RestockTick;

   public override void _Ready()
   {
      RestockTickDown();
   }

   public void AddItemToSelection(ItemResource item, int quantity)
   {
      for (int i = 0; i < actualSelection.Count; i++)
      {
         if (actualSelection[i].item == item)
         {
            actualSelection[i].inStock += quantity;
            return;
         }
      }

      actualSelection.Add(new ShopInventoryItem(item, quantity, 0));
   }

   public void RemoveItemFromSelection(ItemResource item, int quantity)
   {
      for (int i = 0; i < actualSelection.Count; i++)
      {
         if (actualSelection[i].item == item)
         {
            actualSelection[i].inStock -= quantity;

            if (actualSelection[i].inStock <= 0)
            {
               actualSelection.Remove(actualSelection[i]);
            }

            return;
         }
      }
   }

   private async void RestockTickDown()
   {
      while (true)
      {
         await ToSignal(GetTree().CreateTimer(1f), "timeout");
         RestockTick++;

         if (RestockTick == RestockTickTime)
         {
            Restock();
         }
      }
   }

   public void Restock()
   {
      for (int i = 0; i < actualSelection.Count; i++)
      {
         if (actualSelection[i].inStock < actualSelection[i].maxStock)
         {
            actualSelection[i].inStock++;
         }
      }

      RestockTick = 0;
   }*/
}
