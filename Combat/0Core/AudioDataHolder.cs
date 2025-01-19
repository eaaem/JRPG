using Godot;
using System;

public partial class AudioDataHolder : Node
{
	[Export]
   public string HitSoundPath { get; set; } = new string("");
   [Export]
   public string DeathSoundPath { get; set; } = new string("");
}
