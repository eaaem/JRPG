using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

[Tool]
public partial class ObjectSet : MultiMeshInstance3D
{
   [Signal]
   public delegate void SetBrushRadiusEventHandler(float value);

	private bool randomizeScale;
   public bool RandomizeScale
   {
      get
      {
         return randomizeScale;
      }
      set
      {
         randomizeScale = value;
         NotifyPropertyListChanged();
      }
   }

   /// <summary>
   /// Randomizes the rotation of each mesh. Rotation values should be given in degrees.
   /// </summary>
   private bool randomizeRotation;
   public bool RandomizeRotation
   {
      get
      {
         return randomizeRotation;
      }
      set
      {
         randomizeRotation = value;
         NotifyPropertyListChanged();
      }
   }

   private bool collisions;
   public bool Collisions
   {
      get
      {
         return collisions;
      }
      set
      {
         collisions = value;
         NotifyPropertyListChanged();
         ChangeColliders();
      }
   }

   /// <summary>
   /// Set to a path to a StaticBody3D with the desired colliders if collisions are being used. The colliders will scale, rotate, and move with each instance.
   /// </summary>
   private string collisionScenePath = string.Empty;

   private bool applyColliders;
   private bool ApplyColliders
   {
      get
      {
         return applyColliders;
      }
      set
      {
         applyColliders = value;
         ChangeColliders();
      }
   }

   private Vector3 lowerScale = Vector3.One;
   private Vector3 upperScale = Vector3.One;

   private Vector3 lowerRotation;
   private Vector3 upperRotation;

   private int instanceCounter;

   private float brushRadius = 1f;
   [Export]
   private float BrushRadius
   {
      get
      {
         return brushRadius;
      }
      set
      {
         if (brushRadius > 0f)
         {
            brushRadius = value;
         }
         else
         {
            brushRadius = 0.01f;
         }

         EmitSignal(SignalName.SetBrushRadius, brushRadius);
      }
   }

   public override void _EnterTree()
   {
      if (Multimesh == null)
      {
         Multimesh = new MultiMesh();
         Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
         Multimesh.InstanceCount = 100;
         Multimesh.Mesh = new BoxMesh();
      }
   }

   public void AddMesh(Vector3 position, Vector3 overridenRotation)
   {
      instanceCounter++;

      Transform3D transform = new Transform3D(Basis.Identity, position - GlobalPosition);

      if (overridenRotation == Vector3.Zero)
      {
         if (randomizeRotation)
         {
            transform.Basis = transform.Basis.Rotated(Vector3.Right, (float)GD.RandRange(Mathf.DegToRad(lowerRotation.X), Mathf.DegToRad(upperRotation.X)));
            transform.Basis = transform.Basis.Rotated(Vector3.Up, (float)GD.RandRange(Mathf.DegToRad(lowerRotation.Y), Mathf.DegToRad(upperRotation.Y)));
            transform.Basis = transform.Basis.Rotated(Vector3.Back, (float)GD.RandRange(Mathf.DegToRad(lowerRotation.Z), Mathf.DegToRad(upperRotation.Z)));
         }
      }
      else
      {
         transform.Basis = transform.Basis.Rotated(Vector3.Right, overridenRotation.X);
         transform.Basis = transform.Basis.Rotated(Vector3.Up, overridenRotation.Y);
         transform.Basis = transform.Basis.Rotated(Vector3.Back, overridenRotation.Z);
      }

      if (randomizeScale)
      {
         Vector3 scale = new Vector3((float)GD.RandRange(lowerScale.X, upperScale.X), (float)GD.RandRange(lowerScale.Y, upperScale.Y), 
                                     (float)GD.RandRange(lowerScale.Z, upperScale.Z));
         transform.Basis = transform.Basis.Scaled(scale);
      }

      if (collisions && applyColliders)
      {
         AddCollider(transform, instanceCounter);
      }

      if (instanceCounter == Multimesh.InstanceCount)
      {
         ChangeInstanceCount(100);
      }

      Multimesh.SetInstanceTransform(instanceCounter - 1, transform);
   }

