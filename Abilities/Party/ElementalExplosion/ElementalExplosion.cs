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
         List<DamagingEntity> damagers = new List<DamagingEntity>();
         damagers.Add(new DamagingEntity(8, StatType.Intelligence, StatType.Willpower, DamageType.Magical));

         for (int i = 0; i < combatManager.CurrentTarget.stacks.Count; i++)
         {
            if (combatManager.CurrentTarget.stacks[i].stackName == "Elemental Rot (Fire)")
            {
               int stackQuantity = combatManager.CurrentTarget.stacks[i].quantity;
               stacksAndStatusManager.ApplyStatus(100, combatManager.CurrentTarget, StatusEffect.Burn, stackQuantity + 1, stackQuantity + 1);
               damagers.Add(new DamagingEntity(7 * stackQuantity, StatType.Intelligence, StatType.Willpower, DamageType.Fire));

               combatManager.CurrentTarget.model.AddChild(GD.Load<PackedScene>("res://Abilities/Party/ElementalExplosion/elemental_explosion_fire.tscn").Instantiate<AutoOneShotEffect>());
               combatManager.CurrentTarget.model.AddChild(GD.Load<PackedScene>("res://Abilities/Party/ElementalExplosion/fire_audio.tscn").Instantiate<DestructingTimedAudioPlayer>());

               stacksAndStatusManager.RemoveStack(combatManager.CurrentTarget, "Elemental Rot (Fire)", stackQuantity);
            }
         }

         combatManager.ProcessAttack(damagers, combatManager.CurrentTarget);

         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentTarget });
      }
   }
}
