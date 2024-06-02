#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class PathMeshGenerator : MeshInstance3D
{
   private bool generate;
   [Export]
   private bool Generate
   {
      get
      {
         return generate;
      }
      set
      {
         GenerateMesh();
      }
   }
	
	void GenerateMesh()
   {
      Path3D path = GetNode<Path3D>("Path3D");

      Godot.Collections.Array meshData = new Godot.Collections.Array();
      meshData.Resize((int)Mesh.ArrayType.Max);

      List<Vector3> verts = new List<Vector3>();
      List<Vector2> uvs = new List<Vector2>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();

      for (int i = 0; i < path.Curve.PointCount; i++)
      {
         verts.Add(path.Curve.GetPointPosition(i));
         uvs.Add(new Vector2(0, 0));
         normals.Add(new Vector3(0, 0, 1));

      }

      // Convert Lists to arrays and assign to surface array
      meshData[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
      meshData[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
      meshData[(int)Mesh.ArrayType.Normal] = normals.ToArray();
      //meshData[(int)Mesh.ArrayType.Index] = indices.ToArray();

      var arrMesh = Mesh as ArrayMesh;
      if (arrMesh != null)
      {
         arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshData);
      }
   }
}
#endif