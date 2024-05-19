using Godot;
using System;
using System.Collections.Generic;

public partial class Sweep : PlayerAbilityBehavior
{
   public override void _Ready()
   {
      /*combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");
      button = GetParent<Button>();
      button.ButtonDown += OnButtonDown;

      resource = button.GetNode<ResourceHolder>("ResourceHolder").abilityResource;

      combatManager.AbilityCast += OnCast;*/
   }

   public override void OnButtonDown()
   {
      /*combatManager.CurrentAbility = resource;
      combatManager.UnlockFighters();
      combatManager.DisableOtherAbilities();
      cancelButton.Visible = true;
      button.Disabled = true;*/
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         /*List<Fighter> targets = combatManager.GetSurrounding(combatManager.currentTarget);
         targets.Add(combatManager.currentTarget);
         int damage = combatManager.CalculateDamage(combatManager.currentFighter.level, StatType.Strength, StatType.Fortitude, DamageType.Physical);

         for (int i = 0; i < targets.Count; i++)
         {
            combatManager.ProcessAttack(damage, targets[i]);
         }
         
         combatManager.currentFighter.currentMana -= resource.manaCost;
         combatManager.CompleteTurn();*/
      }
   }

   public override void _ExitTree()
   {
     // combatManager.AbilityCast -= OnCast;
   }
}
