using Godot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

public partial class ItemMenuManager : Panel
{
   [Export]
   public PackedScene itemButtonPrefab;
   [Export]
   private PackedScene memberButtonPrefab;
   [Export]
   private ManagerReferenceHolder managers;

   private VBoxContainer itemsContainer;
   public VBoxContainer partyContainer;

   public Member currentTarget;

   public InventoryItem currentItem;

   public bool isUsingItem;

   [Signal]
   public delegate void ItemUseEventHandler();

   public override void _Ready()
   {
      itemsContainer = GetNode<VBoxContainer>("ItemSContainer/VBoxContainer");
      partyContainer = GetNode<VBoxContainer>("ListBackground/VBoxContainer");
   }

   public void LoadItemMenu()
   {
      ClearItems();

      for (int i = 0; i < managers.PartyManager.Items.Count; i++)
      {
         InventoryItem currentItem = managers.PartyManager.Items[i];

         Button currentButton = itemButtonPrefab.Instantiate<Button>();

         currentButton.GetNode<ItemResourceHolder>("ResourceHolder").itemResource = currentItem;

         currentButton.Text = "  " + currentItem.item.name + " (" + currentItem.quantity + "x)";
         currentButton.TooltipText = currentItem.item.description;
         currentButton.Name = "ItemButton" + (i + 1);

         if (!currentItem.item.usableOutsideCombat)
         {
            currentButton.Disabled = true;
         }
         else
         {
            currentButton.ButtonDown += managers.ButtonSoundManager.OnClick;
            currentButton.MouseEntered += managers.ButtonSoundManager.OnHoverOver;
         }

         if (currentItem.item.scriptPath != "")
         {
            Node2D scriptHolder = currentButton.GetNode<Node2D>("ScriptHolder");
            scriptHolder.SetScript(GD.Load<CSharpScript>(currentItem.item.scriptPath));
         }

         itemsContainer.AddChild(currentButton);
      }
   }

   public void ClearItems()
   {
      foreach (Button child in itemsContainer.GetChildren())
      {
         itemsContainer.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void ProcessMemberButtonClick(string memberName)
   {
      Member member = GetMemberFromName(memberName);
      currentTarget = member;

      Popup popup = GD.Load<PackedScene>("res://Core/popup.tscn").Instantiate<Popup>();
      GetNode<CanvasLayer>("/root/BaseNode/UI/Overlay").AddChild(popup);
      popup.ReceiveInfo(1.5f, currentItem.item.outOfCombatUseMessage);

      EmitSignal(SignalName.ItemUse);

      AudioStreamPlayer audioPlayer = GD.Load<PackedScene>("res://Core/self_destructing_audio_player.tscn").Instantiate<AudioStreamPlayer>();
      audioPlayer.Stream = GD.Load<AudioStream>(currentItem.item.outOfCombatAudioPath);
      Node2D child = new Node2D();
      child.AddChild(audioPlayer);
      AddChild(child);
      audioPlayer.Play();

      currentItem.quantity--;

      UpdateItemQuantity(currentItem);

      if (currentItem.quantity <= 0)
      {
         managers.PartyManager.RemoveItem(currentItem);
         CancelItemUsage();
      }

      if (member.currentHealth > member.GetMaxHealth())
      {
         member.currentHealth = member.GetMaxHealth();
      }

      if (member.currentMana > member.GetMaxMana())
      {
         member.currentMana = member.GetMaxMana();
      }

      UpdateMemberButtons();
   }

   void UpdateItemQuantity(InventoryItem inventoryItem)
   {
      Button itemButton = null;

      foreach (Button child in itemsContainer.GetChildren())
      {
         if (child.TooltipText == inventoryItem.item.description)
         {
            itemButton = child;
            break;
         }
      }

      itemButton.Text = "  " + inventoryItem.item.name + " (" + inventoryItem.quantity + "x)";

      if (inventoryItem.quantity <= 0)
      {
         itemsContainer.RemoveChild(itemButton);
         itemButton.QueueFree();
      }
   }

   public void CancelItemUsage()
   {
      partyContainer.Visible = false;
      isUsingItem = false;
      currentItem = null;
      EnableMenu();
      managers.MenuManager.EnableTabs();
   }

   Member GetMemberFromName(string memberName)
   {
      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].characterName == memberName)
         {
            return managers.PartyManager.Party[i];
         }
      }

      return null;
   }

   public void OpenPartyScreen()
   {
      foreach (Button child in partyContainer.GetChildren())
      {
         partyContainer.RemoveChild(child);
         child.QueueFree();
      }

      LoadMemberButtons();
   }

   void LoadMemberButtons()
   {
      if (partyContainer.GetChildCount() == 0)
      {
         for (int i = 0; i < managers.PartyManager.Party.Count; i++)
         {
            LoadMemberButton(managers.PartyManager.Party[i]);
         }
      }
   }

   void LoadMemberButton(Member partyMember)
   {
      Button member = memberButtonPrefab.Instantiate<Button>();
      member.GetNode<RichTextLabel>("Title").Text = partyMember.characterName;
      member.GetNode<RichTextLabel>("Level").Text = "Level: " + partyMember.level;
      member.GetNode<RichTextLabel>("Health").Text = "Health: " + partyMember.currentHealth + "/" + partyMember.GetMaxHealth();
      member.GetNode<RichTextLabel>("Mana").Text = "Mana: " + partyMember.currentMana + "/" + partyMember.GetMaxMana();
      member.GetNode<RichTextLabel>("Exp").Text = "Experience: " + partyMember.experience + "/" + managers.PartyManager.GetExperienceAtLevel(partyMember.level - 1);

      member.ButtonDown += managers.ButtonSoundManager.OnClick;
      member.MouseEntered += managers.ButtonSoundManager.OnHoverOver;

      partyContainer.AddChild(member);
   }

   void UpdateMemberButtons()
   {
      foreach (Button child in partyContainer.GetChildren())
      {
         Member partyMember = GetMemberFromName(child.GetNode<RichTextLabel>("Title").Text);
         child.GetNode<RichTextLabel>("Health").Text = "Health: " + partyMember.currentHealth + "/" + partyMember.GetMaxHealth();
         child.GetNode<RichTextLabel>("Mana").Text = "Mana: " + partyMember.currentMana + "/" + partyMember.GetMaxMana();
      }
   }

   public void EnableMenu()
   {
      foreach (Button child in itemsContainer.GetChildren())
      {
         if (child.GetNode<ItemResourceHolder>("ResourceHolder").itemResource.item.usableOutsideCombat)
         {
            child.Disabled = false;
         }
      }
   }

   public void DisableMenu()
   {
      foreach (Button child in itemsContainer.GetChildren())
      {
         child.Disabled = true;
      }
   }
}
