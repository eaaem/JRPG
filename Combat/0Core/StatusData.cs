using Godot;
using System;

[GlobalClass]
public partial class StatusData : Resource
{
   [Export]
   public StatusEffect effect;
   [Export(PropertyHint.MultilineText)]
   public string description;
   [Export]
   public bool isCleanseable;
   [Export]
   public bool isNegative;
   [Export]
   public bool displayRemainingTurns;
   [Export]
   public string graphicEffectPath;
   [Export]
   public string boneToAttachForGraphic;
}
