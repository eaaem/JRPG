using Godot;
using System;

public partial class CombatItemManager : Node
{
	[Export]
   private CombatManager combatManager;
   [Export]
   private CombatUIManager uiManager;

   [Signal]
   public delegate void ItemUseEventHandler();
   
   public void UseItem(Fighter target)
   {
      ItemResource item = combatManager.CurrentItem.item;
      if (item.hitsSelf && !item.hitsAll && !item.hitsSurrounding && !item.hitsTeam && target != combatManager.CurrentFighter)
      {
         return;
      }

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         combatManager.Fighters[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
      }
      
      combatManager.CurrentTarget = target;
      uiManager.SetItemListVisible(false);
      EmitSignal(SignalName.ItemUse);
      uiManager.UpdateItems(combatManager.CurrentItem);
      uiManager.EnableItems();
      combatManager.CurrentItem = null;
   }
}
