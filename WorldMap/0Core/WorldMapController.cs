using Godot;
using System;

public partial class WorldMapController : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

   private RichTextLabel locationInfo;
   private LevelManager levelManager;

   private string targetLocationName;
   private string externalLocationName;

   private string entrancePointName;

   public bool DisableMovement { get; set; }

   public override void _Ready()
   {
      locationInfo = GetNode<RichTextLabel>("LocationInfo");
      levelManager = GetNode<LevelManager>("/root/BaseNode/LevelManager");
   }

   public override void _PhysicsProcess(double delta)
	{
      if (!DisableMovement)
      {
         Vector2 velocity = Velocity;

         // Get the input direction and handle the movement/deceleration.
         // As good practice, you should replace UI actions with custom gameplay actions.
         Vector2 direction = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
         if (direction != Vector2.Zero)
         {
            velocity.Y = direction.Y * Speed;
            velocity.X = direction.X * Speed;
         }
         else
         {
            velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
         }

         Velocity = velocity;
         MoveAndSlide();
      }
	}

   public void ReceiveIntersectionData(string labelName, string locationName, string entrancePoint)
   {
      locationInfo.Text = "[center]" + labelName + "[/center]";
      targetLocationName = locationName;
      externalLocationName = labelName;
      locationInfo.Visible = true;
      entrancePointName = entrancePoint;
   }

   public void HideIntersectionLabel()
   {
      locationInfo.Visible = false;
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("interact") && locationInfo.Visible)
      {
         levelManager.TransitionLevels(targetLocationName, externalLocationName, entrancePointName);
      }
   }
}
