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
   private float scaleFactor = 1f;
	
   private bool activate;
   [Export]
   private bool Activate
   {
      get { return activate; }
      set { Generate(); }
   }

   void Generate()
   {
      MeshDataTool meshDataTool = new MeshDataTool();
      for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
      {
         meshDataTool.CreateFromSurface((ArrayMesh)mesh.Mesh, i);
         Vector3[] vertices = new Vector3[meshDataTool.GetVertexCount()];

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
         collider.Scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
      }
      
      GD.Print("Created colliders. If they seem invisible, double check their scale and the scale of the source mesh. Remember to remove the MeshSplitter when you're done!");
   }
}