   void ChangeColliders()
   {
      if (collisionScenePath == string.Empty || !collisions || !applyColliders || GetChildCount() > 0)
      {
         // Removes all colliders
         foreach (StaticBody3D child in GetChildren())
         {
            RemoveChild(child);
            child.QueueFree();
         }
      }
      else
      {
         // Adds colliders to preexisting instances
         for (int i = 0; i < instanceCounter; i++)
         {
            AddCollider(Multimesh.GetInstanceTransform(i), i);
         }
      }
   }

   void AddCollider(Transform3D transform, int index)
   {
      StaticBody3D newCollider = GD.Load<PackedScene>(collisionScenePath).Instantiate<StaticBody3D>();
      newCollider.Transform = transform;
      newCollider.Name = index.ToString();
      AddChild(newCollider);
      newCollider.Owner = GetTree().EditedSceneRoot;
   }

   public void AttemptErase(Vector3 position)
   {
      List<StaticBody3D> collidersToRemove = new List<StaticBody3D>();
      for (int i = 0; i < instanceCounter; i++)
      {
         if (Multimesh.GetInstanceTransform(i).Origin.DistanceSquaredTo(position) < brushRadius * 3f)
         {
            Multimesh.SetInstanceTransform(i, Multimesh.GetInstanceTransform(instanceCounter - 1));
            Multimesh.SetInstanceTransform(instanceCounter - 1, new Transform3D());

            if (collisions)
            {
               StaticBody3D toReplace = GetChild<StaticBody3D>(i);
               StaticBody3D toRemove = GetChild<StaticBody3D>(instanceCounter - 1);
               toReplace.Transform = toRemove.Transform;
               RemoveChild(toRemove);
               toRemove.QueueFree();
            }

            instanceCounter--;
         }
      }

      for (int i = 0; i < collidersToRemove.Count; i++)
      {
         RemoveChild(collidersToRemove[i]);
         collidersToRemove[i].QueueFree();
      }

      if (instanceCounter < Multimesh.InstanceCount - 100)
      {
         ChangeInstanceCount(-100);
      }
   }

   void ChangeInstanceCount(int newCount)
   {
      // Changing the instance count of a multimesh clears all transforms, so we have to save them and reapply them after resizing the instance count
      Transform3D[] savedTransforms = new Transform3D[Multimesh.InstanceCount];

      for (int i = 0; i < Multimesh.InstanceCount; i++)
      {
         savedTransforms[i] = Multimesh.GetInstanceTransform(i);
      }

      Multimesh.InstanceCount += newCount;

      // The smaller of the length of the saved transforms and the multimesh instance count must be used to account for both increasing and decreasing the
      // instance count (when decreasing, the size of the saved transform array has to be used; when increasing, the size of the instance count has to be
      // used)
      for (int i = 0; i < Mathf.Min(savedTransforms.Length, Multimesh.InstanceCount); i++)
      {
         Multimesh.SetInstanceTransform(i, savedTransforms[i]);
      }
   }

   public override Array<Dictionary> _GetPropertyList()
   {
      Array<Dictionary> result = new Array<Dictionary>();

      result.Add(new Dictionary()
      {
         { "name", $"RandomizeScale" },
         { "type", (int)Variant.Type.Bool }
      });

      if (RandomizeScale)
      {
         result.Add(new Dictionary()
         {
            { "name", $"lowerScale" },
            { "type", (int)Variant.Type.Vector3 }
         });

         result.Add(new Dictionary()
         {
            { "name", $"upperScale" },
            { "type", (int)Variant.Type.Vector3 }
         });
      }

      result.Add(new Dictionary()
      {
         { "name", $"RandomizeRotation" },
         { "type", (int)Variant.Type.Bool }
      });

      if (RandomizeRotation)
      {
         result.Add(new Dictionary()
         {
            { "name", $"lowerRotation" },
            { "type", (int)Variant.Type.Vector3 }
         });

         result.Add(new Dictionary()
         {
            { "name", $"upperRotation" },
            { "type", (int)Variant.Type.Vector3 }
         });
      }

      result.Add(new Dictionary()
      {
         { "name", $"Collisions" },
         { "type", (int)Variant.Type.Bool }
      });

      if (Collisions)
      {
         result.Add(new Dictionary()
         {
            { "name", $"collisionScenePath" },
            { "type", (int)Variant.Type.String }
         });

         result.Add(new Dictionary()
         {
            { "name", $"ApplyColliders" },
            { "type", (int)Variant.Type.Bool }
         });
      }
      

      return result;
   }
}
