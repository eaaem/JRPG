using Godot;
using System;

public partial class Heal : PlayerAbilityBehavior
{
   public override void OnButtonDown()
   {
      SetTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         float multiplier = 1f;

         if (resource.name == "Heal")
         {
            multiplier = 1.5f;
         }

         int healAmount = Mathf.CeilToInt(combatManager.CurrentTarget.maxHealth * (0.20f * multiplier));
         combatManager.CurrentTarget.currentHealth += healAmount;
         uiManager.ProjectDamageText(combatManager.CurrentTarget, healAmount, DamageType.None, false, true);
         combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentTarget }, false);
      }
   }
}
