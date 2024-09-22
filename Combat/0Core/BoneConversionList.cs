using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Status effects are set to be attached to a bone in a model's skeleton, described in their status data. However, some models may have a different name for
/// a particular bone, or may not have it at all. The purpose of the bone conversion list is to redirect the bone to attach a status effect to in these cases.
/// </summary>
public partial class BoneConversionList : Node
{
	[Export]
   public BoneConversion[] conversions = new BoneConversion[0];
}
