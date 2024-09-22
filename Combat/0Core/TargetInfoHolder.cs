using Godot;
using System;
using System.Collections.Generic;

public partial class TargetInfoHolder : Panel
{
   private CombatManager combatManager;
   private CombatUIManager combatUIManager;

   private List<EffectInformation> effects = new List<EffectInformation>();

   private bool isDead;
   private bool isHovering;

   public override void _Ready()
	{
      combatUIManager = GetNode<CombatUIManager>("/root/BaseNode/CombatManager/UIManager");
      MouseEntered += () => combatUIManager.HoverOverInformation(ConstructInformation());
      MouseExited += combatUIManager.StopHoveringOverInformation;

      MouseEntered += HoverOver;
      MouseExited += ExitHover;
	}

   void HoverOver()
   {
      isHovering = true;
   }

   void ExitHover()
   {
      isHovering = false;
   }

	public void ClearInformation()
   {
      effects.Clear();
      UpdateInformation();
   }

   public void AddToInformation(string effectName, bool isStack, int quantity, bool displayTurns, string description)
   {
      // Check to see if the effect already exists. If it's a stack, update the quantity and exit; otherwise, exit and don't add another effect to the UI
      for (int i = 0; i < effects.Count; i++)
      {
         if (effects[i].effectName == effectName)
         {
            if (isStack)
            {
               effects[i].quantity = quantity;
            }

            UpdateInformation();
            
            return;
         }
      }
      
      effects.Add(new EffectInformation(effectName, isStack, quantity, displayTurns, description));
      UpdateInformation();
   }

   public void ChangeEffectQuantity(string effectName, int quantityToLose)
   {
      for (int i = 0; i < effects.Count; i++)
      {
         if (effects[i].effectName == effectName)
         {
            effects[i].quantity -= quantityToLose;

            if (effects[i].quantity <= 0)
            {
               effects.Remove(effects[i]);
            }

            UpdateInformation();
            
            return;
         }
      }
   }

   public void DecrementEffect(string effectName)
   {
      ChangeEffectQuantity(effectName, 1);
   }

   public void SetDeathStatus(bool death)
   {
      isDead = death;
      UpdateInformation();
   }

   void UpdateInformation()
   {
      if (isHovering)
      {
         combatUIManager.HoverOverInformation(ConstructInformation());
      }
   }

   string GenerateInformationPiece(EffectInformation effectInformation)
   {
      string result = effectInformation.effectName + " (";

      if (effectInformation.displayTurns)
      {
         result += effectInformation.quantity.ToString();

         if (effectInformation.isStack)
         {
            result += " stack";
         }
         else
         {
            result += " turn";
         }

         result += (effectInformation.quantity > 1 ? "s" : "") + (effectInformation.isStack ? "" : ": ");
      }

      if (!effectInformation.isStack)
      {
         result += effectInformation.description + ")";
      }

      return result;
   }

   string ConstructInformation()
   {
      if (effects.Count == 0)
      {
         if (!isDead)
         {
            return "No status effects or stacks.";
         }
         else
         {
            return "This character is dead.";
         }
      }

      string result = "";

      for (int i = 0; i < effects.Count; i++)
      {
         result += GenerateInformationPiece(effects[i]);

         if (i < effects.Count - 1)
         {
            result += ", ";
         }
      }

      return result;
   }
}

partial class EffectInformation
{
   public string effectName;
   public bool isStack;
   public int quantity;
   public bool displayTurns;
   public string description;

   public EffectInformation(string effectName, bool isStack, int quantity, bool displayTurns, string description)
   {
      this.effectName = effectName;
      this.isStack = isStack;
      this.quantity = quantity;
      this.displayTurns = displayTurns;
      this.description = description;
   }
}