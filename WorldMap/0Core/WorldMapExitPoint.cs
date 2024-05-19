using Godot;
using System;

public partial class WorldMapExitPoint : Node
{
   [Export]
   private string mapName;
   [Export]
   private ManagerReferenceHolder managers;

   public void OnBodyEntered(Node3D body)
   {
      if (body.Name == "Member1")
      {
         managers.SaveManager.FadeToBlack();
         managers.SaveManager.FadeFromBlack();
         managers.LevelManager.CallDeferred(nameof(managers.LevelManager.OpenWorldMap), mapName, Vector2.Zero, false);
      }
   }
}
