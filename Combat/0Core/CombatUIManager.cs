using Godot;
using System.Collections.Generic;

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
   private PackedScene victoryNotifPrefab;
   [Export]
   private PackedScene turnOrderPicturePrefab;

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
   private Control victoryScreen;
   [Export]
   private VBoxContainer turnOrderContainer;

   [Export]
   private StyleBoxTexture baseHighlight;
   [Export]
   private StyleBoxTexture alteredHighlight;

   [Export]
   private Color[] damageTypeColors = new Color[0];

   private List<Control> partyPanels = new List<Control>();
   private List<Control> enemyPanels = new List<Control>();

   private bool uiIsUpdated = false;

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
      turnOrderContainer.GetParent().GetParent<Control>().Visible = true;
   }

   public void InitializePartyPanel(Fighter fighter, int index) 
   {
      fighter.UIPanel = GD.Load<PackedScene>("res://Combat/UI/party_panel.tscn").Instantiate<Control>();

      fighter.UIPanel.GetNode<RichTextLabel>("NameLabel").Text = fighter.fighterName;

      fighter.UIPanel.GetNode<RichTextLabel>("HealthLabel").Text = fighter.currentHealth + "/" + fighter.maxHealth;
      fighter.UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = "[right]" + fighter.currentMana + "/" + fighter.maxMana;

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

   public void SetHighlightVisibility(Fighter fighter, bool visible, bool isCompanion) 
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

            if (all[i] == combatManager.CurrentFighter)
            {
               all[i].UIPanel.GetNode<Panel>("Highlight").RemoveThemeStyleboxOverride("panel");
               all[i].UIPanel.GetNode<Panel>("Highlight").AddThemeStyleboxOverride("panel", alteredHighlight);
            }
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = combatManager.GetSurrounding(target);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
            surrounding[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = true;

            if (surrounding[i] == combatManager.CurrentFighter)
            {
               surrounding[i].UIPanel.GetNode<Panel>("Highlight").RemoveThemeStyleboxOverride("panel");
               surrounding[i].UIPanel.GetNode<Panel>("Highlight").AddThemeStyleboxOverride("panel", alteredHighlight);
            }
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

            if (all[i] == combatManager.CurrentFighter)
            {
               all[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
               all[i].UIPanel.GetNode<Panel>("Highlight").RemoveThemeStyleboxOverride("panel");
               all[i].UIPanel.GetNode<Panel>("Highlight").AddThemeStyleboxOverride("panel", baseHighlight);
            }
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = combatManager.GetSurrounding(target);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
            surrounding[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;

            if (surrounding[i] == combatManager.CurrentFighter)
            {
               surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
               surrounding[i].UIPanel.GetNode<Panel>("Highlight").RemoveThemeStyleboxOverride("panel");
               surrounding[i].UIPanel.GetNode<Panel>("Highlight").AddThemeStyleboxOverride("panel", baseHighlight);
            }
         }
      }
   }

   public Control GetPanelFromIndex(int index, bool isEnemy)
   {
      string root = isEnemy ? "Enemy" : "Party";
      return UI.GetNode<Control>("Combat" + root + "List/HBoxContainer/Panel" + (index + 1) + "/Holder");
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
         currentButton.Name = abilitiesToUse[i].name;

         abilityButtonContainer.AddChild(currentButton);
      }

      if (!combatManager.IsCompanionTurn && combatManager.CurrentFighter.level >= 3)
      {
         Button specialButton = GenerateAbilityButton(packedSceneButton, member.specialAbility, manaToUse, path);

         specialButton.Name = "Special";
         specialButton.AddThemeColorOverride("font_color", new Color(0.75f, 0.53f, 1f));

         string cooldownMessage = "";

         if (combatManager.CurrentFighter.specialCooldown > 0)
         {
            cooldownMessage = "[color=red]On cooldown for " + combatManager.CurrentFighter.specialCooldown + " more turn"
                              + (combatManager.CurrentFighter.specialCooldown > 1 ? "s" : "") + ". [/color]";
         }

         if (combatManager.CurrentFighter.currentMana < member.specialAbility.manaCost)
         {
            cooldownMessage += "[color=red]Not enough mana.[/color] ";
         }

         specialButton.MouseEntered += () => HoverOverInformation("[color=#bf87ff](SPECIAL)[/color] " + cooldownMessage 
                                                                  + "Costs " + member.specialAbility.manaCost + " mana. " + member.specialAbility.description);

         if (combatManager.CurrentFighter.specialCooldown > 0)
         {
            specialButton.Disabled = true;
         }

         abilityButtonContainer.AddChild(specialButton);
         abilityButtonContainer.MoveChild(specialButton, 0);
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
      scriptHolder.SetScript(GD.Load<CSharpScript>(ability.scriptPath));

      button.Text = ability.name;
      button.MouseExited += StopHoveringOverInformation;

      if (mana < ability.manaCost)
      {
         button.MouseEntered += () => HoverOverInformation("[color=red]Not enough mana.[/color] Costs " + ability.manaCost + " mana. " + ability.description);
         button.Disabled = true;
      }
      else
      {
         button.MouseEntered += () => HoverOverInformation("Costs " + ability.manaCost + " mana. " + ability.description);
         button.MouseEntered += () => SetManaBarLossIndicator(ability.manaCost);
         button.MouseEntered += managers.ButtonSoundManager.OnHoverOver;
         button.ButtonDown += managers.ButtonSoundManager.OnClick;
      }

      return button;
   }

   public void ClearAbilityUI()
   {
      foreach (Button child in abilityButtonContainer.GetChildren())
      {
         //if (child.Name != "Special")
        // {
            abilityButtonContainer.RemoveChild(child);
            child.QueueFree();
         //}
      }

      // Special button is present
      /*if (abilityButtonContainer.HasNode("Special"))
      {
         Control specialButton = abilityButtonContainer.GetNode<Control>("Special");
         abilityButtonContainer.RemoveChild(specialButton);
         specialButton.QueueFree();
      }*/
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
      ClearAbilityUI();
      GenerateAbilityUI();
      /*Member member = combatManager.GetCurrentMember();

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
         manaToUse = combatManager.CurrentFighter.currentMana;
      }

      int offset = abilityButtonContainer.HasNode("Special") ? 1 : 0;

      for (int i = 0; i < abilitiesToUse.Count; i++)
      {
         if (abilityButtonContainer.GetChild<Button>(i + offset).Name == "Special")
         {
            continue;
         }

         if (abilitiesToUse[i].manaCost <= manaToUse)
         {
            if (abilityButtonContainer.GetChild<Button>(i + offset).Disabled)
            {
               abilityButtonContainer.GetChild<Button>(i + offset).ButtonDown += managers.ButtonSoundManager.OnClick;
               abilityButtonContainer.GetChild<Button>(i + offset).MouseEntered += managers.ButtonSoundManager.OnHoverOver;

               abilityButtonContainer.GetChild<Button>(i + offset).MouseEntered -= () => HoverOverInformation("[color=red]Not enough mana. [/color]Costs " 
                                                                                                              + abilitiesToUse[i].manaCost + " mana. " 
                                                                                                              + abilitiesToUse[i].description);
               abilityButtonContainer.GetChild<Button>(i + offset).MouseEntered += () => HoverOverInformation("Costs " + abilitiesToUse[i].manaCost + " mana. " 
                                                                                                              + abilitiesToUse[i].description); 
            }

            abilityButtonContainer.GetChild<Button>(i + offset).Disabled = false;
         }
         else
         {
            if (!abilityButtonContainer.GetChild<Button>(i + offset).Disabled)
            {
               abilityButtonContainer.GetChild<Button>(i + offset).ButtonDown -= managers.ButtonSoundManager.OnClick;
               abilityButtonContainer.GetChild<Button>(i + offset).MouseEntered -= managers.ButtonSoundManager.OnHoverOver;
             
               abilityButtonContainer.GetChild<Button>(i + offset).MouseEntered -= () => HoverOverInformation("Costs " + abilitiesToUse[i].manaCost + " mana. " 
                                                                                                              + abilitiesToUse[i].description);
               abilityButtonContainer.GetChild<Button>(i + offset).MouseEntered += () => HoverOverInformation("[color=red]Not enough mana. [/color]Costs " 
                                                                                                              + abilitiesToUse[i].manaCost + " mana. " 
                                                                                                              + abilitiesToUse[i].description);
            }

            abilityButtonContainer.GetChild<Button>(i + offset).Disabled = true;
         }
      }

      if (combatManager.CurrentFighter.level >= 3)
      {
         if (member.specialAbility.manaCost <= combatManager.CurrentFighter.currentMana && combatManager.CurrentFighter.specialCooldown <= 0)
         {
            if (abilityButtonContainer.GetNode<Button>("Special").Disabled)
            {
               abilityButtonContainer.GetNode<Button>("Special").ButtonDown += managers.ButtonSoundManager.OnClick;
               abilityButtonContainer.GetNode<Button>("Special").MouseEntered += managers.ButtonSoundManager.OnHoverOver;
            }

            abilityButtonContainer.GetNode<Button>("Special").Disabled = false;
         }
         else
         {
            if (!abilityButtonContainer.GetNode<Button>("Special").Disabled)
            {
               Button specialButton = abilityButtonContainer.GetNode<Button>("Special");
               specialButton.ButtonDown -= managers.ButtonSoundManager.OnClick;
               specialButton.MouseEntered -= managers.ButtonSoundManager.OnHoverOver;

               specialButton.MouseEntered -= () => HoverOverInformation("[color=#bf87ff](SPECIAL)[/color]  Costs " 
                                                                                                            + member.specialAbility.manaCost + " mana. " 
                                                                                                            + member.specialAbility.description);
               
               if (member.specialAbility.manaCost > combatManager.CurrentFighter.currentMana)
               {
                  specialButton.MouseEntered += () => HoverOverInformation("[color=#bf87ff](SPECIAL)[/color]" + 
                                                                                                               " [color=red]Not enough mana.[/color] "
                                                                                                               + "Costs " 
                                                                                                               + member.specialAbility.manaCost + " mana. " 
                                                                                                               + member.specialAbility.description);
               }
               else
               {
                  specialButton.MouseEntered += () => HoverOverInformation("[color=#bf87ff](SPECIAL)[/color] [color=red]On cooldown for " 
                                                                                                               + combatManager.CurrentFighter.specialCooldown + " more turns.[/color] "
                                                                                                               + "Costs " 
                                                                                                               + member.specialAbility.manaCost + " mana. " 
                                                                                                               + member.specialAbility.description);
               }
            }

            abilityButtonContainer.GetNode<Button>("Special").Disabled = true;
         }
      }*/
   }

   public void GenerateItemUI()
   {
      PackedScene packedScene = GD.Load<PackedScene>(itemPrefab.ResourcePath);

      for (int i = 0; i < managers.PartyManager.Items.Count; i++)
      {
         if (managers.PartyManager.Items[i].item.itemCategory == ItemCategory.None && managers.PartyManager.Items[i].item.itemType == ItemType.Consumable)
         {
            // This is much like GenerateAbilities (creates buttons that have scripts inside them), but with items instead
            InventoryItem thisItem = managers.PartyManager.Items[i];
            Button currentButton = packedScene.Instantiate<Button>();

            currentButton.GetNode<ItemResourceHolder>("ResourceHolder").itemResource = thisItem;

            Node2D scriptHolder = currentButton.GetNode<Node2D>("ScriptHolder");
            scriptHolder.SetScript(GD.Load<CSharpScript>(thisItem.item.scriptPath));

            currentButton.Text = "   " + thisItem.item.name + " (" + thisItem.quantity + "x)";
            currentButton.Name = thisItem.item.name;

            currentButton.ButtonDown += managers.ButtonSoundManager.OnClick;
            currentButton.MouseEntered += managers.ButtonSoundManager.OnHoverOver;
            currentButton.MouseExited += StopHoveringOverInformation;
            currentButton.MouseEntered += () => HoverOverInformation(thisItem.item.description);

            itemsContainer.AddChild(currentButton);
         }
      }
   }
   
   public void ClearItemUI()
   {
      foreach (Control child in itemsContainer.GetChildren())
      {
         itemsContainer.RemoveChild(child);
         child.QueueFree();
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
               Control button = itemsContainer.GetNode<Control>(changedItem.item.name);
               itemsContainer.RemoveChild(button);
               button.QueueFree();
            }
            else
            {
               itemsContainer.GetNode<Button>(changedItem.item.name).Text = changedItem.item.name + " (" + changedItem.quantity + "x)";
            }
         }
      }
   }

   public void AlterEffectUI(Fighter target, string effectName, bool isStack, int quantity, bool displayTurns, string description)
   {
      target.UIPanel.GetNode<TargetInfoHolder>("InfoHolder").AddToInformation(effectName, isStack, quantity, displayTurns, description);
   }

   public void DecrementEffectUI(Fighter target, string effectName)
   {
      target.UIPanel.GetNode<TargetInfoHolder>("InfoHolder").DecrementEffect(effectName);
   }

   public void ChangeEffectUIQuantity(Fighter target, string effectName, int quantityToLose)
   {
      target.UIPanel.GetNode<TargetInfoHolder>("InfoHolder").ChangeEffectQuantity(effectName, quantityToLose);
   }

   public void ChangeEffectUIWithDeath(Fighter target)
   {
      TargetInfoHolder infoHolder = target.UIPanel.GetNode<TargetInfoHolder>("InfoHolder");
      infoHolder.ClearInformation();
      infoHolder.SetDeathStatus(true);
   }

   public void HideAll() 
   {
      options.Visible = false;
      cancelButton.Visible = false;
      abilityContainer.Visible = false;
      secondaryOptions.Visible = false;
   }

   public void ShowSelectionBox()
   {
      options.Visible = true;
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
      UI.GetNode<Control>("Overlay/DefeatScreen").Visible = visible;
   }

   public void ExitVictoryScreen() 
   {
      victoryScreen.Visible = false;
   }

   public void ClearVictoryNotifications()
   {
      VBoxContainer container = victoryScreen.GetNode<VBoxContainer>("Back/NotificationContainer");

      foreach (Node child in container.GetChildren())
      {
         container.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void UpdateVictoryExp(Member partyMember, int expGain, int oldLevel, int oldAbilityCount) 
   {
      RichTextLabel notification = victoryNotifPrefab.Instantiate<RichTextLabel>();
      notification.Text = "[b][i]" + partyMember.characterName + "[/i][/b] gains [b]" + expGain + "[/b] EXP";

      if (oldLevel != partyMember.level)
      {
         notification.Text += ". Level up to level " + partyMember.level + "!";
      }

      if (oldAbilityCount != partyMember.abilities.Count)
      {
         notification.Text += " Learns " + partyMember.abilities[oldAbilityCount].name + "!";
      }

      victoryScreen.GetNode<VBoxContainer>("Back/NotificationContainer").AddChild(notification);
   }

   public void CreateGoldGainLabel(int goldGain)
   {
      RichTextLabel notification = victoryNotifPrefab.Instantiate<RichTextLabel>();
      notification.Text = "Gained [b]" + goldGain + "[/b] Gold.";
      victoryScreen.GetNode<VBoxContainer>("Back/NotificationContainer").AddChild(notification);
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
            fighter.UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = "[right]" + fighter.currentMana + "/" + fighter.maxMana;
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
      fighter.UIPanel.GetNode<RichTextLabel>("HealthLabel").Text = fighter.currentHealth + "/" + fighter.maxHealth;
      fighter.UIPanel.GetNode<TextureProgressBar>("HealthBar").Value = (fighter.currentHealth * 1f / fighter.maxHealth) * 100f;

      if (!fighter.isEnemy)
      {
         fighter.UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = "[right]" + fighter.currentMana + "/" + fighter.maxMana;
         fighter.UIPanel.GetNode<TextureProgressBar>("ManaBar").Value = (fighter.currentMana * 1f / fighter.maxMana) * 100f;
         fighter.UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value = 0f;

         if (fighter.companion != null)
         {
            fighter.UIPanel.GetNode<Label>("CompanionHolder/ManaDescription").Text = fighter.companion.currentMana + "/" + fighter.companion.maxMana;
            fighter.UIPanel.GetNode<TextureProgressBar>("CompanionHolder/ManaBar").Value = (fighter.companion.currentMana * 1f / fighter.companion.maxMana) * 100f;
         }
      }

      uiIsUpdated = true;
   }

   public void UpdatePartyMemberManaBar(Fighter fighter)
   {
      fighter.UIPanel.GetNode<TextureProgressBar>("ManaBar").Value = (fighter.currentMana * 1f / fighter.maxMana) * 100f;
      fighter.UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value = 0f;
      fighter.UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = "[right]" + fighter.currentMana + "/" + fighter.maxMana;
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
      SetCancelButtonVisible(true);
   }

   public void ItemButton()
   {
      StopHoveringOverInformation();
      itemsList.Visible = true;
      SetChoicesVisible(false);
      SetCancelButtonVisible(true);
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
               button.ButtonDown += () => combatManager.OnFighterPanelDown(combatManager.Fighters[j]);
               break;
            }
         }

         button.MouseEntered += managers.ButtonSoundManager.OnHoverOver;
         button.ButtonDown += managers.ButtonSoundManager.OnClick;
      }

      options.GetNode<ScrollContainer>("Targets").Visible = true;
      options.GetNode<ScrollContainer>("Targets").ScrollVertical = 0;

      if (combatManager.CurrentAbility != null)
      {
         SetManaBarLossIndicator(combatManager.CurrentAbility.manaCost);
      }
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
         if (combatManager.CurrentItem.item.onlyHitsSelf || combatManager.CurrentItem.item.hitsSelf)
         {
            possibleTargets.Add(combatManager.CurrentFighter);
         }
         
         if (combatManager.CurrentItem.item.onlyHitsTeam || combatManager.CurrentItem.item.hitsTeam)
         {
            possibleTargets.AddRange(GetTeam(false));
         }

         if (combatManager.CurrentItem.item.onlyHitsSelf || combatManager.CurrentItem.item.onlyHitsTeam)
         {
            return possibleTargets;
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
            if (combatManager.Fighters[i].isEnemy)
            {
               possibleTargets.Add(combatManager.Fighters[i]);
            }
         } 
      }

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (!possibleTargets.Contains(combatManager.Fighters[i]) && combatManager.Fighters[i] != combatManager.CurrentFighter && !combatManager.Fighters[i].isDead)
         {
            if (!combatManager.Fighters[i].isEnemy)
            {
               possibleTargets.Add(combatManager.Fighters[i]);
            }
         } 
      }

      return possibleTargets;
   }

   List<Fighter> GetEnemies()
   {
      List<Fighter> enemies = new List<Fighter>();

      for (int i = 0; i < combatManager.Fighters.Count; i++)
      {
         if (combatManager.Fighters[i].isEnemy && !combatManager.Fighters[i].isDead)
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
         else
         {
            ResetManaBarLossIndicator();
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

   public void CreateTurnOrderPortrait(Fighter fighter, int id, int index = -1)
   {
      Control portrait = GD.Load<PackedScene>("res://Combat/UI/turn_order_portrait.tscn").Instantiate<Control>();
      portrait.GetNode<Sprite2D>("Sprite").Texture = GD.Load<Texture2D>(fighter.turnOrderSpritePath);
      portrait.GetNode<RichTextLabel>("Text").Text = "[center]" + fighter.fighterName;
      portrait.GetNode<FighterID>("FighterID").id = id;

      turnOrderContainer.AddChild(portrait);

      if (index != -1)
      {
         turnOrderContainer.MoveChild(portrait, index);
      }
   }

   public void HighlightTurnOrderPortrait()
   {
      if (turnOrderContainer.GetChildCount() > 0)
      {
         turnOrderContainer.GetChild<Control>(0).GetNode<Panel>("Highlight").Visible = true;
      }
   }

   public void DeleteTurnOrderPortrait()
   {
      if (turnOrderContainer.GetChildCount() > 0)
      {
         Control child = turnOrderContainer.GetChild<Control>(0);
         turnOrderContainer.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void ClearAllTurnOrderPortraits()
   {
      foreach (Control child in turnOrderContainer.GetChildren())
      {
         turnOrderContainer.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void ClearTurnOrderPortraitsOfFighter(int fighterID)
   {
      foreach (Control child in turnOrderContainer.GetChildren())
      {
         if (child.GetNode<FighterID>("FighterID").id == fighterID)
         {
            turnOrderContainer.RemoveChild(child);
            child.QueueFree();
         }
      }
   }

   public bool IsEquivalentToMessageText(string toCompare)
   {
      return messageText.Text == toCompare;
   }

   void SetManaBarLossIndicator(int lossAmount)
   {
      Control UIPanel = combatManager.CurrentFighter.UIPanel;

      if (UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value == 0)
      {
         UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value = UIPanel.GetNode<TextureProgressBar>("ManaBar").Value;
         UIPanel.GetNode<TextureProgressBar>("ManaBar").Value = ((combatManager.CurrentFighter.currentMana - lossAmount) 
                                                                  * 1f / combatManager.CurrentFighter.maxMana) * 100f;
         UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = "[right][color=red]" + (combatManager.CurrentFighter.currentMana - lossAmount) + "[/color]/" +
                                                            combatManager.CurrentFighter.maxMana;
      }
   }

   public void ResetManaBarLossIndicator()
   {
      Control UIPanel = combatManager.CurrentFighter.UIPanel;

      if (UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value > 0)
      {
         UIPanel.GetNode<TextureProgressBar>("ManaBar").Value = UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value;
         UIPanel.GetNode<TextureProgressBar>("ManaBar/LossBar").Value = 0;
         UIPanel.GetNode<RichTextLabel>("ManaLabel").Text = "[right]" + combatManager.CurrentFighter.currentMana + "/" + combatManager.CurrentFighter.maxMana;
      }
   }

   public async void MoveHealthBar(Fighter fighter)
   {
      Control UIPanel = fighter.UIPanel;
      TextureProgressBar healthBar = UIPanel.GetNode<TextureProgressBar>("HealthBar");
      TextureProgressBar lossBar = healthBar.GetNode<TextureProgressBar>("LossBar");

      lossBar.Value = healthBar.Value;
      healthBar.Value = (fighter.currentHealth * 1f / fighter.maxHealth) * 100f;


      await ToSignal(GetTree().CreateTimer(0.75f), "timeout");

      while (lossBar.Value > healthBar.Value)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         lossBar.Value--;
      }

      lossBar.Value = 0;
   }

   /// <summary>
   /// This creates the damage indicator texts, but doesn't show or move them; they're simply stored until they're ready to be shown with MoveDamageTexts.
   /// </summary>
   public void ProjectDamageText(Fighter target, int damage, DamageType damageType, bool isCrit, bool isHeal = false, bool isMana = false, bool isStatus = false)
   {
      Label3D damageText = CreateDamageText(target);
      damageText.Text = damage.ToString();

      if (isCrit)
      {
         damageText.Modulate = new Color(1, 0, 0);
         damageText.OutlineModulate = new Color(1, 0, 0);
      }

      if (isHeal)
      {
         damageText.Modulate = new Color(0, 1f, 0.017f);
      }
      else if (isMana)
      {
         damageText.Modulate = new Color(0.192f, 0.192f, 0.996f);
      }
      else
      {
         damageText.Modulate = damageTypeColors[(int)damageType];
      }
   }

   public void ProjectDamageText(Fighter target, int damage, Color color)
   {
      Label3D damageText = CreateDamageText(target);
      damageText.Text = damage.ToString();
      damageText.Modulate = color;
   }

   public void ProjectMissText(Fighter target)
   {
      Label3D damageText = CreateDamageText(target);
      damageText.Text = "MISS";
   }

   Label3D CreateDamageText(Fighter target)
   {
      Label3D damageText = GD.Load<PackedScene>("res://Combat/UI/damage_text.tscn").Instantiate<Label3D>();
      target.UIPanel.AddChild(damageText);
      damageText.Name = "DamageIndicator";

      damageText.GlobalPosition = target.model.GlobalPosition + (Vector3.Up * 0.5f);
      damageText.GlobalPosition = new Vector3((float)GD.RandRange(damageText.GlobalPosition.X - 0.25f, damageText.GlobalPosition.X + 0.25f), 
                                              (float)GD.RandRange(damageText.GlobalPosition.Y - 0.5f, damageText.GlobalPosition.Y + 0.5f),
                                              (float)GD.RandRange(damageText.GlobalPosition.Z - 0.25f, damageText.GlobalPosition.Z + 0.25f));
      return damageText;
   }

   public void MoveDamageTexts(Fighter target)
   {
      foreach (Node child in target.UIPanel.GetChildren())
      {
         if (child.Name == "DamageIndicator")
         {
            ShiftDamageTextUpward((Label3D)child, target);
         }
      }
   }

   async void ShiftDamageTextUpward(Label3D text, Fighter owner)
   {
      Vector3 target = text.GlobalPosition + (Vector3.Up * 0.75f);
      text.Visible = true;

      while (text.GlobalPosition.Y < target.Y)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         text.SetGlobalPosition(text.GlobalPosition + (Vector3.Up * 0.025f));
      }

      if (combatManager.IsInCombat)
      {
         owner.UIPanel.RemoveChild(text);
      }

      text.QueueFree();
   }

   public void EndOfCombatHiding()
   {
      HideAll();
      partyList.Visible = false;
      enemyList.Visible = false;
      partyPanels.Clear();
      enemyPanels.Clear();
      SetMessageTextVisible(false);
      turnOrderContainer.GetParent().GetParent<Control>().Visible = false;
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

   public void SetMessageTextVisible(bool visible)
   {
      messageText.GetParent<Control>().Visible = visible;
   }

   public void SetCastButtonVisible(bool visible) 
   {
      options.GetNode<VBoxContainer>("Choices").GetNode<Button>("CastButton").Disabled = visible;
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
