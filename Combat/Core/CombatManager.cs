using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public enum Affinity
{
   None,
   Solarborn,
   Shadowsworn,
   Shrouded,
   Arboreal,
   Sanguine,
   Corthas,
   Magmus,
   Boltstruck,
   Pestilent
}

public enum DamageType
{
   None,
   Physical,
   Magical,
   Fire,
   Earth,
   Water,
   Air,
   Light,
   Dark,
   Venom,
   Spirit
}

public enum StatusEffect
{
   None,
   Stun,
   Poison,
   Bleed,
   Burn,
   Confuse,
   Disease,
   MegaBuff,
   Birdseye,
   Stealth
}

/*
A brief explanation of the ability system, for posterity:
Every ability has an AbilityResource and a corresponding ability script.
When opening the casting screen, a button is generated for each ability, which contains a ScriptHolder that has the ability's script
and a ResourceHolder that holds the corresponding AbilityResource.
In that script, there is a signal to detect when the ability button is pressed.
Then, in THIS script, there is another signal that emits when a fighter is clicked while an ability is currently being casted.
This signal is received by the individual ability script, allowing it to behave and then complete the turn.
*/

public partial class CombatManager : Node
{
   [Export]
   private PackedScene abilityButton;
   [Export]
   private PackedScene effectPrefab;
   [Export]
   private PackedScene enemyAbilityUseHolder;
   [Export]
   private PackedScene itemPrefab;
   [Export]
   private PackedScene stackPrefab;
   [Export]
   private AffinityStrengths[] affinityTable = new AffinityStrengths[9];
   [Export]
   private StatusData[] statusDatas = new StatusData[1];

   private PackedScene olrenSpecialArrowPrefab;

   private Camera3D playerCamera;
   private Camera3D arenaCamera;

   private Node2D UI;
   private Node3D arena;
   private Node3D baseNode;

   private CanvasGroup partyList;
   private CanvasGroup enemyList;
   private CanvasGroup selectionBox;
   private Panel abilityContainer;
   private VBoxContainer abilityButtonContainer;
   private Panel secondaryOptions;
   private VBoxContainer secondaryOptionsContainer;
   private Label messageText;
   private Button cancelButton;
   private Button finalizeButton;
   private Panel itemsList;
   private VBoxContainer itemsContainer;
   private CanvasGroup victoryScreen;

   private PartyManager partyManager;
   private CharacterController controller;
   private LevelManager levelManager;
   private SaveMenuManager saveMenuManager;
   private MenuManager menuManager;
   private ItemPickupManager itemPickupManager;

   private List<Node2D> partyPanels = new List<Node2D>();
   private List<Node2D> enemyPanels = new List<Node2D>();

   public List<Fighter> Fighters { get; set; }

   private bool isAttacking;
   private bool isCasting;
   public bool IsCompanionTurn { get; set; }
   public Fighter CurrentFighter { get; set; }
   public Fighter CurrentTarget { get; set; }
   public InventoryItem CurrentItem { get; set; }
   public AbilityResource CurrentAbility { get; set; }

   public string AbilityTargetGraphic { get; set; }

   private StyleBoxFlat baseHighlight;
   private StyleBoxFlat alteredHighlight;

   private double deathWaitTime = 0;
   private bool cameraPanCompleted = false;

   [Signal]
   public delegate void AbilityCastEventHandler();
   [Signal]
   public delegate void EnemyAbilityCastEventHandler();
   [Signal]
   public delegate void ItemUseEventHandler();
   [Signal]
   public delegate void AttackAnimationEventHandler();
   [Signal]
   public delegate void AddBadgeEventHandler();

   public bool IsInCombat { get; set; }

   private Vector3 returnPosition;
   private Vector3 returnRotation;

   private List<OlrenSpecialArrow> olrenSpecialArrows = new List<OlrenSpecialArrow>() {
      new OlrenSpecialArrow("None", 1, "No effect"),
      new OlrenSpecialArrow("Poison", 1, "Chance of applying poison for 2 to 4 turns")
   };
   private string currentOlrenSpecialArrow;

   private WorldEnemy currentEnemyScript;

   //private Node3D cameraHolder;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      Fighters = new List<Fighter>();
      AbilityTargetGraphic = string.Empty;

      baseNode = GetNode<Node3D>("/root/BaseNode");
      playerCamera = baseNode.GetNode<Camera3D>("PartyMembers/Member1/CameraTarget/PlayerCamera");

      UI = baseNode.GetNode<Node2D>("UI");
      partyList = UI.GetNode<CanvasGroup>("CombatPartyList");
      enemyList = UI.GetNode<CanvasGroup>("CombatEnemyList");
      selectionBox = UI.GetNode<CanvasGroup>("Options");
      messageText = UI.GetNode<Label>("Message/Text");
      victoryScreen = UI.GetNode<CanvasGroup>("VictoryScreen");
      abilityContainer = selectionBox.GetNode<Panel>("Abilities");
      abilityButtonContainer = abilityContainer.GetNode<ScrollContainer>("ScrollContainer").GetNode<VBoxContainer>("VBoxContainer");
      itemsList = selectionBox.GetNode<Panel>("Items");
      itemsContainer = itemsList.GetNode<ScrollContainer>("ScrollContainer").GetNode<VBoxContainer>("VBoxContainer");
      cancelButton = selectionBox.GetNode<Button>("CancelButton");
      finalizeButton = selectionBox.GetNode<Button>("FinalizeButton");
      secondaryOptions = selectionBox.GetNode<Panel>("SecondaryOptions");
      secondaryOptionsContainer = secondaryOptions.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
      //specialButton = abilityContainer.GetNode<Button>("Special");

	   partyManager = GetNode<PartyManager>("/root/BaseNode/PartyManagerObj");
      controller = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
      levelManager = baseNode.GetNode<LevelManager>("LevelManager");
      saveMenuManager = baseNode.GetNode<SaveMenuManager>("SaveManager");
      menuManager = UI.GetNode<MenuManager>("PartyMenuLayer/PartyMenu/MenuManager");
      itemPickupManager = UI.GetNode<ItemPickupManager>("ItemPickup/Back");

      baseHighlight = GD.Load<StyleBoxFlat>("res://Combat/Themes/base_party_highlight.tres");
      alteredHighlight = GD.Load<StyleBoxFlat>("res://Combat/Themes/altered_party_highlight.tres");

      for (int i = 0; i < 4; i++)
      {
         partyPanels.Add(UI.GetNode<Node2D>("CombatPartyList/HBoxContainer/Panel" + (i + 1) + "/Holder"));
      }

      for (int i = 0; i < 4; i++)
      {
         enemyPanels.Add(UI.GetNode<Node2D>("CombatEnemyList/HBoxContainer/Panel" + (i + 1) + "/Holder"));
      }

