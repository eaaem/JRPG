using Godot;
using System;

public partial class ManaPotion : ItemBehavior
{
   private const int BaseManaGain = 5;

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
         combatManager.CurrentTarget.currentMana += (int)(BaseManaGain * resource.item.specialModifier);
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
         itemMenuManager.currentTarget.currentMana += (int)(BaseManaGain * resource.item.specialModifier);
      }
   }
}
