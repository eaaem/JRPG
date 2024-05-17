using Godot;
using System;

public partial class PeasantBehavior : CharacterBody3D
{
   private PeasantCreator peasantCreator;
   private NavigationAgent3D navigationAgent;

   private PathFollow3D path;

   //private Vector3 targetPosition;
   private Node3D model;

   private int index;

   /*public Vector3 MovementTarget
   {
      get { return navigationAgent.TargetPosition; }
      set { navigationAgent.TargetPosition = value; }
   }*/
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      //navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
      model = GetNode<Node3D>("Model");

      model.GetNode<AnimationPlayer>("AnimationPlayer").Play("Walk");

      //Callable.From(ActorSetup).CallDeferred();
	}

   public void SetupStartingIndex(int index)
   {
      this.index = index;
   }

   public void SetupPath(PathFollow3D path)
   {
      this.path = path;
   }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
      path.ProgressRatio += 0.00001f;
      /*Vector3 velocity = Velocity;
      float distance = GlobalPosition.DistanceTo(path.Curve.GetPointPosition(index));

      if (distance <= 0.15f)
      {
         NextPoint();
      }

      MovementTarget = targetPosition;
      Vector3 currentAgentPosition = GlobalTransform.Origin;
      Vector3 nextPathPosition = navigationAgent.GetNextPathPosition();

      Vector3 modelRotation = model.Rotation;
      modelRotation.Y = Mathf.LerpAngle(model.Rotation.Y, Mathf.Atan2(velocity.X, velocity.Z), 0.25f);
      model.Rotation = modelRotation;

      velocity.X = currentAgentPosition.DirectionTo(nextPathPosition).X * 2f;
      velocity.Z = currentAgentPosition.DirectionTo(nextPathPosition).Z * 2f;

      Velocity = velocity;
      MoveAndSlide();*/
	}

   /*public void NextPoint()
   {
      index++;

      if (index + 1 >= path.Curve.PointCount)
      {
         index = 0;
      }

      targetPosition = path.Curve.GetPointPosition(index);
   }

   private async void ActorSetup()
   {
      // Wait for the first physics frame so the NavigationServer can sync.
      await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

      // Now that the navigation map is no longer empty, set the movement target.
      MovementTarget = targetPosition;
   }*/
}
