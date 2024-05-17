using Godot;
using System;

[GlobalClass]
public partial class ActorStatus : Resource
{
   [Export]
   public string actorName;
   [Export]
   public string idleAnim;
   [Export]
   public string walkAnim;
   [Export]
   public float moveSpeed;
   [Export]
   public string tiedMember;
}
