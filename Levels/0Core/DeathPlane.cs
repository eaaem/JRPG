using Godot;
using System;

public partial class DeathPlane : Node
{
   [Export]
   private bool isWorldMap = false;

	void OnBodyEntered(Node3D body)
   {
      if (!isWorldMap)
      {
         Node3D spawn = GetNode<Node3D>("/root/BaseNode/Level/SpawnPoint");
         GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1").GlobalPosition = spawn.GlobalPosition;
         GetNode<PartyManager>("/root/BaseNode/PartyManager").MovePartyMembersBehindPlayer();
      }
      else
      {
         Node3D spawn = GetNode<Node3D>("/root/BaseNode/WorldMap/SpawnPoint");
         GetNode<CharacterController>("/root/BaseNode/WorldMap/Player").GlobalPosition = spawn.GlobalPosition;
      }
   }
}
