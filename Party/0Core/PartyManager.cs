using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages party members, items, and gold.
/// </summary>
public partial class PartyManager : Node
{
   [Export]
   PartyMemberBase[] baseMembers = new PartyMemberBase[4];
   [Export]
   ItemResource[] vaktholItems = new ItemResource[1];
   [Export]
   public ItemResource[] ThalriaItems { get; set; } = new ItemResource[1];
   [Export]
   public ItemResource[] AthliaItems { get; set; } = new ItemResource[1];
   [Export]
   public ItemResource[] OlrenItems { get; set; } = new ItemResource[1];
   [Export]
   private int[] experienceThreshholds = new int[50];

   public List<Member> Party { get; set; }
   public List<InventoryItem> Items { get; set; }

   public int Gold { get; set; }
   
   [Export]
   private ManagerReferenceHolder managers;

   public override void _Ready()
   {
      Party = new List<Member>();
      Items = new List<InventoryItem>();
   }

   public int GetExperienceAtLevel(int level)
   {
      return experienceThreshholds[level];
   }

   public void InitializeNewParty()
   {
      Party.Clear();
      Items.Clear();

      Member memberToAdd = new Member();
      memberToAdd.baseMember = baseMembers[0];
      memberToAdd.level = 1;
      memberToAdd.characterName = baseMembers[0].memberName;
      memberToAdd.characterType = baseMembers[0].memberType;
      memberToAdd.affinity = baseMembers[0].affinity;
      memberToAdd.stats = new Stat[10];
      memberToAdd.specialAbility = baseMembers[0].specialAbility;
      memberToAdd.isInParty = true;

      for (int i = 0; i < 10; i++) {
         memberToAdd.stats[i] = new Stat();
         memberToAdd.stats[i].statType = baseMembers[0].stats[i].statType;
         memberToAdd.stats[i].value = baseMembers[0].stats[i].value;
         memberToAdd.stats[i].baseValue = baseMembers[0].stats[i].value;
      }

      for (int i = 0; i < vaktholItems.Length; i++)
      {
         EquipItem(vaktholItems[i], memberToAdd);
      }

      memberToAdd.currentHealth = memberToAdd.GetMaxHealth();
      memberToAdd.currentMana = memberToAdd.GetMaxMana();

      memberToAdd.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1");
      managers.PartyMenuManager.AddNewComponentsToModel(memberToAdd);
      GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1").ResetNodes();

      // Make sure you can't try to talk to yourself
      memberToAdd.model.GetNode<StaticBody3D>("DialogueBox").QueueFree();

      Party.Add(memberToAdd);
   }

   public void MovePartyMembersBehindPlayer()
   {
      CharacterController controller = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");
      for (int i = 1; i < 4; i++)
      {
         CharacterBody3D memberNode = GetNode<CharacterBody3D>("/root/BaseNode/PartyMembers/Member" + (i + 1));
         memberNode.Position = controller.Position - (controller.GlobalTransform.Basis.Z * i);
      }
   }

   public Member LoadPartyMember(int characterType, int currentHealth, int currentMana, string[] equipment, int experience, bool isInParty, int level, int partyIndex)
   {
      Member newMember = new Member();
      string fileLocation = ((CharacterType)characterType).ToString().ToLower();
      newMember.baseMember = GD.Load<PartyMemberBase>("res://Party/" + (CharacterType)characterType + "/" + fileLocation + ".tres");

      newMember.characterName = newMember.baseMember.memberName;
      newMember.level = level;
      newMember.experience = experience;
      newMember.characterType = (CharacterType)characterType;
      newMember.affinity = newMember.baseMember.affinity;
      newMember.stats = new Stat[10];
      newMember.specialAbility = newMember.baseMember.specialAbility;
      newMember.isInParty = isInParty;

      // Initialize stats
      for (int i = 0; i < 10; i++)
      {
         newMember.stats[i] = new Stat();
         newMember.stats[i].statType = newMember.baseMember.stats[i].statType;
         newMember.stats[i].value = newMember.baseMember.stats[i].value;
         newMember.stats[i].baseValue = newMember.baseMember.stats[i].value;
      }

      // Increase stats according to level
      for (int i = 0; i < level - 1; i++)
      {
         StatContainer container = newMember.baseMember.statIncreasesPerLevel[i];
         for (int j = 0; j < container.stats.Length; j++)
         {
            newMember.stats[(int)container.stats[j].statType].baseValue += container.stats[j].value;
            newMember.stats[(int)container.stats[j].statType].value += container.stats[j].value;
         }
      }

      // Add abilities
      for (int i = 0; i < newMember.baseMember.abilities.Length; i++)
      {
         if (level >= newMember.baseMember.abilities[i].requiredLevel)
         {
            newMember.abilities.Add(newMember.baseMember.abilities[i]);
         }
      }

      // Add equipment
      for (int i = 0; i < equipment.Length; i++)
      {
         if (equipment[i] != "")
         {
            ItemResource equippedItem = GD.Load<ItemResource>(equipment[i]);
            EquipItem(equippedItem, newMember);
         }
      }

      newMember.currentHealth = currentHealth;
      newMember.currentMana = currentMana;

      // If there's already a party member at the index, add them to the end and replace the original with the new party member
      if (partyIndex < Party.Count)
      {
         Party.Add(Party[partyIndex]);
         Party[partyIndex] = newMember;
      }
      else
      {
         Party.Add(newMember);
      }

      if (partyIndex == 0)
      {
         newMember.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1");
         managers.PartyMenuManager.AddNewComponentsToModel(newMember);
         GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1").ResetNodes();

         // Make sure you can't try to talk to yourself
         newMember.model.GetNode<StaticBody3D>("DialogueBox").QueueFree();
      }
      else if (isInParty)
      {
         managers.PartyMenuManager.AddCharacterToParty(newMember);
         GetNode<OverworldPartyController>("/root/BaseNode/PartyMembers/Member" + (partyIndex + 1)).IsActive = true;
      }

      return newMember;
   }

