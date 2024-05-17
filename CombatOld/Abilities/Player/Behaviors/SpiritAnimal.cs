using Godot;
using System;
using System.Collections.Generic;

public partial class SpiritAnimal : PlayerAbilityBehavior
{
   private Panel secondaryOptions;
   private VBoxContainer secondaryOptionsContainer;

   private Enemy currentData;

   private List<AnimalHolder> animalOptions = new List<AnimalHolder>()
   {
      new AnimalHolder("dire_wolf", "Dire Wolf", 1)
   };

   public override void _Ready()
   {
      secondaryOptions = GetNode<Panel>("/root/BaseNode/UI/Options/SecondaryOptions");
      secondaryOptionsContainer = secondaryOptions.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
      PlayerAbilityReadySetup();
   }

   public override void OnButtonDown()
   {
      SetEnemyTeamOnCast(button);

      secondaryOptions.Visible = true;
      combatManager.MoveSecondaryOptionsToRight();
      combatManager.ClearSecondaryOptions();
      InitializeAnimalOptions();
   }

   void InitializeAnimalOptions()
   {
      for (int i = 0; i < animalOptions.Count; i++)
      {
         if (animalOptions[i].requiredLevel > combatManager.CurrentFighter.level)
         {
            return;
         }

         PackedScene packedScene = GD.Load<PackedScene>("res://Combat/Abilities/UI/spirit_animal_button.tscn");
         Button button = packedScene.Instantiate<Button>();
         button.Text = animalOptions[i].buttonName;

         secondaryOptionsContainer.AddChild(button);
      }
   }

   public void GetAnimalSelection(string dataName)
   {
      for (int i = 0; i < secondaryOptionsContainer.GetChildCount(); i++)
      {
         if (secondaryOptionsContainer.GetChild<Button>(i).Text == dataName)
         {
            currentData = GD.Load<Enemy>("res://Combat/EnemyResources/Resources/" + animalOptions[i].dataName + ".tres");
         }
      }
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource && currentData != null)
      {
         combatManager.CreateCompanion(combatManager.CurrentFighter, currentData, 3);
         secondaryOptions.Visible = false;
         combatManager.CurrentFighter.currentMana -= resource.manaCost;
         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentFighter }, false);
      }
   }
   
	/*private CombatManager combatManager;
   private AbilityResource resource;
   private Button button;

   private Panel secondaryOptions;
   private VBoxContainer secondaryOptionsContainer;

   private Enemy currentData;

   private List<AnimalHolder> animalOptions = new List<AnimalHolder>()
   {
      new AnimalHolder("dire_wolf", "Dire Wolf", 1)
   };

   public override void _Ready()
   {
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");
      button = GetParent<Button>();
      button.ButtonDown += OnButtonDown;

      resource = button.GetNode<ResourceHolder>("ResourceHolder").abilityResource;

      secondaryOptions = GetNode<Panel>("/root/BaseNode/UI/Options/SecondaryOptions");
      secondaryOptionsContainer = secondaryOptions.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");

      combatManager.AbilityCast += OnCast;
   }

   public void OnButtonDown()
   {
      combatManager.currentAbility = resource;
      combatManager.UnlockFighters();
      combatManager.DisableOtherAbilities();
      secondaryOptions.Visible = true;
      combatManager.cancelButton.Visible = true;
      button.Disabled = true;
      combatManager.PointCameraAtParty();

      combatManager.MoveSecondaryOptionsToRight();
      combatManager.ClearSecondaryOptions();
      InitializeAnimalOptions();
   }

   void InitializeAnimalOptions()
   {
      for (int i = 0; i < animalOptions.Count; i++)
      {
         if (animalOptions[i].requiredLevel > combatManager.currentFighter.level)
         {
            return;
         }

         PackedScene packedScene = GD.Load<PackedScene>("res://Combat/Abilities/UI/spirit_animal_button.tscn");
         Button button = packedScene.Instantiate<Button>();
         button.Text = animalOptions[i].buttonName;

         secondaryOptionsContainer.AddChild(button);
      }
   }

   public void GetAnimalSelection(string dataName)
   {
      for (int i = 0; i < secondaryOptionsContainer.GetChildCount(); i++)
      {
         if (secondaryOptionsContainer.GetChild<Button>(i).Text == dataName)
         {
            currentData = GD.Load<Enemy>("res://Combat/EnemyResources/Resources/" + animalOptions[i].dataName + ".tres");
         }
      }
   }

   public void OnCast()
   {
      if (combatManager.currentAbility == resource && currentData != null)
      {
         combatManager.CreateCompanion(combatManager.currentFighter, currentData, 3);
         secondaryOptions.Visible = false;
         combatManager.currentFighter.currentMana -= resource.manaCost;
         combatManager.RegularCast(new List<Fighter>() { combatManager.currentFighter }, false);
      }
   }

   public override void _ExitTree()
   {
      combatManager.AbilityCast -= OnCast;
   }*/
}

public partial class AnimalHolder
{
   public string dataName;
   public string buttonName;
   public int requiredLevel;

   public AnimalHolder(string dataName, string buttonName, int requiredLevel)
   {
      this.dataName = dataName;
      this.buttonName = buttonName;
      this.requiredLevel = requiredLevel;
   }
}