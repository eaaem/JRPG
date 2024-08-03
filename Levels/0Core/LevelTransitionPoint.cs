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
         managers.MenuManager.FadeToBlack();
         managers.Controller.DisableMovement = true;

         while (!managers.MenuManager.BlackScreenIsVisible)
         {
            await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         }

         managers.LevelManager.TransitionLevels(internalLevelName, levelName, spawnPoint);

         managers.MenuManager.FadeFromBlack();
      }
   }
}
