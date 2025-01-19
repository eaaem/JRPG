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
         int regainAmount = (int)(BaseManaGain * resource.item.specialModifier);
         combatManager.CurrentTarget.currentMana += regainAmount;
         uiManager.ProjectDamageText(combatManager.CurrentTarget, regainAmount, DamageType.None, false, false, true);
         combatManager.RegularCast(new System.Collections.Generic.List<Fighter> { combatManager.CurrentTarget}, false );
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
