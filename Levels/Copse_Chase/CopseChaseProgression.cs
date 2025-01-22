using Godot;
using System;

public partial class CopseChaseProgression : LevelProgession
{
   public override void LoadLevel()
   {
      progress = managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].levelProgress;

      if (progress == 0)
      {
         GetNode<AudioStreamPlayer>("/root/BaseNode/MusicPlayer").StreamPaused = true;
      }
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
      enemy.MovementTarget = managers.Controller.GlobalPosition;
      progress = 1;
      GetNode<CombatManager>("/root/BaseNode/CombatManager").BattleEnd += EndCopseBattle;
   }

   void EndCopseBattle()
   {
      GetNode<CombatManager>("/root/BaseNode/CombatManager").BattleEnd -= EndCopseBattle;

      managers.DialogueManager.NextCutsceneDialogue();
   }
}
