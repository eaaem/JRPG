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

   public int baseAttackDamage;

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
   public EnemyAIType enemyAIType;

   public int abilityUseChance;

   public Control UIPanel;
   public Node3D model;
   public Node3D placementNode;
   public Button targetButton;
   public string turnOrderSpritePath;

   public Companion companion;

   public int GetMaxHealth()
   {
	   return Mathf.RoundToInt(stats[0].value * 2.5f);
   }

   public int GetMaxMana()
   {
	  return Mathf.RoundToInt(stats[1].value * 1.5f);
   }
}

public partial class Companion
{
   public Enemy enemyDataSource;

   public string companionName;
   public Stat[] stats = new Stat[10];
   public Affinity affinity;
   public List<AbilityResource> abilities = new List<AbilityResource>();

   public int baseAttackDamage;

   public int currentHealth;
   public int currentMana;
   public int maxHealth;
   public int maxMana;

   public Node3D model;
   public int duration;
   public bool hadTurn;
   public string spritePath;
}