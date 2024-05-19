using Godot;
using System;

public partial class OlrenBowBehavior : Node
{
   private const float TimeUntilDraw = 0.67f;
   private const float TimeUntilGrabArrow = 0.4f;

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
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");

      skeleton = GetNode<Skeleton3D>("../Model/Armature/Skeleton3D");

      attachment = new BoneAttachment3D();
      skeleton.AddChild(attachment);

      attachment.BoneName = "hand.R";

      arrowHolder = GetNode<Node3D>("../SecondaryWeapon/ArrowHolder");
      arrow = arrowHolder.GetNode<Node3D>("Arrow");

      combatManager.AttackAnimation += PlayAnimations;
	}

	async void PlayAnimations()
   {
      if (combatManager.CurrentFighter.fighterName == "Olren" && !combatManager.IsCompanionTurn)
      {
         Vector3 oldRotation = arrow.Rotation;

         // Move the arrow to the attachment when grabbed; play the draw and release bow animations when necessary; then return everything to the rest state
         await ToSignal(GetTree().CreateTimer(TimeUntilGrabArrow), "timeout");
         arrowHolder.RemoveChild(arrow);
         arrow.Rotation = new Vector3(0, 0, Mathf.DegToRad(-30f));
         attachment.AddChild(arrow);

         await ToSignal(GetTree().CreateTimer(TimeUntilDraw - TimeUntilGrabArrow), "timeout");
         bowPlayer.Play("Draw");

         await ToSignal(GetTree().CreateTimer(bowPlayer.CurrentAnimationLength), "timeout");
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
