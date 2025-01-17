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
         List<Fighter> targets = new List<Fighter>();
         targets.Add(combatManager.CurrentTarget);
         targets.AddRange(combatManager.GetSurrounding(combatManager.CurrentTarget));

         for (int i = 0; i < targets.Count; i++)
         {
            combatManager.ProcessAttack(new List<DamagingEntity>() { damager }, targets[i]);

            stacksAndStatusManager.AddStack(targets[i], new Stack("Elemental Rot (Fire)", 1, "elemental_explosion"));
         }

         combatManager.RegularCast(targets);
      }
   }
}
