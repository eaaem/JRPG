using Godot;
using System;

[GlobalClass]
public partial class BoneConversion : Resource
{
	[Export]
   public string originalBone;
   [Export]
   public string overrideBone;
}
