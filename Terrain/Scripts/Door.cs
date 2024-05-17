using Godot;
using System;

public partial class Door : Node
{
   private bool isOpen;
   private bool isChangingStates;
   private AnimationPlayer animationPlayer;

   public override void _Ready()
   {
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
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
      }
      else
      {
         animationPlayer.Play("Open");
         isOpen = true;
      }

      await ToSignal(GetTree().CreateTimer(animationPlayer.CurrentAnimationLength), "timeout");
      isChangingStates = false;
   }
}
