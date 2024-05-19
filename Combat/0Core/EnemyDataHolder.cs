using Godot;
using System;
using System.Threading;

public partial class EnemyDataHolder : Node
{
   public CombatManager combatManager;

   //private CombatManager combatManager;

   public override void _Ready()
   {
      combatManager = GetNode<CombatManager>("/root/BaseNode/CombatManagerObj");
      //CollisionObject3D collider = GetNode<CollisionObject3D>("StaticBody3D");
      //collider.InputEvent += ClickFighter;
   }
}
