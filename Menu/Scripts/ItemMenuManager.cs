using Godot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

public partial class ItemMenuManager : Node
{
   [Export]
   public PackedScene itemButtonPrefab;
   [Export]
   private PackedScene memberButtonPrefab;

	private PartyManager partyManager;
   private MenuManager menuManager;

   private VBoxContainer itemsContainer;
   public VBoxContainer partyContainer;

   public Member currentTarget;

   public InventoryItem currentItem;

   public bool isUsingItem;

   [Signal]
   public delegate void ItemUseEventHandler();

   public override void _Ready()
   {
      partyManager = GetNode<PartyManager>("/root/BaseNode/PartyManagerObj");
      itemsContainer = GetNode<VBoxContainer>("ItemSContainer/VBoxContainer");
      partyContainer = GetNode<VBoxContainer>("ListBackground/VBoxContainer");
      menuManager = GetNode<MenuManager>("../../MenuManager");
   }

   public void LoadItemMenu()
   {
      ClearItems();

      for (int i = 0; i < partyManager.Items.Count; i++)
      {
         InventoryItem currentItem = partyManager.Items[i];
         if (currentItem.item.scriptName != "")
         {
            Button currentButton = itemButtonPrefab.Instantiate<Button>();

            currentButton.GetNode<ItemResourceHolder>("ResourceHolder").itemResource = currentItem;

            Node2D scriptHolder = currentButton.GetNode<Node2D>("ScriptHolder");
            scriptHolder.SetScript(GD.Load<CSharpScript>("res://Combat/Items/Behaviors/" + currentItem.item.scriptName + ".cs"));

            currentButton.Text = currentItem.item.name + " (" + currentItem.quantity + "x)";
            currentButton.TooltipText = currentItem.item.description;
            currentButton.Name = "ItemButton" + (i + 1);
            itemsContainer.AddChild(currentButton);

            if (!currentItem.item.usableOutsideCombat)
            {
               currentButton.Disabled = true;
            }
         }
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

      EmitSignal(SignalName.ItemUse);

      currentItem.quantity--;

      UpdateItemQuantity(currentItem);

      if (currentItem.quantity <= 0)
      {
         partyManager.RemoveItem(currentItem);
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

      itemButton.Text = inventoryItem.item.name + " (" + inventoryItem.quantity + "x, " + inventoryItem.item.price + " each)";

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
      menuManager.EnableTabs();
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
         for (int i = 0; i < partyManager.Party.Count; i++)
         {
            LoadMemberButton(partyManager.Party[i]);
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
      member.GetNode<RichTextLabel>("Exp").Text = "Experience: " + partyMember.experience + "/" + partyManager.GetExperienceAtLevel(partyMember.level - 1);

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
         child.Disabled = false;
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
