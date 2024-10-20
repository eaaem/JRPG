using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

public partial class AbilityCommandInstance : Node
{
   [Export]
   public AbilityCommand[] commands = new AbilityCommand[0];

   [Signal]
   public delegate void ShowDamageEventHandler();
   [Signal]
   public delegate void ResumeRunnersEventHandler();

   private List<Fighter> targets = new List<Fighter>();
   private bool playHitAnimation = true;

   private CombatManager combatManager;

   private float waitTime;

   private Node3D cameraTarget;
   private Vector3 targetPosition;
   private Vector3 targetRotation;

   private Camera3D arenaCamera;

   private bool isPanning;
   private float panSpeed;
   private bool isOrbiting;
   private float orbitSpeed;
   private bool isTracking;
   private float trackSpeed;

   private bool isWaitingForProjectiles;

   List<Node3D> createdNodes = new List<Node3D>();

   private List<AbilityRunner> abilityRunners = new List<AbilityRunner>();
   private List<bool> runnersReached = new List<bool>();
   private bool isWaitingForRunners;

   private List<FighterRotater> rotaters = new List<FighterRotater>();
   private List<bool> rotatersFinished = new List<bool>();
   private bool isWaitingForRotaters;

   public void UpdateData(List<Fighter> targets, bool playHitAnimation)
   {
      this.targets = new List<Fighter>(targets);
      this.playHitAnimation = playHitAnimation;

      InitiateCommands();
   }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManager");
      ShowDamage += () => combatManager.FinishCastingProcess(targets, playHitAnimation);
      arenaCamera = GetNode<Camera3D>("/root/BaseNode/Level/Arena/ArenaCamera");
	}

   async void InitiateCommands()
   {
      for (int i = 0; i < commands.Length; i++)
      {
         ParseCommand(commands[i]);

         while (isWaitingForProjectiles)
         {
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
         }

         if (waitTime > 0f)
         {
            await ToSignal(GetTree().CreateTimer(waitTime), "timeout");
            waitTime = 0f;
         }

         while (isWaitingForRunners || isWaitingForRotaters)
         {
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
         }
      }

      GetParent().CallDeferred("remove_child", this);
      QueueFree();
   }
 
   void ParseCommand(AbilityCommand command)
   {
      switch (command.CommandType)
      {
         case AbilityCommandType.None:
            GD.Print("Ability command type is none; skipping");
            break;
         case AbilityCommandType.CreateNode:
            Node3D newNode = null;

            if (command.PathToScene.Length > 0)
            {
               newNode = GD.Load<PackedScene>(command.PathToScene).Instantiate<Node3D>();
            }
            else
            {
               newNode = new Node3D();
            }

            List<Node3D> allParsed = ParseSpecialPath(command.SpecialCodeOverride, command.Path);

            if (allParsed.Count == 0)
            {
               GetNode<Node>(command.Path).AddChild(newNode);

               newNode.Name = command.NodeName;
               newNode.Position = command.Target;

               createdNodes.Add(newNode);
            }
            else
            {
               newNode.QueueFree();

               for (int i = 0; i < allParsed.Count; i++)
               {
                  Node3D currentNode = command.PathToScene.Length > 0 ? GD.Load<PackedScene>(command.PathToScene).Instantiate<Node3D>() : new Node3D();

                  currentNode.Name = command.NodeName;
                  // Node not showing up in tree
                  allParsed[i].AddChild(currentNode);
                  currentNode.Position = command.Target;

                  createdNodes.Add(currentNode);
               }
            }

            break;
         case AbilityCommandType.CameraSetTarget:
            List<Node3D> parsed = ParseSpecialPath(command.SpecialCodeOverride, command.Path);

            if (parsed.Count == 0)
            {
               cameraTarget = GetNode<Node3D>(command.Path);
            }
            else
            {
               cameraTarget = parsed[0];
            }

            break;
         case AbilityCommandType.CameraSetParent:
            List<Node3D> parsedNodes = ParseSpecialPath(command.SpecialCodeOverride, command.Path);

            Node3D parent = arenaCamera.GetParent<Node3D>();
            parent.RemoveChild(arenaCamera);

            if (parsedNodes.Count == 0)
            {
               GetNode<Node3D>(command.Path).AddChild(arenaCamera);
            }
            else
            {
               parsedNodes[0].AddChild(arenaCamera);
            }
            
            break;
         case AbilityCommandType.CameraPlace:
            if (command.UseLocal)
            {
               arenaCamera.Position = command.Target;
            }
            else
            {
               arenaCamera.GlobalPosition = command.Target;
            }

            break;
         case AbilityCommandType.CameraRotateInstantly:
            arenaCamera.Rotation = new Vector3(Mathf.DegToRad(command.Target.X), Mathf.DegToRad(command.Target.Y), Mathf.DegToRad(command.Target.Z));
            break;
         case AbilityCommandType.CameraPan:
            targetPosition = command.Target;
            panSpeed = command.Speed;
            isPanning = true;
            break;
         case AbilityCommandType.CameraOrbit:
            Vector3 parentRotation = arenaCamera.GetParent<Node3D>().Rotation;
            targetRotation = new Vector3(Mathf.DegToRad(command.Target.X) + parentRotation.X, Mathf.DegToRad(command.Target.Y) + parentRotation.Y, 
                                         Mathf.DegToRad(command.Target.Z) + parentRotation.Z);
            orbitSpeed = command.Speed;
            isOrbiting = true;
            break;
         case AbilityCommandType.CameraLookAtTarget:
            if (cameraTarget == null)
            {
               GD.PushError("Error for camera command: Can't look at target because target doesn't exist");
               return;
            }

            if (command.LookImmediately)
            {
               arenaCamera.LookAt(cameraTarget.GlobalPosition);
            }
            else
            {
               trackSpeed = command.Speed;
               isTracking = true;
            }

            break;
         case AbilityCommandType.TriggerEffect:
            GetChild<AbilityGraphicController>(0).Call(command.Method);
            break;
         case AbilityCommandType.Pause:
            waitTime += command.Amount;

            if (command.PauseAnimation.Length > 0)
            {
               Node3D animationPreferencesParent = ParseSpecialPath(command.SpecialCodeOverride, command.Path)[0];
               AnimationPreferences animationPreferences = animationPreferencesParent.GetNode<AnimationPreferences>("AnimationPreferences");
               AnimationPlayer player = animationPreferencesParent.GetNode<AnimationPlayer>("Model/AnimationPlayer");
               WaitTimeEvent timeEvent = null;

               for (int i = 0; i < animationPreferences.preferences.Length; i++)
               {
                  if (animationPreferences.preferences[i].animationName == player.CurrentAnimation)
                  {
                     for (int j = 0; j < animationPreferences.preferences[i].events.Length; j++)
                     {
                        if (animationPreferences.preferences[i].events[j].eventName == command.PauseAnimation)
                        {
                           timeEvent = animationPreferences.preferences[i].events[j];
                           break;
                        }
                     }
                     break;
                  }
               }

               if (timeEvent == null)
               {
                  GD.PushError("Wait time event not found in Pause command");
                  break;
               }

               if (timeEvent.projectilePath.Length > 0)
               {
                  Node3D projectile = GD.Load<PackedScene>(timeEvent.projectilePath).Instantiate<Node3D>();
                  animationPreferencesParent.AddChild(projectile);
                  projectile.GetNode<Projectile>("Projectile").OnProjectileEnded += ReceiveProjectileReached;
                  projectile.GetNode<Projectile>("Projectile").ReceiveAbilityCommandInstanceInfo(animationPreferencesParent, 
                                                                                                 ParseSpecialPath(SpecialCodeOverride.TargetsModel, ""), createdNodes);
                  isWaitingForProjectiles = true;
               }
               else
               {
                  waitTime += timeEvent.waitTime;
               }
            }
            break;
         case AbilityCommandType.ShowDamage:
            EmitSignal(SignalName.ShowDamage);
            break;
         case AbilityCommandType.PlayAnimation:
            List<Node3D> fightersToPlay = ParseSpecialPath(command.SpecialCodeOverride, command.InvolvedFighter);

            for (int i = 0; i < fightersToPlay.Count; i++)
            {
               fightersToPlay[i].GetNode<AnimationPlayer>("Model/AnimationPlayer").Play(command.TargetName, 0.25f);
            }

            break;
         case AbilityCommandType.RunToFighter:
            List<Node3D> runners = ParseSpecialPath(command.SpecialCodeOverride, command.InvolvedFighter);
            Node3D runnerTarget = ParseSpecialPath(command.TargetCodeOverride, command.TargetName)[0];

            abilityRunners.Clear();

            for (int i = 0; i < runners.Count; i++)
            {
               AbilityRunner abilityRunner = GD.Load<PackedScene>("res://Abilities/0Core/ability_runner.tscn").Instantiate<AbilityRunner>();
               runners[i].AddChild(abilityRunner);
               abilityRunners.Add(abilityRunner);
               ResumeRunners += abilityRunners[i].ResumeRunning;
               float secondaryWaitTime = 0f;

               if (command.PauseAnimation.Length > 0)
               {
                  AnimationPreferences animationPreferences = runners[i].GetNode<AnimationPreferences>("AnimationPreferences");
                  AnimationPlayer player = runners[i].GetNode<AnimationPlayer>("Model/AnimationPlayer");

                  // Find the next PlayAnimation command, and use the wait event for that
                  int index = 0;
                  for (int j = 0; j < commands.Length; j++)
                  {
                     if (commands[j] == command)
                     {
                        index = j;
                        break;
                     }
                  }

                  string animationName = player.CurrentAnimation;
                  for (int j = index; j < commands.Length; j++)
                  {
                     if (commands[j].CommandType == AbilityCommandType.PlayAnimation)
                     {
                        animationName = commands[j].TargetName;
                     }
                  }

                  // TO DO: Optimize this (maybe create a dictionary inside each fighter so we don't have to linear search for the event every single time?)
                  for (int j = 0; j < animationPreferences.preferences.Length; j++)
                  {
                     if (animationPreferences.preferences[j].animationName == animationName)
                     {
                        for (int k = 0; k < animationPreferences.preferences[j].events.Length; k++)
                        {
                           if (animationPreferences.preferences[j].events[k].eventName == command.PauseAnimation)
                           {
                              secondaryWaitTime = animationPreferences.preferences[j].events[k].waitTime;
                              break;
                           }
                        }
                        break;
                     }
                  }
               }

               abilityRunner.ReceiveData(runners[i].Position, runnerTarget, command.Amount, secondaryWaitTime);
            }

            break;
         case AbilityCommandType.PauseDuringRun:
            runnersReached.Clear();
            for (int i = 0; i < abilityRunners.Count; i++)
            {
               abilityRunners[i].ReceiveWaitingInformation(command.PauseUntilTargetReached);
               Node3D abilityParent = abilityRunners[i].GetParent<Node3D>();
               
               if (command.PauseUntilTargetReached)
               {
                  abilityRunners[i].ReachedTarget += () => ReceiveRunnerReached(abilityParent);
               }
               else
               {
                  abilityRunners[i].ReachedOrigin += () => ReceiveRunnerReached(abilityParent);
               }

               runnersReached.Add(false);
            }

            isWaitingForRunners = true;

            break;
         case AbilityCommandType.RotateFighter:
         {
            List<Node3D> toRotate = ParseSpecialPath(command.SpecialCodeOverride, command.InvolvedFighter);
            Node3D target = null;

            rotaters.Clear();
            for (int i = 0; i < toRotate.Count; i++)
            {
               FighterRotater rotater = GD.Load<PackedScene>("res://Abilities/0Core/fighter_rotater.tscn").Instantiate<FighterRotater>();
               toRotate[i].AddChild(rotater);

               if (command.TargetCodeOverride != SpecialCodeOverride.None)
               {
                  target = ParseSpecialPath(command.TargetCodeOverride, command.TargetName)[0];
               }

               rotater.UpdateData(command.Target, command.Amount, command.LookImmediately, target);

               rotaters.Add(rotater);
            }

            break;
         }
         case AbilityCommandType.PauseDuringRotate:
            rotatersFinished.Clear();

            for (int i = 0; i < rotaters.Count; i++)
            {      
               int index = i;
               Node3D rotaterParent = rotaters[i].GetParent<Node3D>();
               rotaters[i].RotationFinished += () => ReceiveRotatedTarget(index);
               rotatersFinished.Add(false);
            }

            isWaitingForRotaters = true;

            break;
         case AbilityCommandType.Reset:
            arenaCamera.GetParent<Node3D>().RemoveChild(arenaCamera);
            GetNode<Node3D>("/root/BaseNode/Level/Arena").AddChild(arenaCamera);
            combatManager.ResetCamera();
            break;
         default:
            GD.Print("Invalid ability command");
            break;
      }

      if (command.CommandType != AbilityCommandType.Pause)
      {
         isTracking = false;
      }
   }

   /// <summary>
   /// Recognizes that a runner has reached their destination. Once all runners have reached their destinations, they'll be signalled to continue and the commands
   /// will stop waiting.
   /// </summary>
   public void ReceiveRunnerReached(Node3D runner)
   {
      for (int i = 0; i < abilityRunners.Count; i++)
      {
         if (abilityRunners[i].GetParent<Node3D>() == runner)
         {
            runnersReached[i] = true;
            break;
         }
      }

      for (int i = 0; i < runnersReached.Count; i++)
      {
         if (!runnersReached[i])
         {
            return;
         }
      }

      EmitSignal(SignalName.ResumeRunners);
      isWaitingForRunners = false;
   }

   public void ReceiveRotatedTarget(int targetIndex)
   {
      for (int i = 0; i < rotaters.Count; i++)
      {
         if (i == targetIndex)
         {
            rotatersFinished[i] = true;
            break;
         }
      }

      for (int i = 0; i < rotatersFinished.Count; i++)
      {
         if (!rotatersFinished[i])
         {
            return;
         }
      }

      isWaitingForRotaters = false;
   }

   public void ReceiveProjectileReached()
   {
      isWaitingForProjectiles = false;
   }

   List<Node3D> ParseSpecialPath(SpecialCodeOverride codeOverride, string createdNodeName)
   {
      switch (codeOverride)
      {
         case SpecialCodeOverride.None:
            return new List<Node3D>();
         case SpecialCodeOverride.CasterPlacement:
            return new List<Node3D>() { combatManager.CurrentFighter.placementNode };
         case SpecialCodeOverride.CasterModel:
            return new List<Node3D>() { combatManager.CurrentFighter.model };
         case SpecialCodeOverride.TargetsPlacement:
         {
            List<Node3D> result = new List<Node3D>();

            for (int i = 0; i < targets.Count; i++)
            {
               result.Add(targets[i].placementNode);
            }

            return result;
         }
         case SpecialCodeOverride.TargetsModel:
         { 
            List<Node3D> result = new List<Node3D>();

            for (int i = 0; i < targets.Count; i++)
            {
               result.Add(targets[i].model);
            }

            return result;
         }
         case SpecialCodeOverride.CreatedNode:
            for (int i = 0; i < createdNodes.Count; i++)
            {
               if (createdNodes[i].Name == createdNodeName)
               {
                  return new List<Node3D>() { createdNodes[i] };
               }
            }

            return new List<Node3D>();
         default:
            GD.Print("Special code invalid");
            return new List<Node3D>();
      }
   }

   public override void _PhysicsProcess(double delta)
   {
      if (isPanning)
      {
         Vector3 positionIncrement = arenaCamera.Position.MoveToward(targetPosition, (float)delta * panSpeed);
         arenaCamera.Position = positionIncrement;

         if (arenaCamera.Position.DistanceSquaredTo(targetPosition) < 0.5f)
         {
            isPanning = false;
         }
      }

      if (isOrbiting)
      {
         Node3D parent = arenaCamera.GetParent<Node3D>();
         Vector3 rotation = parent.Rotation;
         rotation.X = Mathf.Lerp(rotation.X, targetRotation.X, orbitSpeed);
         rotation.Y = Mathf.Lerp(rotation.Y, targetRotation.Y, orbitSpeed);
         rotation.Z = Mathf.Lerp(rotation.Z, targetRotation.Z, orbitSpeed);

         parent.Rotation = rotation;

         if (rotation.DistanceSquaredTo(targetRotation) < 0.05f)
         {
            isOrbiting = false;
         }
      }

      if (isTracking)
      {
         arenaCamera.LookAt(cameraTarget.GlobalPosition);
      }
   }

   public override void _ExitTree()
   {
      ShowDamage -= () => combatManager.FinishCastingProcess(targets, playHitAnimation);
      combatManager.FinishRound();

      for (int i = 0; i < createdNodes.Count; i++)
      {
         createdNodes[i].GetParent().CallDeferred("remove_child", createdNodes[i]);
         createdNodes[i].QueueFree();
      }
   }
}
