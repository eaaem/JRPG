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
      abilityContainer = GetNode<Panel>("/root/BaseNode/UI/Options/Abilities");
      selectionBox = GetNode<CanvasGroup>("/root/BaseNode/UI/Options");

      PlayerAbilityReadySetup();
   }

   public override void OnButtonDown()
   {
      SetTeamOnCast(button);
      finalizeButton.Visible = true;
      finalizeButton.Disabled = true;
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         cancelButton.Visible = true;
         selectionBox.Visible = true;
         abilityContainer.Visible = true;

         if (!currentTargets.Contains(combatManager.CurrentTarget) && currentTargets.Count < 3 && combatManager.CurrentTarget != combatManager.CurrentFighter)
         {
            currentTargets.Add(combatManager.CurrentTarget);
            finalizeButton.Disabled = false;
            combatManager.CurrentTarget.placementNode.GetChild<MeshInstance3D>(0).Visible = true;
         }
         else if (currentTargets.Contains(combatManager.CurrentTarget))
         {
            currentTargets.Remove(combatManager.CurrentTarget);
            combatManager.CurrentTarget.placementNode.GetChild<MeshInstance3D>(0).Visible = false;
            
            if (currentTargets.Count <= 0)
            {
               finalizeButton.Disabled = true;
            }
         }
      }
   }

   public void OnFinalizeDown()
   {
      combatManager.CurrentFighter.wasHit = true;
      currentTargets.Add(combatManager.CurrentFighter);

      for (int i = 0; i < currentTargets.Count; i++)
      {
         currentTargets[i].wasHit = true;
         
         if (!currentTargets[i].isDead)
         {
            currentTargets[i].currentHealth += Mathf.CeilToInt(currentTargets[i].maxHealth * 0.1f);
            
            for (int j = 0; j < currentTargets[i].currentStatuses.Count; j++)
            {
               if (currentTargets[i].currentStatuses[j].isCleanseable && currentTargets[i].currentStatuses[j].isNegative)
               {
                  combatManager.RemoveStatus(currentTargets[i], j);
               }
            }

            combatManager.ApplyStatus(100, currentTargets[i], StatusEffect.MegaBuff, 2, 2);
         }
         else
         {
            currentTargets[i].currentHealth = 1;
            currentTargets[i].isDead = false;
            currentTargets[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
         }
         
         currentTargets[i].placementNode.GetChild<MeshInstance3D>(0).Visible = false;
      }

      combatManager.CurrentFighter.currentMana -= resource.manaCost;
      combatManager.CurrentFighter.specialCooldown = 3;

      combatManager.RegularCast(currentTargets, false);
      abilityContainer.Visible = false;
      cancelButton.Visible = false;
      selectionBox.Visible = false;
      
      finalizeButton.Visible = false;
   }
}
