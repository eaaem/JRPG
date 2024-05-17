using Godot;
using System;

public partial class Bulldoze : Node
{
	private CombatManager combatManager;

   public override void _Ready()
   {
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");
      combatManager.EnemyAbilityCast += OnCast;
   }

   void OnCast()
   {
      /*int damage = combatManager.CalculateDamage(combatManager.currentFighter.level - 5, StatType.Strength, StatType.Fortitude, DamageType.Physical);
      combatManager.ProcessAttack(damage, combatManager.currentTarget);
      combatManager.ApplyStatus(100, StatusEffect.Stun, 2, 3);
      combatManager.currentFighter.currentMana -= combatManager.currentAbility.manaCost;
      combatManager.ProcessValues();*/
      combatManager.EnemyAbilityCast -= OnCast;
   }
}
