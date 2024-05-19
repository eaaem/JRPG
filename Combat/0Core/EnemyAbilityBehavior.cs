using Godot;
using System;

public abstract partial class EnemyAbilityBehavior : AbilityBehavior
{
   public override abstract void OnEnemyCast();

   public override void OnButtonDown() {}
   public override void OnCast() {}
}
