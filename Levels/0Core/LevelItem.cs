using Godot;

/// <summary>
/// Used in the LevelProgressTracker to link together an internal level's name with its corresponding progress script.
/// </summary>
[GlobalClass]
public partial class LevelItem : Resource
{
	[Export]
   public string InternalLevelName { get; set; }
   [Export]
   public string ProgressScriptPath { get; set;}
}
