using Godot;
using System;

public partial class ButtonSounds : Node
{
	[Export]
   private AudioStreamPlayer hoverOverSound;
   [Export]
   private AudioStreamPlayer clickSound;

   public void OnHoverOver()
   {
      hoverOverSound.Play();
   }

   public void OnClick()
   {
      clickSound.Play();
   }
}
