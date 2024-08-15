using Godot;
using System;

public partial class CopseChaseProgression : LevelProgession
{
   public override void LoadLevel()
   {
      progress = managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].levelProgress;
   }

   public void SpawnEnemy()
   {
      WorldEnemy enemy = GetNode<WorldEnemy>("../Chunks/1/WorldEnemy");
      enemy.Visible = true;
   }

   public void ActivateEnemy()
   {
      WorldEnemy enemy = GetNode<WorldEnemy>("../Chunks/1/WorldEnemy");
      enemy.isStaticEnemy = false;
   }
}
