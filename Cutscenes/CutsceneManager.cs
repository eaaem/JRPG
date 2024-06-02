using Godot;
using System;
using System.Collections.Generic;

/*
Here is the cutscene language:
skip : does nothing (used as filler in case that dialogue needs no action)
actor_name-move-x,y,z : move actor_name to point x,y,z
actor_name-rot-y : rotate y to match given rotation (in degrees)
actor_name-quick_rot-y : rotate y to match given rotation immediately
hide_speech : hides dialogue
show_speech : shows dialogue
lock_speech : prevent user from inputting dialogue
unlock_speech : resume user control over speech
speak_next : continue dialogue
actor_name-set_idle-name : sets idle animation to given name
actor_name-set_walk-name : sets walk animation to given name
pause-# : pause for # seconds AT THAT POINT IN THE COMMANDS
actor_name-place-x,y,z : place actor_name at point x,y,z
actor_name-play_anim-anim_name : force actor to play animation; actor returns to idle afterward
actor_name-hide_weapon : hides weapon of actor
actor_name-show_weapon : shows weapon of actor

The 0th actor behavior occurs BEFORE the cutscene loads, allowing for placements.
Actor behaviors always execute BEFORE the corresponding dialogue of that index.
So actor behavior 1 fires after dialogue 0, actor behavior 2 fires after dialogue 1, etc.
*/

/// <summary>
/// Manages in-game cutscenes.
/// </summary>
public partial class CutsceneManager : Node
{
   [Export]
   private ManagerReferenceHolder managers;
   [Export]
   private Camera3D playerCamera;
   [Export]
   private Camera3D cutsceneCamera;

   CutsceneObject currentCutsceneObject;
   int currentID;

   float totalWaitDuration = 0f;

   bool isCutsceneActive;

   public async void InitiateCutscene(CutsceneObject cutsceneObject, int id)
   {
      if (!isCutsceneActive)
      {
         isCutsceneActive = true;

         currentCutsceneObject = cutsceneObject;
         currentID = id;

         managers.Controller.DisableMovement = true;

         GetActors(!cutsceneObject.hideParty);
         GetNode<Node3D>("/root/BaseNode/PartyMembers").Visible = false;
         managers.Controller.isInCutscene = true;

         if (cutsceneObject.hideParty)
         {
            cutsceneCamera.Position = cutsceneObject.cameraPosition;
            cutsceneCamera.Rotation = cutsceneObject.cameraRotation;
            cutsceneCamera.MakeCurrent();

            managers.Controller.DisableCamera = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;

            managers.SaveManager.FadeToBlack();
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            managers.SaveManager.FadeFromBlack();
         }

         for (int i = 0; i < currentCutsceneObject.PreCutsceneCommands.Length; i++)
         {
            ParseCommand(currentCutsceneObject.PreCutsceneCommands[i]);

            if (totalWaitDuration > 0f)
            {
               await ToSignal(GetTree().CreateTimer(totalWaitDuration), "timeout");
               totalWaitDuration = 0f;
            }
         }

         if (cutsceneObject.items.Length > 0)
         {
            DialogueInteraction dialogueInteraction = new DialogueInteraction();
            dialogueInteraction.dialogueList = new DialogueList();
            dialogueInteraction.dialogueList.dialogues = new DialogueObject[cutsceneObject.items.Length];

            for (int i = 0; i < cutsceneObject.items.Length; i++)
            {
               dialogueInteraction.dialogueList.dialogues[i] = cutsceneObject.items[i].dialogue;
            }

            managers.DialogueManager.InitiateDialogue(dialogueInteraction, true);
         }

         ProgressCutscene(0);
      }
   }

   /// <summary>
   /// Progresses the cutscene, executing actor commands as necessary.
   /// <br></br><br></br>
   /// index : the corresponding dialogue index
   /// </summary>
   public async void ProgressCutscene(int index)
   {
      ActorCommand[] commands = currentCutsceneObject.items[index].commands;

      for (int i = 0; i < commands.Length; i++)
      {
         ParseCommand(commands[i]);

         if (totalWaitDuration > 0f)
         {
            await ToSignal(GetTree().CreateTimer(totalWaitDuration), "timeout");
            totalWaitDuration = 0f;
         }
      }
   }

