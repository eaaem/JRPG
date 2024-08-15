using Godot;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;

public partial class CombatUIManager : Node
{
   [Export]
   private ManagerReferenceHolder managers;
   [Export]
   private CombatManager combatManager;
   
   [Export]
   private PackedScene abilityButton;
   [Export]
   private PackedScene effectPrefab;
   [Export]
   private PackedScene itemPrefab;
   [Export]
   private PackedScene stackPrefab;

   [Export]
   private CanvasLayer UI;
   [Export]
   private Control partyList;
   [Export]
   private Control enemyList;
   [Export]
   private Control options;
   [Export]
   private ScrollContainer abilityContainer;
   [Export]
   private GridContainer abilityButtonContainer;
   [Export]
   private Panel secondaryOptions;
   [Export]
   private VBoxContainer secondaryOptionsContainer;
   [Export]
   private RichTextLabel messageText;
   [Export]
   private Button cancelButton;
   [Export]
   private Button finalizeButton;
   [Export]
   private Panel itemsList;
   [Export]
   private VBoxContainer itemsContainer;
   [Export]
   private CanvasGroup victoryScreen;

   [Export]
   private StyleBoxFlat baseHighlight;
   [Export]
   private StyleBoxFlat alteredHighlight;

   private List<Control> partyPanels = new List<Control>();
   private List<Control> enemyPanels = new List<Control>();
	
	public override void _Ready()
	{
      /*for (int i = 0; i < 4; i++)
      {
         partyPanels.Add(UI.GetNode<Node2D>("CombatPartyList/HBoxContainer/Panel" + (i + 1) + "/Holder"));
      }

      for (int i = 0; i < 4; i++)
      {
         enemyPanels.Add(UI.GetNode<Node2D>("CombatEnemyList/HBoxContainer/Panel" + (i + 1) + "/Holder"));
      }*/
	}
   
   public void HidePanels() 
   {
      for (int i = 0; i < partyPanels.Count; i++)
      {
         partyPanels[i].GetNode<Panel>("Highlight").Visible = false;
         partyPanels[i].Visible = false;
      }

      for (int i = 0; i < enemyPanels.Count; i++)
      {
         enemyPanels[i].GetNode<Panel>("Highlight").Visible = false;
         enemyPanels[i].Visible = false;
      }
   }

   public void ShowLists() 
   {
      partyList.Visible = true;
      enemyList.Visible = true;
      messageText.GetParent<Control>().Visible = true;
   }

   public void InitializePartyPanel(Fighter fighter, int index) 
   {
      fighter.UIPanel = GD.Load<PackedScene>("res://Combat/UI/party_panel.tscn").Instantiate<Control>();

      fighter.UIPanel.GetNode<RichTextLabel>("NameLabel").Text = fighter.fighterName;

      fighter.UIPanel.GetNode<RichTextLabel>("HealthLabel").Text = fighter.currentHealth + "/" + fighter.maxHealth;
      fighter.UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = fighter.currentMana + "/" + fighter.maxMana;

      fighter.UIPanel.GetNode<TextureProgressBar>("HealthBar").Value = (fighter.currentHealth * 1f / fighter.maxHealth) * 100f;
      fighter.UIPanel.GetNode<TextureProgressBar>("ManaBar").Value = (fighter.currentMana * 1f / fighter.maxMana) * 100f;

      UI.GetNode<VBoxContainer>("CombatPartyList/VBoxContainer").AddChild(fighter.UIPanel);
   }

   public void InitializeEnemyPanel(Fighter fighter, int index) 
   {
      fighter.UIPanel = GD.Load<PackedScene>("res://Combat/UI/enemy_panel.tscn").Instantiate<Control>();

      fighter.UIPanel.GetNode<RichTextLabel>("NameLabel").Text = fighter.fighterName;
      fighter.UIPanel.GetNode<RichTextLabel>("HealthLabel").Text = fighter.currentHealth + "/" + fighter.maxHealth;

      UI.GetNode<VBoxContainer>("CombatEnemyList/VBoxContainer").AddChild(fighter.UIPanel);
   }

   public void SetHightlightVisibility(Fighter fighter, bool visible, bool isCompanion) 
   {
      if (!isCompanion) 
      {
         fighter.UIPanel.GetNode<Panel>("Highlight").Visible = visible;
      } 
      else 
      {
         fighter.UIPanel.GetNode<Panel>("CompanionHolder/Highlight").Visible = visible;
      }
   }

