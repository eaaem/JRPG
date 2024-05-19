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
      uiManager.MoveSecondaryOptionsToRight();
      uiManager.ClearSecondaryOptions();
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