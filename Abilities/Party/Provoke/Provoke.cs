using Godot;
using System;
using System.Collections.Generic;

public partial class Provoke : PlayerAbilityBehavior
{
	public override void OnButtonDown()
   {
      SetTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         combatManager.CurrentFighter.wasHit = true;
         stacksAndStatusManager.ApplyStatus(100, combatManager.CurrentFighter, StatusEffect.Taunting, 1, 1);
         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget }, false);
      }
   }
}
