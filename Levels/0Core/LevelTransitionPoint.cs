using Godot;
using System;

public partial class LevelTransitionPoint : Node
{
   [Export]
   private string internalLevelName;
   [Export]
   private string levelName;
   [Export]
   private string spawnPoint;

   private ManagerReferenceHolder managers;

   public override void _Ready()
   {
      managers = GetNode<ManagerReferenceHolder>("/root/BaseNode/ManagerReferenceHolder");
   }

	public async void OnBodyEntered(Node3D body)
   {
      if (body.Name == "Member1")
      {
         Tween tween = CreateTween();
         managers.MenuManager.FadeToBlack(tween);
         managers.Controller.DisableMovement = true;

         await ToSignal(tween, Tween.SignalName.Finished);

         managers.LevelManager.TransitionLevels(internalLevelName, levelName, spawnPoint);

         managers.MenuManager.FadeFromBlack();
      }
   }
}
