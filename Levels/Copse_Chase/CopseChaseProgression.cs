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

   async void TriggerHowls(Node3D body)
   {
      progress = 2;

      Area3D area = GetNode<Area3D>("../HowlTrigger");

      GetParent().CallDeferred("remove_child", area);
      area.CallDeferred("queue_free");

      GetNode<AudioStreamPlayer3D>("../Howl1").Play();

      await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

      GetNode<AudioStreamPlayer3D>("../Howl2").Play();
      
      await ToSignal(GetTree().CreateTimer(0.7f), "timeout");

      GetNode<AudioStreamPlayer3D>("../Howl3").Play();
   }

   void MarkOtherOutThereCutscene()
   {
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene1").id.ToString()] = true;
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene2").id.ToString()] = true;

      if (GetParent().HasNode("cutscene1"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene1");
         GetParent().CallDeferred("remove_child", toDelete);
         toDelete.CallDeferred("queue_free");
      }

      if (GetParent().HasNode("cutscene2"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene2");
         GetParent().CallDeferred("remove_child", toDelete);
         toDelete.CallDeferred("queue_free");
      }
   }

   void MarkOtherTurnAroundCutscene()
   {
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene3").id.ToString()] = true;
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[GetNode<CutsceneTrigger>("../cutscene4").id.ToString()] = true;

      if (GetParent().HasNode("cutscene3"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene3");
         GetParent().CallDeferred("remove_child", toDelete);
         toDelete.CallDeferred("queue_free");
      }

      if (GetParent().HasNode("cutscene4"))
      {
         CutsceneTrigger toDelete = GetParent().GetNode<CutsceneTrigger>("cutscene4");
         GetParent().CallDeferred("remove_child", toDelete);
         toDelete.CallDeferred("queue_free");
      }
   }

   async void TriggerOneMonstrosity(Node3D body)
   {
      progress = 3;
      RemoveMonstrosityTriggers();

      ActorStatus actorStatus = new ActorStatus();
      actorStatus.moveSpeed = 4f;
      actorStatus.idleAnim = "StalkStop";
      actorStatus.walkAnim = "Stalk";

      GetNode<Actor>("../monstrosity1").Visible = true;
      GetNode<Actor>("../monstrosity1").MoveCharacter(actorStatus, new Vector3(135, 0.3f, -40f), true);

      await ToSignal(GetNode<Actor>("../monstrosity1"), Actor.SignalName.MoveIsFinished);

      await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

      actorStatus.moveSpeed = 8f;
      actorStatus.walkAnim = "CombatRun";

      GetNode<Actor>("../monstrosity1").MoveCharacter(actorStatus, new Vector3(87, 0.3f, -57f), true);

      await ToSignal(GetTree().CreateTimer(10f), "timeout");

      GetNode<Actor>("../monstrosity1").Visible = false;
   }

   async void TriggerTwoMonstrosity(Node3D body)
   {
      progress = 3;
      RemoveMonstrosityTriggers();

      ActorStatus actorStatus = new ActorStatus();
      actorStatus.moveSpeed = 4f;
      actorStatus.idleAnim = "StalkStop";
      actorStatus.walkAnim = "Stalk";

      GetNode<Actor>("../monstrosity2").Visible = true;
      GetNode<Actor>("../monstrosity2").MoveCharacter(actorStatus, new Vector3(-110, 1, -121f), true);

      await ToSignal(GetNode<Actor>("../monstrosity2"), Actor.SignalName.MoveIsFinished);

      await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

      actorStatus.moveSpeed = 8f;
      actorStatus.walkAnim = "CombatRun";

      GetNode<Actor>("../monstrosity2").MoveCharacter(actorStatus, new Vector3(-46f, 1f, -189f), true);

      await ToSignal(GetTree().CreateTimer(10f), "timeout");

      GetNode<Actor>("../monstrosity2").Visible = false;
   }

   void RemoveMonstrosityTriggers()
   {
      if (GetParent().HasNode("MonstrosityTrigger1"))
      {
         Area3D trigger1 = GetParent().GetNode<Area3D>("MonstrosityTrigger1");
         GetParent().CallDeferred("remove_child", trigger1);
         trigger1.CallDeferred("queue_free");
      }
      
      if (GetParent().HasNode("MonstrosityTrigger2"))
      {
         Area3D trigger2 = GetParent().GetNode<Area3D>("MonstrosityTrigger2");
         GetParent().CallDeferred("remove_child", trigger2);
         trigger2.CallDeferred("queue_free");
      }
   }
}
