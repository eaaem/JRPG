using Godot;
using System;

[GlobalClass]
public partial class Stat : Resource
{
   [Export]
	public StatType statType;
   [Export]
   public int value;
   [Export]
   public int baseValue;

   public Stat() {
      statType = StatType.Constitution;
      value = 0;
      baseValue = 0;
   }
}