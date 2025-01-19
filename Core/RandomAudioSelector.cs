using Godot;
using System;

public partial class RandomAudioSelector : AudioStreamPlayer3D
{
	[Export]
   private string directoryLocation;
   [Export]
   private string startOfSoundName;
   [Export]
   private int numberOfTracks;

   public void PlayRandomAudio()
   {
      Stream = GD.Load<AudioStreamWav>(directoryLocation + "/" + startOfSoundName + GD.RandRange(1, numberOfTracks) + ".wav");
      Play();
   }
}
