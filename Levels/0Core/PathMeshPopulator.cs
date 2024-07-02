#if TOOLS
using Godot;
using Godot.Collections;

/// <summary>
/// Used with a Path3D and MultiMeshInstance to place instances of the multimesh on the points in the path. This makes it easier to customize the exact location
/// of each instance of a multimesh.
/// <br></br>
/// Comes with support for rotation and scale randomization, determining exact rotations, and generating colliders.
/// </summary>
[Tool]
public partial class PathMeshPopulator : MultiMeshInstance3D
{
   private bool populate = false;
   /// <summary>
   /// Used as a button to create the instances of the mesh. This value does not actually do anything.
   /// Set to true to populate. Always return it to false to prevent errors with PathMeshPopulators from other scenes trying to populate (which throws an error).
   /// </summary>
   [Export]
   private bool Populate
   {
      get
      {
         return populate;
      }
      set
      {
         populate = value;

         if (populate)
         {
            PopulateMeshes();
         }
      }
   }
   /// <summary>
   /// Uses the in/out values of each point on the path as rotations. These values should be in degrees.
   /// </summary>
   [Export]
   private bool usePointRotations;

   /// <summary>
   /// Determines whether colliders should be generated for each mesh.
   /// </summary>
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
      }
   }

   /// <summary>
   /// Randomizes the scale of each mesh.
   /// </summary>
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

   /// <summary>
   /// If using collisions, set this to a StaticBody3D with the desired colliders. The colliders will scale, move, and rotate automatically with the mesh.
   /// </summary>
   private PackedScene collisionsPath;

   private Vector3 lowerScale;
   private Vector3 upperScale;

   private Vector3 lowerRotation;
   private Vector3 upperRotation;

   /// <summary>
   /// Places meshes on each point in the path, according to the populator's settings.
   /// </summary>
   void PopulateMeshes()
   {
      if (Engine.IsEditorHint())
      {
         GD.Print(Name);
         Path3D path = GetNode<Path3D>("Path3D");

         Multimesh.InstanceCount = path.Curve.PointCount;

         foreach (Node obj in GetChildren())
         {
            if (obj.Name != "Path3D")
            {
               obj.QueueFree();
               RemoveChild(obj);
            }
         }

         for (int i = 0; i < path.Curve.PointCount; i++)
         {
            Transform3D transform = new Transform3D(Basis.Identity, path.Curve.GetPointPosition(i) + path.GlobalPosition);

            if (randomizeRotation)
            {
               transform.Basis = transform.Basis.Rotated(Vector3.Right, (float)GD.RandRange(Mathf.DegToRad(lowerRotation.X), Mathf.DegToRad(upperRotation.X)));
               transform.Basis = transform.Basis.Rotated(Vector3.Up, (float)GD.RandRange(Mathf.DegToRad(lowerRotation.Y), Mathf.DegToRad(upperRotation.Y)));
               transform.Basis = transform.Basis.Rotated(Vector3.Back, (float)GD.RandRange(Mathf.DegToRad(lowerRotation.Z), Mathf.DegToRad(upperRotation.Z)));
            }

            if (usePointRotations)
            {
               // Try to get from the in
               Vector3 pointRotation = path.Curve.GetPointIn(i);

               // If there's no data for the in, try the out; some points only have an out value
               if (pointRotation == Vector3.Zero)
               {
                  pointRotation = path.Curve.GetPointOut(i);
               }

               // If the rotation is still zero, we shouldn't try to override and rotate. This supports combining randomization and preset point rotations
               // (those with nonzero ins/outs have preset rotations, those without should be randomized)
               if (pointRotation != Vector3.Zero)
               {
                  transform.Basis = Basis.Identity; // Overrides randomized rotations
                  transform.Basis = transform.Basis.Rotated(Vector3.Right, Mathf.DegToRad(pointRotation.X));
                  transform.Basis = transform.Basis.Rotated(Vector3.Up, Mathf.DegToRad(pointRotation.Y));
                  transform.Basis = transform.Basis.Rotated(Vector3.Back, Mathf.DegToRad(pointRotation.Z));
               }
            }

            if (randomizeScale)
            {
               Vector3 scale = new Vector3((float)GD.RandRange(lowerScale.X, upperScale.X), (float)GD.RandRange(lowerScale.Y, upperScale.Y), 
                                          (float)GD.RandRange(lowerScale.Z, upperScale.Z));
               transform.Basis = transform.Basis.Scaled(scale);
            }

            if (collisions)
            {
               StaticBody3D newCollider = GD.Load<PackedScene>(collisionsPath.ResourcePath).Instantiate<StaticBody3D>();
               newCollider.Transform = transform;
               AddChild(newCollider);
               newCollider.Owner = GetTree().EditedSceneRoot;
            }

            Multimesh.SetInstanceTransform(i, transform);
         }
      }
   }

   /// <summary>
   /// Used to show/hide certain settings depending on the checked booleans (e.g. checking RandomizeScale shows scale bounds)
   /// </summary>
   /// <returns>The property list</returns>
   public override Array<Dictionary> _GetPropertyList()
   {
      Array<Dictionary> result = new Array<Dictionary>();

      result.Add(new Dictionary()
      {
         { "name", $"Collisions" },
         { "type", (int)Variant.Type.Bool }
      });

      if (Collisions)
      {
         result.Add(new Dictionary()
         {
            { "name", $"collisionsPath" },
            { "type", (int)Variant.Type.Object }
         });
      }

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

      return result;
   }
}
#endif