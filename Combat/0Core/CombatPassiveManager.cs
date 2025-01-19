using Godot;
using System;
using System.Collections.Generic;

public partial class CombatPassiveManager : Node
{
   [Export]
   private CombatManager combatManager;
   [Export]
   private CombatUIManager uiManager;
   [Export]
   private CombatStackStatusManager stacksAndStatusManager;

   [Export]
   private PackedScene olrenSpecialArrowPrefab;
   private List<OlrenSpecialArrow> olrenSpecialArrows = new List<OlrenSpecialArrow>() {
      new OlrenSpecialArrow("None", 1, "No effect"),
      new OlrenSpecialArrow("Poison", 1, "Chance of applying poison for 2 to 4 turns")
   };
   private string currentOlrenSpecialArrow;

	public void ApplyPassives(Fighter affectedFighter)
   {
      if (affectedFighter.fighterName == "Vakthol")
      {
         // +100% damage at max health; +100% defense at 0 health
         float percentHealth = affectedFighter.currentHealth * 1f / affectedFighter.maxHealth;
         int strengthMod = (int)(percentHealth * affectedFighter.stats[(int)StatType.Strength].baseValue);
         int defenseMod = (int)((1f - percentHealth) * affectedFighter.stats[(int)StatType.Strength].baseValue);

         // Apply the stat modifiers if they aren't there
         if (affectedFighter.statModifiers.Count < 3)
         {
            combatManager.ApplyStatModifier(StatType.Strength, strengthMod, 9999, affectedFighter, affectedFighter, StatusEffect.None);
            combatManager.ApplyStatModifier(StatType.Fortitude, defenseMod, 9999, affectedFighter, affectedFighter, StatusEffect.None);
            combatManager.ApplyStatModifier(StatType.Willpower, defenseMod, 9999, affectedFighter, affectedFighter, StatusEffect.None);
            return;
         }

         // Vakthol's first, second, and third stat modifiers are always related to his passive
         affectedFighter.stats[(int)StatType.Strength].value -= affectedFighter.statModifiers[0].modifier;
         affectedFighter.statModifiers[0].modifier = strengthMod;
         affectedFighter.stats[(int)StatType.Strength].value += affectedFighter.statModifiers[0].modifier;

         affectedFighter.stats[(int)StatType.Fortitude].value -= affectedFighter.statModifiers[1].modifier;
         affectedFighter.stats[(int)StatType.Willpower].value -= affectedFighter.statModifiers[2].modifier;
         affectedFighter.statModifiers[1].modifier = defenseMod;
         affectedFighter.stats[(int)StatType.Fortitude].value += affectedFighter.statModifiers[1].modifier;
         affectedFighter.stats[(int)StatType.Willpower].value += affectedFighter.statModifiers[2].modifier;
      }
   }

   public bool ThalriaPassive(Fighter target)
   {
      if (combatManager.CurrentFighter.fighterName == "Thalria")
      {
         target.wasHit = true;
         return true;
      }

      return false;
   }

   public void OlrenPassive()
   {
      if (combatManager.CurrentFighter.fighterName == "Olren" && !combatManager.IsCompanionTurn)
      {
         currentOlrenSpecialArrow = "None";
         uiManager.ClearSecondaryOptions();
         LoadOlrenPassive();
      }
   }

   void LoadOlrenPassive()
   {
      //uiManager.MoveSecondaryOptionsToLeft();
      for (int i = 0; i < olrenSpecialArrows.Count; i++)
      {
         if (combatManager.CurrentFighter.level >= olrenSpecialArrows[i].requiredLevel)
         {
            Button arrowButton = olrenSpecialArrowPrefab.Instantiate<Button>();
            arrowButton.Text = olrenSpecialArrows[i].arrowName;
            arrowButton.TooltipText = olrenSpecialArrows[i].description;

            // Defaults to selecting "None"
            if (i == 0)
            {
               arrowButton.GetNode<Panel>("Highlight").Visible = true;
            }

            uiManager.AddOlrenArrow(arrowButton);
         }
      }
      uiManager.SetSecondaryOptionsVisible(true);
   }

   public void SetOlrenSpecialArrow(string arrowName)
   {
      currentOlrenSpecialArrow = arrowName;
      uiManager.SetOlrenSpecialArrowHighlights(arrowName);
   }
   
   public void ApplyOlrenPassive()
   {
      if (combatManager.CurrentTarget.wasHit && combatManager.CurrentFighter.fighterName == "Olren" && !combatManager.IsCompanionTurn)
      {
         if (currentOlrenSpecialArrow == "Poison")
         {
            stacksAndStatusManager.ApplyStatus(75, combatManager.CurrentTarget, StatusEffect.Poison, 2, 4);
         }
      }
   }

   public int AthliaPassive(int damage)
   {
      if (combatManager.CurrentFighter.fighterName == "Athlia")
      {
         return (int)(damage * 0.1f);
      }
         
      return damage;
   }
}

public partial class OlrenSpecialArrow
{
   public string arrowName;
   public int requiredLevel;
   public string description;

   public OlrenSpecialArrow(string arrowName, int requiredLevel, string description)
   {
      this.arrowName = arrowName;
      this.requiredLevel = requiredLevel;
      this.description = description;
   }
}