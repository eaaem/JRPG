using Godot;
using System;

public partial class CharacterAudioController : Node
{
   private bool isLeft = false;
   private Camera3D camera;
   private CharacterController characterController;

   public override void _Ready()
   {
      camera = GetNode<Camera3D>("/root/BaseNode/PartyMembers/Member1/CameraTarget/SpringArm3D/PlayerCamera");
      characterController = GetParent<CharacterController>();
   }

   public void PlayFootstep()
   {
      PhysicsDirectSpaceState3D spaceState = GetNode<Node3D>("/root/BaseNode").GetWorld3D().DirectSpaceState;
      Vector2 mousePosition = GetViewport().GetMousePosition();

      Vector3 origin = characterController.Position + Vector3.Up;
      Vector3 end = origin + (Vector3.Down * 5f);
      // 64 = layer 7, which is where terrain is
      PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end, 64);
      query.CollideWithAreas = true;

      var result = spaceState.IntersectRay(query);

      string groupName = "dirt";

      if (result.Count > 0)
      {
         StaticBody3D collided = (StaticBody3D)result["collider"];
         
         if (collided.IsInGroup("grass"))
         {
            groupName = "grass";
         }
         else if (collided.IsInGroup("stone"))
         {
            groupName = "stone";
         }
         else if (collided.IsInGroup("wood"))
         {
            groupName = "wood";
         }
      }

      Node3D group = GetParent().GetNode<Node3D>(groupName + "Footsteps");
      AudioStreamPlayer3D toPlay = group.GetChild<AudioStreamPlayer3D>(GD.RandRange(0, 5));

      /*if (isLeft)
      {
         toPlay.Position = new Vector3(2.5f, 0f, 0f);
      }
      else
      {
         toPlay.Position = new Vector3(-2.5f, 0f, 0f);
      }*/

      toPlay.Play();

      isLeft = !isLeft;

      
   }
}
