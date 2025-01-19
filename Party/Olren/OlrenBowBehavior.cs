using Godot;
using System;

public partial class OlrenBowBehavior : Node
{
   private const float TimeUntilDraw = 1.9f;
   private const float TimeUntilGrabArrow = 1f;
   private const float TimeToHoldArrow = 0.35f;

   private AnimationPlayer bowPlayer;
   private CombatManager combatManager;

   private Skeleton3D skeleton;
   private BoneAttachment3D attachment;
   
   private Node3D arrow;
   private Node3D arrowHolder;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      bowPlayer = GetNode<AnimationPlayer>("../Weapon/AnimationPlayer");
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManager");

      skeleton = GetNode<Skeleton3D>("../Model/Armature/Skeleton3D");

      attachment = new BoneAttachment3D();
      skeleton.AddChild(attachment);

      attachment.BoneName = "hand.R";

      arrowHolder = GetNode<Node3D>("../Model/Armature/Skeleton3D/QuiverHolder/SecondaryWeapon/ArrowHolder");
      arrow = arrowHolder.GetNode<Node3D>("Arrow");

      combatManager.AttackAnimation += PlayAnimations;
	}

	async void PlayAnimations()
   {
      if (combatManager.CurrentFighter.fighterName == "Olren" && !combatManager.IsCompanionTurn)
      {
         while (GetNode<AnimationPlayer>("../Model/AnimationPlayer").CurrentAnimation != "Attack")
         {
            await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         }

         Vector3 oldRotation = arrow.Rotation;

         // Move the arrow to the attachment when grabbed, play the draw and release bow animations when necessary, then return everything to the rest state
         await ToSignal(GetTree().CreateTimer(TimeUntilGrabArrow), "timeout");
         arrowHolder.RemoveChild(arrow);
         arrow.Rotation = new Vector3(0, 0, Mathf.DegToRad(-30f));
         attachment.AddChild(arrow);

         await ToSignal(GetTree().CreateTimer(TimeUntilDraw - TimeUntilGrabArrow), "timeout");
         bowPlayer.Play("Draw");

         await ToSignal(GetTree().CreateTimer(bowPlayer.CurrentAnimationLength + TimeToHoldArrow), "timeout");
         bowPlayer.Play("Release");
         attachment.RemoveChild(arrow);

         await ToSignal(GetTree().CreateTimer(bowPlayer.CurrentAnimationLength), "timeout");
         bowPlayer.Play("AtRest");
         arrowHolder.AddChild(arrow);
         arrow.Rotation = oldRotation;
      }
   }

   public override void _ExitTree()
   {
      combatManager.AttackAnimation -= PlayAnimations;
   }
}
