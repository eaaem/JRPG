using Godot;
using System;

public partial class OlrenSpecialArrowHolder : Node
{
	private Button button;
   private CombatManager combatManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      button = GetParent<Button>();
      button.ButtonDown += OnButtonDown;
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");
	}

   void OnButtonDown()
   {
      combatManager.SetOlrenSpecialArrow(button.Text);
   }

   public override void _ExitTree()
   {
      button.ButtonDown -= OnButtonDown;
   }
}
