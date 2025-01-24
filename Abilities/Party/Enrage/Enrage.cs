using Godot;
using System;

public partial class Enrage : PlayerAbilityBehavior
{  
   public override void OnButtonDown()
   {
      SetTeamOnCast(button);
   }

   public async override void OnCast()
   {
      if (combatManager.CurrentAbility == resource)
      {
         combatManager.CurrentFighter.specialCooldown = 2;
         combatManager.CurrentFighter.wasHit = true;

         if (combatManager.CurrentFighter.specialActive)
         {
            for (int i = 0; i < combatManager.CurrentFighter.currentStatuses.Count; i++)
            {
               if (combatManager.CurrentFighter.currentStatuses[i].effect == StatusEffect.Enraged)
               {
                  stacksAndStatusManager.RemoveStatus(combatManager.CurrentFighter, i);
                  break;
               }
            }

            combatManager.CreateAudioOnFighter(combatManager.CurrentFighter, "res://Abilities/Party/Enrage/vakthol_enrage_end.wav");

            combatManager.CurrentFighter.specialActive = false;
         }
         else
         {
            stacksAndStatusManager.ApplyStatus(100, combatManager.CurrentFighter, StatusEffect.Enraged, 99999, 99999);
            combatManager.CurrentFighter.specialActive = true;
            AnimationPlayer animPlayer = combatManager.CurrentFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer");

            animPlayer.Play("ProvokeCast", 0f);
            await ToSignal(GetTree().CreateTimer(0.9f), "timeout");
            combatManager.CreateAudioOnFighter(combatManager.CurrentFighter, "res://Abilities/Party/Enrage/vakthol_enrage_activate.wav");
            GpuParticles3D activateEffect = GD.Load<PackedScene>("res://Abilities/Party/Enrage/enrage_initiate_effect.tscn").Instantiate<GpuParticles3D>();
            combatManager.CurrentFighter.model.AddChild(activateEffect);

            await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
            stacksAndStatusManager.ShowEffectGraphics();

            await ToSignal(GetTree().CreateTimer(1f), "timeout");

            animPlayer.Play("CombatIdle", 0f);
         }

         cancelButton.Visible = false;

         if (combatManager.CurrentFighter.model.HasNode("EnrageInitiateEffect"))
         {
            GpuParticles3D effect = combatManager.CurrentFighter.model.GetNode<GpuParticles3D>("EnrageInitiateEffect");
            combatManager.CurrentFighter.model.RemoveChild(effect);
            effect.QueueFree();
         }

         GetNode<Control>("/root/BaseNode/UI/Options").Visible = true;
         combatManager.CurrentTarget = null;
         combatManager.CurrentAbility = null;
         uiManager.SetChoicesVisible(true);
      }
   }
}
