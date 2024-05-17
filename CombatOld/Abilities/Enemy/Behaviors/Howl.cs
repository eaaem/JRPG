using Godot;
using System;

public partial class Howl : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      combatManager.CurrentFighter.wasHit = true;
      combatManager.ApplyStatus(100, combatManager.CurrentFighter, StatusEffect.MegaBuff, 2, 4);
      
      combatManager.CurrentFighter.currentMana -= 4;
      combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentFighter }, false);
   }
}
