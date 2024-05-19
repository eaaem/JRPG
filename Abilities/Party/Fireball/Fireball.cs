using Godot;
using System;
using System.Collections.Generic;

public partial class Fireball : PlayerAbilityBehavior
{
   public override void OnButtonDown()
   {
      SetEnemyTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         DamagingEntity damager = new DamagingEntity(combatManager.CurrentFighter.level * 3, StatType.Intelligence, StatType.Willpower, DamageType.Fire);
         int damage = combatManager.CalculateDamage(new List<DamagingEntity>() { damager });
         List<Fighter> targets = combatManager.GetSurrounding(combatManager.CurrentTarget);

         for (int i = 0; i < targets.Count; i++)
         {
            combatManager.ProcessAttack(damage, targets[i]);

            stacksAndStatusManager.AddStack(targets[i], new Stack("Elemental Rot (Fire)", 1, "elemental_explosion"));
         }

         combatManager.ProcessAttack(damage, combatManager.CurrentTarget);
         stacksAndStatusManager.AddStack(combatManager.CurrentTarget, new Stack("Elemental Rot (Fire)", 1, "elemental_explosion"));
         combatManager.CurrentFighter.currentMana -= resource.manaCost;

         targets.Add(combatManager.CurrentTarget);
         combatManager.RegularCast(targets);
      }
   }
}
