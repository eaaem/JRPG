using Godot;
using Godot.NativeInterop;
using System;

/// <summary>
/// Creates a set of trimesh colliders for each of the source mesh's materials. Useful for different sounds for footsteps.
/// </summary>
[Tool]
public partial class MeshSplitter : Node
{
   [Export]
   private MeshInstance3D mesh;
   [Export]
   private bool considerParentTransform;
	
   private bool activate;
   [Export]
   private bool Activate
   {
      get { return activate; }
      set { Generate(); }
   }

   void Generate()
   {
      if (mesh == null)
      {
         GD.PrintErr("Unable to create colliders: No source mesh found (did you forget to delete the MeshSplitter?).");
         return;
      }

      MeshDataTool meshDataTool = new MeshDataTool();
      for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
      {
         meshDataTool.CreateFromSurface((ArrayMesh)mesh.Mesh, i);

         Vector3[] vertices = new Vector3[meshDataTool.GetFaceCount() * 3];

         int vertexIndex = 0;
         for (int j = 0; j < meshDataTool.GetFaceCount(); j++)
         {
            vertices[vertexIndex] = meshDataTool.GetVertex(meshDataTool.GetFaceVertex(j, 0));
            vertices[vertexIndex + 1] = meshDataTool.GetVertex(meshDataTool.GetFaceVertex(j, 1));
            vertices[vertexIndex + 2] = meshDataTool.GetVertex(meshDataTool.GetFaceVertex(j, 2));
            vertexIndex += 3;
         }

         CollisionShape3D collider = new CollisionShape3D();
         ConcavePolygonShape3D shape = new ConcavePolygonShape3D();
         shape.Data = vertices;
         
         collider.Shape = shape;
         StaticBody3D body = new StaticBody3D();
         body.AddChild(collider);

         AddChild(body);
         body.Owner = GetParent();
         collider.Owner = GetParent();
         body.Name = mesh.Mesh.SurfaceGetMaterial(i).ResourceName;

         collider.Scale = mesh.Scale;
         collider.Rotation = collider.Rotation;

         if (considerParentTransform)
         {
            Node3D parent = mesh.GetParent<Node3D>();
            collider.Scale = new Vector3(collider.Scale.X * parent.Scale.X, collider.Scale.Y * parent.Scale.Y, collider.Scale.Z * parent.Scale.Z);
            collider.Rotation = new Vector3(collider.Rotation.X + parent.Rotation.X, collider.Rotation.Y + parent.Rotation.Y, collider.Rotation.Z + parent.Rotation.Z);
         }
      }
      
      GD.Print("Created colliders. Remember to remove the MeshSplitter when you're done!");
   }
}
