using Godot;
using System;

public partial class WindBolt : EnemyAbilityBehavior
{
	public override void OnEnemyCast()
   {
      System.Collections.Generic.List<DamagingEntity> damagers = new System.Collections.Generic.List<DamagingEntity>();
      damagers.Add(new DamagingEntity(4, StatType.Intelligence, StatType.Willpower, DamageType.Air));
      
      combatManager.ProcessAttack(damagers, combatManager.CurrentTarget);
      combatManager.CurrentFighter.currentMana -= 1;
      combatManager.RegularCast(new System.Collections.Generic.List<Fighter> { combatManager.CurrentTarget });
   }
}
