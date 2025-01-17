using Godot;
using System;
using System.Collections.Generic;

public partial class MagicBolt : PlayerAbilityBehavior
{
   public override void OnButtonDown()
   {
      SetEnemyTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         DamagingEntity damager = new DamagingEntity((int)(combatManager.CurrentFighter.level * 2.5), StatType.Intelligence, StatType.Willpower, DamageType.Magical);
         combatManager.ProcessAttack(new List<DamagingEntity>() { damager }, combatManager.CurrentTarget);
         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget });
      }
   }
}
