using Godot;
using System;
using System.Collections.Generic;

public partial class Inspire : PlayerAbilityBehavior
{
	public override void OnButtonDown()
   {
      SetTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         combatManager.CurrentTarget.wasHit = true;
         stacksAndStatusManager.ApplyStatus(100, combatManager.CurrentTarget, StatusEffect.Inspired, 5, 5);
         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget }, false);
      }
   }
}
