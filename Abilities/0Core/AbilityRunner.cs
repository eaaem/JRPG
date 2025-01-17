using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Makes a fighter run as commanded for ability graphics. Attach to a node that will be added as a child to fighter models.
/// </summary>
public partial class AbilityRunner : Node
{
   private Vector3 origin;
   private Node3D target;
   private Node3D parent;
   private AnimationPlayer player;
   private float waitTime;
   private float relativeSize;

   private bool runningToTarget;
   private bool runningBack;
   private bool waitForOthers;
   private bool waitAtTarget;

   [Signal]
   public delegate void ReachedTargetEventHandler();
   [Signal]
   public delegate void ReachedOriginEventHandler();
	
   public void ReceiveData(Vector3 origin, Node3D target, float waitTime, float waitTimeEvent = 0f)
   {
      this.origin = origin;
      this.target = target;
      this.waitTime = waitTime + waitTimeEvent;
      parent = GetParent<Node3D>();
      player = parent.GetNode<AnimationPlayer>("Model/AnimationPlayer");

      float targetSize = target.GetNode<AttackPreferences>("AttackPreferences").FighterSize;
      float parentSize = parent.GetNode<AttackPreferences>("AttackPreferences").FighterSize;

      relativeSize = targetSize + parentSize;

      parent.GetNode<Node3D>("Model").LookAt(target.Position, Vector3.Up, true);
      runningToTarget = true;
      player.Play("CombatRun", 0.25f);
   }

   public void ReceiveWaitingInformation(bool waitAtTarget)
   {
      waitForOthers = true;
      this.waitAtTarget = waitAtTarget;
   }

   async void Pause()
   {
      await ToSignal(GetTree().CreateTimer(waitTime), "timeout");
      runningBack = true;
      parent.GetNode<Node3D>("Model").LookAt(origin, Vector3.Up, true);
      player.Play("CombatRun", 0.25f);
   }

   void Terminate()
   {
      parent.RemoveChild(this);
      CallDeferred("queue_free");
   }

   /// <summary>
   /// Prompts the runners to continue their running (either back to origin or terminating the running object) after waiting for others. Activate this with a signal.
   /// </summary>
   public void ResumeRunning()
   {
      if (!waitForOthers) {
         return;
      }

      if (waitAtTarget)
      {
         Pause();
      }
      else
      {
         Terminate();
      }
   }

	public override void _PhysicsProcess(double delta)
	{
      if (runningToTarget)
      {
         Vector3 positionIncrement = parent.Position.MoveToward(target.GlobalPosition, (float)delta * 6f);
         parent.GlobalPosition = positionIncrement;

         if (parent.GlobalPosition.DistanceSquaredTo(target.GlobalPosition) < relativeSize * relativeSize)
         {
            runningToTarget = false;
            EmitSignal(SignalName.ReachedTarget);

            // No need for to wait for other runners, so immediately work on running back
            if (!waitForOthers || !waitAtTarget)
            {
               Pause();
               return;
            }

            player.Play("CombatIdle", 0.25f);
         }
      }

      if (runningBack)
      {
         Vector3 positionIncrement = parent.Position.MoveToward(origin, (float)delta * 5f);
         parent.Position = positionIncrement;

         if (parent.Position.DistanceSquaredTo(origin) < 0.1f)
         {
            runningBack = false;
            EmitSignal(SignalName.ReachedOrigin);

            if (!waitForOthers || waitAtTarget)
            {
               Terminate();
               return;
            }

            player.Play("CombatIdle", 0.25f);
            parent.GlobalPosition = origin;
         }
      }
	}
}
