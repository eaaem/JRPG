using Godot;
using System;

public partial class DoorBehavior : Node
{
   private Camera3D camera;
   private Node3D baseNode;

   public override void _Ready()
   {
      baseNode = GetNode<Node3D>("/root/BaseNode");
      camera = GetNode<Camera3D>("/root/BaseNode/PartyMembers/Member1/CameraTarget/PlayerCamera");
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("interact"))
      {
         PhysicsDirectSpaceState3D spaceState = baseNode.GetWorld3D().DirectSpaceState;
         Vector2 mousePosition = GetViewport().GetMousePosition();

         Vector3 origin = camera.ProjectRayOrigin(mousePosition);
         Vector3 end = origin + camera.ProjectRayNormal(mousePosition) * 5;
         // 32 = layer 6, the door layer
         PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end, 32);
         query.CollideWithAreas = true;

         var result = spaceState.IntersectRay(query);

         if (result.Count > 0)
         {
            StaticBody3D collided = (StaticBody3D)result["collider"];
            collided.GetParent().GetParent<Door>().ChangeDoorState();
         }
      }
   }
}
