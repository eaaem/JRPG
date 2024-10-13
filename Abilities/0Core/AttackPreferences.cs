using Godot;
using System;

/// <summary>
/// Stores attack details (melee, ranged, or otherwise) for animation purposes. Put one of these inside each fighter.
/// </summary>
public partial class AttackPreferences : Node
{
	[Export]
   public bool IsRanged { get; set; }
   /// <summary>
   /// If this fighter's attacks shouldn't follow the standard, check this, create a new AbilityCommandHolder, and provide the path in the scene tree to it 
   /// in PathToOverride.
   /// </summary>
   [Export]
   public bool OverrideCopy { get; set; }
   /// <summary>
   /// Path should be local, with the fighter as the root.
   /// </summary>
   [Export]
   public string PathToOverride { get; set;}
   [Export]
   public float FighterSize { get; set;}
}
