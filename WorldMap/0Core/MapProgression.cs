using Godot;
using System;

/// <summary>
/// This is basically the same as LevelProgression, but only for world maps. The same general instructions apply.
/// </summary>
public abstract partial class MapProgression : Node
{
	protected int progress = 0;
   protected ManagerReferenceHolder managers;

   public override void _Ready()
   {
      managers = GetNode<ManagerReferenceHolder>("/root/BaseNode/ManagerReferenceHolder");
      managers.LevelManager.SaveLevelProgression += SaveLevel;
      managers.LevelManager.LoadLevelProgression += LoadLevel;
   }

   public void SaveLevel()
   {
      managers.LevelManager.MapDatas[managers.LevelManager.ActiveMapDataID].progress = progress;
   }

   public abstract void LoadLevel();

   public override void _ExitTree()
   {
      managers.LevelManager.SaveLevelProgression -= SaveLevel;
      managers.LevelManager.LoadLevelProgression -= LoadLevel;
   }
}
