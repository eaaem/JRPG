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

   //private 

   /*private void onBodyEntered(Node3D body) 
   {
      combatManager.SetupCombat(enemies, body.Position);
      QueueFree();
   }*/

   /*public void OnClickFighter(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeidx)
   {
      if (@event is InputEventMouseButton mouseButton)
      {
         if (mouseButton.Pressed)
         {
            combatManager.ReceiveFighterSelection(GetParent<Node3D>());
         }
      }
   }*/

   void OnMouseEntered()
   {
      combatManager.HoverOverEnemy(GetParent<Node3D>());
   }

   void OnMouseExited()
   {
      combatManager.StopHoverOverEnemy(GetParent<Node3D>());
   }
}
