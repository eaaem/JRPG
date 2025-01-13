using Godot;
using System;

public partial class TheralinMapProgression : MapProgression
{
   public override void LoadLevel()
   {
      progress = managers.LevelManager.MapDatas[managers.LevelManager.ActiveMapDataID].progress;

      if (progress == 1)
      {
         GetNode<CollisionShape3D>("../ChaseExit/CollisionShape3D").Disabled = false;
      }
   }
}
