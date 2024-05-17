using Godot;
using System;

public partial class HealingPotion : ItemBehavior
{
   private const int BaseHealthGain = 15;

   public override void OnButtonDown()
   {
      if (combatManager.IsInCombat)
      {
         FriendlyItemInCombatUse();
      }
      else
      {
         OutOfCombatItemSelect();
      }
   }

   public override void OnItemUse()
   {
      if (combatManager.CurrentItem == resource) // Combat
      {
         combatManager.CurrentTarget.currentHealth += (int)(BaseHealthGain * resource.item.specialModifier);
         combatManager.ProcessValues();
         combatManager.UpdateSingularUIPanel(combatManager.CurrentTarget);
         combatManager.CurrentFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
         RegularInCombatUse(combatManager.CurrentTarget);
      }
   }

   public override void OnOutOfCombatItemUse()
   {
      if (itemMenuManager.currentItem.item == resource.item)
      {
         itemMenuManager.currentTarget.currentHealth += (int)(BaseHealthGain * resource.item.specialModifier);
      }
   }
}
