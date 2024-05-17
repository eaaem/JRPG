using Godot;
using System;
using System.Collections.Generic;

public partial class PartyManager : Node
{
   [Export]
   PartyMemberBase[] baseMembers = new PartyMemberBase[4];
   [Export]
   ItemResource[] vaktholItems = new ItemResource[1];
   [Export]
   ItemResource[] thalriaItems = new ItemResource[1];
   [Export]
   ItemResource[] athliaItems = new ItemResource[1];
   [Export]
   ItemResource[] olrenItems = new ItemResource[1];
   [Export]
   private int[] experienceThreshholds = new int[50];

   public List<Member> Party { get; set; }
   //public List<Member> unusedParty = new List<Member>();
   public List<InventoryItem> Items { get; set; }

   public int Gold { get; set; }
   
   PartyMenuManager partyMenuManager;

   public override void _Ready()
   {
      Party = new List<Member>();
      Items = new List<InventoryItem>();
      
      partyMenuManager = GetNode<PartyMenuManager>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu/TabContainer/Party");
   }

   public int GetExperienceAtLevel(int level)
   {
      return experienceThreshholds[level];
   }

   public void InitializeNewParty()
   {
      Party.Clear();
      Items.Clear();
      
      //for (int i = 0; i < 4; i++)
     // {
      Member memberToAdd = new Member();
      memberToAdd.baseMember = baseMembers[0];
      memberToAdd.level = 1;
      memberToAdd.characterName = baseMembers[0].memberName;
      memberToAdd.characterType = baseMembers[0].memberType;
      memberToAdd.affinity = baseMembers[0].affinity;
      memberToAdd.stats = new Stat[10];
      memberToAdd.specialAbility = baseMembers[0].specialAbility;
      memberToAdd.isInParty = true;

      //AddItem(new InventoryItem(item, 3));
      //AddItem(new InventoryItem(otherCuirass, 1));

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

      /*if (i == 0)
      {
         for (int j = 0; j < vaktholItems.Length; j++)
         {
            EquipItem(vaktholItems[j], memberToAdd);
         }
      }
      else if (i == 1)
      {
         for (int j = 0; j < thalriaItems.Length; j++)
         {
            EquipItem(thalriaItems[j], memberToAdd);
         }
      }
      else if (i == 2)
      {
         for (int j = 0; j < athliaItems.Length; j++)
         {
            EquipItem(athliaItems[j], memberToAdd);
         }
      }
      else if (i == 3)
      {
         for (int j = 0; j < olrenItems.Length; j++)
         {
            EquipItem(olrenItems[j], memberToAdd);
         }
      }*/

      //LevelUp(experienceThreshholds[0] + experienceThreshholds[1], memberToAdd);

      memberToAdd.currentHealth = memberToAdd.GetMaxHealth();
      memberToAdd.currentMana = memberToAdd.GetMaxMana();

      memberToAdd.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1");

      Party.Add(memberToAdd);

         /*if (i != 0)
         {
            partyMenuManager.AddCharacterToParty(memberToAdd);
         }*/
      //}

      //partyMenuManager.ResetDialogueHolders();
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

   public void LoadPartyMember(int characterType, int currentHealth, int currentMana, string[] equipment, int experience, bool isInParty, int level, int partyIndex)
   {
      Member newMember = new Member();
      string fileLocation = ((CharacterType)characterType).ToString().ToLower();
      newMember.baseMember = GD.Load<PartyMemberBase>("res://PartyMembers/" + (CharacterType)characterType + "/" + fileLocation + ".tres");

      newMember.characterName = newMember.baseMember.memberName;
      newMember.level = level;
      newMember.experience = experience;
      newMember.characterType = (CharacterType)characterType;
      newMember.affinity = newMember.baseMember.affinity;
      newMember.stats = new Stat[10];
      newMember.specialAbility = newMember.baseMember.specialAbility;
      newMember.isInParty = isInParty;

      //AddItem(new InventoryItem(item, 3));
      //AddItem(new InventoryItem(otherCuirass, 1));

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

      if (partyIndex < Party.Count)
      {
         Party.Add(Party[partyIndex]);
         Party[partyIndex] = newMember;
      }
      else
      {
         Party.Add(newMember);
      }

      GD.Print("added");

      if (partyIndex == 0)
      {
         newMember.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1");
         partyMenuManager.AddNewComponentsToModel(newMember);
         GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1").ResetNodes();

         // Make sure you can't try to talk to yourself
         newMember.model.GetNode<StaticBody3D>("DialogueBox").QueueFree();
      }
      else if (isInParty)
      {
         partyMenuManager.AddCharacterToParty(newMember);
         GetNode<OverworldPartyController>("/root/BaseNode/PartyMembers/Member" + (partyIndex + 1)).IsActive = true;
      }
   }

   public void LoadItem(string path, int quantity)
   {
      ItemResource item = GD.Load<ItemResource>(path);

      Items.Add(new InventoryItem(item, quantity));
   }

	// Called when the node enters the scene tree for the first time.
	//public override void _Ready()
	//{
      /*Member memberToAdd = new Member();
      memberToAdd.characterName = baseMember.memberName;
      memberToAdd.level = 1;
      memberToAdd.characterType = baseMember.memberType;
      memberToAdd.affinity = Affinity.Sanguine;
      memberToAdd.stats = new Stat[10];
      memberToAdd.abilities.Add(baseMember.abilities[0]);
      memberToAdd.abilities.Add(baseMember.abilities[1]);
      memberToAdd.specialAbility = baseMember.specialAbility;
      memberToAdd.isInParty = true;

      //AddItem(new InventoryItem(item, 3));
      //AddItem(new InventoryItem(otherCuirass, 1));

      for (int i = 0; i < 10; i++) {
         memberToAdd.stats[i] = new Stat();
         memberToAdd.stats[i].statType = baseMember.stats[i].statType;
         memberToAdd.stats[i].value = baseMember.stats[i].value;
         memberToAdd.stats[i].baseValue = baseMember.stats[i].value;
      }

      //EquipItem(cuirass, memberToAdd);

      for (int i = 0; i < vaktholItems.Length; i++)
      {
         EquipItem(vaktholItems[i], memberToAdd);
      }

      memberToAdd.currentHealth = memberToAdd.GetMaxHealth();
      memberToAdd.currentMana = memberToAdd.GetMaxMana();

      memberToAdd.baseMember = baseMember;

      memberToAdd.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1");

      party.Add(memberToAdd);

      Member secondMember = new Member();
      secondMember.characterName = baseMember2.memberName;
      secondMember.level = 1;
      secondMember.characterType = baseMember2.memberType;
      secondMember.affinity = Affinity.Sanguine;
      secondMember.stats = new Stat[10];
      secondMember.abilities.Add(baseMember2.abilities[0]);
      //memberToAdd.abilities.Add(baseMember.abilities[1]);
      secondMember.specialAbility = baseMember2.specialAbility;
      secondMember.isInParty = false;

      secondMember.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member2");


      for (int i = 0; i < 10; i++) {
         secondMember.stats[i] = new Stat();
         secondMember.stats[i].statType = baseMember2.stats[i].statType;
         secondMember.stats[i].value = baseMember2.stats[i].value;
         secondMember.stats[i].baseValue = baseMember2.stats[i].value;
      }

      for (int i = 0; i < athliaItems.Length; i++)
      {
         EquipItem(athliaItems[i], secondMember);
      }

      secondMember.currentHealth = secondMember.GetMaxHealth();
      secondMember.currentMana = secondMember.GetMaxMana();

      secondMember.baseMember = baseMember2;

      party.Add(secondMember);

      Member thirdMember = new Member();
      thirdMember.characterName = base3.memberName;
      thirdMember.level = 1;
      thirdMember.characterType = base3.memberType;
      thirdMember.affinity = Affinity.Shadowsworn;
      thirdMember.stats = new Stat[10];
      thirdMember.abilities.Add(base3.abilities[0]);
      //memberToAdd.abilities.Add(baseMember.abilities[1]);
      thirdMember.specialAbility = base3.specialAbility;


      for (int i = 0; i < 10; i++) {
         thirdMember.stats[i] = new Stat();
         thirdMember.stats[i].statType = base3.stats[i].statType;
         thirdMember.stats[i].value = base3.stats[i].value;
         thirdMember.stats[i].baseValue = base3.stats[i].value;
      }

      thirdMember.currentHealth = thirdMember.GetMaxHealth();
      thirdMember.currentMana = thirdMember.GetMaxMana();

      for (int i = 0; i < thalriaItems.Length; i++)
      {
         EquipItem(thalriaItems[i], thirdMember);
      }

      thirdMember.baseMember = base3;
      thirdMember.isInParty = false;

      party.Add(thirdMember);

      Member fourthMember = new Member();
      fourthMember.characterName = base4.memberName;
      fourthMember.level = 1;
      fourthMember.characterType = base4.memberType;
      fourthMember.affinity = Affinity.Shadowsworn;
      fourthMember.stats = new Stat[10];
      fourthMember.abilities.Add(base4.abilities[0]);
      //memberToAdd.abilities.Add(baseMember.abilities[1]);
      fourthMember.specialAbility = base4.specialAbility;


      for (int i = 0; i < 10; i++) {
         fourthMember.stats[i] = new Stat();
         fourthMember.stats[i].statType = base4.stats[i].statType;
         fourthMember.stats[i].value = base4.stats[i].value;
         fourthMember.stats[i].baseValue = base4.stats[i].value;
      }

      fourthMember.currentHealth = fourthMember.GetMaxHealth();
      fourthMember.currentMana = fourthMember.GetMaxMana();

      for (int i = 0; i < olrenItems.Length; i++)
      {
         EquipItem(olrenItems[i], fourthMember);
      }

      fourthMember.baseMember = base4;
      fourthMember.isInParty = false;

      party.Add(fourthMember);*/
	//}

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