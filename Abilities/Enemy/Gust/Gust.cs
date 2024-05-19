using Godot;
using System;

public partial class Gust : EnemyAbilityBehavior
{
   public override void OnEnemyCast()
   {
      System.Collections.Generic.List<DamagingEntity> damagers = new System.Collections.Generic.List<DamagingEntity>();
      damagers.Add(new DamagingEntity(combatManager.CurrentFighter.level * 3, StatType.Intelligence, StatType.Willpower, DamageType.Air));
      
      System.Collections.Generic.List<Fighter> targets = new System.Collections.Generic.List<Fighter>();

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (!combatManager.Fighters[i].isDead && !combatManager.Fighters[i].isEnemy)
         {
            combatManager.ProcessAttack(combatManager.CalculateDamage(damagers), combatManager.Fighters[i]);
            targets.Add(combatManager.Fighters[i]);
         }
      }
      
      combatManager.CurrentFighter.currentMana -= 4;
      combatManager.RegularCast(targets);
   }
}
