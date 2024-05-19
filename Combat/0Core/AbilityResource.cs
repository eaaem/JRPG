using Godot;
using System;

[GlobalClass]
public partial class AbilityResource : Resource
{
   [Export]
   public string name;
   [Export(PropertyHint.MultilineText)]
   public string description;
   [Export]
   public int manaCost;
   [Export]
   public string scriptName;
   [Export]
   public bool hitsAll;
   [Export]
   public bool hitsSurrounding;
   [Export]
   public bool hitsSelf;
   [Export]
   public bool hitsTeam;
   [Export]
   public bool onlyHitsTeam;
   [Export]
   public bool onlyHitsSelf;
   [Export]
   public bool bypassesDeaths;
   [Export]
   public string casterGraphic = string.Empty;
   [Export]
   public string targetGraphic = string.Empty;
   [Export]
   public int requiredLevel;
}
