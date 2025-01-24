using System;
using Godot;

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

   public bool IsCutsceneActive { get; set; }

   [Signal]
   public delegate void CutsceneEndedEventHandler();
   [Signal]
   public delegate void CutsceneBeganEventHandler();

   public async void InitiateCutscene(CutsceneObject cutsceneObject, int id)
   {
      if (!IsCutsceneActive)
      {
         managers.MenuManager.canTakeInput = false;

         for (int i = 2; i <= 4; i++)
         {
            GetNode<OverworldPartyController>("/root/BaseNode/PartyMembers/Member" + i).EnablePathfinding = false;
         }

         managers.Controller.DisableMovement = true;

         if (cutsceneObject.fadeBlackIntroTransition)
         {
            Tween tween = CreateTween();
            managers.MenuManager.FadeToBlack(tween);
            await ToSignal(tween, Tween.SignalName.Finished);
         }

         IsCutsceneActive = true;

         EmitSignal(SignalName.CutsceneBegan);

         // Copying the cutscene object is necessary to avoid changing the original instance of the object, which messes up the cutscene if the player
         // quits to the menu and reopens the game
         currentCutsceneObject = CopyCutsceneObject(cutsceneObject);
         currentID = id;

         GetActors(!cutsceneObject.hideParty);
         GetNode<Node3D>("/root/BaseNode/PartyMembers").Visible = false;
         managers.Controller.IsInCutscene = true;
         managers.Controller.IsSprinting = false;

         if (cutsceneObject.hideParty)
         {
            cutsceneCamera.Position = cutsceneObject.cameraPosition;
            cutsceneCamera.Rotation = cutsceneObject.cameraRotation;
            cutsceneCamera.MakeCurrent();

            managers.Controller.DisableCamera = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
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

         for (int i = 0; i < currentCutsceneObject.PreCutsceneCommands.Length; i++)
         {
            ParseCommand(currentCutsceneObject.PreCutsceneCommands[i]);

            if (totalWaitDuration > 0f)
            {
               await ToSignal(GetTree().CreateTimer(totalWaitDuration), "timeout");
               totalWaitDuration = 0f;
            }
         }

         if (cutsceneObject.fadeBlackIntroTransition)
         {
            Tween tween = CreateTween();
            managers.MenuManager.FadeFromBlack(tween);
            await ToSignal(tween, Tween.SignalName.Finished);
         }

         ProgressCutscene(0);
      }
   }

   CutsceneObject CopyCutsceneObject(CutsceneObject toCopy)
   {
      CutsceneObject result = new CutsceneObject();

      result.hideParty = toCopy.hideParty;
      result.cameraPosition = toCopy.cameraPosition;
      result.cameraRotation = toCopy.cameraRotation;
      result.fadeBlackIntroTransition = toCopy.fadeBlackIntroTransition;
      result.fadeBlackExitTransition = toCopy.fadeBlackExitTransition;

      result.items = new CutsceneItem[toCopy.items.Length];

      for (int i = 0; i < toCopy.items.Length; i++)
      {
         result.items[i] = new CutsceneItem();
         result.items[i].dialogue = (DialogueObject)toCopy.items[i].dialogue.Duplicate(true);

         result.items[i].commands = new ActorCommand[toCopy.items[i].commands.Length];
         
         for (int j = 0; j < result.items[i].commands.Length; j++)
         {
            result.items[i].commands[j] = (ActorCommand)toCopy.items[i].commands[j].Duplicate(true);
         }
      }

      result.actors = new ActorStatus[toCopy.actors.Length];

      for (int i = 0; i < toCopy.actors.Length; i++)
      {
         result.actors[i] = (ActorStatus)toCopy.actors[i].Duplicate();
      }

      result.PreCutsceneCommands = new ActorCommand[toCopy.PreCutsceneCommands.Length];

      for (int i = 0; i < toCopy.PreCutsceneCommands.Length; i++)
      {
         result.PreCutsceneCommands[i] = (ActorCommand)toCopy.PreCutsceneCommands[i].Duplicate(true);
      }

      return result;
   }

   /// <summary>
   /// Progresses the cutscene, executing actor commands as necessary.
   /// <br></br><br></br>
   /// index : the corresponding dialogue index
   /// </summary>
   public async void ProgressCutscene(int index)
   {
      for (int i = 0; i < currentCutsceneObject.items[index].commands.Length; i++)
      {
         ParseCommand(currentCutsceneObject.items[index].commands[i]);

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
         totalWaitDuration += command.WaitTime;
         break;
      case CommandType.Place:
         actor = GetActor(command.ActorName);
         actor.PlaceCharacterAtPoint(command.Destination);
         break;
      case CommandType.PlayAnimation:
         actor = GetActor(command.ActorName);
         actor.PlayAnimation(command.AnimationName, GetActorStatus(command.ActorName), command.Blend, command.UseAnimationLength, command.WaitTime, command.ExitBlend);
         break;
      case CommandType.Track:
         actor = GetActor(command.ActorName);
         Actor target = GetActor(command.Target);
         actor.Track(target);
         break;
      case CommandType.StopTrack:
         actor = GetActor(command.ActorName);
         actor.StopTracking();
         break;
      case CommandType.FadeBlack:
         Tween tween = CreateTween();
         if (command.Fade)
         {
            managers.MenuManager.FadeToBlack(tween);
         }
         else
         {
            managers.MenuManager.FadeFromBlack();
         }
         
         break;
      case CommandType.PlaceCamera:
         cutsceneCamera.Position = command.Destination;
         break;
      case CommandType.QuickRotateCamera:
         cutsceneCamera.Rotation = command.Destination;
         break;
      case CommandType.CallMethod:
         Node script = GetNode<Node>(command.ObjectPath);
         script.Call(command.Method);
         break;
      case CommandType.TurnToLookAt:
         actor = GetActor(command.ActorName);

         Vector3 targetPosition;

         if (command.Target.StartsWith("/root/"))
         {
            targetPosition = GetNode<Node3D>(command.Target).GlobalPosition;
         }
         else
         {
            targetPosition = GetActor(command.Target).GlobalPosition;
         }

         actor.TurnCharacterToLookAt(targetPosition);

         break;
      case CommandType.PauseMusic:
         GetNode<AudioStreamPlayer>("/root/BaseNode/MusicPlayer").StreamPaused = true;
         
         break;
      case CommandType.ResumeMusic:
         GetNode<AudioStreamPlayer>("/root/BaseNode/MusicPlayer").Play();

         break;
      case CommandType.EndCutscene:
         managers.DialogueManager.ExitDialogue();
         EndCutscene();
         break;
      default:
         GD.Print("Cutscene command invalid.");
         break;
      }
   }

   /// <summary>
   /// Inserts items from a cutscene object into the current cutscene object. This is useful for cutscenes that have different items depending on player decisions,
   /// since it allows for changing the cutscene's content at runtime.
   /// <br></br><br></br>
   /// <c>cutsceneObject</c> : the cutscene object from which new items will be inserted
   /// <br></br>
   /// <c>index</c> : the starting index at which the new items will be inserted
   /// </summary>
   public void InsertCutsceneItems(CutsceneObject newCutsceneObject, int index)
   {
      // Resizes the array, moves over the existing items, and copies in the new items
      Array.Resize(ref currentCutsceneObject.items, currentCutsceneObject.items.Length + newCutsceneObject.items.Length);
      Array.Copy(currentCutsceneObject.items, index, currentCutsceneObject.items, index + newCutsceneObject.items.Length, 
                 currentCutsceneObject.items.Length - index - newCutsceneObject.items.Length);
      Array.Copy(newCutsceneObject.items, 0, currentCutsceneObject.items, index, newCutsceneObject.items.Length);

      // Constructs a new dialogue interaction and replaces it in the dialogue manager (without this, the dialogue won't update properly)
      DialogueInteraction newInteraction = new DialogueInteraction();
      newInteraction.dialogueList = new DialogueList();
      newInteraction.dialogueList.dialogues = new DialogueObject[currentCutsceneObject.items.Length];
      
      for (int i = 0; i < currentCutsceneObject.items.Length; i++)
      {
         newInteraction.dialogueList.dialogues[i] = currentCutsceneObject.items[i].dialogue;
      }

      managers.DialogueManager.ReplaceDialogueInteraction(newInteraction);
   }

   public async void EndCutscene(bool countAsCompleted = true)
   {
      if (IsCutsceneActive)
      {
         if (countAsCompleted)
         {
            managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].cutscenesSeen[currentID.ToString()] = true;
         }

         if (currentCutsceneObject.fadeBlackExitTransition)
         {
            Tween tween = CreateTween();
            managers.MenuManager.FadeToBlack(tween);
            await ToSignal(tween, Tween.SignalName.Finished);
         }

         for (int i = 0; i < currentCutsceneObject.actors.Length; i++)
         {
            CharacterBody3D actor = GetNode<CharacterBody3D>("/root/BaseNode/" + currentCutsceneObject.actors[i].actorName);

            if (!currentCutsceneObject.hideParty)
            {
               for (int j = 0; j < managers.PartyManager.Party.Count; j++)
               {
                  if (managers.PartyManager.Party[j].characterType.ToString() == currentCutsceneObject.actors[i].tiedMember)
                  {
                     if (managers.PartyManager.Party[j].model != null)
                     {
                        managers.PartyManager.Party[j].model.GlobalPosition = actor.GlobalPosition;
                        managers.PartyManager.Party[j].model.GetNode<Node3D>("Model").Rotation = actor.GetNode<Node3D>("Model").Rotation;

                        if (j == 0)
                        {
                           Node3D cameraHolder = actor.GetNode<Node3D>("CameraTarget");
                           actor.RemoveChild(cameraHolder);
                           managers.PartyManager.Party[0].model.AddChild(cameraHolder);
                           managers.PartyManager.Party[0].model.GetNode<Node3D>("Model").RotateY(-cameraHolder.Rotation.Y);
                        }

                        break;
                     }
                  }
               }
            }

            GetNode<Node3D>("/root/BaseNode").RemoveChild(actor);
            actor.QueueFree();
         }

         managers.Controller.IsInCutscene = false;
         managers.Controller.DisableMovement = false;
         managers.Controller.Rotation = new Vector3(0f, managers.Controller.GetNode<Node3D>("CameraTarget").Rotation.Y, 0f);
         managers.Controller.GetNode<Node3D>("CameraTarget").RotateY(-managers.Controller.GetNode<Node3D>("CameraTarget").Rotation.Y);

         GetNode<Node3D>("/root/BaseNode/PartyMembers").Visible = true;
         playerCamera.MakeCurrent();
         IsCutsceneActive = false;

         for (int i = 2; i <= 4; i++)
         {
            GetNode<OverworldPartyController>("/root/BaseNode/PartyMembers/Member" + i).EnablePathfinding = true;
         }

         if (currentCutsceneObject.fadeBlackExitTransition)
         {
            managers.MenuManager.FadeFromBlack();
         }

         if (GetNode<AudioStreamPlayer>("/root/BaseNode/MusicPlayer").StreamPaused)
         {
            GetNode<AudioStreamPlayer>("/root/BaseNode/MusicPlayer").Play();
         }

         managers.MenuManager.canTakeInput = true;
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
         Actor actor = currentActorScene.Instantiate<Actor>();
         actor.Name = currentCutsceneObject.actors[i].actorName;
         actor.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play(currentCutsceneObject.actors[i].idleAnim);
         GetNode<Node3D>("/root/BaseNode/").AddChild(actor);

         if (isTied)
         {
            for (int j = 0; j < managers.PartyManager.Party.Count; j++)
            {
               if (managers.PartyManager.Party[j].characterType.ToString() == currentCutsceneObject.actors[i].tiedMember)
               {
                  if (managers.PartyManager.Party[j].model != null)
                  {
                     actor.GlobalPosition = managers.PartyManager.Party[j].model.GlobalPosition;
                     actor.GetNode<Node3D>("Model").GlobalRotation = managers.PartyManager.Party[j].model.GetNode<Node3D>("Model").GlobalRotation;

                     if (j == 0)
                     {
                        Node3D cameraHolder = managers.PartyManager.Party[0].model.GetNode<Node3D>("CameraTarget");
                        managers.PartyManager.Party[0].model.RemoveChild(cameraHolder);
                        actor.AddChild(cameraHolder);
                        cameraHolder.RotateY(managers.PartyManager.Party[j].model.Rotation.Y);
                     }
                  }

                  break;
               }
            }
         }
      }
   }
}