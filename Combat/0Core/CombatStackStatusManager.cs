using Godot;
using System.Collections.Generic;

public partial class CombatStackStatusManager : Node
{
   [Export]
   private CombatManager combatManager;
   [Export]
   private CombatUIManager uiManager;
   [Export]
   private Color[] statusColors = new Color[0];

   private List<Node3D> hiddenEffectGraphics = new List<Node3D>();

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
            uiManager.AlterEffectUI(target, statusEffect.ToString(), false, duration, combatManager.StatusDatas[(int)statusEffect].displayRemainingTurns,
                                    combatManager.StatusDatas[(int)statusEffect].description);
            AddStatusGraphic(target, statusEffect);

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
               for (int i = 0; i < 10; i++)
               {
                  combatManager.ApplyStatModifier((StatType)i, Mathf.CeilToInt(target.stats[i].baseValue * 0.25f), duration, target, 
                                                  combatManager.CurrentFighter, StatusEffect.MegaBuff);
               }
               target.maxHealth = target.GetMaxHealth();
               target.maxMana = target.GetMaxMana();
            }
            else if (statusEffect == StatusEffect.KeenEye)
            {
               combatManager.ApplyStatModifier(StatType.Accuracy, Mathf.CeilToInt(target.stats[(int)StatType.Accuracy].baseValue * 0.25f), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.KeenEye);
            }
            else if (statusEffect == StatusEffect.Stealth)
            {
               combatManager.ApplyStatModifier(StatType.Evasion, Mathf.CeilToInt(target.stats[(int)StatType.Evasion].baseValue * 2f), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.Stealth);
            }
            else if (statusEffect == StatusEffect.Taunting)
            {
               combatManager.ApplyStatModifier(StatType.Threat, Mathf.CeilToInt(target.stats[(int)StatType.Threat].baseValue), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.Taunting);
               combatManager.ApplyStatModifier(StatType.Fortitude, Mathf.CeilToInt(target.stats[(int)StatType.Fortitude].baseValue * -0.25f), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.Taunting);
               combatManager.ApplyStatModifier(StatType.Willpower, Mathf.CeilToInt(target.stats[(int)StatType.Willpower].baseValue * -0.25f), duration, 
                                               target, combatManager.CurrentFighter, StatusEffect.Taunting);

               for (int i = 0; i < combatManager.Fighters.Count; i++)
               {
                  if (!combatManager.Fighters[i].isEnemy && !combatManager.Fighters[i].isDead && combatManager.Fighters[i] != combatManager.CurrentFighter)
                  {
                     combatManager.ApplyStatModifier(StatType.Willpower, Mathf.CeilToInt(target.stats[(int)StatType.Willpower].baseValue * 0.25f), duration, 
                                                     combatManager.Fighters[i], combatManager.CurrentFighter, StatusEffect.Taunting);
                     combatManager.ApplyStatModifier(StatType.Fortitude, Mathf.CeilToInt(target.stats[(int)StatType.Fortitude].baseValue * 0.25f), duration, 
                                                     combatManager.Fighters[i], combatManager.CurrentFighter, StatusEffect.Taunting);
                  }
               }
            }
            else if (statusEffect == StatusEffect.Enraged)
            {
               combatManager.ApplyStatModifier(StatType.Strength, Mathf.CeilToInt(target.stats[(int)StatType.Strength].baseValue * 0.75f), duration,
                                               combatManager.CurrentTarget, combatManager.CurrentFighter, StatusEffect.Enraged);
               combatManager.ApplyStatModifier(StatType.Accuracy, -1 * Mathf.CeilToInt(target.stats[(int)StatType.Accuracy].baseValue * 0.5f), duration,
                                               combatManager.CurrentTarget, combatManager.CurrentFighter, StatusEffect.Enraged);       
               combatManager.ApplyStatModifier(StatType.Fortitude, -1 * Mathf.CeilToInt(target.stats[(int)StatType.Fortitude].baseValue * 0.25f), duration,
                                               combatManager.CurrentTarget, combatManager.CurrentFighter, StatusEffect.Enraged);            
               combatManager.ApplyStatModifier(StatType.Willpower, -1 * Mathf.CeilToInt(target.stats[(int)StatType.Willpower].baseValue * 0.25f), duration,
                                               combatManager.CurrentTarget, combatManager.CurrentFighter, StatusEffect.Enraged);                       
            }
         }
      }
   }

   public void IncrementAppliedStatuses()
   {
      for (int i = 0; i < combatManager.CurrentFighter.currentStatuses.Count; i++)
      {
         combatManager.CurrentFighter.currentStatuses[i].remainingTurns--;
         uiManager.DecrementEffectUI(combatManager.CurrentFighter, combatManager.CurrentFighter.currentStatuses[i].effect.ToString());

         if (combatManager.CurrentFighter.currentStatuses[i].remainingTurns <= 0)
         {
            RemoveStatus(combatManager.CurrentFighter, i);
         }
         else
         {
            if (combatManager.CurrentFighter.currentStatuses[i].effect == StatusEffect.Poison)
            {
               int damage = Mathf.Clamp((int)(combatManager.CurrentFighter.currentHealth * 0.05f), 1, 99999);
               combatManager.CurrentFighter.currentHealth -= damage;
               uiManager.UpdateSingularUIPanel(combatManager.CurrentFighter);
               uiManager.ProjectDamageText(combatManager.CurrentFighter, damage, statusColors[combatManager.StatusDatas[(int)StatusEffect.Poison].colorIndex]);
            }
            else if (combatManager.CurrentFighter.currentStatuses[i].effect == StatusEffect.Bleed)
            {
               int damage = Mathf.Clamp((int)(combatManager.CurrentFighter.maxHealth * 0.03f), 1, 99999);
               combatManager.CurrentFighter.currentHealth -= damage;
               uiManager.UpdateSingularUIPanel(combatManager.CurrentFighter);
               uiManager.ProjectDamageText(combatManager.CurrentFighter, damage, statusColors[combatManager.StatusDatas[(int)StatusEffect.Bleed].colorIndex]);
            }
            else if (combatManager.CurrentFighter.currentStatuses[i].effect == StatusEffect.Burn)
            {
               int damage = Mathf.Clamp((int)(combatManager.CurrentFighter.maxHealth * 0.02f), 1, 99999);
               combatManager.CurrentFighter.currentHealth -= damage;
               uiManager.UpdateSingularUIPanel(combatManager.CurrentFighter);
               uiManager.ProjectDamageText(combatManager.CurrentFighter, damage, statusColors[combatManager.StatusDatas[(int)StatusEffect.Burn].colorIndex]);
            }
         }
      }
   }

   public void AddStatusGraphic(Fighter target, StatusEffect statusEffect)
   {
      StatusData statusData = combatManager.StatusDatas[(int)statusEffect];

      Node3D graphic = GD.Load<PackedScene>(statusData.graphicEffectPath).Instantiate<Node3D>();

      if (statusData.boneToAttachForGraphic.Length > 0)
      {
         BoneAttachment3D boneAttachment = new BoneAttachment3D();
         AddChild(boneAttachment);
         boneAttachment.Visible = false;
         boneAttachment.Name = target.fighterName + statusEffect.ToString();
         boneAttachment.SetUseExternalSkeleton(true);
         boneAttachment.SetExternalSkeleton(target.model.GetNode<Node3D>("Model").GetChild(0).GetChild(0).GetPath());

         string boneToAttachTo = statusData.boneToAttachForGraphic;

         if (target.model.HasNode("BoneConversions"))
         {
            BoneConversionList boneConversionList = target.model.GetNode<BoneConversionList>("BoneConversions");

            for (int i = 0; i < boneConversionList.conversions.Length; i++)
            {
               if (boneConversionList.conversions[i].originalBone == statusData.boneToAttachForGraphic)
               {
                  boneToAttachTo = boneConversionList.conversions[i].overrideBone;
                  break;
               }
            }
         }

         boneAttachment.BoneName = boneToAttachTo;
         boneAttachment.AddChild(graphic);

         hiddenEffectGraphics.Add(boneAttachment);
      }
      
      if (statusData.materialEffectPath.Length > 0)
      {
         List<MeshInstance3D> meshes = combatManager.GetMeshes(target.model.GetNode<Node3D>("Model"));

         for (int i = 0; i < meshes.Count; i++)
         {
            meshes[i].MaterialOverlay = GD.Load<Material>(statusData.materialEffectPath);
         }
      }
   }

   public void ShowEffectGraphics()
   {
      for (int i = 0; i < hiddenEffectGraphics.Count; i++)
      {
         hiddenEffectGraphics[i].Visible = true;
      }

      hiddenEffectGraphics.Clear();
   }

   public void RemoveStatusGraphic(Fighter target, StatusEffect statusEffect)
   {
      foreach (BoneAttachment3D child in GetChildren())
      {
         if (child.Name == target.fighterName + statusEffect.ToString())
         {
            RemoveChild(child);
            child.QueueFree();
            break;
         }
      }

      List<MeshInstance3D> meshes = combatManager.GetMeshes(target.model.GetNode<Node3D>("Model"));

      for (int i = 0; i < meshes.Count; i++)
      {
         meshes[i].MaterialOverlay = null;
      }
   }

   public void RemoveStatus(Fighter target, int statusIndex)
   {
      AppliedStatusEffect status = target.currentStatuses[statusIndex];

      for (int i = 0; i < target.statModifiers.Count; i++)
      {
         if (target.statModifiers[i].attachedStatus == status.effect)
         {
            combatManager.RemoveStatModifier(target.statModifiers[i], target);
            i--;
         }
      }

      uiManager.ChangeEffectUIQuantity(target, target.currentStatuses[statusIndex].effect.ToString(), target.currentStatuses[statusIndex].remainingTurns);
      RemoveStatusGraphic(target, target.currentStatuses[statusIndex].effect);
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
            uiManager.AlterEffectUI(target, newStack.stackName, true, target.stacks[i].quantity, true, "");
            return;
         }
      }

      uiManager.AlterEffectUI(target, newStack.stackName, true, newStack.quantity, true, "");
      target.stacks.Add(newStack);
   }

   public void RemoveStack(Fighter target, string stackName, int quantityToLose)
   {
      for (int i = 0; i < target.stacks.Count; i++)
      {
         if (target.stacks[i].stackName == stackName)
         {
            target.stacks[i].quantity -= quantityToLose;
            uiManager.ChangeEffectUIQuantity(target, target.stacks[i].stackName, quantityToLose);
            
            return;
         }
      }
   }

   public void ClearStatuses(Fighter target)
   {
      int size = target.currentStatuses.Count - 1;

      for (int i = size; i >= 0; i--)
      {
         RemoveStatus(target, i);
      }
   }

   public void ClearStacks(Fighter target)
   {
      int size = target.stacks.Count - 1;

      for (int i = size; i >= 0; i--)
      {
         RemoveStack(target, target.stacks[i].stackName, target.stacks[i].quantity);
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