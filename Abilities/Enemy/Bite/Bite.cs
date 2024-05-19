using Godot;
using System;
using System.Collections.Generic;

// THIS IS ALSO A COMPANION ABILITY

public partial class Bite : AbilityBehavior
{
   public override void OnButtonDown()
   {
      SetEnemyTeamOnCast(button);
   }

   public override void OnEnemyCast()
   {
      CompleteAbility();
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         CompleteAbility();
      }
   }

   void CompleteAbility()
   {
      DamagingEntity damager = new DamagingEntity(combatManager.CurrentFighter.level * 2, StatType.Strength, StatType.Fortitude, DamageType.Physical);
      int damage = combatManager.CalculateDamage(new List<DamagingEntity>() { damager });
      combatManager.ProcessAttack(damage, combatManager.CurrentTarget);
      stacksAndStatusManager.ApplyStatus(100, combatManager.CurrentTarget, StatusEffect.Bleed, 2, 3);

      if (!combatManager.IsCompanionTurn)
      {
         combatManager.CurrentFighter.currentMana -= combatManager.CurrentAbility.manaCost;
      }
      else
      {
         combatManager.CurrentFighter.companion.currentMana -= combatManager.CurrentAbility.manaCost;
      }

      combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget });
   }
}