   void OnHoverOverFighterPanel(Fighter fighter)
   {
      Panel highlight = fighter.UIPanel.GetNode<Panel>("Highlight");
      //Fighter fighter = combatManager.GetFighterFromIndex(index, isEnemy);

      if (combatManager.CurrentAbility != null)
      {
         ExtraHoverOverBehavior(combatManager.CurrentAbility.hitsSurrounding, combatManager.CurrentAbility.hitsAll, fighter);
         messageText.Text = "Cast " + combatManager.CurrentAbility.name + " on " + fighter.fighterName + ".";
      }
      else if (combatManager.CurrentItem != null)
      {
         ExtraHoverOverBehavior(combatManager.CurrentItem.item.hitsSurrounding, combatManager.CurrentItem.item.hitsAll, fighter);
         messageText.Text = "Use " + combatManager.CurrentItem.item.name + " on " + fighter.fighterName + ".";
      }
      else
      {
         messageText.Text = "Attack " + fighter.fighterName + ".";
      }

      if (fighter == combatManager.CurrentFighter)
      {
         highlight.AddThemeStyleboxOverride("panel", alteredHighlight);
      }
      else
      {
         //combatManager.CurrentFighter.model.GetNode<Node3D>("Model").LookAt(fighter.model.GlobalPosition, Vector3.Up, true);
         highlight.Visible = true;
      }

      fighter.placementNode.GetNode<Decal>("SelectionHighlight").Visible = true;
   }

   void OnStopHoverOverFighterPanel(Fighter fighter)
   {
      Panel highlight = fighter.UIPanel.GetNode<Panel>("Highlight");
      //Fighter fighter = combatManager.GetFighterFromIndex(index, isEnemy);

      if (combatManager.CurrentAbility != null)
      {
         ExtraStopHoverOverBehavior(combatManager.CurrentAbility.hitsSurrounding, combatManager.CurrentAbility.hitsAll, fighter);
      }
      else if (combatManager.CurrentItem != null)
      {
         ExtraStopHoverOverBehavior(combatManager.CurrentItem.item.hitsSurrounding, combatManager.CurrentItem.item.hitsAll, fighter);
      }

      StopHoveringOverInformation();

      if (fighter == combatManager.CurrentFighter)
      {
         highlight.RemoveThemeStyleboxOverride("panel");
         highlight.AddThemeStyleboxOverride("panel", baseHighlight);
      }
      else
      {
         highlight.Visible = false;
      }

      fighter.placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
   }

