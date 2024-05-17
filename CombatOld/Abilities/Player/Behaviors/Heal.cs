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

         combatManager.CurrentTarget.currentHealth += Mathf.CeilToInt(combatManager.CurrentTarget.maxHealth * (0.20f * multiplier));

         //combatManager.ProcessAttack(damage, combatManager.currentTarget);
        // combatManager.AddStack(combatManager.currentTarget, new Stack("Elemental Rot (Fire)", 1, "elemental_explosion"));
       //  combatManager.currentFighter.currentMana -= resource.manaCost;
         combatManager.CurrentFighter.currentMana -= resource.manaCost;
         combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentTarget }, false);
      }
   }
}
