using Godot;
using System;

public enum EnemyAIType
{
   None,
   Melee,
   Caster,
   Healer
}

[GlobalClass]
public partial class Enemy : Resource
{
   [Export]
   public string enemyName;
   [Export]
   public int level;
   [Export]
   public int baseAttackDamage;
   [Export]
   public Stat[] stats = new Stat[10];
   [Export]
   public PackedScene model;
   [Export]
   public Affinity affinity;
   [Export]
   public EnemyAIType enemyAIType;
   [Export]
   public int chanceOfUsingAbility;
   [Export]
   public AbilityResource[] abilities = new AbilityResource[10];
   [Export]
   public string turnOrderSpritePath;

   public Enemy() : this(null, new Stat[10], null) {}

   public Enemy(string cEnemyName, Stat[] cStats, PackedScene cModel) {
      enemyName = cEnemyName;
      stats = cStats;
      model = cModel;
   }
}