   /// <summary>
   /// Parses the given command and takes action accordingly.
   /// </summary>
   void ParseCommand(ActorCommand command)
   {
      Actor actor;
      switch (command.CommandType)
      {
      case CommandType.None:
         GD.Print("Empty cutscene command found; skipping");
         break;
      case CommandType.Move:
         actor = GetActor(command.ActorName);
         actor.MoveCharacter(GetActorStatus(command.ActorName), command.Destination, command.RotateToFace);
         break;
      case CommandType.Rotate:
         actor = GetActor(command.ActorName);
         actor.RotateCharacter(command.YRotation);
         break;
      case CommandType.QuickRotate:
         actor = GetActor(command.ActorName);
         actor.RotateCharacterInstantly(command.YRotation);
         break;
      case CommandType.ChangeDialogueVisibility:
         managers.DialogueManager.DialogueContainer.Visible = !command.Hide;
         break;
      case CommandType.ChangeWeaponVisibility:
         actor = GetActor(command.ActorName);
         if (command.Hide)
         {
            actor.HideWeapon();
         }
         else
         {
            actor.ShowWeapon();
         }
         break;
      case CommandType.ChangeDialogueLock:
         managers.DialogueManager.LockInput = command.MakeLocked;
         break;
      case CommandType.SpeakNext:
         managers.DialogueManager.NextCutsceneDialogue();
         break;
      case CommandType.SetIdleAnimation:
         actor = GetActor(command.ActorName);
         actor.SetAnimation(true, GetActorStatus(command.ActorName), command.AnimationName);
         break;
      case CommandType.SetWalkAnimation:
         actor = GetActor(command.ActorName);
         actor.SetAnimation(false, GetActorStatus(command.ActorName), command.AnimationName);
         break;
      case CommandType.Pause:
         totalWaitDuration += command.Pause;
         break;
      case CommandType.Place:
         actor = GetActor(command.ActorName);
         actor.PlaceCharacterAtPoint(command.Destination);
         break;
      case CommandType.PlayAnimation:
         actor = GetActor(command.ActorName);
         actor.PlayAnimation(command.AnimationName, GetActorStatus(command.ActorName));
         break;
      default:
         GD.Print("Cutscene command invalid.");
         break;
      }
   }

   public void EndCutscene()
   {
      if (currentCutsceneObject.hideParty)
      {
         managers.SaveManager.FadeToBlack();
      }
      
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[currentID.ToString()] = true;

      for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
      {
         CharacterBody3D actor = GetNode<CharacterBody3D>("/root/BaseNode/" + currentCutsceneObject.actors[i].actorName);

         if (!currentCutsceneObject.hideParty)
         {
            for (int j = 0; j < managers.PartyManager.Party.Count; j++)
            {
               if (managers.PartyManager.Party[i].characterType.ToString() == currentCutsceneObject.actors[i].tiedMember)
               {
                  managers.PartyManager.Party[j].model.GlobalPosition = actor.GlobalPosition;
                  managers.PartyManager.Party[j].model.GlobalRotation = actor.GlobalRotation;

                  if (j == 0)
                  {
                     Node3D cameraHolder = actor.GetNode<Node3D>("CameraTarget");
                     actor.RemoveChild(cameraHolder);
                     managers.PartyManager.Party[0].model.AddChild(cameraHolder);
                  }

                  break;
               }
            }
         }

         managers.Controller.isInCutscene = false;
         managers.Controller.GetNode<Node3D>("CameraTarget").RotateY(-managers.Controller.GetNode<Node3D>("CameraTarget").Rotation.Y);

         GetNode<Node3D>("/root/BaseNode/").RemoveChild(actor);
         actor.QueueFree();
      }

      GetNode<Node3D>("/root/BaseNode/PartyMembers").Visible = true;
      playerCamera.MakeCurrent();
      isCutsceneActive = false;

      if (currentCutsceneObject.hideParty)
      {
         managers.SaveManager.FadeFromBlack();
      }
   }

   Actor GetActor(string actorName)
   {
      return GetNode<Actor>("/root/BaseNode/" + actorName);
   }

   ActorStatus GetActorStatus(string actorName)
   {
      for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
      {
         if (currentCutsceneObject.actors[i].actorName == actorName)
         {
            return currentCutsceneObject.actors[i];
         }
      }

      return null;
   }

   public void CutsceneSignalReceiver(string cutsceneName)
   {
      CutsceneTrigger trigger = (CutsceneTrigger)GetNode<Node3D>("/root/BaseNode/Level").FindChild(cutsceneName);
      InitiateCutscene(trigger.cutsceneObject, trigger.id);
   }

   /// <summary>
   /// Initializes all actors and adds them to the world.
   /// <br></br><br></br>
   /// isTied signals whether the party members are tied to their actors, i.e. the actors should be initialized to the same position and rotation as the party.
   /// </summary>
   void GetActors(bool isTied)
   {
      for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
      {
         PackedScene currentActorScene = GD.Load<PackedScene>("res://Cutscenes/Actors/" + currentCutsceneObject.actors[i].actorName + ".tscn");
         CharacterBody3D actor = currentActorScene.Instantiate<CharacterBody3D>();
         actor.Name = currentCutsceneObject.actors[i].actorName;
         actor.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play(currentCutsceneObject.actors[i].idleAnim);
         GetNode<Node3D>("/root/BaseNode/").AddChild(actor);

         if (isTied)
         {
            for (int j = 0; j < managers.PartyManager.Party.Count; j++)
            {
               if (managers.PartyManager.Party[j].characterType.ToString() == currentCutsceneObject.actors[i].tiedMember)
               {
                  actor.GlobalPosition = managers.PartyManager.Party[j].model.GlobalPosition;
                  actor.GlobalRotation = managers.PartyManager.Party[j].model.GlobalRotation;

                  if (j == 0)
                  {
                     Node3D cameraHolder = managers.PartyManager.Party[0].model.GetNode<Node3D>("CameraTarget");
                     managers.PartyManager.Party[0].model.RemoveChild(cameraHolder);
                     actor.AddChild(cameraHolder);
                  }

                  break;
               }
            }
         }
      }
   }
}