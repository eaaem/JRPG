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
         managers.MenuManager.FadeToBlack();
         managers.Controller.DisableMovement = true;

         while (!managers.MenuManager.BlackScreenIsVisible)
         {
            await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         }

         managers.LevelManager.CallDeferred(nameof(managers.LevelManager.OpenWorldMap), mapName, Vector2.Zero, false, spawnLocation);
      }
   }
}
