using Godot;
using System;

public partial class SpiritAnimalHolder : Node
{
   private Button button;
   private SpiritAnimal spiritAnimalScript;
   private CombatManager combatManager;
   private Panel abilityContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      button = GetParent<Button>();
      button.ButtonDown += OnButtonDown;
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");
      abilityContainer = GetNode<Panel>("/root/BaseNode/UI/Options/Abilities");
      spiritAnimalScript = abilityContainer.GetNode<SpiritAnimal>("Special/ScriptHolder");
	}

   void OnButtonDown()
   {
      spiritAnimalScript.GetAnimalSelection(button.Text);
   }

   public override void _ExitTree()
   {
      button.ButtonDown -= OnButtonDown;
   }
}
