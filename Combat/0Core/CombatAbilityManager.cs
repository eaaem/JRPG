using Godot;
using System;
using System.Collections.Generic;

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
      if (combatManager.CurrentAbility.hitsSelf && !combatManager.CurrentAbility.hitsAll && !combatManager.CurrentAbility.hitsSurrounding 
          && !combatManager.CurrentAbility.hitsTeam && target != combatManager.CurrentFighter)
      {
         return;
      }

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         combatManager.Fighters[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
      }

      combatManager.CurrentTarget = target;
      uiManager.HideAll();
      combatManager.CurrentFighter.currentMana -= combatManager.CurrentAbility.manaCost;
      uiManager.UpdatePartyMemberManaBar(combatManager.CurrentFighter);
      EmitSignal(SignalName.AbilityCast);
   }

   public void SelectEnemyAbility(Fighter target, List<AbilityResource> validAbilities)
   {
      AbilityResource chosenAbility = validAbilities[GD.RandRange(0, validAbilities.Count - 1)];
      combatManager.CurrentAbility = chosenAbility;

      if (combatManager.CurrentAbility.hitsTeam && !combatManager.CurrentAbility.hitsSelf)
      {
         target = combatManager.SelectEnemyTarget(true, false);
      }
      else if (combatManager.CurrentAbility.hitsTeam && combatManager.CurrentAbility.hitsSelf)
      {
         target = combatManager.SelectEnemyTarget(true, true);
      }
      else if (!combatManager.CurrentAbility.hitsTeam && combatManager.CurrentAbility.hitsSelf)
      {
         target = combatManager.CurrentFighter;
      }

      combatManager.CurrentTarget = target;
      EnemyCastAbility();
   }

   void EnemyCastAbility()
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

   public void CreateAbilityGraphicController(List<Fighter> targets, bool playHitAnimation)
   {
      AbilityCommandInstance abilityGraphic = GD.Load<PackedScene>(combatManager.CurrentAbility.graphicPath).Instantiate<AbilityCommandInstance>();
      AddChild(abilityGraphic);

      abilityGraphic.UpdateData(targets, playHitAnimation);
   }
}
