using Godot;
using System;

public partial class OverworldPartyController : CharacterBody3D
{
   [Export]
   private int index;

   private const float DistanceThreshold = 5f;

   [Export]
   private ManagerReferenceHolder managers;

   private AnimationPlayer animationPlayer;
   private NavigationAgent3D navigationAgent;

   private CharacterController player;
   private Node3D playerModel;

   private Node3D model;

   private Node3D weapon;
   private Node3D weaponAnchor;

   private Node3D secondaryWeapon;
   private Node3D secondaryAnchor;

   Skeleton3D skeleton;
   BoneAttachment3D attachment;
   BoneAttachment3D secondaryAttachment;

   private Vector3 targetPosition;
   private float distance;

   private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

   public bool EnablePathfinding { get; set; }

   public bool IsActive { get; set; }

   private Member assignedPartyMember;

   public Vector3 MovementTarget
   {
      get { return navigationAgent.TargetPosition; }
      set { navigationAgent.TargetPosition = value; }
   }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{ 
      EnablePathfinding = true;

      if (IsActive)
      {
         ResetNodes(null);
      }

      managers.PartyMenuManager.PartySwap += OnSwap;
	}

   void OnSwap()
   {
      // Every time we swap, we need to reset the player model for ALL current pathfinders
      player = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");
      playerModel = player.GetNode<Node3D>("Model");
   }

   public void ResetNodes(Member member)
   {
      animationPlayer = GetNode<AnimationPlayer>("Model/AnimationPlayer");
      navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

      model = GetNode<Node3D>("Model");

      player = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");
      playerModel = player.GetNode<Node3D>("Model");

      weapon = GetNode<Node3D>("Weapon");
      weaponAnchor = GetNode<Node3D>("BackAnchor");

      secondaryWeapon = GetNode<Node3D>("SecondaryWeapon");
      secondaryAnchor = GetNode<Node3D>("SecondaryAnchor");

      attachment = new BoneAttachment3D();
      attachment.Name = "WeaponAttachment";
      skeleton = model.GetNode<Skeleton3D>("Armature/Skeleton3D");
      skeleton.AddChild(attachment);
      attachment.BoneName = "torso";

      RemoveChild(weapon);
      RemoveChild(secondaryWeapon);
      attachment.AddChild(weapon);

      secondaryAttachment = new BoneAttachment3D();
      secondaryAttachment.Name = "SecondaryWeaponAttachment";
      secondaryAttachment.BoneName = "torso";

      skeleton.AddChild(secondaryAttachment);
      secondaryAttachment.AddChild(secondaryWeapon);

      PlaceWeaponOnBack();
      PlaceSecondaryWeaponOnBack();

      if (member != null)
      {
         assignedPartyMember = member;
      }

      Callable.From(ActorSetup).CallDeferred();
   }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
      if (EnablePathfinding && IsActive)
      {
         Vector3 velocity = Velocity;
         distance = Position.DistanceTo(player.Position);

         if (!IsOnFloor())
         {
            velocity.Y -= gravity * (float)delta;
         }

         if (distance >= DistanceThreshold)
         {
            targetPosition = player.Position - (playerModel.GlobalTransform.Basis.Z * (1.5f * index));
            MovementTarget = targetPosition;
         }

         if (navigationAgent.IsNavigationFinished())
         {
            if (animationPlayer.CurrentAnimation != "Idle") 
            {
               animationPlayer.Play("Idle");
            }

            return;
         }

         Vector3 currentAgentPosition = GlobalTransform.Origin;
         Vector3 nextPathPosition = navigationAgent.GetNextPathPosition();

         Vector3 modelRotation = model.Rotation;
         modelRotation.Y = Mathf.LerpAngle(model.Rotation.Y, Mathf.Atan2(velocity.X, velocity.Z), 0.25f);
         model.Rotation = modelRotation;

         float speed = player.IsSprinting ? CharacterController.SprintSpeed : CharacterController.RegularSpeed;

         velocity.X = currentAgentPosition.DirectionTo(nextPathPosition).X * speed;
         velocity.Z = currentAgentPosition.DirectionTo(nextPathPosition).Z * speed;

         if (!player.IsSprinting)
         {
            if (animationPlayer.CurrentAnimation != "Walk")
            {
               animationPlayer.Play("Walk");
            }
         }
         else
         {
            if (animationPlayer.CurrentAnimation != "Run")
            {
               animationPlayer.Play("Run");
            }
         }

         Velocity = velocity;
         MoveAndSlide();
      }
	}

   private async void ActorSetup()
   {
      // Wait for the first physics frame so the NavigationServer can sync.
      await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

      // Now that the navigation map is no longer empty, set the movement target.
      MovementTarget = targetPosition;
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
}
