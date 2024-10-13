using Godot;
using System;

/// <summary>
/// Holds information on how to set the points in a path that a projectile follows.
/// </summary>
[GlobalClass]
public partial class ProjectilePathPoint : Resource
{
   [Export]
   public string Path { get; set; }
	[Export]
   public SpecialCodeOverride OverridePath { get; set; }
   [Export]
   public int PointIndex { get; set;}
   [Export]
   public Vector3 Offset { get; set;}
}
