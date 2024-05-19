using Godot;
using System;

public partial class OlrenSpecialArrowHolder : Node
{
	private Button button;
   private CombatPassiveManager passiveManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      button = GetParent<Button>();
      button.ButtonDown += OnButtonDown;
      passiveManager = GetNode<CombatPassiveManager>("/root/BaseNode/CombatManagerObj/PassiveManager");
	}

   void OnButtonDown()
   {
      passiveManager.SetOlrenSpecialArrow(button.Text);
   }

   public override void _ExitTree()
   {
      button.ButtonDown -= OnButtonDown;
   }
}
