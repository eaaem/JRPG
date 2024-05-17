using Godot;
using System;

[GlobalClass]
public partial class Enemy : Resource
{
   [Export]
   public string enemyName;
   [Export]
   public int level;
   [Export]
   public Stat[] stats = new Stat[10];
   [Export]
   public PackedScene model;
   [Export]
   public Affinity affinity;
   [Export]
   public AbilityResource[] abilities = new AbilityResource[10];

   public Enemy() : this(null, new Stat[10], null) {}

   public Enemy(string cEnemyName, Stat[] cStats, PackedScene cModel) {
      enemyName = cEnemyName;
      stats = cStats;
      model = cModel;
   }
}
