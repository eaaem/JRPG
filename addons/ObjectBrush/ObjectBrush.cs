#if TOOLS
using Godot;
using System;
using System.Reflection;

[Tool]
public partial class ObjectBrush : EditorPlugin
{
   private Control containerMenu;

   private bool isPainting;
   private bool isErasing;

   private GodotObject previousObject;
   private GodotObject currentObject;
   private ObjectSet currentSet;

   private Control viewControl;
   private Camera3D viewCamera;

   private MeshInstance3D visibleMesh;

	public override void _EnterTree()
	{
		// Initialization of the plugin
      AddCustomType("ObjectSet", "MultiMeshInstance3D", GD.Load<Script>("res://addons/ObjectBrush/ObjectSet.cs"), GD.Load<Texture2D>("res://addons/ObjectBrush/Icon.png"));
      containerMenu = GD.Load<PackedScene>("res://addons/ObjectBrush/ObjectSetContainer.tscn").Instantiate<Control>();
      containerMenu.GetNode<CheckButton>("PaintButton").ButtonDown += SetPaint;
      containerMenu.GetNode<CheckButton>("EraseButton").ButtonDown += SetErase;
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin
      RemoveCustomType("ObjectSet");
      containerMenu.QueueFree();
      containerMenu.GetNode<CheckButton>("PaintButton").ButtonDown -= SetPaint;
      containerMenu.GetNode<CheckButton>("EraseButton").ButtonDown -= SetErase;
      
      DestroyMesh();
	}

   public override bool _Handles(GodotObject @object)
   {
      previousObject = currentObject;
      currentObject = @object;
   
      if (@object.HasMethod("AddMesh"))
      {
         currentSet = (ObjectSet)currentObject;
         Reset();
      }

      return @object.HasMethod("AddMesh");
   }

   public override void _MakeVisible(bool visible)
   {
      if (visible)
      {
         AddControlToContainer(CustomControlContainer.SpatialEditorMenu, containerMenu);

         currentSet = (ObjectSet)currentObject;
         currentSet.Multimesh.Mesh.Changed += CreateMesh;
      }
      else
      {
         RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, containerMenu);

         Reset();
         currentSet.Multimesh.Mesh.Changed -= CreateMesh;
         DestroyMesh();
         currentSet = null;
      }
   }

   void Reset()
   {
      isPainting = false;
      isErasing = false;
      containerMenu.GetNode<CheckButton>("PaintButton").ButtonPressed = false;
      containerMenu.GetNode<CheckButton>("EraseButton").ButtonPressed = false;
   }

   void SetPaint()
   {
      if (isPainting)
      {
         isPainting = false;
         DestroyMesh();
      }
      else
      {
         if (isErasing)
         {
            isErasing = false;
            containerMenu.GetNode<CheckButton>("EraseButton").ButtonPressed = false;
         }

         isPainting = true;

         if (currentSet.Multimesh.Mesh != null)
         {
            CreateMesh();
         }
      }
   }

   void CreateMesh()
   {
      if (isPainting)
      {
         if (visibleMesh != null)
         {
            visibleMesh.QueueFree();
         }

         visibleMesh = new MeshInstance3D();
         visibleMesh.Mesh = currentSet.Multimesh.Mesh;
         AddChild(visibleMesh);
      }
   }

   void SetErase()
   {
      if (isErasing)
      {
         isErasing = false;
      }
      else
      {
         if (isPainting)
         {
            isPainting = false;
            containerMenu.GetNode<CheckButton>("PaintButton").ButtonPressed = false;
         }

         isErasing = true;
      }

      DestroyMesh();
   }

   void DestroyMesh()
   {
      if (visibleMesh != null)
      {
         RemoveChild(visibleMesh);
         visibleMesh.QueueFree();
         visibleMesh = null;
      }
   }

   public override void _Forward3DDrawOverViewport(Control viewportControl)
   {
      viewControl = viewportControl;
   }

   public override int _Forward3DGuiInput(Camera3D viewportCamera, InputEvent @event)
   {
      viewCamera = viewportCamera;
      if (@event is InputEventMouseButton && @event.IsPressed() && ((InputEventMouseButton)(@event)).ButtonIndex == MouseButton.Left)
      {
         Vector3 raycastedPoint = Raycast(viewportCamera);

         if (raycastedPoint != Vector3.Zero)
         {
            MouseButtonDown(raycastedPoint);
         }

         return (int)AfterGuiInput.Stop;
      }
      else if (@event is InputEventMouseMotion && isPainting)
      {
         Vector3 raycastedPoint = Raycast(viewportCamera);
         visibleMesh.Position = raycastedPoint;
      }

      if (@event is InputEventKey && isPainting)
      {
         if (((InputEventKey)(@event)).Keycode == Key.A)
         {
            visibleMesh.RotateY(-0.1f);
         }
         else if (((InputEventKey)(@event)).Keycode == Key.D)
         {
            visibleMesh.RotateY(0.1f);
         }
         else if (((InputEventKey)(@event)).Keycode == Key.S)
         {
            visibleMesh.Rotation = Vector3.Zero;
         }
      }

      return (int)AfterGuiInput.Pass;
   }

   Vector3 Raycast(Camera3D viewportCamera)
   {
      PhysicsDirectSpaceState3D spaceState = GetViewport().World3D.DirectSpaceState;
      Vector2 mousePosition = viewControl.GetLocalMousePosition();

      Vector3 origin = viewportCamera.ProjectRayOrigin(mousePosition);
      Vector3 end = origin + viewportCamera.ProjectRayNormal(mousePosition) * 5000f;
      // 64 = layer 7, which is where terrain is
      PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end, 64);
      query.CollideWithAreas = true;

      var result = spaceState.IntersectRay(query);

      if (result.Count > 0)
      {
         return (Vector3)result["position"];
      }
      else
      {
         return Vector3.Zero;
      }
   }

   void MouseButtonDown(Vector3 point)
   {
      if (isPainting)
      {
         currentSet.AddMesh(point, visibleMesh.Rotation);
      }
      else
      {
         currentSet.AttemptErase(point);
      }
   }
}
#endif
