using Godot;
using System;

public partial class Enrage : PlayerAbilityBehavior
{  
   CanvasGroup selectionBox;

   public override void OnButtonDown()
   {
      SetTeamOnCast(button);
   }

   public async override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         combatManager.CurrentFighter.specialCooldown = 2;

         if (combatManager.CurrentFighter.specialActive)
         {
            combatManager.CurrentFighter.stats[(int)StatType.Strength].value -= (int)(0.5f * combatManager.CurrentFighter.stats[(int)StatType.Strength].baseValue);
            combatManager.CurrentFighter.stats[(int)StatType.Accuracy].value += (int)(0.5f * combatManager.CurrentFighter.stats[(int)StatType.Accuracy].baseValue);
            combatManager.CurrentFighter.stats[(int)StatType.Fortitude].value += (int)(0.25f * combatManager.CurrentFighter.stats[(int)StatType.Fortitude].baseValue);
            combatManager.CurrentFighter.stats[(int)StatType.Willpower].value += (int)(0.25f * combatManager.CurrentFighter.stats[(int)StatType.Fortitude].baseValue);
            combatManager.CurrentFighter.specialActive = false;
         }
         else
         {
            combatManager.CurrentFighter.stats[(int)StatType.Strength].value += (int)(0.5f * combatManager.CurrentFighter.stats[(int)StatType.Strength].baseValue);
            combatManager.CurrentFighter.stats[(int)StatType.Accuracy].value -= (int)(0.5f * combatManager.CurrentFighter.stats[(int)StatType.Accuracy].baseValue);
            combatManager.CurrentFighter.stats[(int)StatType.Fortitude].value -= (int)(0.25f * combatManager.CurrentFighter.stats[(int)StatType.Fortitude].baseValue);
            combatManager.CurrentFighter.stats[(int)StatType.Willpower].value -= (int)(0.25f * combatManager.CurrentFighter.stats[(int)StatType.Fortitude].baseValue);
            combatManager.CurrentFighter.specialActive = true;
         }

         AnimationPlayer animPlayer = combatManager.CurrentFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer");

         animPlayer.Play("SelfCast");
         await ToSignal(GetTree().CreateTimer(animPlayer.CurrentAnimationLength), "timeout");
         animPlayer.Play("CombatActive");

         cancelButton.Visible = false;

         combatManager.FocusCameraOnFighter(combatManager.CurrentFighter.model);
         selectionBox = GetNode<CanvasGroup>("/root/BaseNode/UI/Options");
         selectionBox.Visible = true;
         uiManager.EnableOptions();
         combatManager.CurrentAbility = null;
      }
   }
}
