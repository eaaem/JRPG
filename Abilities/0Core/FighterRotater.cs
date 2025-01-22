using Godot;
using System;

public partial class FighterRotater : Node
{
   private Node3D targetNode;
   private Vector3 targetRotation;
   private float rotateSpeed;
   private Node3D model;

   private bool isRotating;

   [Signal]
   public delegate void RotationFinishedEventHandler();

   public void UpdateData(Vector3 targetRotation, float rotateSpeed, bool rotateInstantly, Node3D targetNode = null)
   {
      this.targetNode = targetNode;
      this.targetRotation = targetRotation;
      this.rotateSpeed = rotateSpeed;

      model = GetParent().GetNode<Node3D>("Model");

      if (rotateInstantly)
      {
         Vector3 target = new Vector3(Mathf.DegToRad(targetRotation.X), Mathf.DegToRad(targetRotation.Y), Mathf.DegToRad(targetRotation.Z));
         if (targetNode != null)
         {
            Basis lookAt = Basis.LookingAt(targetNode.GlobalPosition - model.GlobalPosition, Vector3.Up, true);
            target = new Vector3(target.X, target.Y + lookAt.GetEuler().Y, target.Z);
         }
         model.Rotation = target;

         EmitSignal(SignalName.RotationFinished);
         GetParent().RemoveChild(this);
         QueueFree();
         return;
      }

      isRotating = true;
   }

   public async override void _PhysicsProcess(double delta)
   {
      if (isRotating)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         Vector3 target = new Vector3(Mathf.DegToRad(targetRotation.X), Mathf.DegToRad(targetRotation.Y), Mathf.DegToRad(targetRotation.Z));
         if (targetNode != null)
         {
            Basis lookAt = Basis.LookingAt(targetNode.GlobalPosition - model.GlobalPosition, Vector3.Up, true);
            target = new Vector3(target.X, target.Y + lookAt.GetEuler().Y, target.Z);
         }

         Vector3 rotation = model.Rotation;

         rotation.Y = Mathf.LerpAngle(rotation.Y, target.Y, (float)delta * rotateSpeed);
         rotation.X = Mathf.LerpAngle(rotation.X, target.X, (float)delta * rotateSpeed);
         rotation.Z = Mathf.LerpAngle(rotation.Z, target.Z, (float)delta * rotateSpeed);

         model.Rotation = rotation;

         if (isRotating && Mathf.Abs(Mathf.AngleDifference(model.Rotation.Y, target.Y)) < 0.01f)
         {
            isRotating = false;
            EmitSignal(SignalName.RotationFinished);
            GetParent().RemoveChild(this);
            QueueFree();
            return;
         }
      }
   }
}
