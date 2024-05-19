using Godot;
using System;
using System.Threading;

public partial class CharacterController : CharacterBody3D
{
   public const float RegularSpeed = 5.0f;
   public const float SprintSpeed = 10.0f;
   public const float JumpVelocity = 4.5f;
   public float HorizontalSensitivity = 0.5f;

   public bool IsSprinting { get; set; }

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

   private Node3D cameraTarget;
   private Node3D model;
   private AnimationPlayer animationPlayer;

   private Node3D weapon;
   private Node3D weaponAnchor;

   private Node3D secondaryWeapon;
   private Node3D secondaryAnchor;

   BoneAttachment3D attachment;
   BoneAttachment3D secondaryAttachment;

   public bool DisableMovement { get; set; }

   public override void _Ready()
   {
      ResetNodes();
   }

   public void ResetNodes()
   {
      cameraTarget = GetNode<Node3D>("CameraTarget");
      model = GetNode<Node3D>("Model");
      animationPlayer = GetNode<AnimationPlayer>("Model/AnimationPlayer");

      weapon = GetNode<Node3D>("Weapon");
      weaponAnchor = GetNode<Node3D>("BackAnchor");

      secondaryWeapon = GetNode<Node3D>("SecondaryWeapon");
      secondaryAnchor = GetNode<Node3D>("SecondaryAnchor");

      RemoveChild(weapon);
      RemoveChild(secondaryWeapon);

      attachment = new BoneAttachment3D();
      attachment.Name = "WeaponAttachment";
      Skeleton3D skeleton = model.GetNode<Skeleton3D>("Armature/Skeleton3D");

      skeleton.AddChild(attachment);
      attachment.AddChild(weapon);

      secondaryAttachment = new BoneAttachment3D();
      secondaryAttachment.Name = "SecondaryWeaponAttachment";

      skeleton.AddChild(secondaryAttachment);
      secondaryAttachment.AddChild(secondaryWeapon);

      PlaceWeaponOnBack();
      PlaceSecondaryWeaponOnBack();
   }

   public override void _PhysicsProcess(double delta)
	{
      Vector3 velocity = Velocity;

      // Add the gravity; gravity should stay, even in combat
      if (!IsOnFloor()) {
         velocity.Y -= gravity * (float)delta;
      }

      // This disables movement in combat (the mouse is unlocked in combat)
      if (!DisableMovement)
      {
         Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

         float speedToUse = IsSprinting ? SprintSpeed : RegularSpeed;

         Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

         if (Input.IsActionPressed("sprint"))
         {
            IsSprinting = true;

            if (animationPlayer.CurrentAnimation != "Run" && direction != Vector3.Zero) 
            {
               animationPlayer.Play("Run");
            }
         }

         if (Input.IsActionJustReleased("sprint"))
         {
            IsSprinting = false;
         }

         if (direction != Vector3.Zero)
         {
            if (animationPlayer.CurrentAnimation != "Walk" && !IsSprinting) 
            {
               animationPlayer.Play("Walk");
            }
            
            //model.LookAt(Position + direction);
            Vector3 modelRotation = model.Rotation;
            modelRotation.Y = Mathf.LerpAngle(model.Rotation.Y, Mathf.Atan2(-inputDir.X, -inputDir.Y), 0.25f);
            model.Rotation = modelRotation;

            velocity.X = -direction.X * speedToUse;
            velocity.Z = -direction.Z * speedToUse;
         }
         else
         {
            if (animationPlayer.CurrentAnimation != "Idle")
            {
               animationPlayer.Play("Idle");
            }

            velocity.X = Mathf.MoveToward(Velocity.X, 0, speedToUse);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speedToUse);
         }

         Velocity = velocity;

         MoveAndSlide();
      }
	}

   public override void _Input(InputEvent @event)
   {
	  if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured && !DisableMovement) {
		 // Rotates the camera target, which in turn rotates the camera
		 RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * HorizontalSensitivity));

		 // Rotate the model accordingly
		 model.RotateY(Mathf.DegToRad(mouseMotion.Relative.X * HorizontalSensitivity));

		 // Rotates along the x-axis, clamping to prevent too much rotation
		 Vector3 clampRotation = cameraTarget.Rotation;
		 clampRotation.X -= mouseMotion.Relative.Y / 50f;
		 clampRotation.X = Mathf.Clamp(clampRotation.X, Mathf.DegToRad(-45f), Mathf.DegToRad(90f));
		 cameraTarget.Rotation = clampRotation;
	  }
   }

   public void PlaceWeaponOnBack()
   {
      attachment.BoneName = "torso";

      weapon.Position = weaponAnchor.Position;
      weapon.Rotation = weaponAnchor.Rotation;
   }

   public void PlaceSecondaryWeaponOnBack()
   {
      secondaryAttachment.BoneName = "torso";

      secondaryWeapon.Position = secondaryAnchor.Position;
      secondaryWeapon.Rotation = secondaryAnchor.Rotation;
   }

   public void HideWeapon()
   {
      weapon.Visible = false;
   }

   public void ShowWeapon()
   {
      weapon.Visible = true;
   }
}
