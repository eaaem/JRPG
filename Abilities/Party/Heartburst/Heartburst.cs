using Godot;
using System;
using System.Collections.Generic;

public partial class Heartburst : PlayerAbilityBehavior
{
   private List<Fighter> currentTargets = new List<Fighter>();

   private Button finalizeButton;

   private Panel abilityContainer;
   private CanvasGroup selectionBox;

   public override void _Ready()
   {
      finalizeButton = GetNode<Button>("/root/BaseNode/UI/Options/FinalizeButton");

      finalizeButton.ButtonDown += OnFinalizeDown;
      //abilityContainer = GetNode<Panel>("/root/BaseNode/UI/Options/Abilities");
     // selectionBox = GetNode<CanvasGroup>("/root/BaseNode/UI/Options");

      base._Ready();
   }

   public override void OnButtonDown()
   {
      SetTeamOnCast(button);
      finalizeButton.Visible = true;
      finalizeButton.Disabled = true;
      finalizeButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      combatManager.OverridePanelDownHiding = true;
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         if (!currentTargets.Contains(combatManager.CurrentTarget) && currentTargets.Count < 3 && combatManager.CurrentTarget != combatManager.CurrentFighter)
         {
            currentTargets.Add(combatManager.CurrentTarget);
            finalizeButton.Disabled = false;
            finalizeButton.MouseFilter = Control.MouseFilterEnum.Stop;
            combatManager.CurrentTarget.placementNode.GetNode<Decal>("SecondaryHighlight").Visible = true;
         }
         else if (currentTargets.Contains(combatManager.CurrentTarget))
         {
            currentTargets.Remove(combatManager.CurrentTarget);
            combatManager.CurrentTarget.placementNode.GetNode<Decal>("SecondaryHighlight").Visible = false;
            
            if (currentTargets.Count <= 0)
            {
               finalizeButton.Disabled = true;
               finalizeButton.MouseFilter = Control.MouseFilterEnum.Ignore;
            }
         }

         uiManager.ShowSelectionBox();
         uiManager.SetTargetsVisible(true);
         uiManager.SetCancelButtonVisible(true);
      }
   }

   public async void OnFinalizeDown()
   {
      combatManager.CurrentFighter.wasHit = true;
      currentTargets.Add(combatManager.CurrentFighter);

      List<Fighter> revivedFighters = new List<Fighter>();

      for (int i = 0; i < currentTargets.Count; i++)
      {
         currentTargets[i].wasHit = true;
         
         if (!currentTargets[i].isDead)
         {
            int healAmount = Mathf.CeilToInt(currentTargets[i].maxHealth * 0.2f);
            currentTargets[i].currentHealth += healAmount;
            
            for (int j = 0; j < currentTargets[i].currentStatuses.Count; j++)
            {
               if (currentTargets[i].currentStatuses[j].isCleanseable && currentTargets[i].currentStatuses[j].isNegative)
               {
                  stacksAndStatusManager.RemoveStatus(currentTargets[i], j);
               }
            }

            stacksAndStatusManager.ApplyStatus(100, currentTargets[i], StatusEffect.MegaBuff, 2, 2);
            uiManager.ProjectDamageText(currentTargets[i], healAmount, DamageType.None, false, true);
         }
         else
         {
            currentTargets[i].currentHealth = 1;
            currentTargets[i].isDead = false;
            revivedFighters.Add(currentTargets[i]);
            uiManager.ProjectDamageText(currentTargets[i], 1, DamageType.None, false, true);
         }
         
         currentTargets[i].placementNode.GetNode<Decal>("SecondaryHighlight").Visible = false;
      }

      combatManager.CurrentFighter.specialCooldown = 4;

      combatManager.RegularCast(currentTargets, false);
      finalizeButton.Visible = false;
      combatManager.OverridePanelDownHiding = false;
      uiManager.HideAll();
      uiManager.SetTargetsVisible(false);

      combatManager.CurrentFighter.currentMana -= combatManager.CurrentAbility.manaCost;

      await ToSignal(GetTree().CreateTimer(2.1f), "timeout");

      for (int i = 0; i < revivedFighters.Count; i++)
      {
         revivedFighters[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Revive", 0f);
         combatManager.ReviveFighter(revivedFighters[i]);
      }
   }
}
