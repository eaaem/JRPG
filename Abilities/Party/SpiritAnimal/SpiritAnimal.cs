using Godot;
using System;
using System.Collections.Generic;

public partial class SpiritAnimal : PlayerAbilityBehavior
{
   private Panel secondaryOptions;
   private VBoxContainer secondaryOptionsContainer;

   private Enemy currentData;
   private int currentIndex = 0;

   private List<AnimalHolder> animalOptions = new List<AnimalHolder>()
   {
      new AnimalHolder("res://Combat/Enemies/DireWolf/dire_wolf.tres", "Dire Wolf", 1, "res://Abilities/Party/SpiritAnimal/dire_wolf_summon.wav")
   };

   public override void _Ready()
   {
      secondaryOptions = GetNode<Panel>("/root/BaseNode/UI/Options/SecondaryOptions");
      secondaryOptionsContainer = secondaryOptions.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
      base._Ready();
   }

   public override void OnButtonDown()
   {
      SetTeamOnCast(button);

      secondaryOptions.Visible = true;
      uiManager.ClearSecondaryOptions();
      InitializeAnimalOptions();
      currentData = GD.Load<Enemy>(animalOptions[0].dataName);

      secondaryOptionsContainer.GetChild<Button>(0).GetNode<Panel>("Highlight").Visible = true;
   }

   void InitializeAnimalOptions()
   {
      for (int i = 0; i < animalOptions.Count; i++)
      {
         if (animalOptions[i].requiredLevel > combatManager.CurrentFighter.level)
         {
            return;
         }

         PackedScene packedScene = GD.Load<PackedScene>("res://Abilities/Party/SpiritAnimal/spirit_animal_button.tscn");
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
            currentData = GD.Load<Enemy>(animalOptions[i].dataName);
            currentIndex = i;
         }
         else
         {
            secondaryOptionsContainer.GetChild<Button>(i).GetNode<Panel>("Highlight").Visible = false;
         }
      }
   }

   public override void OnCast()
   {
      if (combatManager.CurrentAbility == resource && currentData != null)
      {
         combatManager.CreateCompanion(combatManager.CurrentFighter, currentData, 3, GD.Load<Material>("res://Abilities/Party/SpiritAnimal/spirit_animal_material.tres"),
                                       "res://Abilities/Party/SpiritAnimal/spirit_animal_companion_effect.tscn", animalOptions[currentIndex].summonSoundPath);
         secondaryOptions.Visible = false;
         combatManager.CurrentFighter.specialCooldown = 3;
         combatManager.RegularCast(new List<Fighter>() { combatManager.CurrentFighter }, false);
      }
   }
}

public partial class AnimalHolder
{
   public string dataName;
   public string buttonName;
   public int requiredLevel;
   public string summonSoundPath;

   public AnimalHolder(string dataName, string buttonName, int requiredLevel, string summonSoundPath)
   {
      this.dataName = dataName;
      this.buttonName = buttonName;
      this.requiredLevel = requiredLevel;
      this.summonSoundPath = summonSoundPath;
   }
}