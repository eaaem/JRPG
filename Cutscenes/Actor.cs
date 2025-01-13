using Godot;
using System;

public partial class Actor : CharacterBody3D
{
  // int rotationCounter = 0;
  // int movementCounter = 0;

   [Export]
   private bool hasWeapon;

   bool isMoving;
   float currentMoveSpeed = 0f;
   Vector3 currentDestination;
   Vector3 currentDirection;

   private bool isRotating;
   private float currentTargetRotation;

   private Node3D weapon;
   private Node3D weaponAnchor;

   private Node3D secondaryWeapon;
   private Node3D secondaryAnchor;

   private BoneAttachment3D attachment;
   private AnimationPlayer methodsPlayer;

   private bool isTracking;
   Actor trackingTarget;
   private float trackingTargetRotation;
   Node3D model;

   public override void _Ready()
   {
      model = GetNode<Node3D>("Model");
      methodsPlayer = GetNode<AnimationPlayer>("MethodsPlayer");

      if (hasWeapon)
      {
         weapon = GetNode<Node3D>("Weapon");
         weaponAnchor = GetNode<Node3D>("BackAnchor");

         //secondaryWeapon = GetNode<Node3D>("SecondaryWeapon");
         //secondaryAnchor = GetNode<Node3D>("SecondaryAnchor");

         RemoveChild(weapon);

         attachment = new BoneAttachment3D();
         attachment.Name = "WeaponAttachment";
         Skeleton3D skeleton = GetNode<Skeleton3D>("Model/Armature/Skeleton3D");

         skeleton.AddChild(attachment);
         attachment.AddChild(weapon);

         PlaceWeaponOnBack();
      }
   }

   public void WieldWeapon()
   {
      if (!hasWeapon)
      {
         GD.PrintErr("Weapon not found when trying to wield on actor " + Name);
         return;
      }

      attachment.BoneName = "middle_arm.L.001";

      weapon.Position = GetNode<Node3D>("WieldAnchor").Position;
      weapon.Rotation = GetNode<Node3D>("WieldAnchor").Rotation;
   }

   public void PlaceWeaponOnBack()
   {
      if (!hasWeapon)
      {
         GD.PrintErr("Weapon not found when trying to put on back on actor " + Name);
         return;
      }

      attachment.BoneName = "torso";

      weapon.Position = weaponAnchor.Position;
      weapon.Rotation = weaponAnchor.Rotation;
   }

   public void ActorFootstep()
   {
      PhysicsDirectSpaceState3D spaceState = GetNode<Node3D>("/root/BaseNode").GetWorld3D().DirectSpaceState;

      Vector3 origin = Position + Vector3.Up;
      Vector3 end = origin + (Vector3.Down * 5f);
      // 64 = layer 7, which is where terrain is
      PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end, 64);
      query.CollideWithAreas = true;

      var result = spaceState.IntersectRay(query);

      string groupName = "dirt";

      if (result.Count > 0)
      {
         StaticBody3D collided = (StaticBody3D)result["collider"];
         
         if (collided.IsInGroup("grass"))
         {
            groupName = "grass";
         }
         else if (collided.IsInGroup("stone"))
         {
            groupName = "stone";
         }
         else if (collided.IsInGroup("wood"))
         {
            groupName = "wood";
         }
      }

