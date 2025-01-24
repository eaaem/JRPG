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

      if (progress >= 2)
      {
         Area3D area = GetNode<Area3D>("../HowlTrigger");

         GetParent().RemoveChild(area);
         area.QueueFree();
      }

      if (progress >= 3)
      {
         RemoveMonstrosityTriggers();
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

   async void TriggerHowls()
   {
      progress = 2;

      Area3D area = GetNode<Area3D>("../HowlTrigger");

      GetParent().RemoveChild(area);
      area.QueueFree();

      GetNode<AudioStreamPlayer>("../Howl1").Play();

      await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

      GetNode<AudioStreamPlayer>("../Howl2").Play();
      
      await ToSignal(GetTree().CreateTimer(0.7f), "timeout");

      GetNode<AudioStreamPlayer>("../Howl3").Play();
   }

   void MarkOtherOutThereCutscene()
   {
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene1").id.ToString()] = true;
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene2").id.ToString()] = true;

      if (GetParent().HasNode("cutscene1"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene1");
         GetParent().RemoveChild(toDelete);
         toDelete.QueueFree();
      }

      if (GetParent().HasNode("cutscene2"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene2");
         GetParent().RemoveChild(toDelete);
         toDelete.QueueFree();
      }
   }

   void MarkOtherTurnAroundCutscene()
   {
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene3").id.ToString()] = true;
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene4").id.ToString()] = true;

      if (GetParent().HasNode("cutscene3"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene3");
         GetParent().RemoveChild(toDelete);
         toDelete.QueueFree();
      }

      if (GetParent().HasNode("cutscene4"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene4");
         GetParent().RemoveChild(toDelete);
         toDelete.QueueFree();
      }
   }

   async void TriggerOneMonstrosity()
   {
      progress = 3;
      RemoveMonstrosityTriggers();

      ActorStatus actorStatus = new ActorStatus();
      actorStatus.moveSpeed = 1.5f;
      actorStatus.idleAnim = "StalkStop";
      actorStatus.walkAnim = "Stalk";

      GetNode<Actor>("../monstrosity1").Visible = true;
      GetNode<Actor>("../monstrosity1").MoveCharacter(actorStatus, new Vector3(174, 8, -154f), true);

      await ToSignal(GetNode<Actor>("../monstrosity1"), Actor.SignalName.MoveIsFinished);

      await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

      actorStatus.moveSpeed = 5f;
      actorStatus.walkAnim = "CombatRun";

      GetNode<Actor>("../monstrosity1").MoveCharacter(actorStatus, new Vector3(230f, 8, -139f), true);

      await ToSignal(GetTree().CreateTimer(5f), "timeout");

      GetNode<Actor>("../monstrosity1").Visible = false;
   }

   async void TriggerTwoMonstrosity()
   {
      progress = 3;
      RemoveMonstrosityTriggers();

      ActorStatus actorStatus = new ActorStatus();
      actorStatus.moveSpeed = 1.5f;
      actorStatus.idleAnim = "StalkStop";
      actorStatus.walkAnim = "Stalk";

      GetNode<Actor>("../monstrosity2").Visible = true;
      GetNode<Actor>("../monstrosity2").MoveCharacter(actorStatus, new Vector3(-110, 1, -121f), true);

      await ToSignal(GetNode<Actor>("../monstrosity2"), Actor.SignalName.MoveIsFinished);

      await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

      actorStatus.moveSpeed = 5f;
      actorStatus.walkAnim = "CombatRun";

      GetNode<Actor>("../monstrosity2").MoveCharacter(actorStatus, new Vector3(-46f, 1f, -189f), true);

      await ToSignal(GetTree().CreateTimer(5f), "timeout");

      GetNode<Actor>("../monstrosity2").Visible = false;
   }

   void RemoveMonstrosityTriggers()
   {
      if (GetParent().HasNode("MonstrosityTrigger1"))
      {
         Area3D trigger1 = GetParent().GetNode<Area3D>("MonstrosityTrigger1");
         GetParent().RemoveChild(trigger1);
         trigger1.QueueFree();
      }
      
      if (GetParent().HasNode("MonstrosityTrigger2"))
      {
         Area3D trigger2 = GetParent().GetNode<Area3D>("MonstrosityTrigger2");
         GetParent().RemoveChild(trigger2);
         trigger2.QueueFree();
      }
   }
}
