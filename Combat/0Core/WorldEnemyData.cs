using Godot;
using System;

[GlobalClass]
public partial class WorldEnemyData : Resource
{
	[Export]
   public Enemy enemy;
   [Export]
   public int rollChance;
   [Export]
   public int minQuantity;
   [Export]
   public int maxQuantity;
}
