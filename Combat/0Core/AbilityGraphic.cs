using Godot;
using System;

public partial class AbilityGraphic : Node
{
   [Export]
   public float Duration { get; set; }
   [Export]
   public bool SnapToTarget { get; set; }
   [Export]
   public bool GenerateOnlyOnce { get; set; }
   [Export]
   public float VerticalOffset { get; set; }

	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
      await ToSignal(GetTree().CreateTimer(Duration), "timeout");
      GetNode<Node3D>("/root/BaseNode").RemoveChild(GetParent());
      GetParent().QueueFree();
	}
}
