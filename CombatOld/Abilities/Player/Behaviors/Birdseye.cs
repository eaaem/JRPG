using Godot;
using System;
using System.Collections.Generic;

public partial class Birdseye : PlayerAbilityBehavior
{
   public override void OnButtonDown()
   {
      SetTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         List<Fighter> targets = new List<Fighter>();
         for (int i = 0; i < combatManager.Fighters.Count; i++)
         {
            if (combatManager.Fighters[i].isEnemy)
            {
               break;
            }

            combatManager.Fighters[i].wasHit = true;
            targets.Add(combatManager.Fighters[i]);
            combatManager.ApplyStatus(100, combatManager.Fighters[i], StatusEffect.Birdseye, 3, 3);
         }

         combatManager.CurrentFighter.currentMana -= resource.manaCost;

         combatManager.RegularCast(targets, false);
      }
   }
}
