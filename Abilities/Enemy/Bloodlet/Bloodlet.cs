using Godot;
using System;

public partial class Bloodlet : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      combatManager.CurrentFighter.wasHit = true;

      System.Collections.Generic.List<Fighter> targets = new System.Collections.Generic.List<Fighter>();

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         combatManager.Fighters[i].wasHit = true;
         stacksAndStatusManager.ApplyStatus(66, combatManager.Fighters[i], StatusEffect.Bleed, 2, 5);

         if (combatManager.Fighters[i].currentStatuses[combatManager.Fighters[i].currentStatuses.Count - 1].effect != StatusEffect.Bleed)
         {
            combatManager.Fighters[i].wasHit = false;
         }

         targets.Add(combatManager.Fighters[i]);
      }
      
      combatManager.CurrentFighter.currentMana -= 3;
      combatManager.RegularCast(targets);
   }
}
