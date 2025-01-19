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
         int regainAmount = (int)(BaseHealthGain * resource.item.specialModifier);
         combatManager.CurrentTarget.currentHealth += regainAmount;
         uiManager.ProjectDamageText(combatManager.CurrentTarget, regainAmount, DamageType.None, false, true);
         combatManager.RegularCast(new System.Collections.Generic.List<Fighter> { combatManager.CurrentTarget }, false );
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
