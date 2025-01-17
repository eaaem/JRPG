using Godot;
using System;

/// <summary>
/// Use with created nodes in ability commands. Note that this script DOES NOT delete the particle effect; it must be cleaned up elsewhere.
/// </summary>
public partial class TerminatingEffect : GpuParticles3D
{
   [Export]
   private float duration;
	
	public override void _Ready()
	{
      End();
	}

	async void End()
   {
      await ToSignal(GetTree().CreateTimer(duration), "timeout");
      Emitting = false;
   }
}
