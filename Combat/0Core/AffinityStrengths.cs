using Godot;
using System;

[GlobalClass]
public partial class AffinityStrengths : Resource
{
   [Export]
   public Affinity affinity;
   [Export(PropertyHint.Enum)]
   public DamageType strongAgainst1;
   [Export(PropertyHint.Enum)]
   public DamageType strongAgainst2;
   [Export(PropertyHint.Enum)]
   public DamageType weakTo1;
   [Export(PropertyHint.Enum)]
   public DamageType weakTo2;
}
