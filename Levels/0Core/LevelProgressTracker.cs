using Godot;
using System;

public partial class LevelProgressTracker : Node
{
	[Export]
   private LevelItem[] levelItems = new LevelItem[0];
   [Export]
   private ManagerReferenceHolder managers;
   [Export]
   private PackedScene levelProgressHolder;

   private Node3D progressionTrackerHolder;

   [Signal]
   public delegate void LevelSaveEventHandler();
   [Signal]
   public delegate void LevelLoadEventHandler();

   public void ChangeLevelProgressScripts(string newLevelName)
   {
      EmitSignal(SignalName.LevelSave);

      if (HasProgressionScript(newLevelName))
      {
         /*RemoveChild(progressionTrackerHolder);
         progressionTrackerHolder.QueueFree();

         PackedScene packedSceneHolder = GD.Load<PackedScene>(levelProgressHolder.ResourcePath);
         Node3D holder = packedSceneHolder.Instantiate<Node3D>();
         Node3D scriptHolder = holder.GetNode<Node3D>("Holder");

         scriptHolder.SetScript(GD.Load<CSharpScript>(path));
         AddChild(holder);

         progressionTrackerHolder = holder;*/

         EmitSignal(SignalName.LevelLoad);
      }
   }

   bool HasProgressionScript(string levelName)
   {
      for (int i = 0; i < levelItems.Length; i++)
      {
         if (levelItems[i].InternalLevelName == levelName)
         {
            return true;
         }
      }

      return false;
   }
}
