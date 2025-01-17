using Godot;
using System;

public partial class OverworldPartyController : CharacterBody3D
{
   [Export]
   private int index;

   private const float DistanceThreshold = 10f;

   [Export]
   private ManagerReferenceHolder managers;

   private AnimationPlayer animationPlayer;
   private NavigationAgent3D navigationAgent;
   private AnimationTree animationTree;
   private AnimationTree methodsTree;

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
   private bool isSynced = false;

   private Member assignedPartyMember;

   private int movementBlend = 0;

   public Vector3 MovementTarget
   {
      get { return navigationAgent.TargetPosition; }
      set { if (navigationAgent != null) { navigationAgent.TargetPosition = value; } }
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
      animationTree = GetNode<AnimationTree>("BaseTree");
      methodsTree = GetNode<AnimationTree>("MethodsTree");

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

      ResetNavigation();
   }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
      if (!EnablePathfinding && IsActive)
      {
         movementBlend = -10;
         methodsTree.Set("parameters/Movement/blend_position", movementBlend / 10f);
      }

      if (EnablePathfinding && IsActive && isSynced)
      {
         animationTree.Set("parameters/BasicMovement/blend_position", movementBlend / 10f);
         methodsTree.Set("parameters/Movement/blend_position", movementBlend / 10f);
         
         Vector3 velocity = Velocity;
         distance = GlobalPosition.DistanceSquaredTo(player.GlobalPosition);

         if (!IsOnFloor())
         {
            velocity.Y -= gravity * (float)delta;
         }

         if (distance >= DistanceThreshold + (3f * index))
         {
            targetPosition = player.GlobalPosition - (playerModel.GlobalTransform.Basis.Z * (1.5f * index));
            MovementTarget = targetPosition;
         }

         if (navigationAgent.IsNavigationFinished())
         {
            if (movementBlend > -10)
            {
               movementBlend -= 1;
            }

            return;
         }

         Vector3 currentAgentPosition = GlobalTransform.Origin;

         // This offset is necessary because the navmeshes don't actually match the ground collisions. This results in the path being generated at a Y level
         // that the agent can't possibly reach, making them lock in place because they can't get close enough to the next point. By altering the Y offset,
         // the next path target is always close enough to the agent for them to arrive at it and prevent getting stuck.
         navigationAgent.PathHeightOffset = -currentAgentPosition.Y;

         Vector3 nextPathPosition = navigationAgent.GetNextPathPosition();

         Vector3 modelRotation = model.Rotation;
         modelRotation.Y = Mathf.LerpAngle(model.Rotation.Y, Mathf.Atan2(velocity.X, velocity.Z), 0.25f);
         model.Rotation = modelRotation;

         float speed = player.IsSprinting ? managers.Controller.SprintSpeed : managers.Controller.RegularSpeed;

         velocity.X = currentAgentPosition.DirectionTo(nextPathPosition).X * speed * 1.1f;
         velocity.Z = currentAgentPosition.DirectionTo(nextPathPosition).Z * speed * 1.1f;

         if ((player.IsSprinting && movementBlend < 10) || (!player.IsSprinting && movementBlend < 0)) 
         {
            movementBlend += 1;
         }
         else if (!player.IsSprinting && movementBlend > 0)
         {
            movementBlend -= 1;
         }

         Velocity = velocity;
         MoveAndSlide();
      }
	}

   public void ResetNavigation()
   {
      Callable.From(ActorSetup).CallDeferred();
   }

   private async void ActorSetup()
   {
      // Wait for the first physics frame so the NavigationServer can sync.
      await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);


      // Now that the navigation map is no longer empty, set the movement target.
      MovementTarget = targetPosition;
      isSynced = true;
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
