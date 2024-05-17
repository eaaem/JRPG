using Godot;
using System;

public abstract partial class AbilityBehavior : Node
{
	protected CombatManager combatManager;

   protected AbilityResource resource;
   protected Button button;
   protected Button cancelButton;

   protected bool isEnemyAbility;

   public override void _Ready()
   {
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");

      if (!combatManager.CurrentFighter.isEnemy)
      {
         PlayerAbilityReadySetup();
      }
      else
      {
         EnemyAbilityReadySetup();
      }
   }

   public void PlayerAbilityReadySetup()
   {
      // This needs to be its own function for scripts that override the ready behavior (those scripts need to call this, or else they'll miss important behaviors)
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");

      cancelButton = GetNode<Button>("/root/BaseNode/UI/Options/CancelButton");
      button = GetParent<Button>();
      resource = button.GetNode<ResourceHolder>("ResourceHolder").abilityResource;
      button.ButtonDown += OnButtonDown;
      combatManager.AbilityCast += OnCast;
   }

   public void EnemyAbilityReadySetup()
   {
      combatManager.EnemyAbilityCast += OnEnemyCast;
   }

   public void SetTeamOnCast(Button buttonToDisable)
   {
      InitializeOnCast(buttonToDisable);
      combatManager.PointCameraAtParty();
   }

   public void SetEnemyTeamOnCast(Button buttonToDisable)
   {
      InitializeOnCast(buttonToDisable);
   }

   void InitializeOnCast(Button buttonToDisable)
   {
      combatManager.CurrentAbility = resource;
      combatManager.UnlockFighters();
      combatManager.DisableOtherAbilities();
      cancelButton.Visible = true;
      combatManager.EnablePanels();
      buttonToDisable.Disabled = true;
   }

   public abstract void OnButtonDown();
   public abstract void OnCast();
   public abstract void OnEnemyCast();

   public override void _ExitTree()
   {
      if (!combatManager.CurrentFighter.isEnemy)
      {
         combatManager.AbilityCast -= OnCast;
      }
      else
      {
         combatManager.EnemyAbilityCast -= OnEnemyCast;
      }
   }
}
