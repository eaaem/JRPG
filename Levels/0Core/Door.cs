using Godot;
using System;

public partial class Door : Node
{
   private bool isOpen;
   private bool isChangingStates;
   private AnimationPlayer animationPlayer;
   private AudioStreamPlayer3D doorOpenSound;
   private AudioStreamPlayer3D doorCloseSound;

   public override void _Ready()
   {
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
      doorOpenSound = GetNode<AudioStreamPlayer3D>("DoorOpen");
      doorCloseSound = GetNode<AudioStreamPlayer3D>("DoorClose");
   }


   public async void ChangeDoorState()
   {
      if (isChangingStates)
      {
         return;
      }

      isChangingStates = true;
      if (isOpen)
      {
         animationPlayer.Play("Close");
         isOpen = false;
         doorCloseSound.Play();
      }
      else
      {
         animationPlayer.Play("Open");
         isOpen = true;
         doorOpenSound.Play();
      }

      await ToSignal(GetTree().CreateTimer(animationPlayer.CurrentAnimationLength), "timeout");
      isChangingStates = false;
   }

   void OnBodyEntered(Node3D body)
   {
      if (!isOpen)
      {
         ChangeDoorState();
      }
   }
}
