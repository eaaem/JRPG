using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PartyMenuManager : Node
{
   [Export]
   private PackedScene memberButtonPrefab;
   [Export]
   private PackedScene abilityLabelPrefab;
   [Export]
   private PackedScene reequipButtonPrefab;

   private PartyManager partyManager;
   //private CharacterController controller;
   private MenuManager menuManager;

   CanvasGroup menu;
   VBoxContainer partyList;
   VBoxContainer statsList;
   VBoxContainer abilityList;
   VBoxContainer equipmentList;
   VBoxContainer reequipList;

   private DialogueManager dialogueManager;

   CharacterController controller;

   Button addPartyButton;
   Button removePartyButton;

   public bool inputDisabled;
   public bool isReequipping;
   public bool isSwapping;

   public bool isActive;

   Member currentMember;

   public string firstSwap = null;

   [Signal]
   public delegate void PartySwapEventHandler();

   List<CharacterType> mainCharacterTypes = new List<CharacterType> { CharacterType.Vakthol, CharacterType.Thalria, CharacterType.Athlia, CharacterType.Olren };

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      controller = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");

      partyManager = GetNode<PartyManager>("/root/BaseNode/PartyManagerObj");

      menu = GetParent().GetParent<CanvasGroup>();
      menuManager = GetNode<MenuManager>("../../MenuManager");

      partyList = GetNode<VBoxContainer>("ListBackground/List/VBoxContainer");
      statsList = GetNode<VBoxContainer>("Stats/VBoxContainer");
      abilityList = GetNode<VBoxContainer>("Abilities/VBoxContainer");
      equipmentList = GetNode<VBoxContainer>("Equipment/VBoxContainer");
      reequipList = GetNode<VBoxContainer>("ReequipBackground/Reequip/VBoxContainer");
      addPartyButton = GetNode<Button>("AddPartyButton");
      removePartyButton = GetNode<Button>("RemovePartyButton");

      dialogueManager = GetNode<DialogueManager>("/root/BaseNode/UI/DialogueScreen/Back");
	}

   public void CancelReequip()
   {
      ClearReequipList();
      EnableMenu();
      isReequipping = false;
      menuManager.EnableTabs();
   }

   public void CancelSwap()
   {
      EnableMenu();
      firstSwap = null;
      ClearMemberButtons();
      LoadMemberButtons();
      isSwapping = false;
      menuManager.EnableTabs();
   }

   public void LoadPartyMenu()
   {
      ClearMemberButtons();

      LoadMemberButtons();

      isActive = true;

      isSwapping = false;
      isReequipping = false;
      inputDisabled = false;

      GD.Print(partyManager.Party.Count);
      currentMember = partyManager.Party[0];
      LoadNewPartyScreen(currentMember.characterName);
   }

   void LoadMemberButtons()
   {
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         LoadMemberButton(partyManager.Party[i]);
      }
   }

   Member GetMemberFromName(string memberName)
   {
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].characterName == memberName)
         {
            return partyManager.Party[i];
         }
      }

      return null;
   }

   public void LoadNewPartyScreen(string memberName)
   {
      Member member = GetMemberFromName(memberName);

      currentMember = member;
      LoadStats(currentMember);
      LoadAbilities(currentMember);
      LoadEquipment(currentMember);

      RichTextLabel passiveLabel = GetNode<RichTextLabel>("Passive");
      passiveLabel.Text = "Passive: " + currentMember.baseMember.passiveName;
      passiveLabel.TooltipText = currentMember.baseMember.passiveDescription;

      int inPartyCount = 0;

      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].isInParty)
         {
            inPartyCount++;
         }
      }

      if (member.isInParty && inPartyCount > 1 && partyManager.Party[0] != member)
      {
         removePartyButton.Disabled = false;
      }
      else
      {
         removePartyButton.Disabled = true;
      }

      if (!member.isInParty && inPartyCount < 4)
      {
         addPartyButton.Disabled = false;
      }
      else
      {
         addPartyButton.Disabled = true;
      }
   }

   void LoadMemberButton(Member partyMember)
   {
      Button member = memberButtonPrefab.Instantiate<Button>();
      member.GetNode<RichTextLabel>("Title").Text = partyMember.characterName;
      member.GetNode<RichTextLabel>("Level").Text = "Level: " + partyMember.level;
      member.GetNode<RichTextLabel>("Health").Text = "Health: " + partyMember.currentHealth + "/" + partyMember.GetMaxHealth();
      member.GetNode<RichTextLabel>("Mana").Text = "Mana: " + partyMember.currentMana + "/" + partyMember.GetMaxMana();
      member.GetNode<RichTextLabel>("Exp").Text = "Experience: " + partyMember.experience + "/" + partyManager.GetExperienceAtLevel(partyMember.level - 1);

      if (partyMember.isInParty)
      {
         member.GetNode<Panel>("Highlight").Visible = true;
      }

      partyList.AddChild(member);
   }

   void ClearMemberButtons()
   {
      foreach (Button button in partyList.GetChildren())
      {
         partyList.RemoveChild(button);
         button.QueueFree();
      }
   }

   void LoadStats(Member partyMember)
   {
      statsList.GetNode<RichTextLabel>("Affinity").Text = "Affinity: " + partyMember.affinity;
   
      for (int i = 0; i < partyMember.stats.Length - 1; i++)
      {
         statsList.GetNode<RichTextLabel>(partyMember.stats[i].statType + "").Text = partyMember.stats[i].statType + ": " + partyMember.stats[i].value;
      }
   }

   void LoadAbilities(Member partyMember)
   {
      ClearAbilities();
      GenerateAbilityLabel(partyMember.specialAbility);

      for (int i = 0; i < partyMember.abilities.Count; i++)
      {
         GenerateAbilityLabel(partyMember.abilities[i]);
      }
   }

   void ClearAbilities()
   {
      foreach (RichTextLabel label in abilityList.GetChildren())
      {
         abilityList.RemoveChild(label);
         label.QueueFree();
      }
   }

   void GenerateAbilityLabel(AbilityResource abilityResource)
   {
      RichTextLabel abilityLabel = abilityLabelPrefab.Instantiate<RichTextLabel>();
      abilityLabel.Text = abilityResource.name;
      abilityLabel.TooltipText = abilityResource.description;

      abilityList.AddChild(abilityLabel);
   }

   void LoadEquipment(Member partyMember)
   {
      for (int i = 0; i < equipmentList.GetChildCount(); i++)
      {
         Button button = equipmentList.GetNode<Button>(partyMember.equipment[i].itemType + "");
         if (partyMember.equipment[i].name == null)
         {
            button.Text = partyMember.equipment[i].itemType + ": None";
            button.TooltipText = "";
         }
         else
         {
            button.Text = partyMember.equipment[i].itemType + ": " + partyMember.equipment[i].name;
            button.TooltipText = partyMember.equipment[i].description;
         }
      }
   }

   void OnEquipButtonDown(int index)
   {
      isReequipping = true;

      for (int i = 0; i < partyManager.Items.Count; i++)
      {
         if (partyManager.Items[i].item.itemType == (ItemType)index && (partyManager.Items[i].item.itemCategory == currentMember.baseMember.itemCategoryWorn
             || partyManager.Items[i].item.itemCategory == currentMember.baseMember.itemCategoryWielded) || partyManager.Items[i].item.itemType == ItemType.Accessory)
         {
            AddReequipButton(partyManager.Items[i].item);
         }
      }
      menuManager.DisableTabs();
      DisableMenu();
   }

   void AddReequipButton(ItemResource item)
   {
      Button button = reequipButtonPrefab.Instantiate<Button>();
      button.Text = item.name;
      button.TooltipText = item.description;
      button.GetNode<ItemResourceHolder>("ResourceHolder").itemResource = new InventoryItem(item, 1);

      reequipList.AddChild(button);
   }

   public void Reequip(ItemResource item)
   {
      partyManager.EquipItem(item, currentMember);
      ClearReequipList();
      OnEquipButtonDown((int)item.itemType);
      LoadStats(currentMember);
      LoadEquipment(currentMember);
   }

   void ClearReequipList()
   {
      foreach (Button button in reequipList.GetChildren())
      {
         reequipList.RemoveChild(button);
         button.QueueFree();
      }
   }

   void OnSwapButtonDown()
   {
      foreach (Button button in equipmentList.GetChildren())
      {
         button.Disabled = true;
      }

      menuManager.DisableTabs();
      isSwapping = true;
   }

   public void CompleteSwap(string secondSwap)
   {
      Member firstMember = GetMemberFromName(firstSwap);
      Member secondMember = GetMemberFromName(secondSwap);

      Member temp = firstMember;
      bool secondPartyStatus = secondMember.isInParty;

      int firstMemberIndex = -1;
      int secondMemberIndex = -1;
      
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i] == firstMember)
         {
            firstMemberIndex = i;
         }

         if (partyManager.Party[i] == secondMember)
         {
            secondMemberIndex = i;
         }
      }

      if (!CheckSwapViability(firstMemberIndex, secondMemberIndex))
      {
         CancelSwap();
         return;
      }

      partyManager.Party[firstMemberIndex] = secondMember;
      partyManager.Party[firstMemberIndex].isInParty = temp.isInParty;
      partyManager.Party[secondMemberIndex] = temp;
      partyManager.Party[secondMemberIndex].isInParty = secondPartyStatus;

      // Swap playability
      if (firstMemberIndex == 0 || secondMemberIndex == 0)
      {
         Member newPlayableCharacter = partyManager.Party[0];
         Member otherCharacter = null;

         if (firstMemberIndex == 0)
         {
            otherCharacter = partyManager.Party[secondMemberIndex];
         }
         else
         {
            otherCharacter = partyManager.Party[firstMemberIndex];
         }

         // We need to always keep the playable character at Member1, which is why the models of the characters are switched
         if (otherCharacter.model != null)
         {
            Node3D tempModel = newPlayableCharacter.model;
            newPlayableCharacter.model = otherCharacter.model;
            otherCharacter.model = tempModel;
         }
         else
         {
            newPlayableCharacter.model = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1");
         }

         AddNewComponentsToModel(newPlayableCharacter);
         controller.ResetNodes();

         // The holder prefab comes with the navigator, which we no longer need when we're the playable character
         NavigationAgent3D navigator = newPlayableCharacter.model.GetNode<NavigationAgent3D>("NavigationAgent3D");
         newPlayableCharacter.model.RemoveChild(navigator);
         navigator.QueueFree();

         // Remove the dialogue box to prevent talking with self
         newPlayableCharacter.model.GetNode<StaticBody3D>("DialogueBox").QueueFree();

         // We need to populate the contents of the other party member
         if (otherCharacter.isInParty)
         {
            AddNewComponentsToModel(otherCharacter);
            otherCharacter.model.GetNode<OverworldPartyController>("../" + otherCharacter.model.Name).ResetNodes(otherCharacter);
         }

         ResetDialogueHolders();
         EmitSignal(SignalName.PartySwap);
      }

      ClearMemberButtons();
      LoadMemberButtons();
      LoadNewPartyScreen(temp.characterName);
      EnableMenu();
      menuManager.EnableTabs();
      isSwapping = false;
      firstSwap = null;
   }

   public void AddNewComponentsToModel(Member member)
   {
      PackedScene componentsPrefab = GD.Load<PackedScene>("res://PartyMembers/" + member.characterType + "/model_holder.tscn");
      Node3D componentsHolder = componentsPrefab.Instantiate<Node3D>();
      bool hasCameraTarget = false;

      foreach (Node child in member.model.GetChildren())
      {
         // Do NOT remove the camera target to avoid deleting the player camera
         if (child.Name != "CameraTarget")
         {
            member.model.RemoveChild(child);
            child.QueueFree();
         }
         else
         {
            hasCameraTarget = true;
         }
      }

      // Move every child from the holder to the model
      foreach (Node child in componentsHolder.GetChildren())
      {
         if (child.Name != "CameraPositioner")
         {
            componentsHolder.RemoveChild(child);
            member.model.AddChild(child);

            if (child.Name == "DialogueHolder")
            {
               for (int i = 0; i < dialogueManager.dialoguesInThisRegion.Length; i++)
               {
                  if (dialogueManager.dialoguesInThisRegion[i].associatedReceiver == member.characterType
                     && dialogueManager.dialoguesInThisRegion[i].associatedInteractor == partyManager.Party[0].characterType)
                  {
                     child.GetNode<DialogueInteraction>("../DialogueHolder").dialogueList = dialogueManager.dialoguesInThisRegion[i].dialogues;
                  }
               }
            }
         }
         
         if (hasCameraTarget)
         {
            member.model.GetNode<Node3D>("CameraTarget").Position = componentsHolder.GetNode<Node3D>("CameraPositioner").Position;
         }
      }

      componentsHolder.QueueFree();
   }

   public void ResetDialogueHolders()
   {
      for (int i = 1; i < 4; i++)
      {
         DialogueInteraction dialogueHolder = (DialogueInteraction)GetNode<CharacterBody3D>("/root/BaseNode/PartyMembers/Member" + (i + 1)).FindChild("DialogueHolder");

         for (int j = 0; j < dialogueManager.dialoguesInThisRegion.Length; j++)
         {
            if (dialogueManager.dialoguesInThisRegion[j].associatedReceiver == partyManager.Party[i].characterType
               && dialogueManager.dialoguesInThisRegion[j].associatedInteractor == partyManager.Party[0].characterType)
            {
               dialogueHolder.dialogueList = dialogueManager.dialoguesInThisRegion[j].dialogues;
               continue;
            }
         }
      }
   }

   void OnAddPartyButtonDown()
   {
      //currentMember.isInParty = true;
      AddCharacterToParty(currentMember);

      addPartyButton.Disabled = true;
      removePartyButton.Disabled = false;

      ChangeMemberButtonHighlight(currentMember.characterName, true);
   }

   public void AddCharacterToParty(Member member)
   {
      member.isInParty = true;
      for (int i = 1; i < 4; i++)
      {
         Node3D memberNode = GetNode<Node3D>("/root/BaseNode/PartyMembers/Member" + (i + 1));
         
         if (memberNode.GetChildCount() == 0)
         {
            member.model = memberNode;
            break;
         }
      }

      AddNewComponentsToModel(member);
      member.model.GetNode<OverworldPartyController>("../" + member.model.Name).ResetNodes(member);
      member.model.Position = controller.Position - (controller.GlobalTransform.Basis.Z * 2f);
      member.model.GetNode<OverworldPartyController>("../" + member.model.Name).IsActive = true;
   }

   void OnRemovePartyButtonDown()
   {
      currentMember.isInParty = false;
      currentMember.model.GetNode<OverworldPartyController>("../" + currentMember.model.Name).IsActive = false;

      foreach (Node child in currentMember.model.GetChildren())
      {
         currentMember.model.RemoveChild(child);
         child.QueueFree();
      }

      currentMember.model = null;

      removePartyButton.Disabled = true;
      addPartyButton.Disabled = false;

      ChangeMemberButtonHighlight(currentMember.characterName, false);
   }

   void ChangeMemberButtonHighlight(string memberName, bool isHighlightOn)
   {
      foreach (Button child in partyList.GetChildren())
      {
         if (child.GetNode<RichTextLabel>("Title").Text == memberName)
         {
            child.GetNode<Panel>("Highlight").Visible = isHighlightOn;
         }
      }
   }

   bool CheckSwapViability(int firstIndex, int secondIndex)
   {
      if (partyManager.Party[firstIndex].isInParty)
      {
         if (firstIndex == 0)
         {
            // Trying to swap playable character (at index 0) with a character that is NOT a main character
            if (!mainCharacterTypes.Contains(partyManager.Party[secondIndex].characterType))
            {
               return false;
            }
         }
      }

      if (partyManager.Party[secondIndex].isInParty)
      {
         if (secondIndex == 0)
         {
            if (!mainCharacterTypes.Contains(partyManager.Party[firstIndex].characterType))
            {
               return false;
            }
         }
      }

      return true;
   }

   void DisableMenu()
   {
      inputDisabled = true;
      foreach (Button button in partyList.GetChildren())
      {
         button.Disabled = true;
      }

      foreach (Button button in equipmentList.GetChildren())
      {
         button.Disabled = true;
      }
   }

   void EnableMenu()
   {
      inputDisabled = false;
      foreach (Button button in partyList.GetChildren())
      {
         button.Disabled = false;
      }

      foreach (Button button in equipmentList.GetChildren())
      {
         button.Disabled = false;
      }
   }
}
