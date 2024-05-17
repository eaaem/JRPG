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

public partial class CutsceneManager : Node
{
   private Camera3D playerCamera;
   private Camera3D cutsceneCamera;

   private CharacterController controller;
   private DialogueManager dialogueManager;

   private Node3D baseNode;

   private SaveMenuManager saveMenuManager;
   private LevelManager levelManager;
   private PartyManager partyManager;

   CutsceneObject currentCutsceneObject;
   int currentID;

   float totalWaitDuration = 0f;

   bool isCutsceneActive;

   string overrideRotationActorName;
   bool isRotating;

   public override void _Ready()
   {
      baseNode = GetNode<Node3D>("/root/BaseNode");
      playerCamera = baseNode.GetNode<Camera3D>("PartyMembers/Member1/CameraTarget/PlayerCamera");
      cutsceneCamera = baseNode.GetNode<Camera3D>("CutsceneCamera");
      controller = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
      dialogueManager = baseNode.GetNode<DialogueManager>("UI/DialogueScreen/Back");

      saveMenuManager = baseNode.GetNode<SaveMenuManager>("SaveManager");
      levelManager = baseNode.GetNode<LevelManager>("LevelManager");
      partyManager = baseNode.GetNode<PartyManager>("PartyManagerObj");
   }

   public async void InitiateCutscene(CutsceneObject cutsceneObject, int id)
   {
      //CutsceneObject cutsceneObject = GD.Load<CutsceneObject>("res://Cutscenes/CutsceneObjects/" + signalName + ".tres");
      if (!isCutsceneActive)
      {
         isCutsceneActive = true;

         currentCutsceneObject = cutsceneObject;
         currentID = id;

         controller.DisableMovement = true;

         GetActors(!cutsceneObject.hideParty);
         baseNode.GetNode<Node3D>("PartyMembers").Visible = false;

         if (cutsceneObject.hideParty)
         {
            cutsceneCamera.Position = cutsceneObject.cameraPosition;
            cutsceneCamera.Rotation = cutsceneObject.cameraRotation;
            cutsceneCamera.MakeCurrent();

            saveMenuManager.FadeToBlack();
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            saveMenuManager.FadeFromBlack();
         }

         if (cutsceneObject.dialogueInteraction != null)
         {
            DialogueInteraction dialogueInteraction = new DialogueInteraction();
            dialogueInteraction.dialogueList = cutsceneObject.dialogueInteraction.dialogues;
            dialogueManager.InitiateDialogue(dialogueInteraction, true);
         }

         ProgressCutscene(0);
      }
   }

   public async void ProgressCutscene(int index)
   {
      for (int i = 0; i < currentCutsceneObject.actorBehaviors[index].commands.Length; i++)
      {
         ParseCommand(currentCutsceneObject.actorBehaviors[index].commands[i]);

         if (totalWaitDuration > 0f)
         {
            await ToSignal(GetTree().CreateTimer(totalWaitDuration), "timeout");
            totalWaitDuration = 0f;
         }
      }
   }

   void ParseCommand(string command)
   {
      //string earlySubstr = command.Substring(0, 5);
      string parsedCommand = command;
      string actorName = "";

      for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
      {
         // Without this if check, an error will be thrown if the name of the actor is longer than the command itself
         if (currentCutsceneObject.actors[i].actorName.Length > command.Length)
         {
            continue;
         }

         if (command.Substring(0, currentCutsceneObject.actors[i].actorName.Length) == currentCutsceneObject.actors[i].actorName)
         {
            actorName = command.Substring(0, currentCutsceneObject.actors[i].actorName.Length);
            parsedCommand = command.Substring(currentCutsceneObject.actors[i].actorName.Length + 1);
         }
      }

      string earlySubstr = parsedCommand.Substring(0, 5);

      if (earlySubstr == "move-")
      {
         GetActor(actorName).MoveCharacter(GetActorStatus(actorName), parsedCommand.Substring(5));
      }
      else if (earlySubstr.Substring(0, 4) == "rot-")
      {
         GetActor(actorName).RotateCharacter(parsedCommand.Substring(4).ToFloat());
      }
      else if (earlySubstr == "pause")
      {
         totalWaitDuration += parsedCommand.Substring(6).ToFloat();
      }
      else if (earlySubstr == "set_i")
      {
         GetActor(actorName).SetAnimation(true, GetActorStatus(actorName), parsedCommand.Substring(9));
      }
      else if (earlySubstr == "set_w")
      {
         GetActor(actorName).SetAnimation(false, GetActorStatus(actorName), parsedCommand.Substring(9));
      }
      else if (parsedCommand == "hide_speech")
      {
         dialogueManager.dialogueContainer.Visible = false;
      }
      else if (parsedCommand == "show_speech")
      {
         dialogueManager.dialogueContainer.Visible = true;
      }
      else if (parsedCommand == "lock_speech")
      {
         dialogueManager.lockInput = true;
      }
      else if (parsedCommand == "unlock_speech")
      {
         dialogueManager.lockInput = false;
      }
      else if (parsedCommand == "speak_next")
      {
         dialogueManager.NextCutsceneDialogue();
      }
      else if (earlySubstr == "place")
      {
         GetActor(actorName).PlaceCharacterAtPoint(parsedCommand.Substring(6));
      }
      else if (earlySubstr == "quick")
      {
         GetActor(actorName).RotateCharacterInstantly(parsedCommand.Substring(10).ToFloat());
      }
      else if (earlySubstr == "play_")
      {
         GetActor(actorName).PlayAnimation(parsedCommand.Substring(10), GetActorStatus(actorName));
      }
      else if (earlySubstr == "hide_")
      {
         GetActor(actorName).HideWeapon();
      }
      else if (earlySubstr == "show_")
      {
         GetActor(actorName).ShowWeapon();
      }
      else
      {
         GD.Print("Command " + command + " not recognized");
      }
   }

