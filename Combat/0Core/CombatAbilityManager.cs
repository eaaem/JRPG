using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class CombatAbilityManager : Node
{
	[Export]
   private CombatManager combatManager;
   [Export]
   private CombatUIManager uiManager;
   [Export]
   private PackedScene enemyAbilityUseHolder;
   
   [Signal]
   public delegate void AbilityCastEventHandler();
   [Signal]
   public delegate void EnemyAbilityCastEventHandler();

   public void PlayerCastAbility(Fighter target)
   {
      /*if (combatManager.CurrentAbility.hitsSelf && !combatManager.CurrentAbility.hitsAll && !combatManager.CurrentAbility.hitsSurrounding 
          && !combatManager.CurrentAbility.hitsTeam && target != combatManager.CurrentFighter)
      {
         return;
      }*/

      if (!combatManager.OverridePanelDownHiding)
      {
         for (int i = 0; i < combatManager.Fighters.Count; i++)
         {
            combatManager.Fighters[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }

         uiManager.HideAll();
         combatManager.CurrentFighter.currentMana -= combatManager.CurrentAbility.manaCost;
         uiManager.UpdatePartyMemberManaBar(combatManager.CurrentFighter);
      }

      combatManager.CurrentTarget = target;
   
      EmitSignal(SignalName.AbilityCast);
   }

   public void SelectEnemyAbility(List<AbilityResource> validAbilities)
   {
      combatManager.CurrentAbility = RollEnemyAbility(validAbilities);

      if (combatManager.CurrentAbility.hitsTeam && !combatManager.CurrentAbility.hitsSelf)
      {
         combatManager.CurrentTarget = combatManager.SelectEnemyTarget(true, false);
      }
      else if (combatManager.CurrentAbility.hitsTeam && combatManager.CurrentAbility.hitsSelf)
      {
         combatManager.CurrentTarget = combatManager.SelectEnemyTarget(true, true);
      }
      else if (!combatManager.CurrentAbility.hitsTeam && combatManager.CurrentAbility.hitsSelf)
      {
         combatManager.CurrentTarget = combatManager.CurrentFighter;
      }
      else
      {
         combatManager.CurrentTarget = combatManager.SelectEnemyTarget(false, false);
      }

      // Prevent enemies from casting abilities that try to reapply status effects (e.g. if an enemy is already stealthed from an ability, prevent it from trying to
      // stealth again)
      if (combatManager.CurrentAbility.notStackingEffectApplied != StatusEffect.None)
      {
         for (int i = 0; i < combatManager.CurrentTarget.currentStatuses.Count; i++)
         {
            if (combatManager.CurrentTarget.currentStatuses[i].effect == combatManager.CurrentAbility.notStackingEffectApplied)
            {
               validAbilities.Remove(combatManager.CurrentAbility);

               // No abilities left, so default to attacking
               if (validAbilities.Count == 0)
               {
                  Fighter target = combatManager.SelectEnemyTarget(false, false);
                  combatManager.CompleteAttack(target);
               }
               else // Abilities left, so reroll while excluding the invalid ability
               {
                  SelectEnemyAbility(validAbilities);
               }
               return;
            }
         }
      }

      EnemyCastAbility();
   }

   AbilityResource RollEnemyAbility(List<AbilityResource> validAbilities)
   {
      List<int> abilityChoices = new List<int>();
      int abilityWeight = 0;
      combatManager.CurrentAbility = null;

      for (int i = 0; i < validAbilities.Count; i++)
      {
         abilityChoices.Add(abilityWeight + validAbilities[i].abilityWeight);
         abilityWeight += validAbilities[i].abilityWeight;
      }

      int choice = GD.RandRange(0, abilityWeight);

      for (int i = 0; i < validAbilities.Count; i++)
      {
         if (abilityChoices[i] >= choice)
         {
            return validAbilities[i];
         }
      }

      // Fail-safe (if for some reason an ability wasn't chosen, default to the first one)
      GD.PrintErr("Enemy " + combatManager.CurrentFighter.fighterName + " could not select an ability; defaulting to first possible one");
      return validAbilities[0];
   }

   public void EnemyCastAbility()
   {
      // Here, a new node3D is added. It has the necessary script to execute the ability
      // Godot doesn't seem to execute instantiated scripts unless what it's being added to is instantiated itself, which is why this is so roundabout
      PackedScene packedSceneHolder = GD.Load<PackedScene>(enemyAbilityUseHolder.ResourcePath);
      Node3D holder = packedSceneHolder.Instantiate<Node3D>();
      Node3D scriptHolder = holder.GetNode<Node3D>("Holder");

      scriptHolder.SetScript(GD.Load<CSharpScript>(combatManager.CurrentAbility.scriptPath));
      combatManager.CurrentFighter.model.GetNode<EnemyDataHolder>("ScriptHolder").AddChild(holder);

      EmitSignal(SignalName.EnemyAbilityCast);

      combatManager.CurrentFighter.model.GetNode<EnemyDataHolder>("ScriptHolder").RemoveChild(holder);
      holder.QueueFree();

      combatManager.CurrentAbility = null;
   }

   public void CreateAbilityGraphicController(List<Fighter> targets, bool playHitAnimation, string overrideGraphicController = "")
   {
      string pathToUse;

      if (overrideGraphicController.Length > 0)
      {
         pathToUse = overrideGraphicController;
      }
      else
      {
         if (combatManager.CurrentAbility != null)
         {
            pathToUse = combatManager.CurrentAbility.graphicPath;
         }
         else
         {
            pathToUse = combatManager.CurrentItem.item.combatGraphicsPath;
         }
      }
      
      AbilityCommandInstance abilityGraphic = GD.Load<PackedScene>(pathToUse).Instantiate<AbilityCommandInstance>();
      AddChild(abilityGraphic);

      abilityGraphic.UpdateData(targets, playHitAnimation);
   }
}
