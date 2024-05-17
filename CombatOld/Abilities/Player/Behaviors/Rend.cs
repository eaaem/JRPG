using Godot;
using System;
using System.Collections.Generic;

public partial class Rend : PlayerAbilityBehavior
{
   public override void OnButtonDown()
   {
      SetEnemyTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         DamagingEntity damager = new DamagingEntity((int)(combatManager.CurrentFighter.level * 2.5), StatType.Strength, StatType.Fortitude, DamageType.Physical);
         int damage = combatManager.CalculateDamage(new List<DamagingEntity>() { damager });
         combatManager.ProcessAttack(damage, combatManager.CurrentTarget);
         combatManager.ApplyStatus(33, combatManager.CurrentTarget, StatusEffect.Bleed, 2, 3);
         combatManager.CurrentFighter.currentMana -= resource.manaCost;
         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget });
      }
   }
}
