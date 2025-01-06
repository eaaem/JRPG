using Godot;
using Godot.Collections;
using System;

public partial class CutsceneTrigger : Node
{
   [Export]
   public CutsceneObject cutsceneObject;

   public int id;
   
   private CutsceneManager cutsceneManager;

   [Signal]
   public delegate void OnCutsceneInitiateEventHandler();

   public override void _Ready()
	{
      cutsceneManager = GetNode<CutsceneManager>("/root/BaseNode/CutsceneManager");
	}

	private void OnBodyEntered(Node3D body)
   {
      if (!cutsceneManager.IsCutsceneActive)
      {
         cutsceneManager.InitiateCutscene(cutsceneObject, id);
         EmitSignal(SignalName.OnCutsceneInitiate);
         QueueFree();
      }      
   }
}
