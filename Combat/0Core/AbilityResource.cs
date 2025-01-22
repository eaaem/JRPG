using Godot;
using System;

[GlobalClass]
public partial class AbilityResource : Resource
{
   [Export]
   public string name;
   [Export(PropertyHint.MultilineText)]
   public string description;
   [Export]
   public int manaCost;
   [Export]
   public string scriptPath;
   [Export]
   public bool hitsAll;
   [Export]
   public bool hitsSurrounding;
   [Export]
   public bool hitsSelf;
   [Export]
   public bool hitsTeam;
   [Export]
   public bool onlyHitsTeam;
   [Export]
   public bool onlyHitsSelf;
   [Export]
   public bool isEnemyHealingSpell;
   [Export]
   public bool bypassesDeaths;
   [Export]
   public string graphicPath;
   [Export]
   public int requiredLevel;
   [Export]
   public int abilityWeight;
   [Export]
   public StatusEffect notStackingEffectApplied;
}
