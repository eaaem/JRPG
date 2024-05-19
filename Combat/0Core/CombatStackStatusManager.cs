using Godot;
using System;

public partial class CombatStackStatusManager : Node
{
   [Export]
   private CombatManager combatManager;
   [Export]
   private CombatUIManager uiManager;

	public void ApplyStatus(int chance, Fighter target, StatusEffect statusEffect, int minTurn, int maxTurn)
   {
      if (target.wasHit)
      {
         for (int i = 0; i < target.currentStatuses.Count; i++)
         {
            if (target.currentStatuses[i].effect == statusEffect)
            {
               return; // Target already has the status, so don't try to reapply it
            }
         }

         int chosen = GD.RandRange(0, 99);

         if (chosen <= chance)
         {
            int duration = GD.RandRange(minTurn, maxTurn);

            AppliedStatusEffect appliedStatusEffect = new AppliedStatusEffect(statusEffect, duration, combatManager.CurrentFighter);
            appliedStatusEffect.isCleanseable = combatManager.StatusDatas[(int)statusEffect].isCleanseable;
            appliedStatusEffect.isNegative = combatManager.StatusDatas[(int)statusEffect].isNegative;
            
            target.currentStatuses.Add(appliedStatusEffect);
            uiManager.AddEffectUI(statusEffect, target, combatManager.StatusDatas[(int)statusEffect].description);

            if (statusEffect == StatusEffect.Burn)
            {
               combatManager.ApplyStatModifier(StatType.Strength, -1 * (int)(target.stats[2].baseValue * 0.1f), duration, target, 
                                               combatManager.CurrentFighter, StatusEffect.Burn);
            }
            else if (statusEffect == StatusEffect.Disease)
            {
               combatManager.ApplyStatModifier(StatType.Constitution, -1 * (int)(target.stats[2].baseValue * 0.15f), duration, target, 
                                               combatManager.CurrentFighter, StatusEffect.Disease);
               combatManager.ApplyStatModifier(StatType.Knowledge, -1 * (int)(target.stats[2].baseValue * 0.15f), duration, target, 
                                               combatManager.CurrentFighter, StatusEffect.Disease);
               target.maxHealth = target.GetMaxHealth();
               target.maxMana = target.GetMaxMana();
            }
            else if (statusEffect == StatusEffect.MegaBuff)
            {
               appliedStatusEffect.displayRemainingTurns = true;
               for (int i = 0; i < 10; i++)
               {
                  combatManager.ApplyStatModifier((StatType)i, Mathf.CeilToInt(target.stats[i].baseValue * 0.25f), duration, target, 
                                                  combatManager.CurrentFighter, StatusEffect.MegaBuff);
               }
               target.maxHealth = target.GetMaxHealth();
               target.maxMana = target.GetMaxMana();
            }
            else if (statusEffect == StatusEffect.Birdseye)
            {
               appliedStatusEffect.displayRemainingTurns = true;
               combatManager.ApplyStatModifier(StatType.Accuracy, Mathf.CeilToInt(target.stats[(int)StatType.Accuracy].baseValue * 0.25f), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.Birdseye);
            }
            else if (statusEffect == StatusEffect.Stealth)
            {
               appliedStatusEffect.displayRemainingTurns = true;
               combatManager.ApplyStatModifier(StatType.Evasion, Mathf.CeilToInt(target.stats[(int)StatType.Evasion].baseValue * 0.75f), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.Stealth);
            }
         }
      }
   }

   public void IncrementAppliedStatuses()
   {
      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         for (int j = 0; j < combatManager.Fighters[i].currentStatuses.Count; j++)
         {
            if (combatManager.Fighters[i].currentStatuses[j].applier == combatManager.CurrentFighter)
            {
               combatManager.Fighters[i].currentStatuses[j].remainingTurns--;

               if (combatManager.Fighters[i].currentStatuses[j].displayRemainingTurns)
               {
                  uiManager.UpdateStatusTurns(combatManager.Fighters[i], combatManager.Fighters[i].currentStatuses[j], j);
               }

               if (combatManager.Fighters[i].currentStatuses[j].remainingTurns <= 0)
               {
                  RemoveStatus(combatManager.Fighters[i], j);
               }
               else
               {
                  if (combatManager.Fighters[i].currentStatuses[j].effect == StatusEffect.Poison)
                  {
                     combatManager.Fighters[i].currentHealth -= (int)(combatManager.Fighters[i].currentHealth * 0.05f);
                     uiManager.UpdateSingularUIPanel(combatManager.Fighters[i]);
                  }
                  else if (combatManager.Fighters[i].currentStatuses[j].effect == StatusEffect.Bleed)
                  {
                     combatManager.Fighters[i].currentHealth -= (int)(combatManager.Fighters[i].maxHealth * 0.03f);
                     uiManager.UpdateSingularUIPanel(combatManager.Fighters[i]);
                  }
                  else if (combatManager.Fighters[i].currentStatuses[j].effect == StatusEffect.Burn)
                  {
                     combatManager.Fighters[i].currentHealth -= (int)(combatManager.Fighters[i].maxHealth * 0.02f);
                     uiManager.UpdateSingularUIPanel(combatManager.Fighters[i]);
                  }
               }
            }
         }
      }
   }

   public void RemoveStatus(Fighter target, int statusIndex)
   {
      AppliedStatusEffect status = target.currentStatuses[statusIndex];
      uiManager.RemoveEffectUI(status.effect, target);

      for (int i = 0; i < target.statModifiers.Count; i++)
      {
         if (target.statModifiers[i].attachedStatus == status.effect)
         {
            target.statModifiers.Remove(target.statModifiers[i]);
            i--;
         }
      }

      target.currentStatuses.Remove(target.currentStatuses[statusIndex]);
   }

   public void AddStack(Fighter target, Stack newStack)
   {
      for (int i = 0; i < target.stacks.Count; i++)
      {
         if (target.stacks[i].stackName == newStack.stackName)
         {
            target.stacks[i].quantity += newStack.quantity;
            target.stacks[i].needsQuantityUpdate = true;
            return;
         }
      }

      uiManager.AddStackUI(target, newStack);
      target.stacks.Add(newStack);
   }

   public void RemoveStack(Fighter target, string stackName, int quantityToLose)
   {
      HBoxContainer stacksContainer = target.UIPanel.GetNode<HBoxContainer>("Effects/Stacks");

      for (int i = 0; i < target.stacks.Count; i++)
      {
         if (target.stacks[i].stackName == stackName)
         {
            Panel child = stacksContainer.GetChild<Panel>(i);
            target.stacks[i].quantity -= quantityToLose;
            target.stacks[i].needsQuantityUpdate = true;
            
            return;
         }
      }
   }
}

public partial class AppliedStatusEffect
{
   public StatusEffect effect;
   public int remainingTurns;
   public Fighter applier;
   public bool displayRemainingTurns;
   public bool isCleanseable;
   public bool isNegative;

   public AppliedStatusEffect(StatusEffect effect, int remainingTurns, Fighter applier)
   {
      this.effect = effect;
      this.remainingTurns = remainingTurns;
      this.applier = applier;
   }
}

public partial class Stack
{
   public string stackName;
   public int quantity;
   public string spriteName;
   public bool needsQuantityUpdate;
   
   public Stack(string stackName, int quantity, string spriteName)
   {
      this.stackName = stackName;
      this.quantity = quantity;
      this.spriteName = spriteName;
   }
}