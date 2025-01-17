using Godot;
using System;

public partial class AutomaticBoneAttacher : BoneAttachment3D
{
   [Export]
   private string boneToAttachTo;

	public override void _Ready()
	{ 
      if (GetParent().GetType() != typeof(Skeleton3D))
      {
         GD.PrintErr("Parent of bone attachment, " + GetParent().Name + ", is not a skeleton; skipping attachment.");
         return;
      }

      BoneName = boneToAttachTo;
	}
}
