using System.Collections.Generic;
using Godot;

public enum CharacterType {
   None,
   Vakthol,
   Thalria,
   Athlia,
   Olren,
   Dathrel
}

[GlobalClass]
public partial class PartyMemberBase : Resource
{
   [Export]
	public string memberName;
   [Export]
   public CharacterType memberType;
   [Export]
   public Stat[] stats = new Stat[10];
   [Export]
   public StatContainer[] statIncreasesPerLevel = new StatContainer[49];
   [Export]
   public AbilityResource[] abilities = new AbilityResource[10];
   [Export]
   public AbilityResource specialAbility;
   [Export]
   public Affinity affinity;
   [Export]
   public string passiveName;
   [Export(PropertyHint.MultilineText)]
   public string passiveDescription;
   [Export]
   public ItemCategory itemCategoryWorn;
   [Export]
   public ItemCategory itemCategoryWielded;
}
