using Godot;
using System;

public partial class VineWrap : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      System.Collections.Generic.List<DamagingEntity> damagers = new System.Collections.Generic.List<DamagingEntity>();
      damagers.Add(new DamagingEntity(3, StatType.Intelligence, StatType.Willpower, DamageType.Venom));
      combatManager.ProcessAttack(damagers, combatManager.CurrentTarget);
      combatManager.CurrentFighter.currentMana -= 3;
      combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentTarget });
   }
}
