using Godot;
using System;
using System.Collections.Generic;

public enum StatType {
   Constitution,
   Knowledge,
   Strength,
   Intelligence,
   Fortitude,
   Willpower,
   Accuracy,
   Evasion,
   Speed,
   Threat
}

public partial class Fighter : Node
{
	public string fighterName;
   public int level;
   public Stat[] stats = new Stat[10];
   public Affinity affinity;
   public List<AbilityResource> abilities = new List<AbilityResource>();
   public int specialCooldown;
   public bool specialActive;

   public bool wasHit;

   public int currentHealth;
   public int currentMana;
   public int maxHealth;
   public int maxMana;

   public int actionLevel;
   public List<AppliedStatusEffect> currentStatuses = new List<AppliedStatusEffect>();
   public List<StatModifier> statModifiers = new List<StatModifier>();
   public List<Stack> stacks = new List<Stack>();

   public bool isDead;
   public bool isEnemy;

   public Control UIPanel;
   public Node3D model;
   public Node3D placementNode;
   public Button targetButton;

   public Companion companion;

   public int GetMaxHealth()
   {
      if (level == 0)
      {
         return stats[0].value * 2;
      }

	   return stats[0].value * 2 * level;
   }

   public int GetMaxMana()
   {
	  return (int)(stats[1].value * 1.5 * level);
   }
}

public partial class Companion
{
   public Enemy enemyDataSource;

   public string companionName;
   public Stat[] stats = new Stat[10];
   public Affinity affinity;
   public List<AbilityResource> abilities = new List<AbilityResource>();

   public int currentHealth;
   public int currentMana;
   public int maxHealth;
   public int maxMana;

   public Node3D model;
   public int duration;
   public bool hadTurn;
}