   public void LoadItem(string path, int quantity)
   {
      ItemResource item = GD.Load<ItemResource>(path);

      Items.Add(new InventoryItem(item, quantity));
   }

   public void AddItem(InventoryItem itemToAdd)
   {
      for (int i = 0; i < Items.Count; i++)
      {
         if (Items[i].item.name == itemToAdd.item.name)
         {
            Items[i].quantity += itemToAdd.quantity;
            return;
         }
      }

      Items.Add(itemToAdd);
   }

   public void RemoveItem(InventoryItem itemToRemove)
   {
      for (int i = 0; i < Items.Count; i++)
      {
         if (Items[i].item.name == itemToRemove.item.name)
         {
            Items[i].quantity -= itemToRemove.quantity;

            if (Items[i].quantity <= 0)
            {
               Items.Remove(Items[i]);
            }
            return;
         }
      }
   }

   public void EquipItem(ItemResource item, Member member)
   {
      int slot = (int)(item.itemType) - 1;

      if (member.equipment[slot].name != null)
      {
         UnequipItem(slot, member);
      }

      member.equipment[slot] = item;

      for (int i = 0; i < member.equipment[slot].affectedStats.stats.Length; i++)
      {
         member.stats[(int)member.equipment[slot].affectedStats.stats[i].statType].value += member.equipment[slot].affectedStats.stats[i].value;
      }

      RemoveItem(new InventoryItem(item, 1));
   }

   void UnequipItem(int slot, Member member)
   {
      for (int i = 0; i < member.equipment[slot].affectedStats.stats.Length; i++)
      {
         member.stats[(int)member.equipment[slot].affectedStats.stats[i].statType].value -= member.equipment[slot].affectedStats.stats[i].value;
      }

      AddItem(new InventoryItem(member.equipment[slot], 1));

      ItemType type = member.equipment[slot].itemType;
      member.equipment[slot] = new ItemResource();
      member.equipment[slot].itemType = type;
   }

   
   public void LevelUp(int expGain, Member partyMember)
   {
      partyMember.experience += expGain;

      while (partyMember.experience >= experienceThreshholds[partyMember.level - 1])
      {
         int oldMaxHealth = partyMember.GetMaxHealth();
         int oldMaxMana = partyMember.GetMaxMana();
         partyMember.experience -= experienceThreshholds[partyMember.level - 1];
         partyMember.level++;

         partyMember.currentHealth += partyMember.GetMaxHealth() - oldMaxHealth;
         partyMember.currentMana += partyMember.GetMaxMana() - oldMaxMana;

         if (partyMember.currentHealth > partyMember.GetMaxHealth())
         {
            partyMember.currentHealth = partyMember.GetMaxHealth();
         }

         if (partyMember.currentMana > partyMember.GetMaxHealth())
         {
            partyMember.currentMana = partyMember.GetMaxHealth();
         }

         StatContainer container = partyMember.baseMember.statIncreasesPerLevel[partyMember.level - 2];

         for (int i = 0; i < container.stats.Length; i++)
         {
            partyMember.stats[(int)container.stats[i].statType].baseValue += container.stats[i].value;
            partyMember.stats[(int)container.stats[i].statType].value += container.stats[i].value;
         }

         for (int i = 0; i < partyMember.baseMember.abilities.Length; i++)
         {
            if (partyMember.baseMember.abilities[i].requiredLevel == partyMember.level)
            {
               partyMember.abilities.Add(partyMember.baseMember.abilities[i]);
            }
         }
      }
   }
}

public partial class Member : Node
{
   public PartyMemberBase baseMember;
   public string characterName;
   public int level;
   public int experience;
   public int currentHealth;
   public int currentMana;
   public bool isInParty;

   public CharacterType characterType;
   public Affinity affinity;

   public Stat[] stats = new Stat[10];
   public List<AbilityResource> abilities = new List<AbilityResource>();
   public AbilityResource specialAbility;

   public List<ItemResource> equipment = new List<ItemResource>();
   public string[] serializableEquipment = new string[6];

   public Node3D model;

   public int index;

   public Member()
   {
      for (int i = 0; i < 6; i++)
      {
         ItemResource item = new ItemResource();
         item.itemType = (ItemType)(i + 1);

         equipment.Add(item);
      }
   }

   public int GetMaxHealth()
   {
	  return stats[0].value * 2 * level;
   }

   public int GetMaxMana()
   {
	  return stats[1].value * level;
   }

   
   public Godot.Collections.Dictionary<string, Variant> PartyMemberInformation()
   {
      for (int i = 0; i < equipment.Count; i++)
      {
         serializableEquipment[(int)(equipment[i].itemType - 1)] = equipment[i].ResourcePath;
      }

      return new Godot.Collections.Dictionary<string, Variant>()
      {
         { "CharacterType", (int)characterType},
         { "PartyIndex", index },
         { "Level", level },
         { "Experience", experience },
         { "Equipment", serializableEquipment },
         { "IsInParty", isInParty },
         { "CurrentHealth", currentHealth },
         { "CurrentMana", currentMana }
      };
   }
}