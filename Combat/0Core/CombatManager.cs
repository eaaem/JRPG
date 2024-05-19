using Godot;
using System;
using System.Collections.Generic;

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
   private AffinityStrengths[] affinityTable = new AffinityStrengths[9];
   [Export]
   public StatusData[] StatusDatas { get; set; }
   [Export]
   private ManagerReferenceHolder managers;
   [Export]
   private CombatUIManager uiManager;
   [Export]
   private CombatAbilityManager abilityManager;
   [Export]
   private CombatItemManager itemManager;
   [Export]
   private CombatStackStatusManager stacksAndStatusManager;
   [Export]
   private CombatPassiveManager passiveManager;

   [Export]
   private Camera3D playerCamera;
   private Camera3D arenaCamera;

   private Node3D arena;
   [Export]
   private Node3D baseNode;

   public List<Fighter> Fighters { get; set; }

   private bool isAttacking;
   private bool isCasting;
   public bool IsCompanionTurn { get; set; }
   public Fighter CurrentFighter { get; set; }
   public Fighter CurrentTarget { get; set; }
   public InventoryItem CurrentItem { get; set; }
   public AbilityResource CurrentAbility { get; set; }

   public string AbilityTargetGraphic { get; set; }

   private double deathWaitTime = 0;
   private bool cameraPanCompleted = false;

   [Signal]
   public delegate void AttackAnimationEventHandler();
   [Signal]
   public delegate void AddBadgeEventHandler();

   public bool IsInCombat { get; set; }

   private Vector3 returnPosition;
   private Vector3 returnRotation;

   private WorldEnemy currentEnemyScript;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      Fighters = new List<Fighter>();
      StatusDatas = new StatusData[0];
      AbilityTargetGraphic = string.Empty;
	}

   public void ResetNodes()
   {
      arena = baseNode.GetNode<Node3D>("Level/Arena");
      arenaCamera = arena.GetNode<Camera3D>("ArenaCamera");
   }

   public async void SetupCombat(List<Enemy> enemyDatas, Vector3 location, Vector3 rotation, WorldEnemy enemyScript) 
   {
      ResetNodes();

      managers.MenuManager.CloseMenu();
      managers.MenuManager.canTakeInput = false;
      managers.ItemPickupManager.itemPickupContainer.Visible = false;

      returnPosition = location;
      returnRotation = rotation;
      Fighters.Clear();
      IsInCombat = true;
      managers.Controller.DisableMovement = true;
      managers.Controller.IsSprinting = false;
      currentEnemyScript = enemyScript;

      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].isInParty)
         {
            managers.PartyManager.Party[i].model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("Encounter");
         }
      }

      await ToSignal(GetTree().CreateTimer(0.75f), "timeout");
      
      managers.SaveManager.FadeToBlack();

      uiManager.HidePanels();

      InitializeParty();
      InitializeEnemies(enemyDatas);
      uiManager.ClearItemUI();
      uiManager.GenerateItemUI();
      uiManager.EnableOptions();
      uiManager.DisablePanels();

      Input.MouseMode = Input.MouseModeEnum.Visible;
      MovePartyToArena();
      uiManager.UpdateUI();
      playerCamera.Current = false;
      arenaCamera.MakeCurrent();

      enemyScript.QueueFree();

      PointCameraAtEnemies();
      managers.SaveManager.FadeFromBlack();

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

	   uiManager.ShowLists();
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
      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].isInParty)
         {
            Fighter newFighter = new Fighter();
            newFighter.fighterName = managers.PartyManager.Party[i].characterName;
            newFighter.level = managers.PartyManager.Party[i].level;
            newFighter.isEnemy = false;
            newFighter.affinity = managers.PartyManager.Party[i].affinity;

            newFighter.currentHealth = managers.PartyManager.Party[i].currentHealth;
            newFighter.currentMana = managers.PartyManager.Party[i].currentMana;

            newFighter.maxHealth = managers.PartyManager.Party[i].GetMaxHealth();
            newFighter.maxMana = managers.PartyManager.Party[i].GetMaxMana();

            for (int j = 0; j < 10; j++) 
            {
               newFighter.stats[j] = new Stat();
               newFighter.stats[j].statType = managers.PartyManager.Party[i].stats[j].statType;
               newFighter.stats[j].value = managers.PartyManager.Party[i].stats[j].value;
               newFighter.stats[j].baseValue = managers.PartyManager.Party[i].stats[j].value;
            }

            for (int j = 0; j < managers.PartyManager.Party[i].abilities.Count; j++)
            {
               newFighter.abilities.Add(managers.PartyManager.Party[i].abilities[j]);
            }
         
            uiManager.InitializePartyPanel(newFighter, i);

            newFighter.model = managers.PartyManager.Party[i].model;
            newFighter.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");

            BoneAttachment3D attachment = newFighter.model.GetNode<BoneAttachment3D>("Model/Armature/Skeleton3D/WeaponAttachment");
            attachment.BoneName = "hand.L";

            SetWeaponAttachmentToIdle(newFighter);

            if (newFighter.model.Name != "Member1")
            {
               newFighter.model.GetParent().GetNode<OverworldPartyController>("" + newFighter.model.Name).EnablePathfinding = false;
            }

            passiveManager.ApplyPassives(newFighter);

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

         uiManager.InitializeEnemyPanel(newFighter, i);

		   Fighters.Add(newFighter);
      }
   }

   void MovePartyToArena()
   {
      List<Member> inParty = new List<Member>();

      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].isInParty)
         {
            inParty.Add(managers.PartyManager.Party[i]);
         }
      }
   
      Node3D placementGroup = arena.GetNode<Node3D>("PartyGroup" + inParty.Count);
      for (int i = 0; i < inParty.Count; i++)
      {
         inParty[i].model.GlobalPosition = placementGroup.GetNode<Node3D>("PartyPlacement" + (i + 1)).GlobalPosition;
         Fighters[i].placementNode = placementGroup.GetNode<Node3D>("PartyPlacement" + (i + 1));
         inParty[i].model.GetNode<Node3D>("Model").LookAt(arena.GlobalPosition);
         inParty[i].model.GetNode<Node3D>("Model").RotateY(Mathf.DegToRad(-180));
         inParty[i].model.GetNode<Node3D>("Model").Position = Vector3.Zero;
      }       
   }

   /// <summary>
   /// Places the camera at its "default" place
   /// </summary>
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

   async void SelectNextTurn() 
   {
      if (CurrentFighter != null)
      {
         uiManager.SetHightlightVisibility(CurrentFighter, false, false);

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
               uiManager.SetHightlightVisibility(CurrentFighter, true, true);
               uiManager.ClearAbilityUI();
               uiManager.GenerateAbilityUI();
               PlayerTurn();
               return;
            }  
         }
         else if (CurrentFighter.companion != null)
         {
            uiManager.SetHightlightVisibility(CurrentFighter, false, true);
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
                  uiManager.SetActionBar(Fighters[i], Fighters[i].actionLevel);
               }

               if (Fighters[i].actionLevel >= 100) 
               {
                  await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

                  CurrentFighter = Fighters[i];
                  stacksAndStatusManager.IncrementAppliedStatuses();
                  IncrementStatModifiers();
                  uiManager.SetHightlightVisibility(CurrentFighter, true, false);

                  uiManager.ClearAbilityUI();

                  if (Fighters[i].isEnemy)
                  {
                     EnemyTurn();
                  }
                  else
                  {
                     uiManager.GenerateAbilityUI();
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
                     uiManager.UpdateSingularUIPanel(Fighters[i]);
                  }
                  else if (Fighters[i].statModifiers[j].statType == StatType.Knowledge)
                  {
                     Fighters[i].maxMana = Fighters[i].GetMaxMana();
                     uiManager.UpdateSingularUIPanel(Fighters[i]);
                  }
                  
                  Fighters[i].statModifiers.Remove(Fighters[i].statModifiers[j]);
               }
            }
         }
      }
   }

   public async void CompleteTurn()
   {
      uiManager.EnableOptions();

      uiManager.HideAll();
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
      uiManager.SetSelectionBoxVisible(true);

      if (IsCompanionTurn)
      {
         uiManager.SetItemButtonVisible(true);
         FocusCameraOnFighter(CurrentFighter.companion.model);
      }
      else
      {
         uiManager.SetItemButtonVisible(false);
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
         abilityManager.PlayerCastAbility(target);
      }
      else if (CurrentItem != null)
      {
         itemManager.UseItem(target);
      }

      uiManager.DisablePanels();
   }

   void OnAttackButtonDown()
   {
      uiManager.DisableOptions();
      uiManager.EnablePanels();
      isAttacking = true;
      uiManager.SetCancelButtonVisible(true);

      // Olren's arrows get special effects
      passiveManager.OlrenPassive();
   }

   void OnCastButtonDown()
   {
      uiManager.CastButton();
   }

   void OnItemButtonDown()
   {
      uiManager.ItemButton();
   }

   void OnCancelButtonDown()
   {
      if (isAttacking)
      {
         uiManager.EnableOptions();
      }
      else if (CurrentAbility != null)
      {
         uiManager.UpdateAbilities();
         CurrentAbility = null;

         for (int i = 0; i < Fighters.Count; i++)
         {
            Fighters[i].placementNode.GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;
         }
      }
      else if (CurrentItem != null)
      {
         uiManager.EnableItems();
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
      uiManager.DisablePanels();
      uiManager.HideAll();
   }

   public Member GetCurrentMember()
   {
      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (CurrentFighter.fighterName == managers.PartyManager.Party[i].characterName)
         {
            return managers.PartyManager.Party[i];
         }
      }
      return null;
   }

   async void CompleteAttack(Fighter target)
   {
      uiManager.HideAll();

      CurrentTarget = target;
      DamagingEntity damage = new DamagingEntity(CurrentFighter.level, StatType.Strength, StatType.Fortitude, DamageType.Physical);

      if (IsCompanionTurn)
      {
         damage.baseDamage = CurrentFighter.level / 2;
      }

      // Athlia's attacks deal 10% damage
      damage.baseDamage = passiveManager.AthliaPassive(damage.baseDamage);

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
      passiveManager.ApplyOlrenPassive();
      
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
                     stacksAndStatusManager.RemoveStatus(targets[i], j);
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
        AbilityGraphic abilityGraphic = abilityManager.GenerateTargetAbilityGraphic(AbilityTargetGraphic);

         if (abilityGraphic.GenerateOnlyOnce)
         {
            abilityManager.SetTargetAbilityGraphics(abilityGraphic, targets[0].placementNode.GlobalPosition);
         }
         else
         {
            for (int i = 0; i < targets.Count; i++)
            {
               abilityManager.SetTargetAbilityGraphics(abilityGraphic, targets[i].placementNode.GlobalPosition);
            }
         }
      }

      for (int i = 0; i < targets.Count; i++)
      {
         uiManager.UpdateSingularUIPanel(targets[i]);
         uiManager.UpdateStacksAndEffectsUI(targets[i]);
      }

      return longestDuration;
   }

   void EnemyTurn()
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
      }
      else
      {
         abilityManager.SelectEnemyAbility(target, validAbilities);
      }
   }

   public void ProcessAttack(int damage, Fighter target, bool bypassHitChance = false)
   {
      for (int i = 0; i < CurrentFighter.currentStatuses.Count; i++)
      {
         if (CurrentFighter.currentStatuses[i].effect == StatusEffect.Stealth)
         {
            stacksAndStatusManager.RemoveStatus(CurrentFighter, i);
         }
      }

      if (HitAttack(target))
      {
         target.currentHealth -= damage;
         
         if (!target.isEnemy)
         {
            passiveManager.ApplyPassives(target);
         }
      }
   }

   public Fighter SelectEnemyTarget(bool selectAlly, bool includeSelf)
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
      }

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

   public void ApplyStatModifier(StatType statType, int modifier, int duration, Fighter affectedFighter, Fighter applier, StatusEffect attachedStatus)
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
      if (passiveManager.ThalriaPassive(target))
      {
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

   async void Victory()
   {
      ResetCombat();
      PointCameraAtParty();

      int expGain = 0;

      managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].defeatedEnemies[currentEnemyScript.id.ToString()] = true;

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
      
      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].isInParty)
         {
            managers.PartyManager.Party[i].experience += expGain;
            int oldLevel = managers.PartyManager.Party[i].level;
            int oldAbilityCount = managers.PartyManager.Party[i].abilities.Count;

            Fighter fighter = GetFighterFromMember(managers.PartyManager.Party[i]);
            managers.PartyManager.Party[i].currentHealth = Mathf.Clamp(fighter.currentHealth, 1, 9999);
            managers.PartyManager.Party[i].currentMana = fighter.currentMana;
            managers.PartyManager.LevelUp(expGain, managers.PartyManager.Party[i]);
            uiManager.UpdateVictoryExp(managers.PartyManager.Party[i], i, oldLevel, oldAbilityCount);
         }
      }

      uiManager.SetVictoryScreenVisible(true);
   }

   void OnExitButtonDown()
   {
      managers.SaveManager.FadeToBlack();
      uiManager.ExitVictoryScreen();
      managers.PartyManager.Party[0].model.Position = returnPosition;
      managers.PartyManager.Party[0].model.GetNode<Node3D>("Model").GlobalRotation = returnRotation;
      managers.Controller.PlaceWeaponOnBack();
      managers.Controller.PlaceSecondaryWeaponOnBack();

      Vector3 currentReturnPosition = returnPosition;
      currentReturnPosition -= managers.PartyManager.Party[0].model.GlobalTransform.Basis.Z;

      for (int i = 1; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].isInParty)
         {
            managers.PartyManager.Party[i].model.Position = currentReturnPosition;
            currentReturnPosition -= managers.PartyManager.Party[0].model.GlobalTransform.Basis.Z;
            OverworldPartyController overworldController = managers.PartyManager.Party[i].model.GetNode<OverworldPartyController>("../Member" + (i + 1));
            overworldController.EnablePathfinding = true;
            overworldController.PlaceWeaponOnBack();
            overworldController.PlaceSecondaryWeaponOnBack();
         }
      }

      playerCamera.MakeCurrent();
      Input.MouseMode = Input.MouseModeEnum.Captured;
      managers.Controller.DisableMovement = false;

      managers.SaveManager.FadeFromBlack();

      if (currentEnemyScript.postBattleCutsceneName.Length > 0)
      {
         GetNode<CutsceneManager>("/root/BaseNode/CutsceneManager").CutsceneSignalReceiver(currentEnemyScript.postBattleCutsceneName);
      }
   }

   void Loss()
   {
      managers.SaveManager.FadeToBlack();
      uiManager.SetDefeatScreenVisible(true);
      IsInCombat = false;
   }

   void OnReloadLastButtonDown()
   {
      managers.SaveManager.ResetGameState();
      managers.SaveManager.LoadGame(managers.SaveManager.currentSaveIndex);
      GetNode<Sprite2D>("/root/BaseNode/MainMenu/Background").Visible = false;
      managers.SaveManager.FadeFromBlack();
      uiManager.SetDefeatScreenVisible(false);
      Input.MouseMode = Input.MouseModeEnum.Captured;
      managers.Controller.DisableMovement = false;
      ResetCombat();
   }

   void OnQuitToMenuButtonDown()
   {
      managers.SaveManager.ResetGameState();
      managers.SaveManager.FadeFromBlack();
      ResetCombat();
      uiManager.SetDefeatScreenVisible(false);
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

         uiManager.ResetStacksAndStatuses(Fighters[i]);
      }

      IsInCombat = false;
      uiManager.EndOfCombatHiding();

      CurrentItem = null;
      CurrentAbility = null;
      isAttacking = false;

      managers.MenuManager.canTakeInput = true;
   }

   public async void RegularCast(List<Fighter> targets, bool playHitAnimation = true)
   {
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
      uiManager.UpdateSingularUIPanel(CurrentFighter);
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

      PackedScene packedSceneModel = GD.Load<PackedScene>(affectedFighter.companion.enemyDataSource.model.ResourcePath);
      affectedFighter.companion.model = packedSceneModel.Instantiate<Node3D>();
      baseNode.AddChild(affectedFighter.companion.model);

      Node3D fighterModel = affectedFighter.model.GetNode<Node3D>("Model");
      Basis fighterBasis = fighterModel.GlobalTransform.Basis;

      affectedFighter.companion.model.GlobalPosition = fighterModel.GlobalPosition - fighterBasis.Z - (fighterBasis.X * 1.5f);
      affectedFighter.companion.model.Rotation = Vector3.Zero;
      affectedFighter.companion.model.GetNode<Node3D>("Model").Rotation = fighterModel.Rotation;
      affectedFighter.companion.model.GetNode<Node3D>("Model").Position = Vector3.Zero;


      affectedFighter.companion.model.GetNode<AnimationPlayer>("Model/AnimationPlayer").Play("CombatIdle");
      affectedFighter.companion.model.Visible = false;
   }

   public Fighter GetFighterFromIndex(int index, bool isEnemy)
   {
      Node2D UIPanel = uiManager.GetPanelFromIndex(index, isEnemy);

      for (int i = 0; i < Fighters.Count; i++)
      {
         if (Fighters[i].UIPanel == UIPanel)
         {
            return Fighters[i];
         }
      }

      return null;
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