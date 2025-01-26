using Godot;
using System;

public partial class CharacterAudioController : Node
{
   private CharacterBody3D parent;

   public override void _Ready()
   {
      parent = GetParent<CharacterBody3D>();
   }

   public void PlayFootstep()
   {
      PhysicsDirectSpaceState3D spaceState = GetNode<Node3D>("/root/BaseNode").GetWorld3D().DirectSpaceState;

      Vector3 origin = parent.GlobalPosition + (Vector3.Up * 5f);
      Vector3 end = origin + (Vector3.Down * 15f);
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

      GetParent().GetNode<RandomAudioSelector>(groupName).PlayRandomAudio();
   }
}
