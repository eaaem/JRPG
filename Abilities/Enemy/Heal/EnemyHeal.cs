using Godot;
using System;

public partial class EnemyHeal : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      float multiplier = 1f;
      combatManager.CurrentTarget.currentHealth += Mathf.CeilToInt(combatManager.CurrentTarget.maxHealth * (0.15f * multiplier));

      combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentTarget }, false);
   }
}
