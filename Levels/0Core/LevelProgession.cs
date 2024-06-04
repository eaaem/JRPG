using Godot;
using System;

/// <summary>
/// An abstract class used to define the interface for level progression trackers.
/// <br></br>
/// It subscribes/desubscribes to delegates on its own. All the level progression scripts need to implement is loading and the actual progression.
/// <br></br>
/// Each level progression script should be attached to a Node3D INSIDE THE LEVEL ITSELF.
/// <br></br><br></br>
/// To recognize when a cutscene has ended, subscribe to "tree_exited" on the cutscene trigger (since it's deleted when a cutscene is initialized).
/// </summary>
public abstract partial class LevelProgession : Node
{
   protected int progress = 0;
   protected ManagerReferenceHolder managers;

   public override void _Ready()
   {
      managers = GetNode<ManagerReferenceHolder>("/root/BaseNode/ManagerReferenceHolder");
      managers.LevelManager.SaveLevelProgression += SaveLevel;
      managers.LevelManager.LoadLevelProgression += LoadLevel;

      //managers.CutsceneManager.CutsceneEnded += CutsceneEnded;
   }

   public void SaveLevel()
   {
      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].levelProgress = progress;
   }

   public abstract void LoadLevel();

   //public abstract void CutsceneEnded();

   public override void _ExitTree()
   {
      managers.LevelManager.SaveLevelProgression -= SaveLevel;
      managers.LevelManager.LoadLevelProgression -= LoadLevel;

      //managers.CutsceneManager.CutsceneEnded -= CutsceneEnded;
   }
}
