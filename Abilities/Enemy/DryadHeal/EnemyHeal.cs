using Godot;
using System;

public partial class EnemyHeal : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      int healAmount = Mathf.CeilToInt(combatManager.CurrentTarget.maxHealth * 0.20f);
      combatManager.CurrentTarget.currentHealth += healAmount;
      uiManager.ProjectDamageText(combatManager.CurrentTarget, healAmount, DamageType.None, false, true);

      string overridePath = "";

      if (combatManager.CurrentTarget == combatManager.CurrentFighter)
      {
         overridePath = "res://Abilities/Enemy/DryadHeal/dryad_heal_self_directions.tscn";
      }

      combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentTarget }, false, overridePath);
   }
}