      olrenSpecialArrowPrefab = GD.Load<PackedScene>("res://Combat/Abilities/UI/special_arrow.tscn");
	}

   public void ResetNodes()
   {
      arena = baseNode.GetNode<Node3D>("Level/Arena");
      arenaCamera = arena.GetNode<Camera3D>("ArenaCamera");
   }

   public async void SetupCombat(List<Enemy> enemyDatas, Vector3 location, Vector3 rotation, WorldEnemy enemyScript) 
   {
      ResetNodes();

      menuManager.CloseMenu();
      menuManager.canTakeInput = false;
      itemPickupManager.itemPickupContainer.Visible = false;

      returnPosition = location;
      returnRotation = rotation;
      Fighters.Clear();
      IsInCombat = true;
      controller.DisableMovement = true;
      controller.IsSprinting = false;
      currentEnemyScript = enemyScript;

      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].isInParty)
         {
            partyManager.Party[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Encounter");
         }
      }

      await ToSignal(GetTree().CreateTimer(0.75f), "timeout");
      
      saveMenuManager.FadeToBlack();

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

      InitializeParty();
      InitializeEnemies(enemyDatas);
      ClearItems();
      GenerateItems();
      EnableOptions();
      DisablePanels();

      Input.MouseMode = Input.MouseModeEnum.Visible;
      MovePartyToArena();
      UpdateUI();
      playerCamera.Current = false;
      arenaCamera.MakeCurrent();
      //ResetCamera();

      enemyScript.QueueFree();

      PointCameraAtEnemies();
      saveMenuManager.FadeFromBlack();

      if (!enemyScript.isStaticEnemy)
      {
         CreateEnemySpawnEffects();
         await ToSignal(GetTree().CreateTimer(2.75f), "timeout");
      }
      else
      {
         await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
      }
      
      PanCameraOutward();

      while (!cameraPanCompleted)
      {
         await ToSignal(GetTree().CreateTimer(1f), "timeout");
      }

      await ToSignal(GetTree().CreateTimer(1f), "timeout");

      ResetCamera();

	   partyList.Visible = true;
      enemyList.Visible = true;
      SelectNextTurn();
   }

   async void CreateEnemySpawnEffects()
   {
      List<Fighter> enemies = new List<Fighter>();

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].isEnemy)
         {
            enemies.Add(Fighters[i]);
            Fighters[i].placementNode.GetNode<GpuParticles3D>("SpawnParticles").Visible = true;
         }
      }

      await ToSignal(GetTree().CreateTimer(1f), "timeout");

      for (int i = 0; i < enemies.Count; i++)
      {
         enemies[i].placementNode.GetNode<GpuParticles3D>("SpawnParticles").Emitting = false;
         enemies[i].placementNode.GetNode<MeshInstance3D>("SpawnMesh").Visible = true;
      }

      await ToSignal(GetTree().CreateTimer(1.25f), "timeout");

      for (int i = 0; i < enemies.Count; i++)
      {
         enemies[i].placementNode.GetNode<GpuParticles3D>("SpawnParticles").Visible = false;
         enemies[i].placementNode.GetNode<MeshInstance3D>("SpawnMesh").Visible = false;
         enemies[i].model.Visible = true;

         // Reset for next battle
         enemies[i].placementNode.GetNode<GpuParticles3D>("SpawnParticles").Emitting = true;
      }

      //Node3D placementGroup = arena.GetNode<Node3D>("EnemyGroup" + enemies.Count);


   }

   async void PanCameraOutward()
   {
      cameraPanCompleted = false;
      int timer = 0;

      while (timer < 70)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         arenaCamera.GlobalPosition += (Vector3.Left * 0.25f);
         timer++;
      }

      cameraPanCompleted = true;
   }

   void InitializeParty()
   {
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].isInParty)
         {
            Fighter newFighter = new Fighter();
            newFighter.fighterName = partyManager.Party[i].characterName;
            newFighter.level = partyManager.Party[i].level;
            newFighter.isEnemy = false;
            newFighter.affinity = partyManager.Party[i].affinity;

            newFighter.currentHealth = partyManager.Party[i].currentHealth;
            newFighter.currentMana = partyManager.Party[i].currentMana;

            newFighter.maxHealth = partyManager.Party[i].GetMaxHealth();
            newFighter.maxMana = partyManager.Party[i].GetMaxMana();

            for (int j = 0; j < 10; j++) 
            {
               newFighter.stats[j] = new Stat();
               newFighter.stats[j].statType = partyManager.Party[i].stats[j].statType;
               newFighter.stats[j].value = partyManager.Party[i].stats[j].value;
               newFighter.stats[j].baseValue = partyManager.Party[i].stats[j].value;
            }

            for (int j = 0; j < partyManager.Party[i].abilities.Count; j++)
            {
               newFighter.abilities.Add(partyManager.Party[i].abilities[j]);
            }
         
            newFighter.UIPanel = partyPanels[i];
            newFighter.UIPanel.Visible = true;
            newFighter.UIPanel.GetNode<Label>("Title").Text = newFighter.fighterName;

            newFighter.UIPanel.GetNode<Label>("HealthDescription").Text = newFighter.currentHealth + "/" + newFighter.maxHealth;
            newFighter.UIPanel.GetNode<Label>("ManaDescription").Text = newFighter.currentMana + "/" + newFighter.maxMana;

            newFighter.UIPanel.GetNode<ProgressBar>("HealthBar").Value = (newFighter.currentHealth * 1f / newFighter.maxHealth) * 100f;
            newFighter.UIPanel.GetNode<ProgressBar>("ManaBar").Value = (newFighter.currentMana * 1f / newFighter.maxMana) * 100f;

            newFighter.model = partyManager.Party[i].model;
            newFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");

            BoneAttachment3D attachment = newFighter.model.GetNode<BoneAttachment3D>("Model/Armature/Skeleton3D/WeaponAttachment");
            attachment.BoneName = "hand.L";

            SetWeaponAttachmentToIdle(newFighter);

            if (newFighter.model.Name != "Member1")
            {
               newFighter.model.GetParent().GetNode<OverworldPartyController>("" + newFighter.model.Name).EnablePathfinding = false;
            }

            ApplyPassives(newFighter);

            Fighters.Add(newFighter);
         }
      }
   }

   void SetWeaponAttachmentToIdle(Fighter fighter)
   {
      BoneAttachment3D attachment = fighter.model.GetNode<BoneAttachment3D>("Model/Armature/Skeleton3D/WeaponAttachment");
      attachment.BoneName = "hand.L";

      Node3D idleCombatAnchor = fighter.model.GetNode<Node3D>("IdleCombatAnchor");
      attachment.GetChild<Node3D>(0).Position = idleCombatAnchor.Position;
      attachment.GetChild<Node3D>(0).Rotation = idleCombatAnchor.Rotation;
   }

   void InitializeEnemies(List<Enemy> enemyDatas)
   {
      // The reason this ends at 4 is a backup in case the randomizer tried to force more than 4 enemies into battle
      int enemyCount = enemyDatas.Count <= 4 ? enemyDatas.Count : 4;

      Node3D placementGroup = arena.GetNode<Node3D>("EnemyGroup" + enemyCount);
      for (int i = 0; i < enemyCount; i++)
      {
         Fighter newFighter = new Fighter();
         newFighter.fighterName = enemyDatas[i].enemyName;
         newFighter.level = enemyDatas[i].level;
         newFighter.isEnemy = true;
         newFighter.affinity = enemyDatas[i].affinity;

         for (int j = 0; j < 10; j++)
         {
            newFighter.stats[j] = new Stat();
            newFighter.stats[j].statType = enemyDatas[i].stats[j].statType;
            newFighter.stats[j].value = enemyDatas[i].stats[j].value;
            newFighter.stats[j].baseValue = enemyDatas[i].stats[j].value;
         }

         newFighter.maxHealth = newFighter.GetMaxHealth();
         newFighter.currentHealth = newFighter.maxHealth;

         newFighter.maxMana = newFighter.GetMaxMana();
         newFighter.currentMana = newFighter.maxMana;

         for (int j = 0; j < enemyDatas[i].abilities.Length; j++)
         {
            newFighter.abilities.Add(enemyDatas[i].abilities[j]);
         }

         PackedScene packedScene = GD.Load<PackedScene>(enemyDatas[i].model.ResourcePath);
         newFighter.model = packedScene.Instantiate<Node3D>();
         newFighter.model.Position = placementGroup.GetNode<Node3D>("EnemyPlacement" + (i + 1)).GlobalPosition;

         if (!currentEnemyScript.isStaticEnemy)
         {
            newFighter.model.Visible = false;
         }
         
         newFighter.placementNode = placementGroup.GetNode<Node3D>("EnemyPlacement" + (i + 1));
         baseNode.AddChild(newFighter.model);

         newFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
         newFighter.model.GetNode<Node3D>("Model").Rotation = new Vector3(-0, Mathf.DegToRad(-90f), 0);

         newFighter.UIPanel = enemyPanels[i];
         newFighter.UIPanel.Visible = true;

         newFighter.UIPanel.GetNode<Label>("Title").Text = newFighter.fighterName;
         newFighter.UIPanel.GetNode<Label>("HealthDescription").Text = newFighter.currentHealth + "/" + newFighter.maxHealth;

		   Fighters.Add(newFighter);
      }
   }

   void MovePartyToArena()
   {
      List<Member> inParty = new List<Member>();

      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].isInParty)
         {
            inParty.Add(partyManager.Party[i]);
         }
      }
   
      Node3D placementGroup = arena.GetNode<Node3D>("PartyGroup" + inParty.Count);
      for (int i = 0; i < inParty.Count; i++)
      {
         inParty[i].model.GlobalPosition = placementGroup.GetNode<Node3D>("PartyPlacement" + (i + 1)).GlobalPosition;
         Fighters[i].placementNode = placementGroup.GetNode<Node3D>("PartyPlacement" + (i + 1));
         inParty[i].model.GetNode<Node3D>("Model").LookAt(arena.GlobalPosition);
         inParty[i].model.GetNode<Node3D>("Model").RotateY(Mathf.DegToRad(-180));
         //partyManager.party[i].model.GetNode<Node3D>("Model").Rotation = new Vector3(-0, Mathf.DegToRad(90f), 0);
         inParty[i].model.GetNode<Node3D>("Model").Position = Vector3.Zero;
      }       
   }

   // Serves to point the camera at its "default" place, which will happen in between moves
   private void ResetCamera() 
   {
      arenaCamera.Position = new Vector3(-25.82f, 7.33f, 14.64f);
      arenaCamera.Rotation = new Vector3(Mathf.DegToRad(-13f), Mathf.DegToRad(-60.6f), Mathf.DegToRad(0.4f));
   }

   public void PointCameraAtParty()
   {
      arenaCamera.GlobalPosition = arena.GlobalPosition + (Vector3.Up * 2f) + (Vector3.Right * 3f);
      arenaCamera.Rotation = new Vector3(-0, Mathf.DegToRad(90f), 0);
   }

   public void PointCameraAtEnemies()
   {
      arenaCamera.GlobalPosition = arena.GlobalPosition + (Vector3.Up * 2f) + (Vector3.Left * 3f);
      arenaCamera.Rotation = new Vector3(-0, Mathf.DegToRad(-90f), 0);
   }

   public void FocusCameraOnFighter(Node3D target)
   {
      Basis basis = target.GetNode<Node3D>("Model").GlobalTransform.Basis;
      // Places the camera behind the current player fighter. Note that basis.Z is forward, -basis.X is right
      arenaCamera.GlobalPosition = target.GlobalPosition - (basis.Z * 2f) + (Vector3.Up * 2f) - (basis.X * 1.25f);
      arenaCamera.LookAt(target.GlobalPosition + (basis.Z * 2f) + Vector3.Up - basis.X);
   }

   public void ReverseFocusOnTarget(Node3D target)
   {
      Basis basis = target.GetNode<Node3D>("Model").GlobalTransform.Basis;
      arenaCamera.GlobalPosition = target.GlobalPosition + (basis.Z * 6f) + (Vector3.Up * 3.5f) - (basis.X * 1.25f);
      arenaCamera.LookAt(target.Position);
   }

   async void SelectNextTurn() {
      if (CurrentFighter != null)
      {
         CurrentFighter.UIPanel.GetNode<Panel>("Highlight").Visible = false;

         if (CurrentFighter.companion != null && !CurrentFighter.companion.hadTurn)
         {
            CurrentFighter.companion.duration--;
            CurrentFighter.UIPanel.GetNode<Label>("CompanionHolder/Duration").Text = CurrentFighter.companion.duration + " turns";

            if (CurrentFighter.companion.duration <= 0)
            {
               RemoveCompanion(CurrentFighter);
            }
            else
            {
               IsCompanionTurn = true;
               CurrentFighter.companion.hadTurn = true;
               CurrentFighter.UIPanel.GetNode<Panel>("CompanionHolder/Highlight").Visible = true;
               ClearAbilities();
               GenerateAbilities();
               PlayerTurn();
               return;
            }  
         }
         else if (CurrentFighter.companion != null)
         {
            CurrentFighter.UIPanel.GetNode<Panel>("CompanionHolder/Highlight").Visible = false;
         }
      }

      IsCompanionTurn = false;
      
      while (true) 
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
         for (int i = 0; i < Fighters.Count; i++) 
         {
            if (!Fighters[i].isDead)
            {
               Fighters[i].actionLevel += CalculateCurrentSpeed(Fighters[i]);

               // Update action bar BEFORE increasing the action level so that it doesn't stop before reaching 100
               if (!Fighters[i].isEnemy)
               {
                  Fighters[i].UIPanel.GetNode<ProgressBar>("ActionBar").Value = Fighters[i].actionLevel;
               }

               if (Fighters[i].actionLevel >= 100) 
               {
                  await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

                  CurrentFighter = Fighters[i];
                  IncrementAppliedStatuses();
                  IncrementStatModifiers();
                  CurrentFighter.UIPanel.GetNode<Panel>("Highlight").Visible = true;

                  ClearAbilities();

                  if (Fighters[i].isEnemy)
                  {
                     EnemyTurn(Fighters[i]);
                  }
                  else
                  {
                     GenerateAbilities();
                     PlayerTurn();

                     if (Fighters[i].specialCooldown > 0)
                     {
                        Fighters[i].specialCooldown--;
                     }

                     if (Fighters[i].companion != null)
                     {
                        Fighters[i].companion.hadTurn = false;
                     }
                  }

                  Fighters[i].actionLevel = 0;
                  return;
               }
            }
         }
      }
   }

   int CalculateCurrentSpeed(Fighter fighter)
   {
      int baseSpeed = fighter.stats[8].value / 2;

      for (int i = 0; i < fighter.currentStatuses.Count; i++)
      {
         if (fighter.currentStatuses[i].effect == StatusEffect.Stun)
         {
            return 0;
         }
      }

      return baseSpeed;
   }

   void IncrementAppliedStatuses()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         for (int j = 0; j < Fighters[i].currentStatuses.Count; j++)
         {
            if (Fighters[i].currentStatuses[j].applier == CurrentFighter)
            {
               Fighters[i].currentStatuses[j].remainingTurns--;

               if (Fighters[i].currentStatuses[j].displayRemainingTurns)
               {
                  VBoxContainer statusRows = Fighters[i].UIPanel.GetNode<VBoxContainer>("Effects/StatusRows");
                  HBoxContainer row1 = statusRows.GetNode<HBoxContainer>("Row1");
                  HBoxContainer row2 = statusRows.GetNode<HBoxContainer>("Row2");

                  if (j < row1.GetChildCount())
                  {
                     row1.GetChild(j).GetChild(0).GetNode<Label>("Quantity").Text = "" + Fighters[i].currentStatuses[j].remainingTurns;
                  }
                  else
                  {
                     row2.GetChild(j - row1.GetChildCount()).GetChild(0).GetNode<Label>("Quantity").Text = "" + Fighters[i].currentStatuses[j].remainingTurns;
                  }
               }

               if (Fighters[i].currentStatuses[j].remainingTurns <= 0)
               {
                  //RemoveEffectUI(fighters[i].currentStatuses[j].effect, fighters[i]);
                  //fighters[i].currentStatuses.Remove(fighters[i].currentStatuses[j]);
                  RemoveStatus(Fighters[i], j);
               }
               else
               {
                  if (Fighters[i].currentStatuses[j].effect == StatusEffect.Poison)
                  {
                     Fighters[i].currentHealth -= (int)(Fighters[i].currentHealth * 0.05f);
                     UpdateSingularUIPanel(Fighters[i]);
                  }
                  else if (Fighters[i].currentStatuses[j].effect == StatusEffect.Bleed)
                  {
                     Fighters[i].currentHealth -= (int)(Fighters[i].maxHealth * 0.03f);
                     UpdateSingularUIPanel(Fighters[i]);
                  }
                  else if (Fighters[i].currentStatuses[j].effect == StatusEffect.Burn)
                  {
                     Fighters[i].currentHealth -= (int)(Fighters[i].maxHealth * 0.02f);
                     UpdateSingularUIPanel(Fighters[i]);
                  }
               }
            }
         }
      }
   }

   public void RemoveStatus(Fighter target, int statusIndex)
   {
      AppliedStatusEffect status = target.currentStatuses[statusIndex];
      RemoveEffectUI(status.effect, target);

      for (int i = 0; i < target.statModifiers.Count; i++)
      {
         if (target.statModifiers[i].attachedStatus == status.effect)
         {
            target.statModifiers.Remove(target.statModifiers[i]);
            i--;
         }
      }

      target.currentStatuses.Remove(target.currentStatuses[statusIndex]);
   }

   void RemoveEffectUI(StatusEffect effect, Fighter applied)
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

   void IncrementStatModifiers()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         for (int j = 0; j < Fighters[i].statModifiers.Count; j++)
         {
            if (Fighters[i].statModifiers[j].applier == CurrentFighter)
            {
               Fighters[i].statModifiers[j].duration--;

               if (Fighters[i].statModifiers[j].duration <= 0)
               {
                  Fighters[i].stats[(int)Fighters[i].statModifiers[j].statType].value -= Fighters[i].statModifiers[j].modifier;

                  if (Fighters[i].statModifiers[j].statType == StatType.Constitution)
                  {
                     Fighters[i].maxHealth = Fighters[i].GetMaxHealth();
                     UpdateSingularUIPanel(Fighters[i]);
                  }
                  else if (Fighters[i].statModifiers[j].statType == StatType.Knowledge)
                  {
                     Fighters[i].maxMana = Fighters[i].GetMaxMana();
                     UpdateSingularUIPanel(Fighters[i]);
                  }
                  
                  Fighters[i].statModifiers.Remove(Fighters[i].statModifiers[j]);
               }
            }
         }
      }
   }

   public async void CompleteTurn()
   {
      LockFighters();
      //ProcessValues();

      //UpdateUI();
      EnableOptions();

      selectionBox.Visible = false;
      cancelButton.Visible = false;
      abilityContainer.Visible = false;
      isAttacking = false;

      IsCompanionTurn = false;

      CurrentAbility = null;
      AbilityTargetGraphic = string.Empty;

      for (int i = 0; i < Fighters.Count; i++)
      {
         Fighters[i].wasHit = false;
      }

      if (!CurrentFighter.isEnemy)
      {
         SetWeaponAttachmentToIdle(CurrentFighter);
      }

      if (IsInCombat)
      {
         ResetCamera();
         await ToSignal(GetTree().CreateTimer(0.15f), "timeout");
         SelectNextTurn();
      }
   }

   void PlayerTurn()
   {
      selectionBox.Visible = true;

      if (IsCompanionTurn)
      {
         selectionBox.GetNode<Panel>("Panel").GetNode<Button>("ItemButton").Disabled = true;
         FocusCameraOnFighter(CurrentFighter.companion.model);
      }
      else
      {
         selectionBox.GetNode<Panel>("Panel").GetNode<Button>("ItemButton").Disabled = false;
         FocusCameraOnFighter(CurrentFighter.model);
      }

      CurrentFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatActive");

      if (CurrentFighter.model.HasNode("ActiveCombatAnchor"))
      {
         BoneAttachment3D weaponAttachment = CurrentFighter.model.GetNode<BoneAttachment3D>("Model/Armature/Skeleton3D/WeaponAttachment");
         Node3D activeCombatAnchor = CurrentFighter.model.GetNode<Node3D>("ActiveCombatAnchor");
         weaponAttachment.GetChild<Node3D>(0).Position = activeCombatAnchor.Position;
         weaponAttachment.GetChild<Node3D>(0).Rotation = activeCombatAnchor.Rotation;
      }
   }

   public void OnFighterPanelDown(int index, bool isEnemy)
   {
      Fighter target = GetFighterFromIndex(index, isEnemy);

      if (target.isDead)
      {
         if (CurrentAbility != null)
         {
            if (!CurrentAbility.bypassesDeaths)
            {
               return;
            }
         }
         else
         {
            return;
         }
      }

      if (isAttacking)
      {
         CompleteAttack(target);
         target.placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
      }
      else if (CurrentAbility != null)
      {
         if (CurrentAbility.hitsSelf && !CurrentAbility.hitsAll && !CurrentAbility.hitsSurrounding && !CurrentAbility.hitsTeam && target != CurrentFighter)
         {
            return;
         }

         for (int i = 0; i < Fighters.Count; i++)
         {
            Fighters[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }

         CurrentTarget = target;
         abilityContainer.Visible = false;
         cancelButton.Visible = false;
         selectionBox.Visible = false;
         EmitSignal(SignalName.AbilityCast);
      }
      else if (CurrentItem != null)
      {
         ItemResource item = CurrentItem.item;
         if (item.hitsSelf && !item.hitsAll && !item.hitsSurrounding && !item.hitsTeam && target != CurrentFighter)
         {
            return;
         }

         for (int i = 0; i < Fighters.Count; i++)
         {
            Fighters[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }
         
         CurrentTarget = target;
         itemsList.Visible = false;
         EmitSignal(SignalName.ItemUse);
         UpdateItems(CurrentItem);
         EnableItems();
         CurrentItem = null;
      }

      DisablePanels();
   }

   void OnHoverOverFighterPanel(int index, bool isEnemy)
   {
      Panel highlight = GetPanelFromIndex(index, isEnemy).GetNode<Panel>("Highlight");
      Fighter fighter = GetFighterFromIndex(index, isEnemy);

      if (CurrentAbility != null)
      {
         ExtraHoverOverBehavior(CurrentAbility.hitsSurrounding, CurrentAbility.hitsAll, fighter);
      }
      else if (CurrentItem != null)
      {
         ExtraHoverOverBehavior(CurrentItem.item.hitsSurrounding, CurrentItem.item.hitsAll, fighter);
      }

      if (fighter == CurrentFighter)
      {
         highlight.AddThemeStyleboxOverride("panel", alteredHighlight);
      }
      else
      {
         highlight.Visible = true;
      }

      fighter.placementNode.GetNode<Decal>("SelectionHighlight").Visible = true;

      /*Fighter fighter = GetFighterFromIndex(index, isEnemy);

      if (fighter == null || fighter.isDead)
      {
         if (currentAbility != null)
         {
            if (!currentAbility.bypassesDeaths)
            {
               return;
            }
         }
         else
         {
            return;
         }
      }

      if (currentAbility != null)
      {
         CompareHoverOver(fighter, currentAbility.hitsSelf, currentAbility.hitsSurrounding, currentAbility.hitsAll);
      }
      else if (currentItem != null)
      {
         CompareHoverOver(fighter, currentItem.item.hitsSelf, currentItem.item.hitsSurrounding, currentItem.item.hitsAll);
      }
      
      fighter.UIPanel.GetNode<Panel>("Highlight").Visible = true;*/
   }

   void OnStopHoverOverFighterPanel(int index, bool isEnemy)
   {
      Panel highlight = GetPanelFromIndex(index, isEnemy).GetNode<Panel>("Highlight");
      Fighter fighter = GetFighterFromIndex(index, isEnemy);

      if (CurrentAbility != null)
      {
         ExtraStopHoverOverBehavior(CurrentAbility.hitsSurrounding, CurrentAbility.hitsAll, fighter);
      }
      else if (CurrentItem != null)
      {
         ExtraStopHoverOverBehavior(CurrentItem.item.hitsSurrounding, CurrentItem.item.hitsAll, fighter);
      }

      if (fighter == CurrentFighter)
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
         List<Fighter> all = GetAll(target);
         for (int i = 0; i < all.Count; i++)
         {
            all[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
            all[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = true;
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = GetSurrounding(target);
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
         List<Fighter> all = GetAll(target);
         for (int i = 0; i < all.Count; i++)
         {
            all[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
            all[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = GetSurrounding(target);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
            surrounding[i].placementNode.GetNode<Decal>("SelectionHighlight").Visible = false;
         }
      }
   }

   Node2D GetPanelFromIndex(int index, bool isEnemy)
   {
      string root = isEnemy ? "Enemy" : "Party";
      return UI.GetNode<Node2D>("Combat" + root + "List/HBoxContainer/Panel" + (index + 1) + "/Holder");
   }

   Fighter GetFighterFromIndex(int index, bool isEnemy)
   {
      Node2D UIPanel = GetPanelFromIndex(index, isEnemy);

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].UIPanel == UIPanel)
         {
            return Fighters[i];
         }
      }

      return null;
   }

   public void EnablePanels()
   {
      if (CurrentAbility != null && CurrentAbility.onlyHitsSelf)
      {
         CurrentFighter.UIPanel.GetNode<Button>("Selection").Disabled = false;
         CurrentFighter.UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Stop;
         return;
      }

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i] != CurrentFighter)
         {
            if (CurrentAbility == null || !CurrentAbility.onlyHitsTeam || (CurrentAbility.onlyHitsTeam && !Fighters[i].isEnemy))
            {
               Fighters[i].UIPanel.GetNode<Button>("Selection").Disabled = false;
               Fighters[i].UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Stop;
            }
         }
      }

      if (CurrentAbility != null && CurrentAbility.hitsSelf)
      {
         CurrentFighter.UIPanel.GetNode<Button>("Selection").Disabled = false;
         CurrentFighter.UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Stop;
      }
   }

   void DisablePanels()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         Fighters[i].UIPanel.GetNode<Button>("Selection").Disabled = true;
         Fighters[i].UIPanel.GetNode<Button>("Selection").MouseFilter = Control.MouseFilterEnum.Ignore;
      }
   }

   void ClearAbilities()
   {
      int size = abilityButtonContainer.GetChildCount();
      for (int i = 0; i < size; i++)
      {
         Control button = abilityButtonContainer.GetNode<Control>("AbilityButton" + (i + 1));
         abilityButtonContainer.RemoveChild(button);
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

   void OnAttackButtonDown()
   {
      UnlockFighters();
      DisableOptions();
      EnablePanels();
      isAttacking = true;
      cancelButton.Visible = true;

      // Olren's arrows get special effects
      if (CurrentFighter.fighterName == "Olren" && !IsCompanionTurn)
      {
         currentOlrenSpecialArrow = "None";
         ClearSecondaryOptions();
         LoadOlrenPassive();
      }
   }

   void LoadOlrenPassive()
   {
      MoveSecondaryOptionsToLeft();
      for (int i = 0; i < olrenSpecialArrows.Count; i++)
      {
         if (CurrentFighter.level >= olrenSpecialArrows[i].requiredLevel)
         {
            Button arrowButton = olrenSpecialArrowPrefab.Instantiate<Button>();
            arrowButton.Text = olrenSpecialArrows[i].arrowName;
            arrowButton.TooltipText = olrenSpecialArrows[i].description;

            // Defaults to selecting "None"
            if (i == 0)
            {
               arrowButton.GetNode<Panel>("Highlight").Visible = true;
            }

            secondaryOptionsContainer.AddChild(arrowButton);
         }
      }
      secondaryOptions.Visible = true;
   }

   public void SetOlrenSpecialArrow(string arrowName)
   {
      currentOlrenSpecialArrow = arrowName;

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

   void MoveSecondaryOptionsToLeft()
   {
      secondaryOptions.Position = new Vector2(250, 500);
   }

   public void MoveSecondaryOptionsToRight()
   {
      secondaryOptions.Position = new Vector2(450, 500);
   }

   void OnCastButtonDown()
   {
      if (abilityContainer.Visible == false)
      {
         UpdateAbilities();
         abilityContainer.Visible = true;
         DisableOptions();
         selectionBox.GetNode<Panel>("Panel").GetNode<Button>("CastButton").Disabled = false;
      }
      else
      {
         abilityContainer.Visible = false;
         EnableOptions();
      }
   }

   void OnItemButtonDown()
   {
      if (itemsList.Visible == false)
      {
         itemsList.Visible = true;
         DisableOptions();
         selectionBox.GetNode<Panel>("Panel").GetNode<Button>("ItemButton").Disabled = false;
      }
      else
      {
         itemsList.Visible = false;
         EnableOptions();
      }
   }

   void OnCancelButtonDown()
   {
      if (isAttacking)
      {
         EnableOptions();
      }
      else if (CurrentAbility != null)
      {
         UpdateAbilities();
         CurrentAbility = null;

         for (int i = 0; i < Fighters.Count; i++)
         {
            Fighters[i].placementNode.GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;
         }
      }
      else if (CurrentItem != null)
      {
         EnableItems();
         CurrentItem = null;
      }

      if (IsCompanionTurn)
      {
         FocusCameraOnFighter(CurrentFighter.companion.model);
      }
      else
      {
         FocusCameraOnFighter(CurrentFighter.model);
      }
      
      isAttacking = false;
      LockFighters();
      DisablePanels();
      cancelButton.Visible = false;
      finalizeButton.Visible = false;
      secondaryOptions.Visible = false;
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

   void GenerateAbilities()
   {
      Member member = GetCurrentMember();
      PackedScene packedSceneButton = GD.Load<PackedScene>(abilityButton.ResourcePath);

      List<AbilityResource> abilitiesToUse;
      int manaToUse;

      string path = IsCompanionTurn ? "Enemy" : "Player";

      if (IsCompanionTurn)
      {
         abilitiesToUse = new List<AbilityResource>(CurrentFighter.companion.abilities);
         manaToUse = CurrentFighter.companion.currentMana;
      }
      else
      {
         abilitiesToUse = new List<AbilityResource>(CurrentFighter.abilities);
         manaToUse = CurrentFighter.currentMana;
      }

      for (int i = 0; i < abilitiesToUse.Count; i++)
      {
         Button currentButton = GenerateAbilityButton(packedSceneButton, abilitiesToUse[i], manaToUse, path);
         currentButton.Name = "AbilityButton" + (i + 1);

         abilityButtonContainer.AddChild(currentButton);
      }

      if (!IsCompanionTurn)
      {
         Button specialButton = GenerateAbilityButton(packedSceneButton, member.specialAbility, manaToUse, path);

         specialButton.Name = "Special";

         if (CurrentFighter.specialCooldown > 0)
         {
            specialButton.Disabled = true;
         }

         abilityContainer.AddChild(specialButton);
         specialButton.Position = new Vector2(0, -30);
      }
   }

   Button GenerateAbilityButton(PackedScene packedSceneButton, AbilityResource ability, int mana, string path) {
      // Here, a button is created from the prefab, then the corresponding script is loaded in and attached to the ScriptHolder (a child of the button)
      // Each ability has its own script, which has special behavior, including what happens upon pressing the button
      Button button = packedSceneButton.Instantiate<Button>();

      button.GetNode<ResourceHolder>("ResourceHolder").abilityResource = ability;

      Node2D scriptHolder = button.GetNode<Node2D>("ScriptHolder");
      scriptHolder.SetScript(GD.Load<CSharpScript>("res://Combat/Abilities/" + path + "/Behaviors/" + ability.scriptName + ".cs"));

      button.Text = ability.name;
      button.TooltipText = ability.description;

      if (mana < ability.manaCost)
      {
         button.Disabled = true;
      }

      return button;
   }

   public void DisableOtherAbilities()
   {
      //Member member = GetCurrentMember();

      for (int i = 0; i < abilityButtonContainer.GetChildCount(); i++)
      {
         //if (member.abilities[i] != currentAbility)
         //{
            abilityButtonContainer.GetNode<Button>("AbilityButton" + (i + 1)).Disabled = true;
         //}
      }

      if (abilityContainer.GetChildCount() > 1)
      {
         abilityContainer.GetNode<Button>("Special").Disabled = true;
      }
   }

   void UpdateAbilities()
   {
      Member member = GetCurrentMember();

      List<AbilityResource> abilitiesToUse;
      int manaToUse;

      if (IsCompanionTurn)
      {
         abilitiesToUse = new List<AbilityResource>(CurrentFighter.companion.abilities);
         manaToUse = CurrentFighter.companion.currentMana;
      }
      else
      {
         abilitiesToUse = new List<AbilityResource>(CurrentFighter.abilities);
         manaToUse = CurrentFighter.currentMana;;
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
         if (member.specialAbility.manaCost <= CurrentFighter.currentMana && CurrentFighter.specialCooldown <= 0)
         {
            abilityContainer.GetNode<Button>("Special").Disabled = false;
         }
         else
         {
            abilityContainer.GetNode<Button>("Special").Disabled = true;
         }
      }
   }

   Member GetCurrentMember()
   {
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (CurrentFighter.fighterName == partyManager.Party[i].characterName)
         {
            return partyManager.Party[i];
         }
      }
      return null;
   }

   void GenerateItems()
   {
      PackedScene packedScene = GD.Load<PackedScene>(itemPrefab.ResourcePath);

      for (int i = 0; i < partyManager.Items.Count; i++)
      {
         if (partyManager.Items[i].item.itemType == ItemType.None)
         {
            // This is much like GenerateAbilities (creates buttons that have scripts inside them), but with items instead
            InventoryItem thisItem = partyManager.Items[i];
            Button currentButton = packedScene.Instantiate<Button>();

            currentButton.GetNode<ItemResourceHolder>("ResourceHolder").itemResource = thisItem;

            Node2D scriptHolder = currentButton.GetNode<Node2D>("ScriptHolder");
            scriptHolder.SetScript(GD.Load<CSharpScript>("res://Combat/Items/Behaviors/" + thisItem.item.scriptName + ".cs"));

            currentButton.Text = thisItem.item.name + " (" + thisItem.quantity + "x)";
            currentButton.TooltipText = thisItem.item.description;
            currentButton.Name = "ItemButton" + (i + 1);
            itemsContainer.AddChild(currentButton);
         }
      }
   }

   void ClearItems()
   {
      int size = itemsContainer.GetChildCount();
      for (int i = 0; i < size; i++)
      {
         Control button = itemsContainer.GetNode<Control>("ItemButton" + (i + 1));
         itemsContainer.RemoveChild(button);
         button.QueueFree();
      }
   }

   void UpdateItems(InventoryItem changedItem)
   {
      for (int i = 0; i < partyManager.Items.Count; i++)
      {
         if (partyManager.Items[i] == changedItem)
         {
            partyManager.Items[i].quantity--;

            // Update UI accordingly
            if (partyManager.Items[i].quantity <= 0)
            {
               partyManager.Items.Remove(partyManager.Items[i]);
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

   void EnableItems()
   {
      for (int i = 0; i < itemsContainer.GetChildCount(); i++)
      {
         itemsContainer.GetNode<Button>("ItemButton" + (i + 1)).Disabled = false;
      }
   }

   async void CompleteAttack(Fighter target)
   {
      LockFighters();
      secondaryOptions.Visible = false;
      cancelButton.Visible = false;
      selectionBox.Visible = false;

      CurrentTarget = target;
      DamagingEntity damage = new DamagingEntity(CurrentFighter.level, StatType.Strength, StatType.Fortitude, DamageType.Physical);

      if (IsCompanionTurn)
      {
         damage.baseDamage = CurrentFighter.level / 2;
      }

      // Athlia's attacks deal 10% damage
      if (CurrentFighter.fighterName == "Athlia")
      {
         damage.baseDamage = (int)(damage.baseDamage * 0.1f);
      }

      AnimationPlayer player;

      if (IsCompanionTurn)
      {
         player = CurrentFighter.companion.model.GetNode<AnimationPlayer>("Model/AnimationPlayer");
         ReverseFocusOnTarget(CurrentFighter.companion.model);
      }
      else
      {
         player = CurrentFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer");
         ReverseFocusOnTarget(CurrentFighter.model);
      }

      bool crit = RollForCrit();

      if (crit)
      {
         damage.baseDamage *= 2;
      }

      ProcessAttack(CalculateDamage(new List<DamagingEntity>() { damage }), target);

      // Olren's attacks have special effects
      if (CurrentFighter.fighterName == "Olren" && !IsCompanionTurn)
      {
         ApplyOlrenPassive();
      }
      
      player.Play("Attack");
      EmitSignal(SignalName.AttackAnimation);

      await ToSignal(GetTree().CreateTimer(player.CurrentAnimationLength + 0.35f), "timeout");
      
      double waitTime = FocusOnTargets(new List<Fighter>() { target }, true);

      Label3D critBillboard = target.placementNode.GetNode<Label3D>("CritLabel");

      if (target.wasHit)
      {
         if (crit)
         {
            critBillboard.Text = "CRIT";
            critBillboard.Visible = true;
         }
      }

      await ToSignal(GetTree().CreateTimer(waitTime + 0.35f), "timeout");

      critBillboard.Visible = false;
      
      ProcessValues();

      if (target.currentHealth > 0)
      {
         target.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
         player.Play("CombatIdle");
      }
   }

   bool RollForCrit()
   {
      int roll = GD.RandRange(0, 99);

      int accuracyStat;

      if (IsCompanionTurn)
      {
         accuracyStat = CurrentFighter.companion.stats[(int)StatType.Accuracy].value;
      }
      else
      {
         accuracyStat = CurrentFighter.stats[(int)StatType.Accuracy].value;
      }

      if (roll < accuracyStat)
      {
         return true;
      }

      return false;
   }

   void ApplyOlrenPassive()
   {
      if (CurrentTarget.wasHit)
      {
         if (currentOlrenSpecialArrow == "Poison")
         {
            ApplyStatus(75, CurrentTarget, StatusEffect.Poison, 2, 4);
         }
      }
   }

   double FocusOnTargets(List<Fighter> targets, bool playHitAnimation)
   {
      if (targets.Count == 1)
      {
         ReverseFocusOnTarget(targets[0].model);
      }
      else
      {
         if (targets[0].isEnemy)
         {
            PointCameraAtEnemies();
         }
         else
         {
            PointCameraAtParty();
         }
      }

      double longestDuration = 0;

      if (playHitAnimation)
      {
         for (int i = 0; i < targets.Count; i++)
         {
            AnimationPlayer player = targets[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer");

            if (targets[i].wasHit)
            {
               player.Play("Hit");

               for (int j = 0; j < targets[i].currentStatuses.Count; j++)
               {
                  if (targets[i].currentStatuses[j].effect == StatusEffect.Stealth)
                  {
                     RemoveStatus(targets[i], j);
                  }
               }
            }
            else
            {
               Label3D missBillboard = targets[i].placementNode.GetNode<Label3D>("CritLabel");
               missBillboard.Text = "MISS";
               missBillboard.Visible = true;

               player.Play("Dodge");
            }

            float currentDuration = (float)player.CurrentAnimationLength;
            
            if (currentDuration > longestDuration)
            {
               longestDuration = currentDuration;
            }
         }
      }
      else
      {
         longestDuration = 0.6f;
      }

      if (CurrentAbility != null)
      {
         AbilityTargetGraphic = CurrentAbility.targetGraphic;
      }

      if (AbilityTargetGraphic.Length > 0)
      {
        AbilityGraphic abilityGraphic = GenerateTargetAbilityGraphic(AbilityTargetGraphic);

         if (abilityGraphic.GenerateOnlyOnce)
         {
            SetTargetAbilityGraphics(abilityGraphic, targets[0].placementNode.GlobalPosition);
         }
         else
         {
            for (int i = 0; i < targets.Count; i++)
            {
               SetTargetAbilityGraphics(abilityGraphic, targets[i].placementNode.GlobalPosition);
            }
         }
      }

      for (int i = 0; i < targets.Count; i++)
      {
         UpdateSingularUIPanel(targets[i]);
         UpdateStacksAndEffectsUI(targets[i]);
      }

      return longestDuration;
   }

   AbilityGraphic GenerateTargetAbilityGraphic(string abilityName)
   {
      PackedScene targetGraphicsScene = GD.Load<PackedScene>("res://Combat/Abilities/Graphics/" + abilityName + ".tscn");
      Node3D targetGraphics = targetGraphicsScene.Instantiate<Node3D>();
      AbilityGraphic abilityGraphicScript = targetGraphics.GetNode<AbilityGraphic>("ScriptHolder");

      return abilityGraphicScript;
   }

   void SetTargetAbilityGraphics(AbilityGraphic abilityGraphic, Vector3 targetPosition)
   {
      Node3D graphic = abilityGraphic.GetParent<Node3D>();
      baseNode.AddChild(graphic);

      if (abilityGraphic.SnapToTarget)
      {
         graphic.GlobalPosition = targetPosition;
         graphic.GlobalPosition += new Vector3(0, abilityGraphic.VerticalOffset, 0);
      }
   }

   void EnemyTurn(Fighter fighter)
   {
      Fighter target = SelectEnemyTarget(false, false);
      List<AbilityResource> validAbilities = new List<AbilityResource>();

      for (int i = 0; i < CurrentFighter.abilities.Count; i++)
      {
         if (CurrentFighter.abilities[i].manaCost <= CurrentFighter.currentMana)
         {
            validAbilities.Add(CurrentFighter.abilities[i]);
         }
      }

      int selectedOption;

      if (validAbilities.Count <= 0)
      {
         selectedOption = 0;
      }
      else
      {
         selectedOption = GD.RandRange(0, 2);
      }

      if (selectedOption == 0)
      {
         CompleteAttack(target);
        // GD.Print("ENEMY ATTACK");
      }
      else
      {
         AbilityResource chosenAbility = validAbilities[GD.RandRange(0, validAbilities.Count - 1)];
         CurrentAbility = chosenAbility;

         if (CurrentAbility.hitsTeam && !CurrentAbility.hitsSelf)
         {
            target = SelectEnemyTarget(true, false);
         }
         else if (CurrentAbility.hitsTeam && CurrentAbility.hitsSelf)
         {
            target = SelectEnemyTarget(true, true);
         }
         else if (!CurrentAbility.hitsTeam && CurrentAbility.hitsSelf)
         {
            target = CurrentFighter;
         }

         CurrentTarget = target;
         //GD.Print("ENEMY CAST");
         EnemyCast();
      }
   }

   void EnemyCast()
   {
      // Here, a new node3D is added. It has the necessary script to execute the ability
      // Godot doesn't seem to execute instantiated scripts unless what it's being added to is instantiated itself, which is why this is so roundabout
      PackedScene packedSceneHolder = GD.Load<PackedScene>(enemyAbilityUseHolder.ResourcePath);
      Node3D holder = packedSceneHolder.Instantiate<Node3D>();
      Node3D scriptHolder = holder.GetNode<Node3D>("Holder");

      scriptHolder.SetScript(GD.Load<CSharpScript>("res://Combat/Abilities/Enemy/Behaviors/" + CurrentAbility.scriptName + ".cs"));
      CurrentFighter.model.GetNode<EnemyDataHolder>("ScriptHolder").AddChild(holder);

      EmitSignal(SignalName.EnemyAbilityCast);

      AbilityTargetGraphic = CurrentAbility.targetGraphic;

      CurrentFighter.model.GetNode<EnemyDataHolder>("ScriptHolder").RemoveChild(holder);
      holder.QueueFree();

      CurrentAbility = null;
   }

   public void ProcessAttack(int damage, Fighter target, bool bypassHitChance = false)
   {
      for (int i = 0; i < CurrentFighter.currentStatuses.Count; i++)
      {
         if (CurrentFighter.currentStatuses[i].effect == StatusEffect.Stealth)
         {
            RemoveStatus(CurrentFighter, i);
         }
      }

      if (HitAttack(target))
      {
         target.currentHealth -= damage;
         
         if (!target.isEnemy)
         {
            ApplyPassives(target);
         }
      }
   }

   Fighter SelectEnemyTarget(bool selectAlly, bool includeSelf)
   {
      List<int> threatThreshholds = new List<int>();
      int threatValue = 0;

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (!Fighters[i].isDead && Fighters[i].isEnemy == selectAlly && (CurrentFighter != Fighters[i] || (CurrentFighter == Fighters[i] && includeSelf)))
         {
            threatThreshholds.Add(threatValue + Fighters[i].stats[9].value);
            threatValue += Fighters[i].stats[9].value;
         }
         else
         {
            // This is necessary to add "placeholders" so we can find the right fighter later (we can't skip, or threatThreshholds won't align with fighters)
            threatThreshholds.Add(-1);
         }
      }

      int chosen = GD.RandRange(0, threatValue);

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (threatThreshholds[i] != -1)
         {
            if (threatThreshholds[i] >= chosen)
            {
               return Fighters[i];
            }
         }
      }

      return null;
   }

   public void HoverOverEnemy(Node3D model)
   {
      //ProcessHoverOver(ConvertModelToFighter(model));
   }

   public void StopHoverOverEnemy(Node3D model)
   {
      //ProcessStopHoverOver(ConvertModelToFighter(model));
   }

   /*public void OnHoverOverPartyMember(string fighterName)
   {
      GD.Print(Name);
      ProcessHoverOver(GetFighterFromName(fighterName));
   }

   public void OnStopHoverOverPartyMember(string fighterName)
   {
      ProcessStopHoverOver(GetFighterFromName(fighterName));
   }*/

   public Fighter GetFighterFromMember(Member member)
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].fighterName == member.characterName)
         {
            return Fighters[i];
         }
      }

      return null;
   }

   public void ProcessHoverOver(Fighter fighter)
   {
      
   }

   void CompareHoverOver(Fighter fighter, bool hitsSelf, bool hitsSurrounding, bool hitsAll)
   {
      if (hitsSelf && !hitsSurrounding && !hitsAll && fighter.isEnemy)
      {
         // This ability should ONLY be used on the player, so don't bother highlighting enemies
         fighter.UIPanel.GetNode<Panel>("Highlight").Visible = false;
         return;
      }
      else if (hitsSelf && !fighter.isEnemy)
      {
         // Hovering over the current fighter when using a self-cast; change the color
         fighter.UIPanel.GetNode<Panel>("Highlight").AddThemeStyleboxOverride("panel", alteredHighlight);
      }
      else if (hitsAll)
      {
         List<Fighter> all = GetAll(fighter);
         for (int i = 0; i < all.Count; i++)
         {
            all[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = GetSurrounding(fighter);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = true;
         }
      }
   }

   public void ProcessStopHoverOver(Fighter fighter)
   {
      if (fighter == null)
      {
         return;
      }
      
      if (fighter != CurrentFighter)
      {
         fighter.UIPanel.GetNode<Panel>("Highlight").Visible = false;
      }

      if (CurrentAbility != null)
      {
         CompareStopHoverOver(fighter, CurrentAbility.hitsSelf, CurrentAbility.hitsSurrounding, CurrentAbility.hitsAll);
      }
      else if (CurrentItem != null)
      {
         CompareStopHoverOver(fighter, CurrentItem.item.hitsSelf, CurrentItem.item.hitsSurrounding, CurrentItem.item.hitsAll);
      }

      if (!fighter.isEnemy)
      {
         fighter.UIPanel.GetNode<Panel>("Highlight").RemoveThemeStyleboxOverride("panel");
         fighter.UIPanel.GetNode<Panel>("Highlight").AddThemeStyleboxOverride("panel", baseHighlight);
      }
   }

   void CompareStopHoverOver(Fighter fighter, bool hitsSelf, bool hitsSurrounding, bool hitsAll)
   {
      if (hitsAll)
      {
         List<Fighter> all = GetAll(fighter);
         for (int i = 0; i < all.Count; i++)
         {
            all[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
         }
      }
      else if (hitsSurrounding)
      {
         List<Fighter> surrounding = GetSurrounding(fighter);
         for (int i = 0; i < surrounding.Count; i++)
         {
            surrounding[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
         }
      }
      else if (hitsSelf && CurrentFighter == fighter)
      {
         // Don't disable the highlight if you hovered over a self-cast
         fighter.UIPanel.GetNode<Panel>("Highlight").Visible = true;
      }
   }

   public List<Fighter> GetAll(Fighter target)
   {
      List<Fighter> fightersResult = new List<Fighter>();

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].isEnemy == target.isEnemy)
         {
            fightersResult.Add(Fighters[i]);
         }
      }

      return fightersResult;
   }

   public List<Fighter> GetSurrounding(Fighter target)
   {
      List<Fighter> fightersResult = new List<Fighter>();

      int index = -1;

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i] == target)
         {
            index = i;
            break;
         }
      }

      // Get fighter to the LEFT
      if (index > 0)
      {
         if (Fighters[index - 1].isEnemy == target.isEnemy && !Fighters[index - 1].isDead)
         {
            fightersResult.Add(Fighters[index - 1]);
         }
      }

      // Get fighter to the RIGHT
      if (index < Fighters.Count - 1 && Fighters[index + 1].isEnemy == target.isEnemy && !Fighters[index + 1].isDead)
      {
         fightersResult.Add(Fighters[index + 1]);
      }

      return fightersResult;
   }

   Fighter ConvertModelToFighter(Node3D model)
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].model == model)
         {
            return Fighters[i];
         }
      }
      return null;
   }

   public void UnlockFighters()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i] != CurrentFighter)
         {
            if (Fighters[i].isEnemy)
            {
               Fighters[i].model.GetNode<CollisionObject3D>("ScriptHolder/StaticBody3D").InputRayPickable = true;
            }
            else
            {
               Fighters[i].model.GetNode<CollisionObject3D>("../Member" + (i + 1)).InputRayPickable = true;
            }
         }
         else
         {
            if ((CurrentAbility == null || !CurrentAbility.hitsSelf) && (CurrentItem == null || !CurrentItem.item.hitsSelf))
            {
               Fighters[i].model.GetNode<CollisionObject3D>("../Member" + (i + 1)).InputRayPickable = false;
            }
            else if ((CurrentAbility != null && CurrentAbility.hitsSelf) || (CurrentItem != null && CurrentItem.item.hitsSelf))
            {
               Fighters[i].model.GetNode<CollisionObject3D>("../Member" + (i + 1)).InputRayPickable = true;
            }
         }    
      } 
   }

   public void LockFighters()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].isEnemy)
         {
            Fighters[i].model.GetNode<CollisionObject3D>("ScriptHolder/StaticBody3D").InputRayPickable = false;
         }
         else
         {
            Fighters[i].model.GetNode<CollisionObject3D>("../Member" + (i + 1)).InputRayPickable = false;
         }  
      } 
   }

   void UpdateUI()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         Fighters[i].UIPanel.GetNode<Panel>("Highlight").Visible = false;
         Fighters[i].UIPanel.GetNode<Label>("HealthDescription").Text = Fighters[i].currentHealth + "/" + Fighters[i].maxHealth;
         Fighters[i].UIPanel.GetNode<ProgressBar>("HealthBar").Value = (Fighters[i].currentHealth * 1f / Fighters[i].maxHealth) * 100f;

         if (!Fighters[i].isEnemy)
         {
            Fighters[i].UIPanel.GetNode<Label>("ManaDescription").Text = Fighters[i].currentMana + "/" + Fighters[i].maxMana;
            Fighters[i].UIPanel.GetNode<ProgressBar>("ManaBar").Value = (Fighters[i].currentMana * 1f / Fighters[i].maxMana) * 100f;
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

   void DisableOptions()
   {
      Panel panel = selectionBox.GetNode<Panel>("Panel");
      panel.GetNode<Button>("AttackButton").Disabled = true;
      panel.GetNode<Button>("CastButton").Disabled = true;
      panel.GetNode<Button>("ItemButton").Disabled = true;
   }

   public void EnableOptions()
   {
      Panel panel = selectionBox.GetNode<Panel>("Panel");
      panel.GetNode<Button>("AttackButton").Disabled = false;
      panel.GetNode<Button>("CastButton").Disabled = false;

      if (!IsCompanionTurn)
      {
         panel.GetNode<Button>("ItemButton").Disabled = false;
      }
   }

   // Prevents negative health/mana and processes death
   public async void ProcessValues()
   {
      bool hasAliveEnemy = false;
      bool hasAlivePartyMember = false;

      List<Fighter> deadFighters = new List<Fighter>();

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].currentHealth <= 0)
         {
            Fighters[i].currentHealth = 0;

            if (!Fighters[i].isDead)
            {
               deadFighters.Add(Fighters[i]);
            }
            
            Fighters[i].isDead = true;

            if (Fighters[i].companion != null)
            {
               RemoveCompanion(Fighters[i]);
            }
         }
         else
         {
            if (Fighters[i].isEnemy)
            {
               hasAliveEnemy = true;
            }
            else
            {
               hasAlivePartyMember = true;
            }
         }

         /*if (fighters[i].currentMana < 0)
         {
            fighters[i].currentMana = 0;
         }

         if (fighters[i].currentHealth > fighters[i].maxHealth)
         {
            fighters[i].currentHealth = fighters[i].maxHealth;
         }

         if (fighters[i].currentMana > fighters[i].maxMana)
         {
            fighters[i].currentMana = fighters[i].maxMana;
         }*/
      }

      //UpdateUI();

      for (int i = 0; i < deadFighters.Count; i++)
      {
         ReverseFocusOnTarget(deadFighters[i].model);
         AnimationPlayer player = deadFighters[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer");
         player.Play("Death");
         await ToSignal(GetTree().CreateTimer(player.CurrentAnimationLength + 0.35f), "timeout");
      }

      if (!hasAliveEnemy)
      {
         Victory();
      }
      else if (!hasAlivePartyMember)
      {
         Loss();
      }
      else
      {
         CompleteTurn();
      }
   }

   void RemoveCompanion(Fighter affectedFighter)
   {
      affectedFighter.UIPanel.GetNode<Panel>("CompanionHolder").Visible = false;
      GetNode<Node3D>("/root/BaseNode").RemoveChild(affectedFighter.companion.model);
      affectedFighter.companion.model.QueueFree();
      affectedFighter.companion = null;
   }

   public int CalculateDamage(List<DamagingEntity> damagers)
   {
      int currentDamageTotal = 0;
      for (int i = 0; i < damagers.Count; i++)
      {
         int attackStat = 0;
         int defenseStat = 0;
         for (int j = 0; j < 10; j++)
         {
            if (CurrentFighter.stats[j].statType == damagers[i].scalingAttack)
            {
               attackStat = CurrentFighter.stats[j].value;
            }

            if (CurrentTarget.stats[j].statType == damagers[i].scalingDefense)
            {
               defenseStat = CurrentFighter.stats[j].value;
            }
         }

         float modifier = 1f;

         if (CurrentFighter.affinity == Affinity.Shrouded)
         {
            for (int j = 0; j < CurrentFighter.currentStatuses.Count; j++)
            {
               if (CurrentFighter.currentStatuses[j].effect == StatusEffect.Stealth)
               {
                  modifier += 1f;
                  break;
               }
            }
         }

         AffinityStrengths targetStrsWeaks = affinityTable[(int)CurrentTarget.affinity];
         AffinityStrengths casterStrsWeaks = affinityTable[(int)CurrentFighter.affinity];

         if (targetStrsWeaks.weakTo1 == damagers[i].damageType || targetStrsWeaks.weakTo2 == damagers[i].damageType)
         {
            modifier += 0.25f;
         }
         else if (targetStrsWeaks.strongAgainst1 == damagers[i].damageType || targetStrsWeaks.strongAgainst2 == damagers[i].damageType)
         {
            modifier -= 0.25f;
         }

         if (casterStrsWeaks.strongAgainst1 == damagers[i].damageType || casterStrsWeaks.strongAgainst2 == damagers[i].damageType)
         {
            modifier += 0.25f;
         }

         int defenseApplier = defenseStat / 3;

         if (defenseApplier <= 0)
         {
            defenseApplier = 1;
         }

         currentDamageTotal += (int)(((damagers[i].baseDamage * (attackStat / 2)) / defenseApplier) * modifier);
      }
      

      return Mathf.Clamp(currentDamageTotal, 1, 99999);
   }

   public void ApplyStatus(int chance, Fighter target, StatusEffect statusEffect, int minTurn, int maxTurn)
   {
      if (target.wasHit)
      {
         for (int i = 0; i < target.currentStatuses.Count; i++)
         {
            if (target.currentStatuses[i].effect == statusEffect)
            {
               return; // Target already has the status, so don't try to reapply it
            }
         }

         int chosen = GD.RandRange(0, 99);

         if (chosen <= chance)
         {
            int duration = GD.RandRange(minTurn, maxTurn);

            AppliedStatusEffect appliedStatusEffect = new AppliedStatusEffect(statusEffect, duration, CurrentFighter);
            appliedStatusEffect.isCleanseable = statusDatas[(int)statusEffect].isCleanseable;
            appliedStatusEffect.isNegative = statusDatas[(int)statusEffect].isNegative;
            
            target.currentStatuses.Add(appliedStatusEffect);
            AddEffectUI(statusEffect, target);

            if (statusEffect == StatusEffect.Burn)
            {
               ApplyStatModifier(StatType.Strength, -1 * (int)(target.stats[2].baseValue * 0.1f), duration, target, CurrentFighter, StatusEffect.Burn);
            }
            else if (statusEffect == StatusEffect.Disease)
            {
               ApplyStatModifier(StatType.Constitution, -1 * (int)(target.stats[2].baseValue * 0.15f), duration, target, CurrentFighter, StatusEffect.Disease);
               ApplyStatModifier(StatType.Knowledge, -1 * (int)(target.stats[2].baseValue * 0.15f), duration, target, CurrentFighter, StatusEffect.Disease);
               target.maxHealth = target.GetMaxHealth();
               target.maxMana = target.GetMaxMana();
            }
            else if (statusEffect == StatusEffect.MegaBuff)
            {
               appliedStatusEffect.displayRemainingTurns = true;
               for (int i = 0; i < 10; i++)
               {
                  ApplyStatModifier((StatType)i, Mathf.CeilToInt(target.stats[i].baseValue * 0.25f), duration, target, CurrentFighter, StatusEffect.MegaBuff);
               }
               target.maxHealth = target.GetMaxHealth();
               target.maxMana = target.GetMaxMana();
            }
            else if (statusEffect == StatusEffect.Birdseye)
            {
               appliedStatusEffect.displayRemainingTurns = true;
               ApplyStatModifier(StatType.Accuracy, Mathf.CeilToInt(target.stats[(int)StatType.Accuracy].baseValue * 0.25f), duration, target, CurrentFighter,
                                 StatusEffect.Birdseye);
            }
            else if (statusEffect == StatusEffect.Stealth)
            {
               appliedStatusEffect.displayRemainingTurns = true;
               ApplyStatModifier(StatType.Evasion, Mathf.CeilToInt(target.stats[(int)StatType.Evasion].baseValue * 0.75f), duration, target, CurrentFighter,
                                 StatusEffect.Stealth);
            }
         }
      }
   }

   void AddEffectUI(StatusEffect effect, Fighter applied)
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
      effectBack.TooltipText = statusDatas[(int)effect].description;
      Texture2D sprite = GD.Load<Texture2D>("res://Combat/StatusEffects/" + effect + ".png");
      Sprite2D spriteNode = effectBack.GetNode<Sprite2D>("Sprite");
      spriteNode.Texture = sprite;
      spriteNode.Name = effect + "";

      effectBack.Visible = false;
      
      rowToUse.AddChild(effectBack);
   }

   void ApplyStatModifier(StatType statType, int modifier, int duration, Fighter affectedFighter, Fighter applier, StatusEffect attachedStatus)
   {
      affectedFighter.statModifiers.Add(new StatModifier(statType, modifier, duration, applier, attachedStatus));

      for (int i = 0; i < affectedFighter.stats.Length; i++)
      {
         if (affectedFighter.stats[i].statType == statType)
         {
            affectedFighter.stats[i].value += modifier;
         }
      }
   }

   bool HitAttack(Fighter target)
   {
      // Thalria's passive means she hits every attack
      if (CurrentFighter.fighterName == "Thalria")
      {
         target.wasHit = true;
         return true;
      }
      
      int rand = GD.RandRange(0, 99);
      int chance = 8 - CurrentFighter.stats[6].value + target.stats[7].value;

      if (rand < chance)
      {
         target.wasHit = false;
         return false;
      }
      else
      {
         target.wasHit = true;
         return true;
      }
   }

   public void AddStack(Fighter target, Stack newStack)
   {
      HBoxContainer stacksContainer = target.UIPanel.GetNode<HBoxContainer>("Effects/Stacks");
      stacksContainer.Visible = true;

      for (int i = 0; i < target.stacks.Count; i++)
      {
         if (target.stacks[i].stackName == newStack.stackName)
         {
            target.stacks[i].quantity += newStack.quantity;
            target.stacks[i].needsQuantityUpdate = true;
            //stacksContainer.GetChild(i).GetNode<Label>("Sprite/Quantity").Text = "" + target.stacks[i].quantity;
            return;
         }
      }

      PackedScene packedScene = GD.Load<PackedScene>(stackPrefab.ResourcePath);
      Panel stackButton = packedScene.Instantiate<Panel>();
      stackButton.TooltipText = newStack.stackName;
      stackButton.GetNode<Sprite2D>("Sprite").Texture = GD.Load<Texture2D>("res://Combat/Stacks/" + newStack.spriteName + ".png");
      stackButton.GetNode<Label>("Sprite/Quantity").Text = "" + newStack.quantity;
      target.stacks.Add(newStack);

      stackButton.Visible = false;

      stacksContainer.AddChild(stackButton);
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

      /*foreach (Panel child in row1.GetChildren())
      {
         child.Visible = true;
      }

      foreach (Panel child in row2.GetChildren())
      {
         child.Visible = true;
      }*/
   }

   public void RemoveStack(Fighter target, string stackName, int quantityToLose)
   {
      HBoxContainer stacksContainer = target.UIPanel.GetNode<HBoxContainer>("Effects/Stacks");

      for (int i = 0; i < target.stacks.Count; i++)
      {
         if (target.stacks[i].stackName == stackName)
         {
            Panel child = stacksContainer.GetChild<Panel>(i);
            target.stacks[i].quantity -= quantityToLose;
            //child.GetNode<Label>("Sprite/Quantity").Text = "" + target.stacks[i].quantity;
            target.stacks[i].needsQuantityUpdate = true;
            
            /*if (target.stacks[i].quantity <= 0)
            {
               target.stacks.Remove(target.stacks[i]);
               

               if (stacksContainer.GetChildCount() == 0)
               {
                  stacksContainer.Visible = false;
               }
            }*/
            return;
         }
      }
   }

   void ApplyPassives(Fighter affectedFighter)
   {
      if (affectedFighter.fighterName == "Vakthol")
      {
         // +100% damage at max health; +100% defense at 0 health
         float percentHealth = affectedFighter.currentHealth * 1f / affectedFighter.maxHealth;
         int strengthMod = (int)(percentHealth * affectedFighter.stats[(int)StatType.Strength].baseValue);
         int defenseMod = (int)((1f - percentHealth) * affectedFighter.stats[(int)StatType.Strength].baseValue);

         // Apply the stat modifiers if they aren't there
         if (affectedFighter.statModifiers.Count < 3)
         {
            ApplyStatModifier(StatType.Strength, strengthMod, 9999, affectedFighter, affectedFighter, StatusEffect.None);
            ApplyStatModifier(StatType.Fortitude, defenseMod, 9999, affectedFighter, affectedFighter, StatusEffect.None);
            ApplyStatModifier(StatType.Willpower, defenseMod, 9999, affectedFighter, affectedFighter, StatusEffect.None);
            return;
         }

         // Vakthol's first, second, and third stat modifiers are always related to his passive
         affectedFighter.stats[(int)StatType.Strength].value -= affectedFighter.statModifiers[0].modifier;
         affectedFighter.statModifiers[0].modifier = strengthMod;
         affectedFighter.stats[(int)StatType.Strength].value += affectedFighter.statModifiers[0].modifier;

         affectedFighter.stats[(int)StatType.Fortitude].value -= affectedFighter.statModifiers[1].modifier;
         affectedFighter.stats[(int)StatType.Willpower].value -= affectedFighter.statModifiers[2].modifier;
         affectedFighter.statModifiers[1].modifier = defenseMod;
         affectedFighter.stats[(int)StatType.Fortitude].value += affectedFighter.statModifiers[1].modifier;
         affectedFighter.stats[(int)StatType.Willpower].value += affectedFighter.statModifiers[2].modifier;
      }
   }

   async void Victory()
   {
      ResetCombat();

      //arenaCamera.GlobalPosition = arena.GlobalPosition + (Vector3.Up * 2f) + (Vector3.Back * 3f);
      //arenaCamera.Rotation = new Vector3(-0, Mathf.DegToRad(-180f), 0);
      PointCameraAtParty();

      int expGain = 0;

      levelManager.LocationDatas[levelManager.ActiveLocationDataID].defeatedEnemies[currentEnemyScript.id.ToString()] = true;

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].isEnemy)
         {
            expGain += Fighters[i].level * 5;
         }
         else
         {
            Fighters[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Victory");
         }
      }

      await ToSignal(GetTree().CreateTimer(2f), "timeout");
      
      for (int i = 0; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].isInParty)
         {
            partyManager.Party[i].experience += expGain;
            int oldLevel = partyManager.Party[i].level;
            int oldAbilityCount = partyManager.Party[i].abilities.Count;

            Fighter fighter = GetFighterFromMember(partyManager.Party[i]);
            partyManager.Party[i].currentHealth = Mathf.Clamp(fighter.currentHealth, 1, 9999);
            partyManager.Party[i].currentMana = fighter.currentMana;

            victoryScreen.GetNode<Label>("Back/ExpNotif" + (i + 1)).Text = Fighters[i].fighterName + " gains " + expGain + " EXP";
            partyManager.LevelUp(expGain, partyManager.Party[i]);

            if (oldLevel != partyManager.Party[i].level)
            {
               victoryScreen.GetNode<Label>("Back/ExpNotif" + (i + 1)).Text += "; level up to soul grade " + partyManager.Party[i].level + "!";
            }

            if (oldAbilityCount != partyManager.Party[i].abilities.Count)
            {
               victoryScreen.GetNode<Label>("Back/ExpNotif" + (i + 1)).Text += "Learns " + partyManager.Party[i].abilities[oldAbilityCount].name;
            }

            victoryScreen.GetNode<Label>("Back/ExpNotif" + (i + 1)).Visible = true;
         }
      }

      victoryScreen.Visible = true;
   }

   void OnExitButtonDown()
   {
      saveMenuManager.FadeToBlack();
      victoryScreen.Visible = false;
      //arena.GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1").Position = returnPosition;
      //arena.GetNode<Node3D>("/root/BaseNode/PartyMembers/Member1/Model").Rotation = Vector3.Zero;
      partyManager.Party[0].model.Position = returnPosition;
      partyManager.Party[0].model.GetNode<Node3D>("Model").GlobalRotation = returnRotation;
      controller.PlaceWeaponOnBack();
      controller.PlaceSecondaryWeaponOnBack();

      Vector3 currentReturnPosition = returnPosition;
      currentReturnPosition -= partyManager.Party[0].model.GlobalTransform.Basis.Z;

      for (int i = 1; i < partyManager.Party.Count; i++)
      {
         if (partyManager.Party[i].isInParty)
         {
            partyManager.Party[i].model.Position = currentReturnPosition;
            currentReturnPosition -= partyManager.Party[0].model.GlobalTransform.Basis.Z;
            OverworldPartyController overworldController = partyManager.Party[i].model.GetNode<OverworldPartyController>("../Member" + (i + 1));
            overworldController.EnablePathfinding = true;
            overworldController.PlaceWeaponOnBack();
            overworldController.PlaceSecondaryWeaponOnBack();
         }
      }

      playerCamera.MakeCurrent();
      Input.MouseMode = Input.MouseModeEnum.Captured;
      controller.DisableMovement = false;

      saveMenuManager.FadeFromBlack();

      for (int i = 0; i < 4; i++)
      {
         victoryScreen.GetNode<Label>("Back/ExpNotif" + (i + 1)).Visible = false;
      }

      if (currentEnemyScript.postBattleCutsceneName.Length > 0)
      {
         GetNode<CutsceneManager>("/root/BaseNode/CutsceneManager").CutsceneSignalReceiver(currentEnemyScript.postBattleCutsceneName);
      }
   }

   void Loss()
   {
      saveMenuManager.FadeToBlack();
      UI.GetNode<Node2D>("Overlay/DefeatScreen").Visible = true;
      IsInCombat = false;
   }

   void OnReloadLastButtonDown()
   {
      saveMenuManager.ResetGameState();
      saveMenuManager.LoadGame(saveMenuManager.currentSaveIndex);
      GetNode<Sprite2D>("/root/BaseNode/MainMenu/Background").Visible = false;
      saveMenuManager.FadeFromBlack();
      UI.GetNode<Node2D>("Overlay/DefeatScreen").Visible = false;

      Input.MouseMode = Input.MouseModeEnum.Captured;
      controller.DisableMovement = false;
      ResetCombat();
   }

   void OnQuitToMenuButtonDown()
   {
      saveMenuManager.ResetGameState();
      saveMenuManager.FadeFromBlack();
      ResetCombat();
      UI.GetNode<Node2D>("Overlay/DefeatScreen").Visible = false;
   }

   void ResetCombat()
   {
      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].isEnemy)
         {
            baseNode.RemoveChild(Fighters[i].model);
            Fighters[i].model.QueueFree();
         }

         if (Fighters[i].companion != null)
         {
            Fighters[i].UIPanel.GetNode<Panel>("CompanionHolder").Visible = false;

            baseNode.RemoveChild(Fighters[i].companion.model);
            Fighters[i].companion.model.QueueFree();
            
            Fighters[i].companion = null;
         }

         VBoxContainer statusRows = Fighters[i].UIPanel.GetNode<VBoxContainer>("Effects/StatusRows");
         HBoxContainer row1 = statusRows.GetNode<HBoxContainer>("Row1");
         HBoxContainer row2 = statusRows.GetNode<HBoxContainer>("Row2");
         HBoxContainer stacksContainer = Fighters[i].UIPanel.GetNode<HBoxContainer>("Effects/Stacks");
         
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

      IsInCombat = false;
      selectionBox.Visible = false;
      partyList.Visible = false;
      enemyList.Visible = false;

      CurrentItem = null;
      CurrentAbility = null;
      isAttacking = false;

      menuManager.canTakeInput = true;
   }

   public async void RegularCast(List<Fighter> targets, bool playHitAnimation = true)
   {
      LockFighters();

      AnimationPlayer player;

      if (IsCompanionTurn)
      {
         player = CurrentFighter.companion.model.GetNode<AnimationPlayer>("Model/AnimationPlayer");
         ReverseFocusOnTarget(CurrentFighter.companion.model);
      }
      else
      {
         player = CurrentFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer");
         ReverseFocusOnTarget(CurrentFighter.model);
      }

      player.Play("Cast");
      await ToSignal(GetTree().CreateTimer(player.CurrentAnimationLength + 0.35f), "timeout");
      UpdateSingularUIPanel(CurrentFighter);
      player.Play("CombatIdle");

      Panel companionUIHolder = CurrentFighter.UIPanel.GetNode<Panel>("CompanionHolder");
      if (CurrentFighter.companion != null && companionUIHolder.Visible == false)
      {
         companionUIHolder.Visible = true;
         CurrentFighter.companion.model.Visible = true;
      }

      double waitTime = FocusOnTargets(targets, playHitAnimation);
      
      await ToSignal(GetTree().CreateTimer(waitTime + 0.35f), "timeout");

      for (int i = 0; i < targets.Count; i++)
      {
         targets[i].placementNode.GetNode<Label3D>("CritLabel").Visible = false;

         if (targets[i].currentHealth > 0)
         {
            targets[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
         }
      }

      ProcessValues();
   }

   public void CreateCompanion(Fighter caster, Enemy companionData, int duration)
   {
      Companion newCompanion = new Companion();
      newCompanion.enemyDataSource = companionData;

      newCompanion.companionName = companionData.enemyName;

      for (int i = 0; i < 10; i++)
      {
         newCompanion.stats[i] = new Stat();
         newCompanion.stats[i].statType = companionData.stats[i].statType;
         newCompanion.stats[i].value = companionData.stats[i].value;
         newCompanion.stats[i].baseValue = companionData.stats[i].value;
      }

      newCompanion.affinity = companionData.affinity;

      for (int i = 0; i < companionData.abilities.Length; i++)
      {
         if (companionData.abilities[i].requiredLevel <= CurrentFighter.level)
         {
            newCompanion.abilities.Add(companionData.abilities[i]);
         }
      }

      //newCompanion.maxHealth = companionData.stats[0].value * caster.level;
      //newCompanion.currentHealth = newCompanion.maxHealth;

      newCompanion.maxMana = companionData.stats[1].value * caster.level / 2;
      newCompanion.currentMana = newCompanion.maxMana;

      newCompanion.duration = duration;

      caster.companion = newCompanion;
      InitializeCompanionGraphics(caster);
   }

   void InitializeCompanionGraphics(Fighter affectedFighter)
   {
      Panel companionUIHolder = affectedFighter.UIPanel.GetNode<Panel>("CompanionHolder");

      companionUIHolder.GetNode<Label>("Title").Text = affectedFighter.companion.companionName;
      companionUIHolder.GetNode<Label>("ManaDescription").Text = affectedFighter.companion.currentMana + "/" + affectedFighter.companion.maxMana;
      companionUIHolder.GetNode<ProgressBar>("ManaBar").Value = 100;
      companionUIHolder.GetNode<Label>("Duration").Text = "" + affectedFighter.companion.duration;

      //companionUIHolder.Visible = true;

      //GetNode<Node3D>("/root/BaseNode").AddChild(affectedFighter.companion.model);

      PackedScene packedSceneModel = GD.Load<PackedScene>(affectedFighter.companion.enemyDataSource.model.ResourcePath);
      affectedFighter.companion.model = packedSceneModel.Instantiate<Node3D>();
      baseNode.AddChild(affectedFighter.companion.model);

      Node3D fighterModel = affectedFighter.model.GetNode<Node3D>("Model");
      Basis fighterBasis = fighterModel.GlobalTransform.Basis;

      affectedFighter.companion.model.GlobalPosition = fighterModel.GlobalPosition - fighterBasis.Z - (fighterBasis.X * 1.5f);
      affectedFighter.companion.model.Rotation = Vector3.Zero;
      affectedFighter.companion.model.GetNode<Node3D>("Model").Rotation = fighterModel.Rotation;
      //affectedFighter.companion.model.GetNode<Node3D>("Model").RotateY(Mathf.DegToRad(-180f));
      //partyManager.party[i].model.GetNode<Node3D>("Model").Rotation = new Vector3(-0, Mathf.DegToRad(90f), 0);
      affectedFighter.companion.model.GetNode<Node3D>("Model").Position = Vector3.Zero;


      affectedFighter.companion.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
      affectedFighter.companion.model.Visible = false;
   }
}

