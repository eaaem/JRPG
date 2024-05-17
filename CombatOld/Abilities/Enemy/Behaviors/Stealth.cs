using Godot;
using System;

public partial class Stealth : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      combatManager.CurrentFighter.wasHit = true;
      combatManager.ApplyStatus(100, combatManager.CurrentFighter, StatusEffect.Stealth, 3, 3);
      combatManager.CurrentFighter.currentMana -= 2;
      combatManager.RegularCast(new System.Collections.Generic.List<Fighter>() { combatManager.CurrentFighter }, false);
   }
}
