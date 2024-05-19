using Godot;
using System;

public abstract partial class PlayerAbilityBehavior : AbilityBehavior
{
	public abstract override void OnButtonDown();
   public abstract override void OnCast();

   
   public override void OnEnemyCast() {}
}
