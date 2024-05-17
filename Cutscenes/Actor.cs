using Godot;
using System;

public partial class Actor : CharacterBody3D
{
  // int rotationCounter = 0;
  // int movementCounter = 0;

   bool isMoving;
   float currentMoveSpeed = 0f;
   Vector3 currentDestination;
   Vector3 currentDirection;

   private Node3D weapon;
   private Node3D weaponAnchor;

   private Node3D secondaryWeapon;
   private Node3D secondaryAnchor;

   private BoneAttachment3D attachment;

   public override void _Ready()
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

   public void PlaceWeaponOnBack()
   {
      attachment.BoneName = "torso";

      weapon.Position = weaponAnchor.Position;
      weapon.Rotation = weaponAnchor.Rotation;
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

   public async void MoveCharacter(ActorStatus actorStatus, string destinationString)
   {
      Vector3 destination = GetVector3FromString(destinationString);

      Vector3 direction = GlobalPosition.DirectionTo(destination);
      float distance = GlobalPosition.DistanceTo(destination);

      AnimationPlayer player = GetNode<AnimationPlayer>("Model/AnimationPlayer");

      if (player.CurrentAnimation != actorStatus.walkAnim)
      {
         player.Play(actorStatus.walkAnim);
      }

      //int id = movementCounter;
      //movementCounter++;

      currentMoveSpeed = actorStatus.moveSpeed;
      currentDestination = destination;
      currentDirection = direction;
      isMoving = true;

      while (distance > 0.1f)
      {
         /*if (movementCounter > id + 1)
         {
            break;
         }*/

         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         distance = GlobalPosition.DistanceTo(destination); 
      }

      isMoving = false;

      Velocity = Vector3.Zero;
      player.Play(actorStatus.idleAnim);
   }

   public async void RotateCharacter(float targetRotation)
   {
      //int id = rotationCounter;
     // rotationCounter++;

      targetRotation = Mathf.DegToRad(targetRotation);

      Node3D model = GetNode<Node3D>("Model");

      while (Mathf.Abs(model.Rotation.Y - targetRotation) > 0.05f)
      {
         /*if (rotationCounter > id + 1)
         {
            break;
         }*/

         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");

         Vector3 rotation = model.Rotation;
         rotation.Y = Mathf.Lerp(rotation.Y, targetRotation, 0.25f);
         model.Rotation = rotation;
      }
   }

   public override void _PhysicsProcess(double delta)
   {
      if (isMoving)
      {
         Velocity = currentDirection * currentMoveSpeed;
         MoveAndSlide();
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

   public async void PlayAnimation(string animationName, ActorStatus actorStatus)
   {
      AnimationPlayer player = GetNode<AnimationPlayer>("Model/AnimationPlayer");
      player.Play(animationName);
      await ToSignal(GetTree().CreateTimer(player.CurrentAnimationLength), "timeout");
      player.Play(actorStatus.idleAnim);
   }

   public void PlaceCharacterAtPoint(string pointString)
   {
      Vector3 point = GetVector3FromString(pointString);
      GlobalPosition = point;
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