public partial class AppliedStatusEffect
{
   public StatusEffect effect;
   public int remainingTurns;
   public Fighter applier;
   public bool displayRemainingTurns;
   public bool isCleanseable;
   public bool isNegative;

   public AppliedStatusEffect(StatusEffect effect, int remainingTurns, Fighter applier)
   {
      this.effect = effect;
      this.remainingTurns = remainingTurns;
      this.applier = applier;
   }
}

public partial class StatModifier
{
   public StatType statType;
   public int modifier;
   public int duration;
   public Fighter applier;
   public StatusEffect attachedStatus;

   public StatModifier(StatType statType, int modifier, int duration, Fighter applier, StatusEffect attachedStatus)
   {
      this.statType = statType;
      this.modifier = modifier;
      this.duration = duration;
      this.applier = applier;
      this.attachedStatus = attachedStatus;
   }
}

public partial class Stack
{
   public string stackName;
   public int quantity;
   public string spriteName;
   public bool needsQuantityUpdate;
   
   public Stack(string stackName, int quantity, string spriteName)
   {
      this.stackName = stackName;
      this.quantity = quantity;
      this.spriteName = spriteName;
   }
}

public partial class DamagingEntity
{
   public int baseDamage;
   public StatType scalingAttack;
   public StatType scalingDefense;
   public DamageType damageType;

   public DamagingEntity(int baseDamage, StatType scalingAttack, StatType scalingDefense, DamageType damageType)
   {
      this.baseDamage = baseDamage;
      this.scalingAttack = scalingAttack;
      this.scalingDefense = scalingDefense;
      this.damageType = damageType;
   }
}

public partial class OlrenSpecialArrow
{
   public string arrowName;
   public int requiredLevel;
   public string description;

   public OlrenSpecialArrow(string arrowName, int requiredLevel, string description)
   {
      this.arrowName = arrowName;
      this.requiredLevel = requiredLevel;
      this.description = description;
   }
}