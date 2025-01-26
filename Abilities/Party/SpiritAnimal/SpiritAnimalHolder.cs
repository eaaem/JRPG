using Godot;
using System;

public partial class SpiritAnimalHolder : Node
{
   private Button button;
   private SpiritAnimal spiritAnimalScript;
   private CombatManager combatManager;
   //private Panel abilityContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      button = GetParent<Button>();
      button.ButtonDown += OnButtonDown;
      button.ButtonDown += GetNode<ButtonSounds>("/root/BaseNode/AudioPlayers/ButtonSoundController").OnClick;
      button.MouseEntered += GetNode<ButtonSounds>("/root/BaseNode/AudioPlayers/ButtonSoundController").OnHoverOver;

      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManager");
      //abilityContainer = GetNode<Panel>("/root/BaseNode/UI/Options/AbilityContainer/GridContainer");
      spiritAnimalScript = GetNode<SpiritAnimal>("/root/BaseNode/UI/Options/AbilityContainer/GridContainer/Special/ScriptHolder");
	}

   void OnButtonDown()
   {
      GetNode<Panel>("Highlight").Visible = true;
      spiritAnimalScript.GetAnimalSelection(button.Text);
   }

   public override void _ExitTree()
   {
      button.ButtonDown -= OnButtonDown;
   }
}
