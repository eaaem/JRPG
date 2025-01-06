using Godot;
using System;

public partial class WorldMapExitPoint : Node
{
   [Export]
   private string mapName;
   [Export]
   private string spawnLocation;

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
         
         managers.LevelManager.CallDeferred(nameof(managers.LevelManager.OpenWorldMap), mapName, Vector3.Zero, false, spawnLocation);
      }
   }
}
