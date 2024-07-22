using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ShopItem : Node
{
   [Export]
   public ItemResource[] selection = new ItemResource[0];

   public int id;
}
