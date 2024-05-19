using Godot;
using Godot.Collections;
using System;

public partial class CutsceneTrigger : Node
{
   [Export]
   public CutsceneObject cutsceneObject;

   public int id;
   
   private CutsceneManager cutsceneManager;

   bool hasStartedCutscene = false;

   public override void _Ready()
	{
      cutsceneManager = GetNode<CutsceneManager>("/root/BaseNode/CutsceneManager");
	}

	private void OnBodyEntered(Node3D body)
   {
      if (!hasStartedCutscene)
      {
         cutsceneManager.InitiateCutscene(cutsceneObject, id);
         hasStartedCutscene = true;
      }
      
      QueueFree();
   }
}