   /*async void MoveCharacter(string actorName, string destinationString)
   {
      CharacterBody3D actor = GetActor(actorName);
      ActorStatus actorStatus = GetActorStatus(actorName);

      Vector3 destination = GetVector3FromString(destinationString);

      Vector3 direction = actor.GlobalPosition.DirectionTo(destination);
      float distance = actor.GlobalPosition.DistanceTo(destination);

      AnimationPlayer player = actor.GetNode<AnimationPlayer>("Model/AnimationPlayer");

      if (player.CurrentAnimation != actorStatus.walkAnim)
      {
         player.Play(actorStatus.walkAnim);
      }

      while (distance > 0.1f)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         distance = actor.GlobalPosition.DistanceTo(destination);
         actor.Velocity = direction * actorStatus.moveSpeed;
         actor.MoveAndSlide();
      }

      actor.Velocity = Vector3.Zero;
      player.Play(actorStatus.idleAnim);
   }

   async void RotateCharacter(string actorName, float targetRotation)
   {
      CharacterBody3D actor = GetActor(actorName);
      targetRotation = Mathf.DegToRad(targetRotation);

      Node3D model = actor.GetNode<Node3D>("Model");

      while (Mathf.Abs(model.Rotation.Y - targetRotation) > 0.05f)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         Vector3 rotation = model.Rotation;
         rotation.Y = Mathf.Lerp(rotation.Y, targetRotation, 0.25f);
         model.Rotation = rotation;
      }
   }

   void SetAnimation(bool isIdle, string actorName, string animName)
   {
      ActorStatus actorStatus = GetActorStatus(actorName);

      if (isIdle)
      {
         actorStatus.idleAnim = animName;
      }
      else
      {
         actorStatus.walkAnim = animName;
      }
   }

   void PlaceCharacterAtPoint(string actorName, string pointString)
   {
      CharacterBody3D actor = GetActor(actorName);
      Vector3 point = GetVector3FromString(pointString);
      actor.GlobalPosition = point;
   }*/

   public void EndCutscene()
   {
      if (currentCutsceneObject.hideParty)
      {
         saveMenuManager.FadeToBlack();
      }
      
      levelManager.LocationDatas[levelManager.ActiveLocationDataID].cutscenesSeen[currentID.ToString()] = true;

      for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
      {
         CharacterBody3D actor = baseNode.GetNode<CharacterBody3D>(currentCutsceneObject.actors[i].actorName);
         baseNode.RemoveChild(actor);
         actor.QueueFree();
      }

      baseNode.GetNode<Node3D>("PartyMembers").Visible = true;
      playerCamera.MakeCurrent();
      isCutsceneActive = false;

      if (currentCutsceneObject.hideParty)
      {
         saveMenuManager.FadeFromBlack();
      }
   }

   /*Vector3 GetVector3FromString(string pointString)
   {
      string[] stringParts = pointString.Split(',');
      return new Vector3(stringParts[0].ToFloat(), stringParts[1].ToFloat(), stringParts[2].ToFloat());
   }*/

   Actor GetActor(string actorName)
   {
      return baseNode.GetNode<Actor>(actorName);
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

   /*public async void ActivateNextActorStep(int dialogueIndex)
   {
      dialogueManager.lockInput = true;
      
      if (dialogueManager.cutsceneReadyForNextDialogue)
      {
         dialogueManager.NextDialogue(dialogueIndex + 1);
         return;
      }

      for (int i = 0; i < currentCutsceneObject.actorBehaviors.Length; i++)
      {
         ActorBehavior presentBehavior = currentCutsceneObject.actorBehaviors[i];
         if (presentBehavior.dialogueIndex == dialogueIndex)
         {
            CharacterBody3D actor = baseNode.GetNode<CharacterBody3D>(presentBehavior.actorName);

            if (presentBehavior.move)
            {
               MoveActor(actor, presentBehavior.destination, presentBehavior.animationName, actor.GetNode<AnimationPlayer>("Model/AnimationPlayer").CurrentAnimation);
            }
            
            if (presentBehavior.rotate)
            {
               GD.Print("rotate");
               RotateActor(actor, presentBehavior.rotation);
            }
         }
      }

      dialogueManager.lockInput = false;

      await ToSignal(GetTree().CreateTimer(currentCutsceneObject.waitPerDialogueIndex[dialogueIndex]), "timeout");

      if (currentCutsceneObject.immediatelyOpenDialoguePerIndex[dialogueIndex] == 1)
      {
         dialogueManager.NextDialogue(dialogueIndex + 1);
      }
      else
      {
         dialogueManager.cutsceneReadyForNextDialogue = true;
      }
   }*/

   void GetActors(bool isTied)
   {
      for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
      {
         PackedScene currentActorScene = GD.Load<PackedScene>("res://Cutscenes/Actors/" + currentCutsceneObject.actors[i].actorName + ".tscn");
         CharacterBody3D actor = currentActorScene.Instantiate<CharacterBody3D>();
         actor.Name = currentCutsceneObject.actors[i].actorName;
         actor.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play(currentCutsceneObject.actors[i].idleAnim);
         baseNode.AddChild(actor);

         if (isTied)
         {
            for (int j = 0; j < partyManager.Party.Count; j++)
            {
               if (partyManager.Party[i].characterType.ToString() == currentCutsceneObject.actors[i].tiedMember)
               {
                  actor.GlobalPosition = partyManager.Party[i].model.GlobalPosition;
                  actor.GlobalRotation = partyManager.Party[i].model.GlobalRotation;
                  break;
               }
            }
         }
      }
   }
}