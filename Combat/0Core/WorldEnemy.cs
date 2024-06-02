using Godot;
using System;
using System.Collections.Generic;

public partial class WorldEnemy : CharacterBody3D
{
   [Export]
   public WorldEnemyData[] datas = new WorldEnemyData[1];
   [Export]
   public int id;
   [Export]
   public bool isStaticEnemy;
   [Export]
   public float introWaitTime;
   [Export]
   public string postBattleCutsceneName = new string("");

   private const float DistanceThreshhold = 15f;
   private const float LoseAggroThreshhold = 25f;
   private const float Speed = 7.1f;

   public List<Enemy> enemies = new List<Enemy>();

   private AnimationPlayer animationPlayer;
   private NavigationAgent3D navigationAgent;

   //private Vector3 targetPosition;
   private float distance;
   bool isChasingPlayer = false;

   private CharacterBody3D player;
   private CharacterController playerController;

   private CombatManager combatManager;

   public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

   Vector3 originalPosition;
   Vector3 originalRotation;

   Node3D model;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      RandomizeEnemies();

      //animationPlayer = GetNode<AnimationPlayer>("Model/AnimationPlayer");
      navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

      player = GetNode<CharacterBody3D>("/root/BaseNode/PartyMembers/Member1");
      playerController = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");

      model = GetNode<Node3D>("Model");

      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");

      if (isStaticEnemy)
      {
         animationPlayer = model.GetNode<AnimationPlayer>("AnimationPlayer");
         animationPlayer.Play("OnHill");
      }

      /*if (animationPlayer.CurrentAnimation != "Idle")
      {
         animationPlayer.Play("Idle");
      }*/

      originalPosition = GlobalPosition;
      originalRotation = GlobalRotation;

      Callable.From(ActorSetup).CallDeferred();
	}

   void RandomizeEnemies()
   {
      for (int i = 0; i < datas.Length; i++)
      {
         int roll = GD.RandRange(0, 99);

         // Rolled this enemy; add it
         if (roll < datas[i].rollChance)
         {
            int quantity = GD.RandRange(datas[i].minQuantity, datas[i].maxQuantity);

            for (int j = 0; j < quantity; j++)
            {
               enemies.Add(datas[i].enemy);
            }
         }
      }
   }

   public Vector3 MovementTarget
    {
        get { return navigationAgent.TargetPosition; }
        set { navigationAgent.TargetPosition = value; }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
      if (isStaticEnemy)
      {
         return;
      }

      Vector3 velocity = Velocity;
      distance = GlobalPosition.DistanceTo(player.GlobalPosition);

      //float distanceFromOriginalPosition = Position.DistanceTo(originalPosition);

      if (distance < DistanceThreshhold)
      {
         //ActorSetup();
         isChasingPlayer = true;
         GlobalRotation = Vector3.Zero;
      }
      else
      {
         isChasingPlayer = false;
      }

      ActorSetup();
      
      if (navigationAgent.IsNavigationFinished())
      {
         /*if (animationPlayer.CurrentAnimation != "Idle")
         {
            animationPlayer.Play("Idle");
         }*/
         // When the enemy is placed, it needs to have a certain rotation, but when it moves, that rotation has to be Vector3.Zero, or else movement looks strange
         if (!isChasingPlayer)
         {
            GlobalRotation = originalRotation;
         }
         return;
      }

      Vector3 currentAgentPosition = GlobalTransform.Origin;
      Vector3 nextPathPosition = navigationAgent.GetNextPathPosition();

      Vector3 modelRotation = model.Rotation;
      modelRotation.Y = Mathf.LerpAngle(model.Rotation.Y, Mathf.Atan2(velocity.X, velocity.Z), 0.25f);
      model.Rotation = modelRotation;
      
      velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
      velocity.Y = 0f;

      /*if (animationPlayer.CurrentAnimation != "Run")
      {
         animationPlayer.Play("Run");
      }*/

      if (!IsOnFloor())
      {
         velocity.Y -= gravity * (float)delta;
      }

      Velocity = velocity;
      MoveAndSlide();
	}

   private async void ActorSetup()
   {
      if (isStaticEnemy)
      {
         return;
      }

      // Wait for the first physics frame so the NavigationServer can sync.
      await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

      // Now that the navigation map is no longer empty, set the movement target.
      if (isChasingPlayer)
      {
         MovementTarget = player.GlobalPosition;
      }
      else
      {
         MovementTarget = originalPosition;
      }
   }

   private async void OnBodyEntered(Node3D body) 
   {
      if (!isStaticEnemy)
      {
         combatManager.SetupCombat(enemies, body.GlobalPosition, body.GetNode<Node3D>("Model").GlobalRotation, this);
      }
      else
      {
         playerController.DisableMovement = true;
         playerController.DisableCamera = true;

         animationPlayer.Play("InitiateBattle");
         await ToSignal(GetTree().CreateTimer(introWaitTime), "timeout");
         combatManager.SetupCombat(enemies, body.GlobalPosition, body.GetNode<Node3D>("Model").GlobalRotation, this);
      }
   }
}
