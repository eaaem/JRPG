using Godot;

public partial class CharacterController : CharacterBody3D
{
   public float RegularSpeed = 5.0f;
   public float SprintSpeed = 10.0f;
   public const float JumpVelocity = 4.5f;
   public float HorizontalSensitivity = 0.5f;
   float sensitivityModifier = 1f;
   float maxClamp = 90f;

   public bool IsSprinting { get; set; }

   [Export]
   private bool isWorldMap;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

   private Node3D cameraTarget;
   private Node3D model;
   private AnimationPlayer animationPlayer;
   private AnimationTree animationTree;
   private AnimationTree methodsTree;

   private Node3D weapon;
   private Node3D weaponAnchor;

   private Node3D secondaryWeapon;
   private Node3D secondaryAnchor;

   BoneAttachment3D attachment;
   BoneAttachment3D secondaryAttachment;

   public bool DisableMovement { get; set; }
   public bool DisableCamera { get; set; }
   public bool DisableGravity { get; set; }
   public bool IsInCutscene { get; set; }

   public int MovementBlend { get; set; } = 0;
   public Vector3 OverridenTargetLocation { get; set; } = Vector3.Zero;
   public bool IsOverridingMovement { get; set; }
   public float TargetOverridenRotation { get; set; }
   private int overrideTimeoutTimer = 0;

   public override void _Ready()
   {
      if (!isWorldMap)
      {
         sensitivityModifier = 1f;
         maxClamp = 90f;
         ResetNodes();
      }
      else
      {
         sensitivityModifier = 0.5f;
         maxClamp = 45f;
         GetWorldMapNodes();
      }
   }

   public void ResetNodes()
   {
      cameraTarget = GetNode<Node3D>("CameraTarget");
      model = GetNode<Node3D>("Model");
      animationPlayer = GetNode<AnimationPlayer>("Model/AnimationPlayer");
      animationTree = GetNode<AnimationTree>("BaseTree");
      methodsTree = GetNode<AnimationTree>("MethodsTree");

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

   private void GetWorldMapNodes()
   {
      cameraTarget = GetNode<Node3D>("CameraTarget");
      model = GetNode<Node3D>("Model");
      animationTree = GetNode<AnimationTree>("AnimationTree");
   }

   public override void _PhysicsProcess(double delta)
	{
      Vector3 velocity = Velocity;

      // Add the gravity
      if (!IsOnFloor() && !DisableGravity) {
         velocity.Y -= gravity * (float)delta;
      }

      if (!DisableMovement)
      {
         Vector3 direction;
         Vector2 inputDir;

         if (!IsOverridingMovement)
         {
            inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
         }
         else
         {
            direction = GlobalPosition.DirectionTo(OverridenTargetLocation);
            inputDir = new Vector2(direction.X, direction.Y);

            float distance = new Vector3(GlobalPosition.X, 0f, GlobalPosition.Z).DistanceSquaredTo(new Vector3(OverridenTargetLocation.X, 0f, OverridenTargetLocation.Z));
            overrideTimeoutTimer++;

            if (distance < 0.5f || overrideTimeoutTimer > 120)
            {
               IsOverridingMovement = false;
               RotateY(cameraTarget.Rotation.Y);
               model.RotateY(-cameraTarget.Rotation.Y);
               cameraTarget.RotateY(-cameraTarget.Rotation.Y);
               overrideTimeoutTimer = 0;
               return;
            }
         }

         float speedToUse = IsSprinting ? SprintSpeed : RegularSpeed;

         if (Input.IsActionPressed("sprint") && !isWorldMap)
         {
            IsSprinting = true;
         }

         if (Input.IsActionJustReleased("sprint") || IsOverridingMovement)
         {
            IsSprinting = false;
         }

         if (direction != Vector3.Zero)
         {
            if ((IsSprinting && MovementBlend < 10) || (!IsSprinting && MovementBlend < 0)) 
            {
               MovementBlend += 1;
            }
            else if (!IsSprinting && MovementBlend > 0)
            {
               MovementBlend -= 1;
            }
            
            Vector3 modelRotation = model.Rotation;

            if (!IsOverridingMovement)
            {
               velocity.X = -direction.X * speedToUse;
               velocity.Z = -direction.Z * speedToUse;
               modelRotation.Y = Mathf.LerpAngle(model.Rotation.Y, Mathf.Atan2(-inputDir.X, -inputDir.Y), 0.25f);
            }
            else
            {
               direction = new Vector3(direction.X, 0f, direction.Z);
               velocity = direction * speedToUse;

               modelRotation.Y = Mathf.Lerp(modelRotation.Y, TargetOverridenRotation - Rotation.Y, 0.25f);
            }

            model.Rotation = modelRotation;
         }
         else
         {
            if (MovementBlend > -10)
            {
               MovementBlend -= 1;
            }

            velocity.X = Mathf.MoveToward(Velocity.X, 0, speedToUse);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speedToUse);
         }

         animationTree.Set("parameters/BasicMovement/blend_position", MovementBlend / 10f);

         if (!isWorldMap)
         {
            methodsTree.Set("parameters/Movement/blend_position", MovementBlend / 10f);
         }

         Velocity = velocity;

         MoveAndSlide();
      }
      else
      {
         Velocity = Vector3.Zero;
         animationTree.Set("parameters/BasicMovement/blend_position", -1f);
         MovementBlend = -10;

         if (!isWorldMap)
         {
            methodsTree.Set("parameters/Movement/blend_position", -1f);
         }
      }
	}

   public override void _Input(InputEvent @event)
   {
      if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured && !DisableCamera)
      {
         // Rotates the character controller, which in turn rotates the camera
         if (!IsInCutscene && !IsOverridingMovement)
         {
            RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * HorizontalSensitivity * sensitivityModifier * 0.25f));
         }
         else
         {
            cameraTarget.RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * HorizontalSensitivity * sensitivityModifier * 0.25f));
         }

         // Rotate the model accordingly
         if (!IsInCutscene && !IsOverridingMovement)
         {
            model.RotateY(Mathf.DegToRad(mouseMotion.Relative.X * HorizontalSensitivity * sensitivityModifier * 0.25f));
         }

         // Rotates along the x-axis, clamping to prevent too much rotation
         Vector3 clampRotation = cameraTarget.Rotation;
         clampRotation.X -= (mouseMotion.Relative.Y * HorizontalSensitivity * sensitivityModifier * 0.25f) / 50f;
         clampRotation.X = Mathf.Clamp(clampRotation.X, Mathf.DegToRad(-45f), Mathf.DegToRad(maxClamp));
         cameraTarget.Rotation = clampRotation;
      }
   }

   public void PlaceWeaponOnBack()
   {
      attachment.BoneName = weaponAnchor.GetChild(0).Name;

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
