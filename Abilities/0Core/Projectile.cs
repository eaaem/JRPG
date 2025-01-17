using Godot;
using System;
using System.Collections.Generic;

public partial class Projectile : Node
{
	[Export]
   private bool signalOnCollision;
   [Export]
   private bool deleteOnCollision;
   [Export]
   private Node3D projectile;
   [Export]
   private bool usePath;
   [Export]
   private PathFollow3D pathToFollow;
   [Export]
   private float pathSpeed;
   [Export]
   private ProjectilePathPoint[] points = new ProjectilePathPoint[0];
   /// <summary>
   /// Mark this for projectiles created by ability command instances (since they try to clean up created nodes at the end, and will try to delete projectiles that
   /// are already deleted, causing an error)
   /// </summary>
   [Export]
   private bool doNotDeleteOnFinish;

   private bool isTraveling;

   [Signal]
   public delegate void OnProjectileEndedEventHandler();

   public void ReceiveAbilityCommandInstanceInfo(Node3D source, List<Node3D> targets, List<Node3D> createdNodes)
   {
      if (usePath)
      {
         Path3D path = pathToFollow.GetParent<Path3D>();
         path.GlobalRotation = source.GetNode<Node3D>("Model").GlobalRotation;

         for (int i = 0; i < points.Length; i++)
         {
            if (points[i].OverridePath == SpecialCodeOverride.None)
            {
               path.Curve.SetPointPosition(points[i].PointIndex, GetNode<Node3D>(points[i].Path).GlobalPosition - path.GlobalPosition + points[i].Offset);
            }
            else if (points[i].OverridePath == SpecialCodeOverride.CasterModel)
            {
               path.Curve.SetPointPosition(points[i].PointIndex, source.GetNode<Node3D>("Model").GlobalPosition - path.GlobalPosition + points[i].Offset);
            }
            else if (points[i].OverridePath == SpecialCodeOverride.CasterPlacement)
            {
               path.Curve.SetPointPosition(points[i].PointIndex, source.GlobalPosition - path.GlobalPosition + points[i].Offset);
            }
            else if (points[i].OverridePath == SpecialCodeOverride.TargetsModel)
            {
               path.Curve.SetPointPosition(points[i].PointIndex, targets[0].GetNode<Node3D>("Model").GlobalPosition - path.GlobalPosition + points[i].Offset);
            }
            else if (points[i].OverridePath == SpecialCodeOverride.TargetsPlacement)
            {
               path.Curve.SetPointPosition(points[i].PointIndex, targets[0].GlobalPosition - path.GlobalPosition + points[i].Offset);
            }
            else
            {
               for (int j = 0; j < createdNodes.Count; j++)
               {
                  if (createdNodes[j].Name == points[i].Path)
                  {
                     path.Curve.SetPointPosition(points[i].PointIndex, createdNodes[j].GlobalPosition - path.GlobalPosition + points[i].Offset);
                     break;
                  }
               }
            }

            // Accomodates for the path being rotated
            path.Curve.SetPointPosition(points[i].PointIndex, path.Curve.GetPointPosition(points[i].PointIndex).Rotated(Vector3.Up, -path.GlobalRotation.Y));
         }

      }

      isTraveling = true;
   }

   public void OnBodyEntered(Node3D body)
   {
      if (body.GetParent() == GetParent())
      {
         return;
      }

      if (signalOnCollision)
      {
         EmitSignal(SignalName.OnProjectileEnded);
      }

      if (deleteOnCollision)
      {
         isTraveling = false;
         GetParent().GetParent().CallDeferred("remove_child", GetParent());
         GetParent().QueueFree();
      }
   }

   public override void _PhysicsProcess(double delta)
   {
      if (usePath && isTraveling)
      {
         pathToFollow.Progress += (float)delta * pathSpeed;

         if (pathToFollow.ProgressRatio > 0.99f)
         {
            isTraveling = false;
            EmitSignal(SignalName.OnProjectileEnded);

            if (!doNotDeleteOnFinish)
            {
               GetParent().GetParent().RemoveChild(GetParent());
               GetParent().QueueFree();
            }
         }
      }
   }
}
