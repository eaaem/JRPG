using Godot;

public partial class DestructingTimedAudioPlayer : AudioStreamPlayer3D
{
	[Export]
   private float timeBeforePlay;

   public override void _Ready()
   {
      StartTimer();
   }

   async void StartTimer()
   {
      await ToSignal(GetTree().CreateTimer(timeBeforePlay), "timeout");
      Playing = true;
   }

   void OnFinish()
   {
      GetParent().RemoveChild(this);
      QueueFree();
   }
}
