using Godot;
using System;
using System.Collections.Generic;

public partial class ElementalExplosion : PlayerAbilityBehavior
{
   public override void OnButtonDown()
   {
      SetEnemyTeamOnCast(button);
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         combatManager.CurrentTarget.wasHit = true;
         //int damage = combatManager.CalculateDamage(combatManager.currentFighter.level * 20, StatType.Intelligence, StatType.Willpower, DamageType.Magical);
         //combatManager.ProcessAttack(damage, combatManager.currentTarget);
         List<DamagingEntity> damagers = new List<DamagingEntity>();
         damagers.Add(new DamagingEntity(combatManager.CurrentFighter.level * 2, StatType.Intelligence, StatType.Willpower, DamageType.Magical));

         for (int i = 0; i < combatManager.CurrentTarget.stacks.Count; i++)
         {
            if (combatManager.CurrentTarget.stacks[i].stackName == "Elemental Rot (Fire)")
            {
               int stackQuantity = combatManager.CurrentTarget.stacks[i].quantity;
               stacksAndStatusManager.ApplyStatus(100, combatManager.CurrentTarget, StatusEffect.Burn, stackQuantity + 1, stackQuantity + 1);
               damagers.Add(new DamagingEntity(combatManager.CurrentFighter.level * 2, StatType.Intelligence, StatType.Willpower, DamageType.Fire));

               stacksAndStatusManager.RemoveStack(combatManager.CurrentTarget, "Elemental Rot (Fire)", stackQuantity);
            }
         }

         combatManager.ProcessAttack(combatManager.CalculateDamage(damagers), combatManager.CurrentTarget);

         combatManager.CurrentFighter.currentMana -= resource.manaCost;

         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget });
      }
   }
}
