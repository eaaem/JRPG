using Godot;
using System;

public abstract partial class AbilityBehavior : Node
{
	protected CombatManager combatManager;
   protected CombatAbilityManager abilityManager;
   protected CombatUIManager uiManager;
   protected CombatStackStatusManager stacksAndStatusManager;

   protected AbilityResource resource;
   protected Button button;
   protected Button cancelButton;

   protected bool isEnemyAbility;

   public override void _Ready()
   {
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManager");
      abilityManager = combatManager.GetNode<CombatAbilityManager>("AbilityManager");
      uiManager = combatManager.GetNode<CombatUIManager>("UIManager");
      stacksAndStatusManager = combatManager.GetNode<CombatStackStatusManager>("StacksStatusManager");

      if (!combatManager.CurrentFighter.isEnemy)
      {
         PlayerAbilityReadySetup();
      }
      else
      {
         EnemyAbilityReadySetup();
      }
   }

   // This needs to be its own function for scripts that override the ready behavior (those scripts need to call this, or else they'll miss important behaviors)
   public void PlayerAbilityReadySetup()
   {
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManager");
      abilityManager = combatManager.GetNode<CombatAbilityManager>("AbilityManager");

      cancelButton = GetNode<Button>("/root/BaseNode/UI/Options/CancelButton");
      button = GetParent<Button>();
      resource = button.GetNode<ResourceHolder>("ResourceHolder").abilityResource;
      button.ButtonDown += OnButtonDown;
      abilityManager.AbilityCast += OnCast;
   }

   public void EnemyAbilityReadySetup()
   {
      abilityManager.EnemyAbilityCast += OnEnemyCast;
   }

   public void SetTeamOnCast(Button buttonToDisable)
   {
      InitializeOnCast(buttonToDisable);
      //combatManager.PointCameraAtParty();
   }

   public void SetEnemyTeamOnCast(Button buttonToDisable)
   {
      InitializeOnCast(buttonToDisable);
   }

   void InitializeOnCast(Button buttonToDisable)
   {
      combatManager.CurrentAbility = resource;
      //uiManager.DisableOtherAbilities();
      uiManager.GenerateTargets();
      uiManager.SetAbilityContainerVisible(false);
      cancelButton.Visible = true;
      //uiManager.EnablePanels();
      buttonToDisable.Disabled = true;
   }

   public abstract void OnButtonDown();
   public abstract void OnCast();
   public abstract void OnEnemyCast();

   public override void _ExitTree()
   {
      if (!combatManager.CurrentFighter.isEnemy)
      {
         abilityManager.AbilityCast -= OnCast;
      }
      else
      {
         abilityManager.EnemyAbilityCast -= OnEnemyCast;
      }
   }
}
