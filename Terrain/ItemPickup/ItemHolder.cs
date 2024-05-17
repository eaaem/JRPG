using Godot;
using System;

public partial class ItemHolder : Node
{
	[Export]
   public ItemResource heldItem;
   [Export]
   public int quantity;
   [Export]
   public bool initialize;
   [Export]
   public int id;

   public override void _Ready()
   {
      if (initialize)
      {
         ItemHolder parentHolder = GetParent<ItemHolder>();
         heldItem = parentHolder.heldItem;
         quantity = parentHolder.quantity;
      }
   }
}