   void ExtraHoverOverBehavior(bool hitsSurrounding, bool hitsAll, Fighter target)
   {
      if (hitsAll)
      {
         List<Fighter> all = combatManager.GetAll(target);
         for (int i = 0; i < all.Count; i++)
         {
            all[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
            all[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = true;
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = combatManager.GetSurrounding(target);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
            surrounding[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = true;
         }
      }
   }

   void ExtraStopHoverOverBehavior(bool hitsSurrounding, bool hitsAll, Fighter target)
   {
      if (hitsAll)
      {
         List<Fighter> all = combatManager.GetAll(target);
         for (int i = 0; i < all.Count; i++)
         {
            all[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
            all[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = combatManager.GetSurrounding(target);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
            surrounding[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }
      }
   }

   public Control GetPanelFromIndex(int index, bool isEnemy)
   {
      string root = isEnemy ? "Enemy" : "Party";
      return UI.GetNode<Control>("Combat" + root + "List/HBoxContainer/Panel" + (index + 1) + "/Holder");
   }

   public void EnablePanels()
   {
      /*if (combatManager.CurrentAbility != null && combatManager.CurrentAbility.onlyHitsSelf)
      {
         combatManager.CurrentFighter.UIPanel.GetNode<Button>("Selection").Disabled = false;
         combatManager.CurrentFighter.UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Stop;
         return;
      }

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (combatManager.Fighters[i] != combatManager.CurrentFighter)
         {
            if (combatManager.CurrentAbility == null || !combatManager.CurrentAbility.onlyHitsTeam 
                || (combatManager.CurrentAbility.onlyHitsTeam && !combatManager.Fighters[i].isEnemy))
            {
               combatManager.Fighters[i].UIPanel.GetNode<Button>("Selection").Disabled = false;
               combatManager.Fighters[i].UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Stop;
            }
         }
      }

      if (combatManager.CurrentAbility != null && combatManager.CurrentAbility.hitsSelf)
      {
         combatManager.CurrentFighter.UIPanel.GetNode<Button>("Selection").Disabled = false;
         combatManager.CurrentFighter.UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Stop;
      }*/
   }

   public void DisablePanels()
   {
      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         //combatManager.Fighters[i].UIPanel.GetNode<Button>("Selection").Disabled = true;
        // combatManager.Fighters[i].UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Ignore;
      }
   }

   public void GenerateAbilityUI()
   {
      Member member = combatManager.GetCurrentMember();
      PackedScene packedSceneButton = GD.Load<PackedScene>(abilityButton.ResourcePath);

      List<AbilityResource> abilitiesToUse;
      int manaToUse;

      string path = combatManager.IsCompanionTurn ? "Enemy" : "Party";

      if (combatManager.IsCompanionTurn)
      {
         abilitiesToUse = new List<AbilityResource>(combatManager.CurrentFighter.companion.abilities);
         manaToUse = combatManager.CurrentFighter.companion.currentMana;
      }
      else
      {
         abilitiesToUse = new List<AbilityResource>(combatManager.CurrentFighter.abilities);
         manaToUse = combatManager.CurrentFighter.currentMana;
      }

      for (int i = 0; i < abilitiesToUse.Count; i++)
      {
         Button currentButton = GenerateAbilityButton(packedSceneButton, abilitiesToUse[i], manaToUse, path);
         currentButton.Name = "AbilityButton" + (i + 1);

         abilityButtonContainer.AddChild(currentButton);
      }

      if (!combatManager.IsCompanionTurn && combatManager.CurrentFighter.level >= 3)
      {
         Button specialButton = GenerateAbilityButton(packedSceneButton, member.specialAbility, manaToUse, path);

         specialButton.Name = "Special";

         if (combatManager.CurrentFighter.specialCooldown > 0)
         {
            specialButton.Disabled = true;
         }

         abilityContainer.AddChild(specialButton);
         specialButton.Position = new Vector2(0, -30);
      }
   }

   Button GenerateAbilityButton(PackedScene packedSceneButton, AbilityResource ability, int mana, string path) 
   {
      // Here, a button is created from the prefab, then the corresponding script is loaded in and attached to the ScriptHolder (a child of the button)
      // Each ability has its own script, which has special behavior, including what happens upon pressing the button
      Button button = packedSceneButton.Instantiate<Button>();

      button.GetNode<ResourceHolder>("ResourceHolder").abilityResource = ability;

      Node2D scriptHolder = button.GetNode<Node2D>("ScriptHolder");
      scriptHolder.SetScript(GD.Load<CSharpScript>("res://Abilities/" + path + "/" + ability.scriptName + "/" + ability.scriptName + ".cs"));

      button.GetNode<Label>("Label").Text = ability.name;
      button.MouseEntered += () => HoverOverInformation(ability.description);
      button.MouseExited += StopHoveringOverInformation;

      if (mana < ability.manaCost)
      {
         button.Disabled = true;
      }

      return button;
   }

   public void ClearAbilityUI()
   {
      int size = abilityButtonContainer.GetChildCount();
      for (int i = 0; i < size; i++)
      {
         Button button = abilityButtonContainer.GetNode<Button>("AbilityButton" + (i + 1));
         abilityButtonContainer.RemoveChild(button);
         //button.MouseEntered -= () => HoverOverInformation;
         //button.MouseExited -= StopHoveringOverInformation;
         button.QueueFree();
      }

      // Special button is present
      if (abilityContainer.GetChildCount() > 1)
      {
         Control specialButton = abilityContainer.GetNode<Control>("Special");
         abilityContainer.RemoveChild(specialButton);
         specialButton.QueueFree();
      }
   }

   public void DisableOtherAbilities()
   {
      for (int i = 0; i < abilityButtonContainer.GetChildCount(); i++)
      {
         abilityButtonContainer.GetNode<Button>("AbilityButton" + (i + 1)).Disabled = true;
      }

      if (abilityContainer.GetChildCount() > 1)
      {
         abilityContainer.GetNode<Button>("Special").Disabled = true;
      }
   }

   public void UpdateAbilities()
   {
      Member member = combatManager.GetCurrentMember();

      List<AbilityResource> abilitiesToUse;
      int manaToUse;

      if (combatManager.IsCompanionTurn)
      {
         abilitiesToUse = new List<AbilityResource>(combatManager.CurrentFighter.companion.abilities);
         manaToUse = combatManager.CurrentFighter.companion.currentMana;
      }
      else
      {
         abilitiesToUse = new List<AbilityResource>(combatManager.CurrentFighter.abilities);
         manaToUse = combatManager.CurrentFighter.currentMana;;
      }

      for (int i = 0; i < abilityButtonContainer.GetChildCount(); i++)
      {
         if (abilitiesToUse[i].manaCost <= manaToUse)
         {
            abilityButtonContainer.GetNode<Button>("AbilityButton" + (i + 1)).Disabled = false;
         }
         else
         {
            abilityButtonContainer.GetNode<Button>("AbilityButton" + (i + 1)).Disabled = true;
         }
      }

      if (abilityContainer.GetChildCount() > 1)
      {
         if (member.specialAbility.manaCost <= combatManager.CurrentFighter.currentMana && combatManager.CurrentFighter.specialCooldown <= 0)
         {
            abilityContainer.GetNode<Button>("Special").Disabled = false;
         }
         else
         {
            abilityContainer.GetNode<Button>("Special").Disabled = true;
         }
      }
   }

   public void GenerateItemUI()
   {
      PackedScene packedScene = GD.Load<PackedScene>(itemPrefab.ResourcePath);

      for (int i = 0; i < managers.PartyManager.Items.Count; i++)
      {
         if (managers.PartyManager.Items[i].item.itemType == ItemType.None && managers.PartyManager.Items[i].item.itemType == ItemType.Consumable)
         {
            // This is much like GenerateAbilities (creates buttons that have scripts inside them), but with items instead
            InventoryItem thisItem = managers.PartyManager.Items[i];
            Button currentButton = packedScene.Instantiate<Button>();

            currentButton.GetNode<ItemResourceHolder>("ResourceHolder").itemResource = thisItem;

            Node2D scriptHolder = currentButton.GetNode<Node2D>("ScriptHolder");
            scriptHolder.SetScript(GD.Load<CSharpScript>(thisItem.item.scriptPath));

            currentButton.Text = thisItem.item.name + " (" + thisItem.quantity + "x)";
            currentButton.TooltipText = thisItem.item.description;
            currentButton.Name = "ItemButton" + (i + 1);
            itemsContainer.AddChild(currentButton);
         }
      }
   }
   
   public void ClearItemUI()
   {
      int size = itemsContainer.GetChildCount();
      for (int i = 0; i < size; i++)
      {
         Control button = itemsContainer.GetNode<Control>("ItemButton" + (i + 1));
         itemsContainer.RemoveChild(button);
         button.QueueFree();
      }
   }

   public void UpdateItems(InventoryItem changedItem)
   {
      for (int i = 0; i < managers.PartyManager.Items.Count; i++)
      {
         if (managers.PartyManager.Items[i] == changedItem)
         {
            managers.PartyManager.Items[i].quantity--;

            // Update UI accordingly
            if (managers.PartyManager.Items[i].quantity <= 0)
            {
               managers.PartyManager.Items.Remove(managers.PartyManager.Items[i]);
               Control button = itemsContainer.GetNode<Control>("ItemButton" + (i + 1));
               itemsContainer.RemoveChild(button);
               button.QueueFree();
            }
            else
            {
               itemsContainer.GetNode<Button>("ItemButton" + (i + 1)).Text = changedItem.item.name + " (" + changedItem.quantity + "x)";
            }
         }
      }
   }

   public void DisableItems()
   {
      for (int i = 0; i < itemsContainer.GetChildCount(); i++)
      {
         itemsContainer.GetNode<Button>("ItemButton" + (i + 1)).Disabled = true;
      }
   }

   public void EnableItems()
   {
      for (int i = 0; i < itemsContainer.GetChildCount(); i++)
      {
         itemsContainer.GetNode<Button>("ItemButton" + (i + 1)).Disabled = false;
      }
   }

   public void UpdateStatusTurns(Fighter fighter, AppliedStatusEffect currentStatus, int statusIndex)
   {
      VBoxContainer statusRows = fighter.UIPanel.GetNode<VBoxContainer>("Effects/StatusRows");
      HBoxContainer row1 = statusRows.GetNode<HBoxContainer>("Row1");
      HBoxContainer row2 = statusRows.GetNode<HBoxContainer>("Row2");

      if (statusIndex < row1.GetChildCount())
      {
         row1.GetChild(statusIndex).GetChild(0).GetNode<Label>("Quantity").Text = "" + currentStatus.remainingTurns;
      }
      else
      {
         row2.GetChild(statusIndex - row1.GetChildCount()).GetChild(0).GetNode<Label>("Quantity").Text = "" + currentStatus.remainingTurns;
      }
   }

   public void RemoveEffectUI(StatusEffect effect, Fighter applied)
   {
      VBoxContainer statusRows = applied.UIPanel.GetNode<VBoxContainer>("Effects").GetNode<VBoxContainer>("StatusRows");
      HBoxContainer row1 = statusRows.GetNode<HBoxContainer>("Row1");
      HBoxContainer row2 = statusRows.GetNode<HBoxContainer>("Row2");
      bool affectRow1 = true;

      Panel childToRemove = null;

      foreach (Panel child in row1.GetChildren())
      {
         if (child.HasNode(effect + ""))
         {
            childToRemove = child;
            break;
         }
      }

      if (childToRemove == null)
      {
         affectRow1 = false;

         foreach (Panel child in row2.GetChildren())
         {
            if (child.GetNode<Sprite2D>(effect + "") != null)
            {
               childToRemove = child;
               break;
            }
         }
      }

      if (affectRow1)
      {
         row1.RemoveChild(childToRemove);

         if (row1.GetChildCount() == 0 && row2.GetChildCount() == 0)
         {
            row1.Visible = false;
         }
         else if (row2.GetChildCount() > 0)
         {
            // If row2 has effects, then we need to move one to the first row in order to keep things tidy
            Node toMove = row2.GetChild(0);
            row2.RemoveChild(toMove);
            row1.AddChild(toMove);

            if (row2.GetChildCount() == 0)
            {
               row2.Visible = false;
            }
         }
      }
      else
      {
         row2.RemoveChild(childToRemove);
      }

      childToRemove.QueueFree();
   }

   public void UpdateStacksAndEffectsUI(Fighter target)
   {
      HBoxContainer stacksContainer = target.UIPanel.GetNode<HBoxContainer>("Effects/Stacks");
      List<Stack> stacksToRemove = new List<Stack>();

      for (int i = 0; i < stacksContainer.GetChildCount(); i++)
      {
         Panel child = stacksContainer.GetChild<Panel>(i);
         child.Visible = true;

         if (target.stacks[i].needsQuantityUpdate)
         {
            child.GetNode<Label>("Sprite/Quantity").Text = "" + target.stacks[i].quantity;
            target.stacks[i].needsQuantityUpdate = false;
         }

         if (target.stacks[i].quantity <= 0)
         {
            stacksToRemove.Add(target.stacks[i]);
            stacksContainer.RemoveChild(child);
            child.QueueFree();
         }
      }

      foreach (Stack stack in stacksToRemove)
      {
         target.stacks.Remove(stack);
      }

      VBoxContainer statusRows = target.UIPanel.GetNode<VBoxContainer>("Effects/StatusRows");
      HBoxContainer row1 = statusRows.GetNode<HBoxContainer>("Row1");
      HBoxContainer row2 = statusRows.GetNode<HBoxContainer>("Row2");

      for (int i = 0; i < row1.GetChildCount() + row2.GetChildCount(); i++)
      {
         Panel child;
         int increment = i;

         if (i < row1.GetChildCount())
         {
            child = row1.GetChild<Panel>(i);
         }
         else
         {
            increment = i - row1.GetChildCount();
            child = row2.GetChild<Panel>(increment);
         }

         if (!child.Visible)
         {
            child.Visible = true;
            Label quantityLabel = child.GetChild(0).GetNode<Label>("Quantity");

            if (target.currentStatuses[increment].displayRemainingTurns)
            {
               quantityLabel.Text = "" + target.currentStatuses[increment].remainingTurns;
               quantityLabel.Visible = true;
            }
            else
            {
               quantityLabel.Visible = false;
            }
         }
         
      }
   }

   public void AddStackUI(Fighter target, Stack stack) 
   {
      HBoxContainer stacksContainer = target.UIPanel.GetNode<HBoxContainer>("Effects/Stacks");
      stacksContainer.Visible = true;
      Panel stackButton = stackPrefab.Instantiate<Panel>();
      stackButton.TooltipText = stack.stackName;
      stackButton.GetNode<Sprite2D>("Sprite").Texture = GD.Load<Texture2D>("res://Combat/Stacks/" + stack.spriteName + ".png");
      stackButton.GetNode<Label>("Sprite/Quantity").Text = "" + stack.quantity;
      stackButton.Visible = false;
      stacksContainer.AddChild(stackButton);
   }

   public void AddEffectUI(StatusEffect effect, Fighter applied, string description)
   {
      VBoxContainer statusRows = applied.UIPanel.GetNode<VBoxContainer>("Effects/StatusRows");
      HBoxContainer rowToUse = statusRows.GetNode<HBoxContainer>("Row1");

      if (rowToUse.GetChildCount() == 0)
      {
         rowToUse.Visible = true;
      }
      else if (rowToUse.GetChildCount() == 13) // Row1 is full
      {
         rowToUse = statusRows.GetNode<HBoxContainer>("Row2");
         rowToUse.Visible = true;
      }

      Panel effectBack = stackPrefab.Instantiate<Panel>();
      effectBack.TooltipText = description;
      Texture2D sprite = GD.Load<Texture2D>("res://Combat/StatusEffects/" + effect + ".png");
      Sprite2D spriteNode = effectBack.GetNode<Sprite2D>("Sprite");
      spriteNode.Texture = sprite;
      spriteNode.Name = effect + "";

      effectBack.Visible = false;
      
      rowToUse.AddChild(effectBack);
   }

   public void HideAll() 
   {
      options.Visible = false;
      cancelButton.Visible = false;
      abilityContainer.Visible = false;
      cancelButton.Visible = false;
      secondaryOptions.Visible = false;
      messageText.GetParent<Control>().Visible = false;
   }

   public void ShowSelectionBox()
   {
      options.Visible = true;
   }

	public void MoveSecondaryOptionsToLeft()
   {
      secondaryOptions.Position = new Vector2(250, 500);
   }

   public void MoveSecondaryOptionsToRight()
   {
      secondaryOptions.Position = new Vector2(450, 500);
   }

   public void ClearSecondaryOptions()
   {
      VBoxContainer container = secondaryOptions.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
      foreach (Button child in container.GetChildren())
      {
         container.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void SetDefeatScreenVisible(bool visible) 
   {
      UI.GetNode<Node2D>("Overlay/DefeatScreen").Visible = false;
   }

   public void ExitVictoryScreen() 
   {
      victoryScreen.Visible = false;

      for (int i = 0; i < 4; i++)
      {
         victoryScreen.GetNode<Label>("Back/ExpNotif" + (i + 1)).Visible = false;
      }
   }

   public void UpdateVictoryExp(Member partyMember, int index, int oldLevel, int oldAbilityCount) 
   {
      if (oldLevel != partyMember.level)
      {
         victoryScreen.GetNode<Label>("Back/ExpNotif" + (index + 1)).Text += "; level up to soul grade " + partyMember.level + "!";
      }

      if (oldAbilityCount != partyMember.abilities.Count)
      {
         victoryScreen.GetNode<Label>("Back/ExpNotif" + (index + 1)).Text += "Learns " + partyMember.abilities[oldAbilityCount].name;
      }

      victoryScreen.GetNode<Label>("Back/ExpNotif" + (index + 1)).Visible = true;
   }

   public void UpdateUI()
   {
      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         Fighter fighter = combatManager.Fighters[i];
         fighter.UIPanel.GetNode<Panel>("Highlight").Visible = false;
         fighter.UIPanel.GetNode<RichTextLabel>("HealthLabel").Text = fighter.currentHealth + "/" + fighter.maxHealth;
         fighter.UIPanel.GetNode<TextureProgressBar>("HealthBar").Value = (fighter.currentHealth * 1f / fighter.maxHealth) * 100f;

         if (!fighter.isEnemy)
         {
            fighter.UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = fighter.currentMana + "/" + fighter.maxMana;
            fighter.UIPanel.GetNode<TextureProgressBar>("ManaBar").Value = (fighter.currentMana * 1f / fighter.maxMana) * 100f;
         }
      }
   }

   public void UpdateSingularUIPanel(Fighter fighter)
   {
      if (fighter.currentHealth > fighter.maxHealth)
      {
         fighter.currentHealth = fighter.maxHealth;
      }
      else if (fighter.currentHealth < 0)
      {
         fighter.currentHealth = 0;
      }

      if (fighter.currentMana > fighter.maxMana)
      {
         fighter.currentMana = fighter.maxMana;
      }
      else if (fighter.currentMana < 0)
      {
         fighter.currentMana = 0;
      }

      fighter.UIPanel.GetNode<Panel>("Highlight").Visible = false;
      fighter.UIPanel.GetNode<Label>("HealthDescription").Text = fighter.currentHealth + "/" + fighter.maxHealth;
      fighter.UIPanel.GetNode<ProgressBar>("HealthBar").Value = (fighter.currentHealth * 1f / fighter.maxHealth) * 100f;

      if (!fighter.isEnemy)
      {
         fighter.UIPanel.GetNode<Label>("ManaDescription").Text = fighter.currentMana + "/" + fighter.maxMana;
         fighter.UIPanel.GetNode<ProgressBar>("ManaBar").Value = (fighter.currentMana * 1f / fighter.maxMana) * 100f;

         if (fighter.companion != null)
         {
            fighter.UIPanel.GetNode<Label>("CompanionHolder/ManaDescription").Text = fighter.companion.currentMana + "/" + fighter.companion.maxMana;
            fighter.UIPanel.GetNode<ProgressBar>("CompanionHolder/ManaBar").Value = (fighter.companion.currentMana * 1f / fighter.companion.maxMana) * 100f;
         }
      }
   }

   public void EnableOptions()
   {
      VBoxContainer panel = options.GetNode<VBoxContainer>("Choices");
      panel.GetNode<Button>("AttackButton").Disabled = false;
      panel.GetNode<Button>("CastButton").Disabled = false;

      if (!combatManager.IsCompanionTurn)
      {
         panel.GetNode<Button>("ItemButton").Disabled = false;
      }
   }

   public void DisableOptions()
   {
      VBoxContainer panel = options.GetNode<VBoxContainer>("Choices");
      panel.GetNode<Button>("AttackButton").Disabled = true;
      panel.GetNode<Button>("CastButton").Disabled = true;
      panel.GetNode<Button>("ItemButton").Disabled = true;
   }

   public void CastButton()
   {
      UpdateAbilities();
      StopHoveringOverInformation();
      SetAbilityContainerVisible(true);
      SetChoicesVisible(false);
   }

   public void ItemButton()
   {
      if (itemsList.Visible == false)
      {
         StopHoveringOverInformation();
         itemsList.Visible = true;
         SetChoicesVisible(false);
      }
      else
      {
         itemsList.Visible = false;
         EnableOptions();
      }
   }

   public void GenerateTargets()
   {
      VBoxContainer targetsHolder = options.GetNode<VBoxContainer>("Targets/VBoxContainer");
      foreach (Node child in targetsHolder.GetChildren())
      {
         targetsHolder.RemoveChild(child);
         child.QueueFree();
      }

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         combatManager.Fighters[i].targetButton = null;
      }

      List<Fighter> targets = GetPossibleTargets();
      PackedScene scene = GD.Load<PackedScene>("res://Combat/UI/target_button.tscn");

      for (int i = 0; i < targets.Count; i++)
      {
         Button button = scene.Instantiate<Button>();

         button.GetNode<Label>("FighterName").Text = targets[i].fighterName;

         if (targets[i].isEnemy)
         {
            button.GetNode<Label>("FighterName").AddThemeColorOverride("font_color", new Color(1, 0.16f, 0.16f));
         }

         targetsHolder.AddChild(button);

         for (int j = 0; j < combatManager.Fighters.Count; j++)
         {
            if (combatManager.Fighters[j] == targets[i])
            {
               button.MouseEntered += () => OnHoverOverFighterPanel(combatManager.Fighters[j]);
               button.MouseExited += () => OnStopHoverOverFighterPanel(combatManager.Fighters[j]);
               button.ButtonDown += () => combatManager.OnFighterPanelDown(combatManager.Fighters[i]);
               break;
            }
         }
      }

      options.GetNode<ScrollContainer>("Targets").Visible = true;
   }

   List<Fighter> GetPossibleTargets()
   {
      List<Fighter> possibleTargets = new List<Fighter>();

      if (combatManager.CurrentAbility != null)
      {
         if (combatManager.CurrentAbility.onlyHitsSelf || combatManager.CurrentAbility.hitsSelf)
         {
            possibleTargets.Add(combatManager.CurrentFighter);
         }
         
         if (combatManager.CurrentAbility.onlyHitsTeam || combatManager.CurrentAbility.hitsTeam)
         {
            possibleTargets.AddRange(GetTeam(combatManager.CurrentAbility.bypassesDeaths));
         }

         if (combatManager.CurrentAbility.onlyHitsSelf || combatManager.CurrentAbility.onlyHitsTeam)
         {
            return possibleTargets;
         }
      }
      else if (combatManager.CurrentItem != null)
      {
         if (combatManager.CurrentItem.item.hitsSelf)
         {
            possibleTargets.Add(combatManager.CurrentFighter);
         }
         
         if (combatManager.CurrentItem.item.hitsTeam)
         {
            possibleTargets.AddRange(GetTeam(false));
         }
      }
      else
      {
         possibleTargets.AddRange(GetEnemies());
      }

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (!possibleTargets.Contains(combatManager.Fighters[i]) && combatManager.Fighters[i] != combatManager.CurrentFighter && !combatManager.Fighters[i].isDead)
         {
            possibleTargets.Add(combatManager.Fighters[i]);
         }
      }

      return possibleTargets;
   }

   List<Fighter> GetEnemies()
   {
      List<Fighter> enemies = new List<Fighter>();

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (combatManager.Fighters[i].isEnemy)
         {
            enemies.Add(combatManager.Fighters[i]);
         }
      }

      return enemies;
   }

   List<Fighter> GetTeam(bool includeDead)
   {
      List<Fighter> team = new List<Fighter>();

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (!combatManager.Fighters[i].isEnemy && combatManager.Fighters[i] != combatManager.CurrentFighter && (!combatManager.Fighters[i].isDead || includeDead))
         {
            team.Add(combatManager.Fighters[i]);
         }
      }

      return team;
   }

   public void SetChoicesVisible(bool visible)
   {
      options.GetNode<VBoxContainer>("Choices").Visible = visible;
   }

   public void HoverOverInformation(string information)
   {
      messageText.Text = information;
   }

   public void StopHoveringOverInformation()
   {
      if (abilityContainer.Visible)
      {
         messageText.Text = "Select an ability to cast.";

         if (combatManager.CurrentAbility != null)
         {
            messageText.Text = "Select a target to cast " + combatManager.CurrentAbility.name + " on.";
         }
      }
      else if (itemsList.Visible)
      {
         messageText.Text = "Select an item to use.";

         if (combatManager.CurrentItem != null)
         {
            messageText.Text = "Select a target to use " + combatManager.CurrentItem.item.name + " on.";
         }
      }
      else if (combatManager.IsAttacking)
      {
         messageText.Text = "Select a target to attack.";
      }
      else
      {
         messageText.Text = "Select an option.";
      }
   }

   public void EndOfCombatHiding()
   {
      HideAll();
      partyList.Visible = false;
      enemyList.Visible = false;
      partyPanels.Clear();
      enemyPanels.Clear();

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         Control panel = combatManager.Fighters[i].UIPanel;
         VBoxContainer container = null;
         
         if (combatManager.Fighters[i].isEnemy)
         {
            container = UI.GetNode<VBoxContainer>("CombatEnemyList/VBoxContainer");
         }
         else
         {
            container = UI.GetNode<VBoxContainer>("CombatPartyList/VBoxContainer");
         }
         
         container.RemoveChild(panel);
         panel.QueueFree();
      }
   }

   public void ResetStacksAndStatuses(Fighter fighter)
   {
      VBoxContainer statusRows = fighter.UIPanel.GetNode<VBoxContainer>("Effects/StatusRows");
      HBoxContainer row1 = statusRows.GetNode<HBoxContainer>("Row1");
      HBoxContainer row2 = statusRows.GetNode<HBoxContainer>("Row2");
      HBoxContainer stacksContainer = fighter.UIPanel.GetNode<HBoxContainer>("Effects/Stacks");
      
      foreach (Node child in row1.GetChildren())
      {
         row1.RemoveChild(child);
         child.QueueFree();
      }

      foreach (Node child in row2.GetChildren())
      {
         row2.RemoveChild(child);
         child.QueueFree();
      }

      foreach (Node child in stacksContainer.GetChildren())
      {
         stacksContainer.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void SetOlrenSpecialArrowHighlights(string arrowName)
   {
      foreach (Button child in secondaryOptionsContainer.GetChildren())
      {
         if (child.Text != arrowName)
         {
            child.GetNode<Panel>("Highlight").Visible = false;
         }
         else
         {
            child.GetNode<Panel>("Highlight").Visible = true;
         }
      }
   }

   public void AddOlrenArrow(Button arrowButton)
   {
      secondaryOptionsContainer.AddChild(arrowButton);
   }

   public void SetSecondaryOptionsVisible(bool visible)
   {
      secondaryOptions.Visible = visible;
   }

   public void SetVictoryScreenVisible(bool visible) 
   {
      victoryScreen.Visible = visible;
   }

   public void SetSelectionBoxVisible(bool visible) 
   {
      options.Visible = visible;
   }

   public void SetItemButtonVisible(bool visible) 
   {
      options.GetNode<VBoxContainer>("Choices").GetNode<Button>("ItemButton").Disabled = visible;
   }

   public void SetCancelButtonVisible(bool visible) 
   {
      cancelButton.Visible = visible;
   }

   public void SetCastButtonVisible(bool visible) 
   {
      options.GetNode<VBoxContainer>("Choices").GetNode<Button>("CastButton").Disabled = visible;
   }

   public void SetActionBar(Fighter fighter, int value)
   {
      //fighter.UIPanel.GetNode<ProgressBar>("ActionBar").Value = value;
   }

   public void SetTargetsVisible(bool visible)
   {
      options.GetNode<ScrollContainer>("Targets").Visible = visible;
   }

   public void SetAbilityContainerVisible(bool visible)
   {
      abilityContainer.Visible = visible;
   }

   public void SetItemListVisible(bool visible)
   {
      itemsList.Visible = visible;
   }

   public bool GetAbilityContainerVisible()
   {
      return abilityContainer.Visible;
   }

   public bool GetItemListVisible()
   {
      return itemsList.Visible;
   }
}
