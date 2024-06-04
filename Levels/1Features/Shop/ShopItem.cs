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
}