      Node3D group = GetNode<Node3D>(groupName + "Footsteps");
      AudioStreamPlayer3D toPlay = group.GetChild<AudioStreamPlayer3D>(GD.RandRange(0, 5));
      toPlay.Play();
   }

   /*public void PlaceSecondaryWeaponOnBack()
   {
      secondaryAttachment.BoneName = "torso";

      secondaryWeapon.Position = secondaryAnchor.Position;
      secondaryWeapon.Rotation = secondaryAnchor.Rotation;
   }*/

   public void HideWeapon()
   {
      weapon.Visible = false;
   }

   public void ShowWeapon()
   {
      weapon.Visible = true;
   }

   public async void MoveCharacter(ActorStatus actorStatus, Vector3 destination, bool turnToDestination)
   {
      Vector3 direction = GlobalPosition.DirectionTo(destination);
      float distance = GlobalPosition.DistanceSquaredTo(destination);

      currentMoveSpeed = actorStatus.moveSpeed;
      currentDestination = destination;
      currentDirection = direction;
      isMoving = true;

      if (turnToDestination)
      {
         Node3D model = GetNode<Node3D>("Model");
         model.LookAt(destination);
         model.RotateY(Mathf.DegToRad(180));
         model.Rotation = new Vector3(0f, model.Rotation.Y, 0f);
      }

      AnimationPlayer player = GetNode<AnimationPlayer>("Model/AnimationPlayer");
      player.Play(actorStatus.walkAnim, 0.5f);
      methodsPlayer.Play("WalkSounds", 0.5f);

      while (distance > 0.5f)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         distance = GlobalPosition.DistanceSquaredTo(destination); 
      }

      isMoving = false;

      Velocity = Vector3.Zero;
      player.Play(actorStatus.idleAnim);
      methodsPlayer.Stop();
   }

   public void RotateCharacter(float targetRotation)
   {
      currentTargetRotation = Mathf.DegToRad(targetRotation);
      isRotating = true;
   }

   public void TurnCharacterToLookAt(Vector3 targetPosition)
   {
      Basis lookAt = Basis.LookingAt(targetPosition - GlobalPosition, Vector3.Up, true);
      currentTargetRotation = lookAt.GetEuler().Y;
      isRotating = true;
   }

   public async override void _PhysicsProcess(double delta)
   {
      if (isMoving)
      {
         Velocity = currentDirection * currentMoveSpeed;
         MoveAndSlide();
      }

      if (isRotating)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         Vector3 rotation = model.Rotation;
         rotation.Y = Mathf.LerpAngle(rotation.Y, currentTargetRotation, 0.25f);
         model.Rotation = rotation;

         if (Mathf.Abs(Mathf.AngleDifference(model.Rotation.Y, currentTargetRotation)) < 0.01f)
         {
            isRotating = false;
         }
      }

      if (isTracking)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         model.LookAt(trackingTarget.GlobalPosition);
         model.RotateY(Mathf.DegToRad(180f));
         model.Rotation = new Vector3(0f, model.Rotation.Y, 0f);
      }
   }

   public void SetAnimation(bool isIdle, ActorStatus actorStatus, string animName)
   {
      if (isIdle)
      {
         actorStatus.idleAnim = animName;
      }
      else
      {
         actorStatus.walkAnim = animName;
      }
   }

   public async void PlayAnimation(string animationName, ActorStatus actorStatus, float blend, bool useLength, float waitTime = 0f, float exitBlend = 0f)
   {
      AnimationPlayer player = GetNode<AnimationPlayer>("Model/AnimationPlayer");

      player.Play(animationName, blend);

      if (useLength)
      {
         await ToSignal(GetTree().CreateTimer(player.CurrentAnimationLength), "timeout");
      }
      else
      {
         await ToSignal(GetTree().CreateTimer(waitTime), "timeout");
      }

      if (player.CurrentAnimation == animationName) // Avoid interrupting other animation plays, in case they have started playing since
      {
         player.Play(actorStatus.idleAnim, exitBlend);
      }
   }

   public void Track(Actor target)
   {
      isTracking = true;
      trackingTarget = target;
   }

   public void StopTracking()
   {
      isTracking = false;
   }

   public void PlaceCharacterAtPoint(Vector3 destination)
   {
      GlobalPosition = destination;
   }

   public void RotateCharacterInstantly(float targetRotation)
   {
      targetRotation = Mathf.DegToRad(targetRotation);
      GetNode<Node3D>("Model").Rotation = new Vector3(GetNode<Node3D>("Model").Rotation.X, targetRotation, GetNode<Node3D>("Model").Rotation.Z);
   }

   Vector3 GetVector3FromString(string pointString)
   {
      string[] stringParts = pointString.Split(',');
      return new Vector3(stringParts[0].ToFloat(), stringParts[1].ToFloat(), stringParts[2].ToFloat());
   }
}
