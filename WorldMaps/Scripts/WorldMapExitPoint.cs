using Godot;
using System;

public partial class WorldMapExitPoint : Node
{
   [Export]
   private string mapName;

   private SaveMenuManager saveMenuManager;
   private LevelManager levelManager;

   public override void _Ready()
   {
      levelManager = GetNode<LevelManager>("/root/BaseNode/LevelManager");
      saveMenuManager = GetNode<SaveMenuManager>("/root/BaseNode/SaveManager");
   }

   public void OnBodyEntered(Node3D body)
   {
      if (body.Name == "Member1")
      {
         saveMenuManager.FadeToBlack();
         saveMenuManager.FadeFromBlack();
         levelManager.CallDeferred(nameof(levelManager.OpenWorldMap), mapName, Vector2.Zero, false);
      }
   }
}
