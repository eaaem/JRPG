using Godot;
using System;

public partial class AutoOneShotEffect : GpuParticles3D
{
   [Export]
   private float timeBeforeEmit = 0f;

	public override void _Ready()
	{
      Emitting = false;
      ShowEffect();
	}

   async void ShowEffect()
   {
      await ToSignal(GetTree().CreateTimer(timeBeforeEmit), "timeout");
      Emitting = true;

      await ToSignal(GetTree().CreateTimer(10f), "timeout");
      if (IsInsideTree())
      {
         GetParent().RemoveChild(this);
         QueueFree();
      }
   }
}
