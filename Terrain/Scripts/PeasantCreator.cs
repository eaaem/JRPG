using Godot;
using System;
using System.Collections.Generic;

public partial class PeasantCreator : Node
{
   [Export]
   private Path3D[] paths;
   [Export]
   private int numberOfPeasants;
   [Export]
   private PackedScene peasantPrefab;
   [Export]
   private Material[] hairMaterials;
   [Export]
   private Material[] clothesMaterials;
   [Export]
   private Material[] eyesMaterials;

   private List<PathFollow3D> followPaths = new List<PathFollow3D>();
   private List<CharacterBody3D> peasants = new List<CharacterBody3D>();

   private CharacterController characterController;

	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
      characterController = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");
      for (int i = 0; i < numberOfPeasants; i++)
      {
         CharacterBody3D peasant = peasantPrefab.Instantiate<CharacterBody3D>();

         Material mainClothesMaterial = clothesMaterials[GD.RandRange(0, clothesMaterials.Length - 1)];
         peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Tunic").MaterialOverride = mainClothesMaterial;
         peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Pants").MaterialOverride = mainClothesMaterial;
         peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Shoes").MaterialOverride = clothesMaterials[GD.RandRange(0, clothesMaterials.Length - 1)];

         peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Eyes").MaterialOverride = eyesMaterials[GD.RandRange(0, eyesMaterials.Length - 1)];

         int hairNumber = GD.RandRange(0, 1);

         Material hairMaterial = hairMaterials[GD.RandRange(0, hairMaterials.Length - 1)];
         for (int j = 0; j <= 1; j++)
         {
            if (j != hairNumber)
            {
               peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Hair" + j).Visible = false;
            }
            else
            {
               peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Hair" + j).Visible = true;
               peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Hair" + j).MaterialOverride = hairMaterial;
            }
         }

         peasant.GetNode<MeshInstance3D>("Model/Armature/Skeleton3D/Eyebrows").MaterialOverride = hairMaterial;

         Path3D path = paths[GD.RandRange(0, paths.Length - 1)];
         float startingPoint = GD.Randf();
         //peasant.SetupStartingIndex(startingPoint);

         PathFollow3D pathFollow3D = new PathFollow3D();
         path.AddChild(pathFollow3D);
         pathFollow3D.AddChild(peasant);
        // peasant.SetupPath(pathFollow3D);

         pathFollow3D.ProgressRatio = startingPoint;

         followPaths.Add(pathFollow3D);
         peasants.Add(peasant);

         //AddChild(peasant);

         //peasant.GlobalPosition = path.Curve.GetPointPosition(startingPoint);

         peasant.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Walk");

         // Stagger slightly
         await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
      }
	}

   public override void _PhysicsProcess(double delta)
   {
      for (int i = 0; i < followPaths.Count; i++)
      {
         followPaths[i].Progress += 0.05f;

         if (peasants[i].GlobalPosition.DistanceTo(characterController.GlobalPosition) > 35f)
         {
            peasants[i].GetNode<AnimationPlayer>("Model/AnimationPlayer").Stop();
            peasants[i].Visible = false;
         }
         else if (peasants[i].GlobalPosition.DistanceTo(characterController.GlobalPosition) <= 35f && !peasants[i].Visible)
         {
            peasants[i].GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Walk");
            peasants[i].Visible = true;
         }
      }

      GD.Print(Engine.GetFramesPerSecond());
   }
}
